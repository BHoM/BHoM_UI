using BH.oM.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Modify
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static Tree<MethodBase> GroupMethodsByName<MethodBase>(this Tree<MethodBase> tree)
        {

            if (tree.Children.Count > 0)
            {
                if (tree.Children.Values.Any(x => x.Value != null))
                {
                    var groups = tree.Children.GroupBy(x => 
                    {
                        int index = x.Key.IndexOf('(');
                        if (index > 0)
                            return x.Key.Substring(0, index);
                        else
                            return x.Key;
                    });

                    if (groups.Count() > 1)
                    {
                        Dictionary<string, Tree<MethodBase>> children = new Dictionary<string, Tree<MethodBase>>();
                        foreach (var group in groups)
                        {
                            if (group.Count() == 1)
                            {
                                if (group.First().Value.Value == null)
                                    children.Add(group.Key, group.First().Value);
                                else
                                children.Add(group.Key, new Tree<MethodBase> { Name = group.Key, Value = group.First().Value.Value });
                            }
                            else
                                children.Add(group.Key, new Tree<MethodBase> { Name = group.Key, Children = group.ToDictionary(x => x.Key, x => x.Value) });
                        }
                        tree.Children = children;
                    }
                }
                else
                {
                    foreach (var child in tree.Children.Values)
                        GroupMethodsByName(child);
                }
            }

            return tree;
        }

        /*************************************/
    }
}
