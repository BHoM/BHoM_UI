using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Templates
{
    public abstract class DataAccessor
    {
        /*************************************/
        /**** Input Getter Methods        ****/
        /*************************************/

        public abstract T GetDataItem<T>(int index);

        /*************************************/

        public abstract List<T> GetDataList<T>(int index);

        /*************************************/

        public abstract List<List<T>> GetDataTree<T>(int index);


        /*************************************/
        /**** Output Setter Methods       ****/
        /*************************************/

        public abstract bool SetDataItem<T>(int index, T data);

        /*************************************/

        public abstract bool SetDataList<T>(int index, IEnumerable<T> data);

        /*************************************/

        public abstract bool SetDataTree<T>(int index, IEnumerable<IEnumerable<T>> data);

        /*************************************/
    }
}
