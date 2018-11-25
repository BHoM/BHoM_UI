using BH.Engine.DataStructure;
using BH.Engine.Reflection.Convert;
using BH.oM.DataStructure;
using BH.oM.Reflection;
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

        public static Output<List<Tuple<string, T>>, Tree<T>> OrganiseItems<T>(this IEnumerable<T> items)
        {
            if (typeof(T) == typeof(MethodBase))
                return OrganiseMethods(items.Cast<MethodBase>()) as Output<List<Tuple<string, T>>, Tree<T>>;
            if (typeof(T) == typeof(Type))
                return OrganiseTypes(items.Cast<Type>()) as Output<List<Tuple<string, T>>, Tree<T>>;
            else
                return OrganiseOthers(items) as Output<List<Tuple<string, T>>, Tree<T>>;
        }

        public static Output<List<Tuple<string, MethodBase>>, Tree<MethodBase>> OrganiseMethods(this IEnumerable<MethodBase> methods)
        {
            // Create method list
            IEnumerable<string> fullNames = methods.Select(x => x.ToText(true));
            List<Tuple<string, MethodBase>> list = fullNames.Zip(methods, (k, v) => new Tuple<string, MethodBase>(k, v)).ToList();

            //Create method tree
            IEnumerable<string> paths = methods.Select(x => x.DeclaringType.Namespace + "." + x.ToText(false));
            Tree<MethodBase> tree = BH.Engine.DataStructure.Create.Tree(methods, paths.Select(x => x.Split('.')), "Select a method");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;
            tree = tree.GroupMethodsByName();
            
            return new Output<List<Tuple<string, MethodBase>>, Tree<MethodBase>> { Item1 = list, Item2 = tree };
        }

        /*************************************/

        public static Output<List<Tuple<string, Type>>, Tree<Type>> OrganiseTypes(this IEnumerable<Type> types)
        {
            // Create type list
            IEnumerable<string> paths = types.Select(x => x.ToText(true));
            List<Tuple<string, Type>> list = paths.Zip(types, (k, v) => new Tuple<string, Type>(k, v)).ToList();

            //Create type tree
            Tree<Type> tree = Engine.DataStructure.Create.Tree(types, paths.Select(x => x.Split('.')), "select a type");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;

            return new Output<List<Tuple<string, Type>>, Tree<Type>> { Item1 = list, Item2 = tree };
        }

        /*************************************/

        public static Output<List<Tuple<string, T>>, Tree<T>> OrganiseOthers<T>(this IEnumerable<T> items)
        {
            // Create item list
            IEnumerable<string> paths = items.Select(x => x.ToString());
            List<Tuple<string, T>> list = paths.Zip(items, (k, v) => new Tuple<string, T>(k, v)).ToList();

            //Create ietm tree
            Tree<T> tree = Engine.DataStructure.Create.Tree(items, paths.Select(x => x.Split(new char[] { '.', '/', '\\' })), "select an item");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;

            return new Output<List<Tuple<string, T>>, Tree<T>> { Item1 = list, Item2 = tree };
        }

        /*************************************/
    }
}
