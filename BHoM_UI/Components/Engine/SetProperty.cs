﻿using BH.Engine.Reflection;
using BH.oM.Base;
using BH.oM.Reflection;
using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Components
{
    public class SetPropertyCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.BHoM_SetProperty;

        public override Guid Id { get; protected set; } = new Guid("A186D4F1-FC80-499B-8BBF-ECDD49BF6E6E");

        public override string Name { get; protected set; } = "SetProperty";

        public override int GroupIndex { get; protected set; } = 2;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public SetPropertyCaller() : base(typeof(BH.Engine.Reflection.Modify).GetMethod("PropertyValue", new Type[] { typeof(BHoMObject), typeof(string), typeof(object) })) {}


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        protected override object[] CollectInputs()
        {
            object[] inputs = new object[] { };
            try
            {
                if (m_CompiledGetters.Count == 3)
                {
                    object obj = m_CompiledGetters[0](DataAccessor);
                    string propName = m_CompiledGetters[1](DataAccessor) as string;

                    if (propName != m_CurrentProperty && obj != null)
                    {
                        m_CurrentProperty = propName;

                        Type objType = obj.GetType();
                        if (objType != null)
                        {
                            PropertyInfo propInfo = objType.GetProperty(propName);
                            if (propInfo != null)
                                m_CompiledGetters[2] = CreateInputAccessor(propInfo.PropertyType, 2);
                        }
                    }

                    inputs = new object[] { obj, propName, m_CompiledGetters[2](DataAccessor) };
                }
            }
            catch (Exception e)
            {
                RecordError(e, "This component failed to run properly. Inputs cannot be collected properly.\n");
                inputs = null;
            }

            return inputs;
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private string m_CurrentProperty = "";

        /*************************************/
    }
}
