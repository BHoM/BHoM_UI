/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.Engine.Base.Objects;
using BH.Engine.UI;
using BH.oM.Base;
using BH.oM.Base.Reflection;
using BH.oM.UI;
using BH.UI.Base.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;


namespace BH.UI.Base.Global
{
    public static class Initialisation
    {
        /*************************************/
        /**** Events                      ****/
        /*************************************/

        public static event EventHandler<CustomRibbonEntry> CustomRibbonEntryLoaded;


        /*************************************/
        /**** Public Properties           ****/
        /*************************************/

        public static DateTime? CompletionTime { get; set; } = null;

        public static List<CodeElementRecord> CodeElements { get; set; } = new List<CodeElementRecord>();

        public static AssemblyResolver AssemblyResolver { get; set; } = new AssemblyResolver();

        public static List<SearchItem> SearchItems { get; set; } = new List<SearchItem>();

        public static string AssemblyContentFilePath { get; set; } = BH.Engine.Base.Objects.Initialisation.DefaultAssemblyContentFilePath;

        public static List<CustomRibbonEntry> CustomRibbonEntries { get; set; } = new List<CustomRibbonEntry>();

        public static List<string> ExcludedToolkits { get; set; } = new List<string>();


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static bool Activate()
        {
            bool success = true;

            success &= LoadCodeElements();
            success &= CreateAssemblyResolver();
            success &= LoadToolkitSettings();
            success &= LoadNewAssemblies();
            success &= CreateSearchItems(CodeElements);

            CompletionTime = DateTime.UtcNow;

            return success;
        }

        /*************************************/

        public static bool LoadToolkitSettings()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string directory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "BHoM", "Settings");
            if (!Directory.Exists(directory))
            {
                BH.Engine.Base.Compute.RecordWarning($"{directory} doesn't exist. Toolkit settings are not loaded.");
                return false;
            }

            BH.Engine.Settings.Compute.LoadSettings(directory);
            BH.Engine.Settings.Compute.LoadSettings(directory, "*.cfg"); //Legacy cfg files to be loaded in

            bool success = true;
            List<ISettings> allSettings = BH.Engine.Settings.Query.GetAllSettings();

            success &= LoadInitialisationSettings(allSettings);
            success &= LoadCustomRibbons(allSettings);
            success &= LoadSearchSettings(allSettings);

            stopwatch.Stop();
            BH.Engine.Base.Compute.RecordNote($"Time to load toolkit settings: {stopwatch.Elapsed.TotalMilliseconds / 1000} s");

            return success;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static bool LoadInitialisationSettings(List<ISettings> allSettings)
        {
            bool success = true;

            List<IInitialisationSettings> initialisationSettings = allSettings.OfType<IInitialisationSettings>().ToList();
            foreach (IInitialisationSettings settings in initialisationSettings)
            {
                try
                {
                    success &= InitialiseToolkit(settings);
                }
                catch (Exception e)
                {
                    BH.Engine.Base.Compute.RecordWarning(e, $"Failed to load settings of type {settings.GetType().Name}.");
                    success = false;
                }
            }

            return success;
        }

        /*************************************/

        private static bool LoadCustomRibbons(List<ISettings> allSettings)
        {
            bool success = true;

            List<CustomRibbonSettings> ribbonSettings = allSettings.OfType<CustomRibbonSettings>().ToList();
            foreach (CustomRibbonSettings settings in ribbonSettings)
            {
                foreach (CustomRibbonEntry entry in settings.Entries)
                {
                    try
                    {
                        CustomRibbonEntryLoaded?.Invoke(null, entry);
                        CustomRibbonEntries.Add(entry);
                    }
                    catch (Exception e)
                    {
                        BH.Engine.Base.Compute.RecordWarning(e, $"Failed to load custom entry for ribbon. Tab name: {entry.TabName}, Category: {entry.Category}, json: {entry.ItemJson}.");
                        success = false;
                    }
                }
            }

            return success;
        }

