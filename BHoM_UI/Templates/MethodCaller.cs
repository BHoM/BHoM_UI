using BH.Engine.Reflection;
using BH.oM.Reflection;
using BH.Engine.UI;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Templates
{
    public class MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public virtual System.Drawing.Bitmap Icon_24x24 { get; protected set; }

        public virtual Guid Id { get; protected set; }

        public virtual int GroupIndex { get; } = 1;

        public MethodInfo Method { get { return m_Method; } }

        public DataAccessor DataAccessor { get { return m_DataAccessor; } }


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public MethodCaller(Type methodDeclaringType, string methodName, List<Type> paramTypes)
        {
            MethodInfo method = BH.Engine.UI.Create.MethodInfo(methodDeclaringType, methodName, paramTypes);
            CompileFunction();
        }


        /*************************************/

        public MethodCaller(MethodInfo method)
        {
            m_Method = method;
            CompileFunction();
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public void SetDataAccessor(DataAccessor accessor)
        {
            m_DataAccessor = accessor;
            CompileInputGetters();
            CompileOutputSetters();
        }


        /*************************************/

        public virtual bool Run()
        {
            BH.Engine.Reflection.Compute.ClearCurrentEvents();

            // Get all the inputs
            object[] inputs = new object[] { };
            try
            {
                inputs = m_CompiledGetters.Select(x => x(m_DataAccessor)).ToArray();
            }
            catch (Exception e)
            {
                RecordError(e, "This component failed to run properly. Inputs cannot be collected properly.\n");
                return false;
            }

            // Execute the method
            dynamic result = null;
            try
            {
                result = Run(inputs);
            }
            catch (Exception e)
            {
                RecordError(e, "This component failed to run properly. Are you sure you have the correct type of inputs?\n" +
                                 "Check their description for more details. Here is the error provided by the method:\n");
                return false;
            }

            // Set the output
            try
            {
                if (m_CompiledSetters.Count == 1)
                    m_CompiledSetters.First()(m_DataAccessor, result);
                else if (m_CompiledSetters.Count > 0)
                {
                    for (int i = 0; i < m_CompiledSetters.Count; i++)
                        m_CompiledSetters[i](m_DataAccessor, BH.Engine.Reflection.Query.Item(result as dynamic, i));
                }
            }
            catch (Exception e)
            {
                RecordError(e, "This component failed to run properly. Output data is calculated but cannot be set.\n");
                return false;
            }

            return true;
        }

        /*************************************/

        public virtual object Run(object[] inputs)
        {
            if (m_CompiledFunc != null)
                return m_CompiledFunc(inputs);
            else
            {
                BH.Engine.Reflection.Compute.RecordError("The component is not linked to a method.");
                return null;
            }    
        }

        /*************************************/

        public virtual string Name()
        {
            if (m_Method == null)
                return "Undefined";
            else
                return m_Method.Name;
        }

        /*************************************/

        public virtual string Description()
        {
            if (m_Method == null)
                return "";
            else
                return m_Method.Description();
        }

        /*************************************/

        public virtual string Category()
        {
            if (m_Method == null)
                return "Undefined";
            else
            {
                string[] nameSpace = m_Method.DeclaringType.Namespace.Split('.');
                if (nameSpace.Length >= 2 && nameSpace[0] == "BH")
                    return nameSpace[1];
                else
                    return "Other";
            }
        }

        /*************************************/

        public virtual List<ParamInfo> InputParams()
        {
            if (m_Method == null)
                return new List<ParamInfo>();
            else
            {
                Dictionary<string, string> descriptions = m_Method.InputDescriptions();
                return m_Method.GetParameters().Select(x => new ParamInfo
                {
                    Name = x.Name,
                    DataType = x.ParameterType,
                    Description = descriptions.ContainsKey(x.Name) ? descriptions[x.Name] : "",
                    Kind = ParamKind.Input,
                    HasDefaultValue = x.HasDefaultValue,
                    DefaultValue = x.DefaultValue
                }).ToList();
            }
        }

        /*************************************/

        public virtual List<ParamInfo> OutputParams()
        {
            if (m_Method == null)
                return null;
            else
            {
                if (m_Method.IsMultipleOutputs())
                {
                    Type[] subTypes = m_Method.ReturnType.GenericTypeArguments;
                    List<OutputAttribute> attributes = m_Method.OutputAttributes();
                    if (subTypes.Length == attributes.Count)
                    {
                        return m_Method.OutputAttributes().Select((x, i) => new ParamInfo
                        {
                            Name = x.Name,
                            DataType = subTypes[i],
                            Description = x.Description,
                            Kind = ParamKind.Output
                        }).ToList();
                    }
                    else
                    {
                        return subTypes.Select(x => new ParamInfo
                        {
                            Name = x.UnderlyingType().Type.Name.Substring(0, 1),
                            DataType = x,
                            Description = "",
                            Kind = ParamKind.Output
                        }).ToList();
                    }
                }
                else
                {
                    Type nameType = m_Method.ReturnType.UnderlyingType().Type;
                    string name = m_Method.OutputName();
                    return new List<ParamInfo> {
                        new ParamInfo
                        {
                            Name = (name == "") ? nameType.Name.Substring(0, 1) : name,
                            DataType = m_Method.ReturnType,
                            Description = m_Method.OutputDescription(),
                            Kind = ParamKind.Output
                        }
                    };
                }
            }  
        }

        
        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        protected virtual void CompileFunction()
        {
            if (m_Method == null)
                return;

            ParameterExpression lambdaInput = Expression.Parameter(typeof(object[]), "x");
            Expression[] inputs = m_Method.GetParameters().Select((x, i) => Expression.Convert(Expression.ArrayIndex(lambdaInput, Expression.Constant(i)), x.ParameterType)).ToArray();
            MethodCallExpression methodExpression = Expression.Call(m_Method, inputs);

            m_CompiledFunc = Expression.Lambda<Func<object[], object>>(Expression.Convert(methodExpression, typeof(object)), lambdaInput).Compile();
        }

        /*************************************/

        private void CompileInputGetters()
        {
            List<ParamInfo> inputParams = InputParams();
            m_CompiledGetters = new List<Func<DataAccessor, object>>();

            for (int index = 0; index < inputParams.Count; index++)
            {
                ParamInfo param = inputParams[index];
                UnderlyingType subType = param.DataType.UnderlyingType();
                string methodName = (subType.Depth == 0) ? "GetDataItem" : (subType.Depth == 1) ? "GetDataList" : "GetDataTree";
                MethodInfo method = m_DataAccessor.GetType().GetMethod(methodName).MakeGenericMethod(subType.Type);

                ParameterExpression lambdaInput1 = Expression.Parameter(typeof(DataAccessor), "accessor");
                ParameterExpression[] lambdaInputs = new ParameterExpression[] { lambdaInput1 };

                Expression[] methodInputs = new Expression[] { Expression.Constant(index) };
                MethodCallExpression methodExpression = Expression.Call(Expression.Convert(lambdaInput1, m_DataAccessor.GetType()), method, methodInputs);

                Func<DataAccessor, object> func = Expression.Lambda<Func<DataAccessor, object>>(Expression.Convert(methodExpression, typeof(object)), lambdaInputs).Compile();
                m_CompiledGetters.Add(func);
            }
        }

        /*************************************/

        private void CompileOutputSetters()
        {
            List<ParamInfo> outputParams = OutputParams();
            m_CompiledSetters = new List<Func<DataAccessor, object, bool>>();

            for (int index = 0; index < outputParams.Count; index++)
            {
                ParamInfo param = outputParams[index];
                UnderlyingType subType = param.DataType.UnderlyingType();
                string methodName = (subType.Depth == 0) ? "SetDataItem" : (subType.Depth == 1) ? "SetDataList" : "SetDataTree";
                MethodInfo method = m_DataAccessor.GetType().GetMethod(methodName).MakeGenericMethod(subType.Type);

                ParameterExpression lambdaInput1 = Expression.Parameter(typeof(DataAccessor), "accessor");
                ParameterExpression lambdaInput2 = Expression.Parameter(typeof(object), "data");
                ParameterExpression[] lambdaInputs = new ParameterExpression[] { lambdaInput1, lambdaInput2 };

                Expression[] methodInputs = new Expression[] { Expression.Constant(index), Expression.Convert(lambdaInput2, method.GetParameters()[1].ParameterType) };
                MethodCallExpression methodExpression = Expression.Call(Expression.Convert(lambdaInput1, m_DataAccessor.GetType()), method, methodInputs);

                Func<DataAccessor, object, bool> function = Expression.Lambda<Func<DataAccessor, object, bool>>(methodExpression, lambdaInputs).Compile();
                m_CompiledSetters.Add(function);
            }
        }

        /*******************************************/

        private static void RecordError(Exception e, string message = "")
        {
            if (e.InnerException != null)
                message += e.InnerException.Message;
            else
                message += e.Message;
            BH.Engine.Reflection.Compute.RecordError(message);
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected MethodInfo m_Method = null;
        protected Func<object[], object> m_CompiledFunc = null;

        private DataAccessor m_DataAccessor = null;

        protected List<Func<DataAccessor, object>> m_CompiledGetters = new List<Func<DataAccessor, object>>();
        protected List<Func<DataAccessor, object, bool>> m_CompiledSetters = new List<Func<DataAccessor, object, bool>>();

        /*************************************/
    }
}
