using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Create
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static MethodInfo MethodInfo(this Type declaringType, string methodName, List<Type> paramTypes)
        {
            Compute.LoadAssemblies();

            MethodInfo foundMethod = null;
            List<MethodInfo> methods = declaringType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();

            for (int k = 0; k < methods.Count; k++)
            {
                MethodInfo method = methods[k];

                if (method.Name == methodName)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == paramTypes.Count)
                    {
                        if (method.ContainsGenericParameters)
                        {
                            Type[] generics = method.GetGenericArguments().Select(x => GetTypeFromGenericParameters(x)).ToArray();
                            method = method.MakeGenericMethod(generics);
                            parameters = method.GetParameters();
                        }

                        bool matching = true;
                        for (int i = 0; i < paramTypes.Count; i++)
                        {
                            matching &= (paramTypes[i] == null || parameters[i].ParameterType == paramTypes[i]);
                        }
                        if (matching)
                        {
                            foundMethod = method;
                            break;
                        }
                    }
                }
            }

            return foundMethod;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static Type GetTypeFromGenericParameters(Type type)
        {
            Type[] constrains = type.GetGenericParameterConstraints();
            if (constrains.Length == 0)
                return typeof(object);
            else
                return constrains[0];
        }

        /*************************************/
    }
}
