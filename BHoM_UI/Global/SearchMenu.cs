using BH.Adapter;
using BH.Engine.Reflection;
using BH.Engine.Reflection.Convert;
using BH.oM.UI;
using BH.UI.Components;
using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Global
{
    public abstract class SearchMenu
    {
        /*************************************/
        /**** Events                      ****/
        /*************************************/

        public event EventHandler<ComponentRequest> ItemSelected;


        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public List<SearchItem> PossibleItems { get; set; }


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public SearchMenu()
        {
            PossibleItems = GetAllPossibleItems();
        }

        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public abstract bool SetParent(object parent);


        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected void NotifySelection(SearchItem item)
        {
            ItemSelected?.Invoke(this, new ComponentRequest { CallerType = item.CallerType, SelectedItem = item.Item });
        }

        /*************************************/

        protected List<SearchItem> GetHits(string search)
        {
            string text = search.Trim().ToLower();
            string[] parts = text.Split(' ');

            List<SearchItem> hits = new List<SearchItem>();
            if (text.Length > 0)
                hits = PossibleItems.Select(x => new Tuple<SearchItem, double>(x, GetWeight(x, parts)))
                                    .Where(x => x.Item2 > 0)
                                    .OrderByDescending(x => x.Item2)
                                    .Take(20)
                                    .Select(x => x.Item1)
                                    .ToList();
            return hits;
        }

        /*************************************/

        protected virtual List<SearchItem> GetAllPossibleItems()
        {
            // All methods defined from the BHoM_UI
            // Reflection is pretty slow on this one so better to just do it manually even if less elegant
            List<SearchItem> items = new List<SearchItem>
            {
                new SearchItem { Item = typeof(DeleteCaller).GetMethod("Delete"), CallerType = typeof(DeleteCaller), Icon = Properties.Resources.Delete },
                new SearchItem { Item = typeof(ExecuteCaller).GetMethod("Execute"), CallerType = typeof(ExecuteCaller), Icon = Properties.Resources.Execute },
                new SearchItem { Item = typeof(MoveCaller).GetMethod("Move"), CallerType = typeof(MoveCaller), Icon = Properties.Resources.Move },
                new SearchItem { Item = typeof(PullCaller).GetMethod("Pull"), CallerType = typeof(PullCaller), Icon = Properties.Resources.Pull },
                new SearchItem { Item = typeof(PushCaller).GetMethod("Push"), CallerType = typeof(PushCaller), Icon = Properties.Resources.Push },
                new SearchItem { Item = typeof(UpdatePropertyCaller).GetMethod("UpdateProperty"), CallerType = typeof(UpdatePropertyCaller), Icon = Properties.Resources.UpdateProperty },
                new SearchItem { Item = typeof(BH.Engine.Serialiser.Convert).GetMethod("FromJson"), CallerType = typeof(FromJsonCaller), Icon = Properties.Resources.FromJson },
                new SearchItem { Item = typeof(BH.Engine.Serialiser.Convert).GetMethod("ToJson"), CallerType = typeof(ToJsonCaller), Icon = Properties.Resources.ToJson },
                new SearchItem { Item = null, CallerType = typeof(ExplodeCaller), Icon = Properties.Resources.Explode, Text = "BH.UI.Components.ExplodeCaller.Explode" },
                new SearchItem { Item = null, CallerType = typeof(GetPropertyCaller), Icon = Properties.Resources.BHoM_GetProperty, Text = "BH.UI.Components.GetPropertyCaller.GetProperty" },
                new SearchItem { Item = null, CallerType = typeof(SetPropertyCaller), Icon = Properties.Resources.BHoM_SetProperty, Text = "BH.UI.Components.SetPropertyCaller.SetProperty" },
                new SearchItem { Item = null, CallerType = typeof(CreateCustomCaller), Icon = Properties.Resources.CustomObject, Text = "BH.UI.Components.CreateCustomCaller.CreateCustom" },
                new SearchItem { Item = typeof(CreateDictionaryCaller).GetMethod("CreateDictionary"), CallerType = typeof(CreateDictionaryCaller), Icon = Properties.Resources.Dictionary }
            };
            foreach (SearchItem item in items)
            {
                if (string.IsNullOrEmpty(item.Text) && item.Item != null)
                    item.Text = ((MethodInfo)item.Item).ToText(true);
            }
                
            // All methods for the BHoM Engine
            items.AddRange(Query.BHoMMethodList().Where(x => !x.IsNotImplemented() && !x.IsDeprecated())
                                    .Select(x => new SearchItem { Item = x, CallerType = GetCallerType(x), Icon = GetIcon(x), Text = x.ToText(true) }));

            // All adapter constructors
            items.AddRange(Query.AdapterTypeList().Where(x => x.IsSubclassOf(typeof(BHoMAdapter))).SelectMany(x => x.GetConstructors())
                                    .Select(x => new SearchItem { Item = x, CallerType = typeof(CreateAdapterCaller), Icon = Properties.Resources.Adapter, Text = x.ToText(true) }));

            // All query constructors
            Type queryType = typeof(BH.oM.DataManipulation.Queries.IQuery);
            items.AddRange(Query.BHoMMethodList().Where(x => queryType.IsAssignableFrom(x.ReturnType))
                                    .Select(x => new SearchItem { Item = x, CallerType = typeof(CreateQueryCaller), Icon = Properties.Resources.QueryAdapter, Text = x.ToText(true) }));

            // All Types
            items.AddRange(Query.BHoMTypeList()
                                    .Select(x => new SearchItem { Item = x, CallerType = typeof(CreateTypeCaller), Icon = Properties.Resources.Type, Text = x.ToText(true) }));

            // All Enums
            items.AddRange(Query.BHoMEnumList()
                                    .Select(x => new SearchItem { Item = x, CallerType = typeof(CreateEnumCaller), Icon = Properties.Resources.BHoM_Enum, Text = x.ToText(true) }));

            // All data libraries
            items.AddRange(Engine.Library.Query.LibraryNames()
                                    .Select(x => new SearchItem { Item = x, CallerType = typeof(CreateDataCaller), Icon = Properties.Resources.BHoM_Data, Text = x }));

            // Return the list
            return items;
        }

        /*************************************/

        protected double GetWeight(SearchItem item, string[] search)
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
                
                foreach( string part in search.Where(p => method.DeclaringType.Namespace.ToLower().Contains(p)))
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

        protected Bitmap GetIcon(MethodInfo item)
        {
            if (item == null)
                return Properties.Resources.Empty;

            switch (item.DeclaringType.Name)
            {
                case "Compute":
                    return Properties.Resources.Compute;
                case "Convert":
                    return Properties.Resources.Convert;
                case "Create":
                    return Properties.Resources.CreateBHoM;
                case "Modify":
                    return Properties.Resources.Modify;
                case "Query":
                    return Properties.Resources.Query;
                default:
                    return Properties.Resources.Empty;
            }
        }

        /*************************************/

        private static Type GetCallerType(MethodInfo item)
        {
            if (item.DeclaringType.Namespace.StartsWith("BH.Engine"))
            {
                switch (item.DeclaringType.Name)
                {
                    case "Compute":
                        return typeof(ComputeCaller);
                    case "Convert":
                        return typeof(ConvertCaller);
                    case "Create":
                        return typeof(CreateObjectCaller);
                    case "Modify":
                        return typeof(ModifyCaller);
                    case "Query":
                        return typeof(QueryCaller);
                    default:
                        return null;
                }
            }
            else
                return null;
        }

        /*************************************/
    }
}
