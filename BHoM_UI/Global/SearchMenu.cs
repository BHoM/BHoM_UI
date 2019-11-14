/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.Engine.Reflection;
using BH.Engine.UI;
using BH.oM.UI;
using BH.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

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

        public List<SearchItem> PossibleItems { get; set; } = new List<SearchItem>();


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
            NotifySelection(item, null);
        }

        /*************************************/

        protected void NotifySelection(SearchItem item, BH.oM.Geometry.Point location)
        {
            ItemSelected?.Invoke(this, new ComponentRequest { CallerType = item.CallerType, SelectedItem = item.Item, Location = location });
        }

        /*************************************/

        protected virtual List<SearchItem> GetAllPossibleItems()
        {
            // All methods defined from the BHoM_UI
            // Reflection is pretty slow on this one so better to just do it manually even if less elegant
            List<SearchItem> items = new List<SearchItem>
            {
                new SearchItem { Item = typeof(RemoveCaller).GetMethod("Delete"), CallerType = typeof(RemoveCaller), Icon = Properties.Resources.Delete },
                new SearchItem { Item = typeof(ExecuteCaller).GetMethod("Execute"), CallerType = typeof(ExecuteCaller), Icon = Properties.Resources.Execute },
                new SearchItem { Item = typeof(MoveCaller).GetMethod("Move"), CallerType = typeof(MoveCaller), Icon = Properties.Resources.Move },
                new SearchItem { Item = typeof(PullCaller).GetMethod("Pull"), CallerType = typeof(PullCaller), Icon = Properties.Resources.Pull },
                new SearchItem { Item = typeof(PushCaller).GetMethod("Push"), CallerType = typeof(PushCaller), Icon = Properties.Resources.Push },
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

            // All constructors for the BHoM objects
            items.AddRange(BH.Engine.UI.Query.ConstructableTypeItems()
                .Select(x => new SearchItem { Item = x, CallerType = typeof(CreateObjectCaller), Icon = Properties.Resources.CreateBHoM, Text = x.ConstructorText() }));

            // All methods for the BHoM Engine
            items.AddRange(BH.Engine.UI.Query.EngineItems()
                        .Select(x => new SearchItem { Item = x, CallerType = GetCallerType(x), Icon = GetIcon(x), Text = x.ToText(true) }));

            // All methods from External class
            items.AddRange(BH.Engine.UI.Query.ExternalItems()
                .Select(x => new SearchItem { Item = x, CallerType = typeof(ExternalCaller), Icon = Properties.Resources.External, Text = x.ToText(true) }));

            // All adapter constructors
            items.AddRange(BH.Engine.UI.Query.AdapterConstructorItems()
                .Select(x => new SearchItem { Item = x, CallerType = typeof(CreateAdapterCaller), Icon = Properties.Resources.Adapter, Text = x.ToText(true) }));

            // All Types
            items.AddRange(BH.Engine.UI.Query.TypeItems()
                .Select(x => new SearchItem { Item = x, CallerType = typeof(CreateTypeCaller), Icon = Properties.Resources.Type, Text = x.ToText(true) }));

            // All Enums
            items.AddRange(BH.Engine.UI.Query.EnumItems()
                .Select(x => new SearchItem { Item = x, CallerType = typeof(CreateEnumCaller), Icon = Properties.Resources.BHoM_Enum, Text = x.ToText(true) }));

            // All data libraries
            items.AddRange(BH.Engine.UI.Query.LibraryItems()
                .Select(x => new SearchItem { Item = x, CallerType = typeof(CreateDataCaller), Icon = Properties.Resources.BHoM_Data, Text = x }));

            // Return the list
            return items;
        }

        /*************************************/

        protected Bitmap GetIcon(MethodBase item)
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

        private static Type GetCallerType(MethodBase item)
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
