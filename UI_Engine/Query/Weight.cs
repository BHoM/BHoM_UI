using BH.Engine.Reflection;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static double Weight(this SearchItem item, string[] search)
        {
            // Get the key for initial filtering
            string key = item.Text.ToLower();
            if (item.Item is MethodInfo && ((MethodInfo)item.Item).DeclaringType.Name == "Create")
                key = "create." + key;

            // if the search doesn't match the key, just return 0.
            if (!search.All(p => key.Contains(p)))
                return 0;

            // else start with a weight of 1.0
            double weight = 1.0;

            // Collect the name of the item
            string name = "";
            if (item.Item is MethodBase)
            {
                MethodBase method = item.Item as MethodBase;
                string declaringName = method.DeclaringType.Name.ToLower();

                if (method is MethodInfo)
                {
                    name = method.Name;

                    if (search.Any(p => p == declaringName))
                        weight += 8;
                    else if (search.Any(p => declaringName.Contains(p)))
                        weight += 4;
                }
                else
                    name = declaringName;

                foreach (string part in search.Where(p => method.DeclaringType.Namespace.ToLower().Contains(p)))
                    weight += 2;
            }
            else if (item.Item is Type)
            {
                Type type = item.Item as Type;
                name = type.Name;
            }
            else if (item.Item == null || item.Item is string)
            {
                name = item.Text.Split(new char[] { '.', '\\', '/' }).First();
            }

            // Increase weight if name is matching
            name = name.ToLower();
            if (search.Any(p => p == name))
                weight += 20;
            else if (search.Any(p => name.Contains(p)))
                weight += 10;

            return weight;
        }

        /*************************************/
    }
}