        /*************************************/

        private static bool LoadSearchSettings(List<ISettings> allSettings)
        {
            SearchSettings searchSettings = allSettings.OfType<SearchSettings>().FirstOrDefault();
            if (searchSettings?.ExcludedToolkits != null)
                ExcludedToolkits = searchSettings.ExcludedToolkits;

            return true;
        }

        /*************************************/

        private static bool InitialiseToolkit(IInitialisationSettings settings)
        {
            // Get details about intialisation method to run
            int separatorIndex = settings.InitialisationMethod.LastIndexOf('.');
            string typeName = settings.InitialisationMethod.Substring(0, separatorIndex);
            string methodName = settings.InitialisationMethod.Substring(separatorIndex + 1);

            // Make sure the assembly is loaded for that method
            if (!string.IsNullOrEmpty(settings.InitialisationAssembly) && !BH.Engine.Base.Query.IsAssemblyLoaded(settings.InitialisationAssembly))
            {
                string initAssemblyPath = BH.Engine.Base.Objects.Initialisation.AssemblyFilePath(settings.InitialisationAssembly);
                BH.Engine.Base.Compute.LoadAssembly(initAssemblyPath);
            }

            // Get method declaring type
            List<Type> typeCandidates = Engine.Base.Create.AllTypes(typeName).Where(x => x.FullName == typeName).ToList();
            if (typeCandidates.Count == 0)
            {
                Engine.Base.Compute.RecordWarning("Type " + typeName + " is unknown");
                return false;
            }
            Type type = typeCandidates.First();

            // Get the method itself
            MethodInfo method = Engine.Reflection.Create.MethodInfo(type, methodName, new List<Type>());
            if (method == null)
            {
                Engine.Base.Compute.RecordWarning("A static method with no argument could not be found for " + settings.InitialisationMethod);
                return false;
            }

            // Calling the method
            try
            {
                method.Invoke(null, null);
                return true;
            }
            catch (Exception e)
            {
                Engine.Base.Compute.RecordWarning("Method " + settings.InitialisationMethod + " failed to run properly during toolkit initialisation. Error: \n" + e.Message);
                return false;
            }
        }

        /*************************************/

        private static bool LoadCodeElements()
        {
            if (!File.Exists(AssemblyContentFilePath))
                return true;

            List<CodeElementRecord> loaded = BH.Engine.Base.Objects.Initialisation.LoadCodeElements(AssemblyContentFilePath, x => x.FromTsv());
            if (loaded == null)
                return false;

            CodeElements = loaded;
            return true;
        }

        /*************************************/

        private static bool CreateAssemblyResolver()
        {
            AssemblyResolver = BH.Engine.Base.Objects.Initialisation.CreateAssemblyResolver(CodeElements);
            BH.Engine.Base.Compute.SetAssemblyResolver(AssemblyResolver);
            return true;
        }

        /*************************************/

        private static bool LoadNewAssemblies()
        {
            CodeElements = BH.Engine.Base.Objects.Initialisation.RefreshFromNewAssemblies(
                CodeElements,
                BH.Engine.Base.Objects.Initialisation.DefaultAssemblyNameFilter,
                AssemblyContentFilePath,
                x => x.ToTsv(),
                names => BH.Engine.Reflection.Query.CodeElements(names));

            return true;
        }

        /*************************************/

        private static bool CreateSearchItems(List<CodeElementRecord> codeElements)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // All methods defined from the BHoM_UI
            SearchItems = GetComponentItems();

            // All code elements
            SearchItems.AddRange(codeElements
                .Select(x => new SearchItem { CallerType = GetCallerType(x), Icon = GetIcon(x), Text = x.DisplayText, InputKeys = x.InputKeys, OutputKeys = x.OutputKeys }));

            // All data libraries
            SearchItems.AddRange(BH.Engine.UI.Query.LibraryItems()
                .Select(x => new SearchItem { CallerType = typeof(CreateDataCaller), Icon = Properties.Resources.BHoM_Data, Text = x.Replace(Path.DirectorySeparatorChar, '.'), Item = x }));

