/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BH.Engine.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.oM.Reflection.Attributes;
using System.Diagnostics;

namespace BHoM_UI
{
    partial class Program
    {
        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static void CopyUpgrades(string sourceFolder, string targetFolder)
        {
            BsonDocument content = CollectUpgrades(sourceFolder);
            CreateUpgradeFile(targetFolder, content);
        }


        /***************************************************/
        /**** Helper Methods                            ****/
        /***************************************************/

        private static BsonDocument CollectUpgrades(string sourceFolder)
        {
            // Get name of versioning file for current version of the BHoM
            string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            version = version.Split('.').Take(2).Aggregate((a, b) => a + b);
            string versionFileName = "Versioning_" + version + ".json";

            // Create intial dictionaries
            BsonDocument versioning = CreateEmptyUpgradeDocument();

            // Collect Versioning files
            List<string> versioningFiles = GetVersionFiles(sourceFolder, versionFileName);
            foreach (string file in versioningFiles)
                ReadVersioningFile(file, versioning);

            // Get versioning from attributes
            Compute.LoadAllAssemblies(Query.BHoMFolder());
            CollectMethodVersioning(versioning["Method"] as BsonDocument);

            // return upgrade document
            return versioning;
        }

        /***************************************************/

        private static BsonDocument CreateEmptyUpgradeDocument()
        {
            Dictionary<string, object> namespaceUpgrades = new Dictionary<string, object> {
                { "ToNew", new Dictionary<string, object>() },
                { "ToOld", new Dictionary<string, object>() }
            };
            Dictionary<string, object> typeUpgrades = new Dictionary<string, object> {
                { "ToNew", new Dictionary<string, object>() },
                { "ToOld", new Dictionary<string, object>() }
            };
            Dictionary<string, object> methodUpgrades = new Dictionary<string, object> {
                { "ToNew", new Dictionary<string, object>() },
                { "ToOld", new Dictionary<string, object>() }
            };
            Dictionary<string, object> propertyUpgrades = new Dictionary<string, object> {
                { "ToNew", new Dictionary<string, object>() },
                { "ToOld", new Dictionary<string, object>() }
            };

            return new BsonDocument(new Dictionary<string, object>
            {
                { "Namespace", namespaceUpgrades },
                { "Type", typeUpgrades },
                { "Method", methodUpgrades },
                { "Property", propertyUpgrades }
            });
        }

        /***************************************************/

        private static List<string> GetVersionFiles(string sourceFolder, string versionFileName)
        {
            return Directory.GetDirectories(sourceFolder)
                .SelectMany(x => Directory.GetDirectories(x))
                .SelectMany(x => Directory.GetFiles(x, versionFileName))
                .ToList();
        }

        /***************************************************/

        private static bool ReadVersioningFile(string file, BsonDocument versioning)
        {
            string json = File.ReadAllText(file);
            BsonDocument upgrades = null;
            if (!BsonDocument.TryParse(json, out upgrades))
            {
                Console.WriteLine("Failed to load the versioning file : " + file);
                return false;
            }

            CopyDocumentAccross(upgrades, versioning);
            Console.WriteLine("Read the versioning file : " + file);

            return true;
        }

        /***************************************************/

        private static void CopyDocumentAccross(BsonDocument source, BsonDocument target)
        {
            if (source.Contains("Namespace"))
                CopySectionAccross(source["Namespace"] as BsonDocument, target["Namespace"] as BsonDocument);

            if (source.Contains("Type"))
                CopySectionAccross(source["Type"] as BsonDocument, target["Type"] as BsonDocument);

            if (source.Contains("Method"))
                CopySectionAccross(source["Method"] as BsonDocument, target["Method"] as BsonDocument);

            if (source.Contains("Property"))
                CopySectionAccross(source["Property"] as BsonDocument, target["Property"] as BsonDocument);
        }

        /***************************************************/

        private static void CopySectionAccross(BsonDocument source, BsonDocument target)
        {
            if (source.Contains("ToNew"))
            {
                BsonDocument toNewSource = source["ToNew"] as BsonDocument;
                BsonDocument toNewTarget = target["ToNew"] as BsonDocument;
                foreach (BsonElement element in toNewSource.Elements)
                {
                    if (!toNewTarget.Contains(element.Name))
                        toNewTarget.Add(element);
                }   
            }

            if (source.Contains("ToOld"))
            {
                BsonDocument toOldSource = source["ToOld"] as BsonDocument;
                BsonDocument toOldTarget = target["ToOld"] as BsonDocument;
                foreach (BsonElement element in toOldSource.Elements)
                {
                    if (!toOldTarget.Contains(element.Name))
                        toOldTarget.Add(element);
                }
            }
        }

        /***************************************************/

        private static void CreateUpgradeFile(string targetFolder, BsonDocument content)
        {
            // Get target folder
            string folder = Path.Combine(targetFolder, "bin");
            if (!Directory.Exists(folder))
                return;

            IEnumerable<string> upgraderFolders = Directory.GetDirectories(folder).Where(x => x.Contains(@"BHoMUpgrader"));
            if (upgraderFolders.Count() == 0)
                return;

            string upgraderFolder = upgraderFolders.OrderBy(x => x).Last();

            // Save the content
            string upgraderFile = Path.Combine(upgraderFolder, "Upgrades.json");
            string json = FormatJson(content.ToJson());
            File.WriteAllText(upgraderFile, json);

            Console.WriteLine("Adding versioning file to " + upgraderFolder);
        }

        /***************************************************/

        private static void CollectMethodVersioning(BsonDocument content)
        {
            // Get the current version of the BHoM
            string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            version = version.Split('.').Take(2).Aggregate((a, b) => a + '.' + b);

            // Get PreviousVersionAttributes for the current BHoM version
            BsonDocument toNewContent = content["ToNew"] as BsonDocument;
            foreach (MethodBase method in Query.AllMethodList())
            {
                PreviousVersionAttribute attribute = method.GetCustomAttribute<PreviousVersionAttribute>();
                if (attribute != null && attribute.FromVersion == version)
                    toNewContent.Add(new BsonElement(attribute.PreviousVersionAsText, BH.Engine.Serialiser.Convert.ToBson(method)));
            }
        }

        /***************************************************/

        private static string FormatJson(string json)
        {
            const string TAB = "  ";
            int indentation = 0;
            int quoteCount = 0;
            char previous = ' ';
            IEnumerable<string> result = json.Select(x =>
            {
                int quotes = (x == '"' && previous != '\\') ? quoteCount++ : quoteCount;
                previous = x;
                if (quotes % 2 == 1)
                    return x.ToString();

                switch (x)
                {
                    case ',':
                        return x + Environment.NewLine + String.Concat(Enumerable.Repeat(TAB, indentation));
                    case ' ':
                        return "";
                    case ':':
                        return ": ";
                    case '{':
                    case '[':
                        return x + Environment.NewLine + String.Concat(Enumerable.Repeat(TAB, ++indentation));
                    case '}':
                    case ']':
                        return Environment.NewLine + String.Concat(Enumerable.Repeat(TAB, --indentation)) + x;
                    default:
                        return x.ToString();
                }
            });

            return String.Concat(result);
        }

        /***************************************************/
    }
}
