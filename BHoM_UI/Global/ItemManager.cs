/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.Engine.Reflection;
using BH.Engine.UI;
using BH.oM.Base;
using BH.oM.UI;
using BH.UI.Base.Components;
using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BH.UI.Base.Global
{
    public static class ItemManager
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static bool ResolveSearchItem(SearchItem searchItem)
        {
            if (searchItem == null || string.IsNullOrEmpty(searchItem.ClassFullName))
                return false;

            if (searchItem.Item != null)
                return true;

            Assembly assembly = null;
            if (m_LoadedAssemblies.ContainsKey(searchItem.Text))
            {
                assembly = m_LoadedAssemblies[searchItem.Text];
            }
            else
            {
                assembly = BH.Engine.Base.Compute.LoadAssembly(Path.Combine(BH.Engine.Base.Query.BHoMFolder(), searchItem.AssemblyName + ".dll"));
                
                if (assembly != null)
                    m_LoadedAssemblies[searchItem.Text] = assembly;
            }

            if (assembly == null)
                return false;

            Type type = assembly.GetType(searchItem.ClassFullName);

            switch (searchItem.ItemType)
            {
                case "Type":
                    searchItem.Item = type;
                    break;
                case "MethodInfo":
                    searchItem.Item = GetMethod(type, searchItem.ItemName, searchItem.Text);
                    break;
                case "ConstructorInfo":
                    searchItem.Item = GetConstructor(type, searchItem.Text);
                    break;
                default:
                    break;
            }

            return true;
        }

        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static MethodInfo GetMethod(Type type, string methodName, string signature)
        {
            List<MethodInfo> matches = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(x => x.Name == methodName)
                .ToList();

            if (matches.Count == 0)
                return null;

            if (matches.Count == 1) 
                return matches.First();

            string[] parts = signature.Split(new char[] { '(', ')' });
            if (parts.Length < 2)
                return matches.First();

            List<string> arguments = new List<string>();
            
            foreach (string arg in parts[1].Split(new char[] { ',' }))
            {
                string[] split = arg.Split(new char[] { ' ' });
                if (split.Length == 2)
                {
                    arguments.Add(split[1]);
                }   
                else if (split.Length == 4)
                {
                    // Try to find method based on number of arguments
                    int count = 0;
                    int.TryParse(split[1], out count);
                    if (count == 0)
                    {
                        // Something is wrong, just return the first math
                        return matches.First();
                    }
                    else
                    {
                        count += arguments.Count;
                        List<MethodInfo> matches2 = matches.Where(x => x.GetParameters().Count() == count).ToList();
                        if (matches2.Count > 0)
                            return matches2.First();
                        else
                            return matches.First();
                    }
                }
                else
                {
                    // Something is wrong, just return the first math
                    return matches.First();
                }
            }

            // Try returning the method that matches all parameter names
            foreach (MethodInfo method in matches)
            {
                List<string> parameters = method.GetParameters().Select(x => x.Name).ToList();
                if (parameters.Count == arguments.Count)
                {
                    if (parameters.Zip(arguments, (a, b) => a == b).All(x => x))
                        return method;
                }
            }

            // Try returning method that matches the number of parameters
            List<MethodInfo> matches3 = matches.Where(x => x.GetParameters().Count() == arguments.Count).ToList();
            if (matches3.Count > 0)
                return matches3.First();
            else
                return matches.First();
        }

        /*************************************/

        private static ConstructorInfo GetConstructor(Type type, string signature)
        {
            List<ConstructorInfo> matches = type.GetConstructors(BindingFlags.Public).ToList();

            if (matches.Count == 0)
                return null;

            if (matches.Count == 1)
                return matches.First();

            string[] parts = signature.Split(new char[] { '(', ')' });
            if (parts.Length < 2)
                return matches.First();

            List<string> arguments = new List<string>();

            foreach (string arg in parts[1].Split(new char[] { ',' }))
            {
                string[] split = arg.Split(new char[] { ' ' });
                if (split.Length == 2)
                {
                    arguments.Add(split[1]);
                }
                else if (split.Length == 4)
                {
                    // Try to find method based on number of arguments
                    int count = 0;
                    int.TryParse(split[1], out count);
                    if (count == 0)
                    {
                        // Something is wrong, just return the first math
                        return matches.First();
                    }
                    else
                    {
                        count += arguments.Count;
                        List<ConstructorInfo> matches2 = matches.Where(x => x.GetParameters().Count() == count).ToList();
                        if (matches2.Count > 0)
                            return matches2.First();
                        else
                            return matches.First();
                    }
                }
                else
                {
                    // Something is wrong, just return the first math
                    return matches.First();
                }
            }

            // Try returning the method that matches all parameter names
            foreach (ConstructorInfo method in matches)
            {
                List<string> parameters = method.GetParameters().Select(x => x.Name).ToList();
                if (parameters.Count == arguments.Count)
                {
                    if (parameters.Zip(arguments, (a, b) => a == b).All(x => x))
                        return method;
                }
            }

            // Try returning method that matches the number of parameters
            List<ConstructorInfo> matches3 = matches.Where(x => x.GetParameters().Count() == arguments.Count).ToList();
            if (matches3.Count > 0)
                return matches3.First();
            else
                return matches.First();
        }

        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static Dictionary<string, Assembly> m_LoadedAssemblies = new Dictionary<string, Assembly>();

        private static Dictionary<string, object> m_LoadedItems = new Dictionary<string, object>();

        /*************************************/
    }

}