            // All system types
            SearchItems.AddRange(BH.Engine.Reflection.Query.SystemTypes()
                .Select(x => new SearchItem { CallerType = typeof(CreateTypeCaller), Icon = Properties.Resources.Type, Text = x.ToText(true), Item = x }));

            // Filter out excluded toolkits
            if (ExcludedToolkits?.Count > 0)
                SearchItems = SearchItems.Where(x => !ExcludedToolkits.Contains(x.Toolkit())).ToList();

            stopwatch.Stop();
            BH.Engine.Base.Compute.RecordNote($"Time to create all items for the menu: {stopwatch.Elapsed.TotalMilliseconds / 1000} s.");

            return true;
        }

        /*************************************/

        private static Type GetCallerType(CodeElementRecord codeElement)
        {
            switch (codeElement.Type)
            {
                case CodeElementType.Constructor:
                    if (codeElement.IsAdapterConstructor())
                        return typeof(CreateAdapterCaller);
                    if (codeElement.IsRequestConstructor())
                        return typeof(CreateRequestCaller);
                    else
                        return typeof(CreateObjectCaller);
                case CodeElementType.Enum:
                    return typeof(CreateEnumCaller);
                case CodeElementType.Method_Create:
                    if (codeElement.IsRequestCreator())
                        return typeof(CreateRequestCaller);
                    else
                        return typeof(CreateObjectCaller);
                case CodeElementType.Method_Compute:
                    return typeof(ComputeCaller);
                case CodeElementType.Method_Convert:
                    return typeof(ConvertCaller);
                case CodeElementType.Method_External:
                    return typeof(ExternalCaller);
                case CodeElementType.Method_Modify:
                    return typeof(ModifyCaller);
                case CodeElementType.Method_Query:
                    return typeof(QueryCaller);
                case CodeElementType.Type:
                    return typeof(CreateTypeCaller);
                default:
                    return null;
            }
        }

        /*************************************/

        private static bool IsAdapterConstructor(this CodeElementRecord codeElement)
        {
            return codeElement.Type == CodeElementType.Constructor && codeElement.OutputKeys.Contains("BH.oM.Adapter.IBHoMAdapter");
        }

        /*************************************/

        private static bool IsRequestConstructor(this CodeElementRecord codeElement)
        {
            return codeElement.Type == CodeElementType.Constructor && codeElement.OutputKeys.Contains("BH.oM.Data.Requests.IRequest");
        }

        /*************************************/

        private static bool IsRequestCreator(this CodeElementRecord codeElement)
        {
            return codeElement.Type == CodeElementType.Method_Create && codeElement.OutputKeys.Contains("BH.oM.Data.Requests.IRequest");
        }

        /*************************************/

        private static Bitmap GetIcon(CodeElementRecord codeElement)
        {
            switch (codeElement.Type)
            {
                case CodeElementType.Constructor:
                    if (codeElement.IsAdapterConstructor())
                        return Properties.Resources.Adapter;
                    if (codeElement.IsRequestConstructor())
                        return Properties.Resources.CreateRequest;
                    else
                        return Properties.Resources.CreateBHoM;
                case CodeElementType.Enum:
                    return Properties.Resources.BHoM_Enum;
                case CodeElementType.Method_Create:
                    if (codeElement.IsRequestCreator())
                        return Properties.Resources.CreateRequest;
                    else
                        return Properties.Resources.CreateBHoM;
                case CodeElementType.Method_Compute:
                    return Properties.Resources.Compute;
                case CodeElementType.Method_Convert:
                    return Properties.Resources.Convert;
                case CodeElementType.Method_External:
                    return Properties.Resources.External;
                case CodeElementType.Method_Modify:
                    return Properties.Resources.Modify;
                case CodeElementType.Method_Query:
                    return Properties.Resources.Query;
                case CodeElementType.Type:
                    return Properties.Resources.Type;
                default:
                    return null;
            }
        }

        /*************************************/

        private static List<SearchItem> GetComponentItems()
        {
            // Reflection is pretty slow on this one so better to just do it manually even if less elegant
            List<SearchItem> items = new List<SearchItem>
            {
                new SearchItem {
                    Item = typeof(RemoveCaller).GetMethod("Remove"),
                    CallerType = typeof(RemoveCaller),
                    Icon = Properties.Resources.Delete,
                    Text = "BH.Adapter.Remove"
                },
                new SearchItem {
                    Item = typeof(ExecuteCaller).GetMethod("Execute"),
                    CallerType = typeof(ExecuteCaller),
                    Icon = Properties.Resources.Execute,
                    Text = "BH.Adapter.Execute"
                },
                new SearchItem {
                    Item = typeof(MoveCaller).GetMethod("Move"),
                    CallerType = typeof(MoveCaller),
                    Icon = Properties.Resources.Move,
                    Text = "BH.Adapter.Move"
                },
                new SearchItem {
                    Item = typeof(PullCaller).GetMethod("Pull"),
                    CallerType = typeof(PullCaller),
                    Icon = Properties.Resources.Pull,
                    Text = "BH.Adapter.Pull"
                },
                new SearchItem {
                    Item = typeof(PushCaller).GetMethod("Push"),
                    CallerType = typeof(PushCaller),
                    Icon = Properties.Resources.Push,
                    Text = "BH.Adapter.Push"
                },
                new SearchItem {
                    Item = typeof(BH.Engine.Serialiser.Convert).GetMethod("FromJson"),
                    CallerType = typeof(FromJsonCaller),
                    Icon = Properties.Resources.FromJson,
                    Text = "BH.Engine.FromJson"
                },
                new SearchItem {
                    Item = typeof(BH.Engine.Serialiser.Convert).GetMethod("ToJson"),
                    CallerType = typeof(ToJsonCaller),
                    Icon = Properties.Resources.ToJson,
                    Text = "BH.Engine.ToJson"
                },
                new SearchItem {
                    Item = null,
                    CallerType = typeof(ExplodeCaller),
                    Icon = Properties.Resources.Explode,
                    Text = "BH.Engine.Explode"
                },
                new SearchItem {
                    Item = null,
                    CallerType = typeof(GetPropertyCaller),
                    Icon = Properties.Resources.BHoM_GetProperty,
                    Text = "BH.Engine.GetProperty"
                },
                new SearchItem {
                    Item = null,
                    CallerType = typeof(SetPropertyCaller),
                    Icon = Properties.Resources.BHoM_SetProperty,
                    Text = "BH.Engine.SetProperty"
                },
                new SearchItem {
                    Item = null,
                    CallerType = typeof(GetInfoCaller),
                    Icon = Properties.Resources.GetInfo,
                    Text = "BH.Engine.GetInfo"
                },
                new SearchItem {
                    Item = null,
                    CallerType = typeof(GetEventsCaller),
                    Icon = Properties.Resources.GetEvents,
                    Text = "BH.Engine.GetEvents"
                },
                new SearchItem {
                    Item = null,
                    CallerType = typeof(CreateCustomCaller),
                    Icon = Properties.Resources.CustomObject,
                    Text = "BH.oM.CreateCustom"
                },
                new SearchItem {
                    Item = typeof(CreateDictionaryCaller).GetMethod("CreateDictionary"),
                    CallerType = typeof(CreateDictionaryCaller),
                    Icon = Properties.Resources.Dictionary,
                    Text = "BH.oM.CreateDictionary"
                }
            };

            foreach (SearchItem item in items.Where(x => x.Item is MethodBase))
                item.InputKeys = ((MethodBase)item.Item).GetParameters()
                .Select(x => x.ParameterType?.ToText(true))
                .ToList();

            return items;
        }

        /*************************************/
    }

}







