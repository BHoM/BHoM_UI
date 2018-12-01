using BH.Engine.Reflection;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static List<SearchItem> Hits(this List<SearchItem> items, string search, int nbMax = 20)
        {
            string text = search.Trim().ToLower();
            string[] parts = text.Split(' ');

            List<SearchItem> hits = new List<SearchItem>();
            if (text.Length > 0)
                hits = items.Select(x => new Tuple<SearchItem, double>(x, x.Weight(parts)))
                                    .Where(x => x.Item2 > 0)
                                    .OrderByDescending(x => x.Item2)
                                    .Take(nbMax)
                                    .Select(x => x.Item1)
                                    .ToList();
            return hits;
        }

        /*************************************/
    }
}
