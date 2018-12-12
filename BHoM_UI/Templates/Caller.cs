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
using System.Windows.Forms;

namespace BH.UI.Templates
{
    public abstract class Caller
    {
        /*************************************/
        /**** Events                      ****/
        /*************************************/

        public event EventHandler<object> ItemSelected;

        public event EventHandler SolutionExpired;


        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public virtual System.Drawing.Bitmap Icon_24x24 { get; protected set; }

        public virtual Guid Id { get; protected set; }

        public virtual string Name { get; protected set; } = "Undefined";

        public virtual string Category { get; protected set; } = "Undefined";

        public virtual string Description { get; protected set; } = "";

        public virtual int GroupIndex { get; protected set; } = 1;

        public virtual ISelector Selector { get; protected set; } = null;

        public DataAccessor DataAccessor { get; protected set; } = null;

        public List<ParamInfo> InputParams { get; protected set; } = new List<ParamInfo>();

        public List<ParamInfo> OutputParams { get; protected set; } = new List<ParamInfo>();

        public object SelectedItem { get; set; } = null;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public Caller()
        {
            Engine.UI.Compute.LoadAssemblies();
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public virtual bool Run()
        {
            BH.Engine.Reflection.Compute.ClearCurrentEvents();

            // Get all the inputs
            object[] inputs = CollectInputs();
            if (inputs == null)
                return false;

            // Execute the method
            object result = null;
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
            return PushOutputs(result);
        }

        /*************************************/

        public abstract object Run(object[] inputs);

        /*************************************/

        public virtual bool SetItem(object item)
        {
            SelectedItem = item;
            return true;
        }

        /*************************************/

        public virtual void SetDataAccessor(DataAccessor accessor)
        {
            DataAccessor = accessor;
            CompileInputGetters();
            CompileOutputSetters();
        }

        /*************************************/

        public virtual void AddToMenu(ToolStripDropDown menu)
        {
            if (Selector != null && SelectedItem == null)
                Selector.AddToMenu(menu);
        }

        /*************************************/

        public virtual void AddToMenu(System.Windows.Controls.ContextMenu menu)
        {
            if (Selector != null && SelectedItem == null)
                Selector.AddToMenu(menu);
        }

        /*************************************/

        public virtual string Write()
        {
            try
            {
                if (SelectedItem == null)
                    return "";
                else
                    return SelectedItem.ToJson();
            }
            catch
            {
                return "";
            }
        }

        /*************************************/

        public virtual bool Read(string json)
        {
            if (json == "")
                return true;

            try
            {
                SetItem(BH.Engine.Serialiser.Convert.FromJson(json));

                if (SelectedItem != null)
                    ItemSelected?.Invoke(this, SelectedItem);

                return true;
            }
            catch
            {
                return false;
            }
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        protected virtual object[] CollectInputs()
        {
            object[] inputs = new object[] { };
            try
            {
                inputs = m_CompiledGetters.Select(x => x(DataAccessor)).ToArray();
            }
            catch (Exception e)
            {
                RecordError(e, "This component failed to run properly. Inputs cannot be collected properly.\n");
                inputs = null;
            }

            return inputs;
        }

        /*************************************/

        protected virtual bool PushOutputs(object result)
        {
            try
            {
                if (m_CompiledSetters.Count == 1)   // There is a problem when the output is a list of one apparently (try to explode a tree with a single branch on the first level)
                    m_CompiledSetters.First()(DataAccessor, result);
                else if (m_CompiledSetters.Count > 0)
                {
                    for (int i = 0; i < m_CompiledSetters.Count; i++)
                        m_CompiledSetters[i](DataAccessor, BH.Engine.Reflection.Query.Item(result as dynamic, i));
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

        protected virtual void CompileInputGetters()
        {
            if (DataAccessor == null)
                return;

            m_CompiledGetters = new List<Func<DataAccessor, object>>();

            for (int index = 0; index < InputParams.Count; index++)
            {
                ParamInfo param = InputParams[index];
                Func<DataAccessor, object> func = CreateInputAccessor(param.DataType, index);
                m_CompiledGetters.Add(func);
            }
        }

        /*************************************/

        protected virtual void CompileOutputSetters()
        {
            if (DataAccessor == null)
                return;

            m_CompiledSetters = new List<Func<DataAccessor, object, bool>>();

            for (int index = 0; index < OutputParams.Count; index++)
            {
                ParamInfo param = OutputParams[index];
                UnderlyingType subType = param.DataType.UnderlyingType();
                string methodName = (subType.Depth == 0) ? "SetDataItem" : (subType.Depth == 1) ? "SetDataList" : "SetDataTree";
                MethodInfo method = DataAccessor.GetType().GetMethod(methodName).MakeGenericMethod(subType.Type);

                ParameterExpression lambdaInput1 = Expression.Parameter(typeof(DataAccessor), "accessor");
                ParameterExpression lambdaInput2 = Expression.Parameter(typeof(object), "data");
                ParameterExpression[] lambdaInputs = new ParameterExpression[] { lambdaInput1, lambdaInput2 };

                Expression[] methodInputs = new Expression[] { Expression.Constant(index), Expression.Convert(lambdaInput2, method.GetParameters()[1].ParameterType) };
                MethodCallExpression methodExpression = Expression.Call(Expression.Convert(lambdaInput1, DataAccessor.GetType()), method, methodInputs);

                Func<DataAccessor, object, bool> function = Expression.Lambda<Func<DataAccessor, object, bool>>(methodExpression, lambdaInputs).Compile();
                m_CompiledSetters.Add(function);
            }
        }

        /*******************************************/

        protected virtual Func<DataAccessor, object> CreateInputAccessor(Type dataType, int index)
        {
            UnderlyingType subType = dataType.UnderlyingType();
            string methodName = (subType.Depth == 0) ? "GetDataItem" : (subType.Depth == 1) ? "GetDataList" : "GetDataTree";
            MethodInfo method = DataAccessor.GetType().GetMethod(methodName).MakeGenericMethod(subType.Type);

            ParameterExpression lambdaInput1 = Expression.Parameter(typeof(DataAccessor), "accessor");
            ParameterExpression[] lambdaInputs = new ParameterExpression[] { lambdaInput1 };

            Expression[] methodInputs = new Expression[] { Expression.Constant(index) };
            MethodCallExpression methodExpression = Expression.Call(Expression.Convert(lambdaInput1, DataAccessor.GetType()), method, methodInputs);

            return Expression.Lambda<Func<DataAccessor, object>>(Expression.Convert(methodExpression, typeof(object)), lambdaInputs).Compile();
        }

        /*******************************************/

        protected static void RecordError(Exception e, string message = "")
        {
            if (e.InnerException != null)
                message += e.InnerException.Message;
            else
                message += e.Message;
            BH.Engine.Reflection.Compute.RecordError(message);
        }

        /*******************************************/

        protected void SetPossibleItems<T>(IEnumerable<T> items)
        {
            Selector = new Selector<T>(items, Name);
            Selector.ItemSelected += (sender, e) =>
            {
                SetItem(e);
                if (SelectedItem != null)
                    ItemSelected?.Invoke(this, SelectedItem);
            };
        }

        /*************************************/

        protected void OnDataUpdated()
        {
            if (SolutionExpired != null)
                SolutionExpired?.Invoke(this, new EventArgs());
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected List<Func<DataAccessor, object>> m_CompiledGetters = new List<Func<DataAccessor, object>>();
        protected List<Func<DataAccessor, object, bool>> m_CompiledSetters = new List<Func<DataAccessor, object, bool>>();

        /*************************************/
    }
}
