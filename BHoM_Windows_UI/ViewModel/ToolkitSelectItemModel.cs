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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BH.oM.UI;

namespace BH.UI.Base.Windows.ViewModel
{
    internal class ToolkitSelectItemModel : INotifyPropertyChanged
    {
        public ToolkitSelectItemModel(ToolkitSelectItem item)
        {
            Toolkit = item.Toolkit;
            Include = item.Include;
        }

        public ToolkitSelectItem ToItem()
        {
            return new ToolkitSelectItem()
            {
                Toolkit = this.Toolkit,
                Include = this.Include,
            };
        }

        private bool m_Include;

        public virtual string Toolkit { get; set; }
        public virtual bool Include
        { 
            get
            {
                return m_Include;
            }
            set
            {
                m_Include = value;
                NotifyPropertyChanged();
            }    
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

