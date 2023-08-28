using BH.oM.Base.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        [Description("Obtain how many levels of nesting are involved in an object, if the object is an IEnumerable object (List, array, etc.).")]
        [Input("obj", "Object to check how many levels of nesting it contains.")]
        [Output("levelsOfNesting", "0 if the object is not an IEnumerable or is a char[]. 1 if the object is a flat IEnumerable of objects (e.g. List<in>). 2 or more for greater nesting (e.g. 2 for List<List<int>>).")]
        public static int LevelsOfNesting(object obj)
        {
            var type = obj.GetType();

            int levelsOfNest = 0;
            while (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                try
                {
                    obj = ((IEnumerable<object>)obj).FirstOrDefault();
                    type = obj.GetType();
                }
                catch
                {
                    break;
                }
                levelsOfNest++;
            }

            return levelsOfNest;
        }
    }
}
