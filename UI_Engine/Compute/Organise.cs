using BH.Engine.DataStructure;
using BH.Engine.Reflection.Convert;
using BH.oM.DataStructure;
using BH.oM.Reflection;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static Output<List<SearchItem>, Tree<T>> OrganiseItems<T>(this List<T> items)
        {
            if (typeof(T) == typeof(MethodBase))
                return OrganiseMethods(items.Cast<MethodBase>().ToList()) as Output<List<SearchItem>, Tree<T>>;
            if (typeof(T) == typeof(Type))
                return OrganiseTypes(items.Cast<Type>().ToList()) as Output<List<SearchItem>, Tree<T>>;
            else
                return OrganiseOthers(items) as Output<List<SearchItem>, Tree<T>>;
        }

        /*************************************/

        public static Output<List<SearchItem>, Tree<MethodBase>> OrganiseMethods(this List<MethodBase> methods)
        {
            // Create method list
            IEnumerable<string> paths = methods.Select(x => x.ToText(true));
            List<SearchItem> list = paths.Zip(methods, (k, v) => new SearchItem { Text = k, Item = v }).ToList();

            //Create method tree
            List<string> toSkip = new List<string> { "Compute", "Convert", "Create", "Modify", "Query" };
            Tree<MethodBase> tree = DataStructure.Create.Tree(methods, paths.Select(x => x.Split('.').Except(toSkip).ToList()).ToList(), "Select a method");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;
            tree = tree.GroupMethodsByName();
            
            return new Output<List<SearchItem>, Tree<MethodBase>> { Item1 = list, Item2 = tree };
        }

        /*************************************/

        public static Output<List<SearchItem>, Tree<Type>> OrganiseTypes(this List<Type> types)
        {
            // Create type list
            IEnumerable<string> paths = types.Select(x => x.ToText(true));
            List<SearchItem> list = paths.Zip(types, (k, v) => new SearchItem { Text = k, Item = v }).ToList();

            //Create type tree
            Tree<Type> tree = DataStructure.Create.Tree(types, paths.Select(x => x.Split('.').ToList()).ToList(), "select a type");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;

            return new Output<List<SearchItem>, Tree<Type>> { Item1 = list, Item2 = tree };
        }

        /*************************************/

        public static Output<List<SearchItem>, Tree<T>> OrganiseOthers<T>(this List<T> items)
        {
            // Create item list
            IEnumerable<string> paths = items.Select(x => x.ToString());
            List<SearchItem> list = paths.Zip(items, (k, v) => new SearchItem { Text = k, Item = v }).ToList();

            //Create ietm tree
            Tree<T> tree = DataStructure.Create.Tree(items, paths.Select(x => x.Split(new char[] { '.', '/', '\\' }).ToList()).ToList(), "select an item");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;

            return new Output<List<SearchItem>, Tree<T>> { Item1 = list, Item2 = tree };
        }

        /*************************************/
    }
}
