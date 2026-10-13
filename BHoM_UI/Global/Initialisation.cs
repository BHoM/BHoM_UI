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

using BH.Adapter;
using BH.Engine.Base;
using BH.Engine.Base.Objects;
using BH.Engine.Reflection;
using BH.Engine.UI;
using BH.oM.Base;
using BH.oM.UI;
using BH.UI.Base.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;


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

        public static string AssemblyContentFilePath { get; set; } = Path.Combine(BH.Engine.Base.Query.BHoMFolderResources(), "AssemblyContent.tsv");

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
            if(!Directory.Exists(directory))
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
            foreach (var settings in initialisationSettings)
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
            foreach (var settings in ribbonSettings)
            {
                foreach (var entry in settings.Entries)
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
                string initAssemblyPath = BH.Engine.UI.Query.AssemblyPath(settings.InitialisationAssembly);
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

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Load the code elements
            try
            {
                CodeElements = File.ReadAllLines(AssemblyContentFilePath)
                    .Select(x => BH.Engine.UI.Convert.CodeElementFromTsv(x))
                    .Where(x => x != null)
                    .ToList();
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError(e, $"Failed to load the code elements from '{Path.GetFileName(AssemblyContentFilePath)}'.");
                return false;
            }

            stopwatch.Stop();
            BH.Engine.Base.Compute.RecordNote($"Time to load all code elements: {stopwatch.Elapsed.TotalMilliseconds / 1000} s.");

            return true;
        }

        /*************************************/

        private static bool CreateAssemblyResolver()
        {
            // Create the assembly resolver and link it the the BHoM engine
            AssemblyResolver = new AssemblyResolver(CodeElements);
            BH.Engine.Base.Compute.SetAssemblyResolver(AssemblyResolver);

            return true;
        }

        /*************************************/

        private static bool LoadNewAssemblies()
        {
            CodeElements = BH.Engine.UI.Compute.RefreshAssemblyContent(CodeElements, null, AssemblyContentFilePath);
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
                .Select(x => new SearchItem { CallerType = GetCallerType(x.Type), Icon = GetIcon(x.Type), Text = x.DisplayText, Json = x.Json, InputKeys = x.InputKeys, OutputKeys = x.OutputKeys }));

            // All data libraries
            SearchItems.AddRange(BH.Engine.UI.Query.LibraryItems()
                .Select(x => new SearchItem { CallerType = typeof(CreateDataCaller), Icon = Properties.Resources.BHoM_Data, Text = x.Replace(Path.DirectorySeparatorChar, '.'), Item = x }));

            // All system types
            SearchItems.AddRange(BH.Engine.UI.Query.SystemTypes()
                .Select(x => new SearchItem { CallerType = typeof(CreateTypeCaller), Icon = Properties.Resources.Type, Text = x.ToText(true), Item = x }));

            // Filter out excluded toolkits
            if (ExcludedToolkits?.Count > 0) 
                SearchItems = SearchItems.Where(x => !ExcludedToolkits.Contains(x.Toolkit())).ToList();

            stopwatch.Stop();
            BH.Engine.Base.Compute.RecordNote($"Time to create all items for the menu: {stopwatch.Elapsed.TotalMilliseconds / 1000} s.");

            return true;
        }

        /*************************************/

        private static Type GetCallerType(CodeElementType codeElementType)
        {
            switch (codeElementType)
            {
                case CodeElementType.AdapterConstructor:
                    return typeof(CreateAdapterCaller);
                case CodeElementType.ConstructableObject:
                    return typeof(CreateObjectCaller);
                case CodeElementType.ConstructableRequest:
                    return typeof(CreateRequestCaller);
                case CodeElementType.Enum:
                    return typeof(CreateEnumCaller);
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
                case CodeElementType.ObjectCreator:
                    return typeof(CreateObjectCaller);
                case CodeElementType.RequestCreator:
                    return typeof(CreateRequestCaller);
                case CodeElementType.Type:
                    return typeof(CreateTypeCaller);
                default:
                    return null;
            }
        }

        /*************************************/

        private static Bitmap GetIcon(CodeElementType codeElementType)
        {
            switch (codeElementType)
            {
                case CodeElementType.AdapterConstructor:
                    return Properties.Resources.Adapter;
                case CodeElementType.ConstructableObject:
                    return Properties.Resources.CreateBHoM;
                case CodeElementType.ConstructableRequest:
                    return Properties.Resources.CreateRequest;
                case CodeElementType.Enum:
                    return Properties.Resources.BHoM_Enum;
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
                case CodeElementType.ObjectCreator:
                    return Properties.Resources.CreateBHoM;
                case CodeElementType.RequestCreator:
                    return Properties.Resources.CreateRequest;
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







