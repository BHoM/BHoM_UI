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
using BH.Engine.Serialiser;

namespace BH.UI.Templates
{
    public class MethodCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public MethodBase Method { get; protected set; } = null;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public MethodCaller() : base()
        {
        }
        
        /*************************************/

        public MethodCaller(MethodBase method) : base()
        {
            SetItem(method);
        }

        /*************************************/

        public MethodCaller(Type methodDeclaringType, string methodName, List<Type> paramTypes) : base()
        {
            SetItem(BH.Engine.UI.Create.MethodInfo(methodDeclaringType, methodName, paramTypes));
            CompileFunction();
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public override bool SetItem(object method)
        {
            Method = method as MethodBase;

            SetName();
            SetCategory();
            SetDescription();

            SetInputParams();
            SetOutputParams();

            CompileFunction();
            CompileInputGetters();
            CompileOutputSetters();

            return true;
        }

        /*************************************/

        public override object Run(object[] inputs)
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
        /**** Private Methods             ****/
        /*************************************/

        protected virtual void CompileFunction()
        {
            if (Method == null)
                return;

            ParameterExpression lambdaInput = Expression.Parameter(typeof(object[]), "x");
            Expression[] inputs = Method.GetParameters().Select((x, i) => Expression.Convert(Expression.ArrayIndex(lambdaInput, Expression.Constant(i)), x.ParameterType)).ToArray();

            if (Method is MethodInfo)
            {
                MethodCallExpression methodExpression = Expression.Call(Method as MethodInfo, inputs);
                m_CompiledFunc = Expression.Lambda<Func<object[], object>>(Expression.Convert(methodExpression, typeof(object)), lambdaInput).Compile();
            }
            else if (Method is ConstructorInfo)
            {
                NewExpression constructorExpression = Expression.New(Method as ConstructorInfo, inputs);
                m_CompiledFunc = Expression.Lambda<Func<object[], object>>(Expression.Convert(constructorExpression, typeof(object)), lambdaInput).Compile();
            }
            
        }

        /*************************************/
        protected virtual void SetName()
        {
            if (Method == null)
                return;

            if (Method is MethodInfo)
                Name = Method.Name;
            else if (Method is ConstructorInfo)
                Name = Method.DeclaringType.Name;
            else
                Name = "UnknownMethod";
        }

        /*************************************/

        protected virtual void SetDescription()
        {
            if (Method != null)
                Description = Method.Description();
        }

        /*************************************/

        protected virtual void SetCategory()
        {
            if (Method != null)
            {
                string[] nameSpace = Method.DeclaringType.Namespace.Split('.');
                if (nameSpace.Length >= 2 && nameSpace[0] == "BH")
                    Description = nameSpace[1];
                else
                    Description = "Other";
            }
        }

        /*************************************/

        public virtual void SetInputParams()
        {
            if (Method == null)
                InputParams = new List<ParamInfo>();
            else
            {
                Dictionary<string, string> descriptions = Method.InputDescriptions();
                InputParams = Method.GetParameters().Select(x => new ParamInfo
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

        public virtual void SetOutputParams()
        {
            if (Method == null)
                OutputParams = new List<ParamInfo>();
            else
            {
                if (Method.IsMultipleOutputs())
                {
                    Type[] subTypes = Method.OutputType().GenericTypeArguments;
                    List<OutputAttribute> attributes = Method.OutputAttributes();
                    if (subTypes.Length == attributes.Count)
                    {
                        OutputParams = Method.OutputAttributes().Select((x, i) => new ParamInfo
                        {
                            Name = x.Name,
                            DataType = subTypes[i],
                            Description = x.Description,
                            Kind = ParamKind.Output
                        }).ToList();
                    }
                    else
                    {
                        OutputParams = subTypes.Select(x => new ParamInfo
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
                    Type nameType = Method.OutputType().UnderlyingType().Type;
                    string name = Method.OutputName();
                    OutputParams = new List<ParamInfo> {
                        new ParamInfo
                        {
                            Name = (name == "") ? nameType.Name.Substring(0, 1) : name,
                            DataType = Method.OutputType(),
                            Description = Method.OutputDescription(),
                            Kind = ParamKind.Output
                        }
                    };
                }
            }
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected Func<object[], object> m_CompiledFunc = null;

        /*************************************/
    }
}
