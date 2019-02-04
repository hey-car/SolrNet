using System.Collections.Generic;
using System.Linq;
using SolrNet.Utils;

namespace SolrNet.Impl.FacetQuerySerializers {
    /// <summary>
    /// Serializes <see cref="SolrFacetPivotQuery"/>
    /// </summary>
    public class SolrFacetPivotQuerySerializer : SingleTypeFacetQuerySerializer<SolrFacetPivotQuery> {
        /// <summary>
        /// Serializes <see cref="SolrFacetPivotQuery"/>
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public override IEnumerable<KeyValuePair<string, string>> Serialize(SolrFacetPivotQuery q) {
            foreach (var pivotQ in q.Fields)
                yield return KV.Create("facet.pivot", string.Join(",", pivotQ.ToArray()));
            if (q.MinCount.HasValue)
                yield return KV.Create("facet.pivot.mincount", q.MinCount.ToString());

            //foreach (var pivotQ in q.Fields)
            //{
            //    string name = "pivot." + string.Join(",", pivotQ.ToArray());
            //    string value = "{" + string.Join(",facet:{",
            //        pivotQ.ToArray().Select(f => string.Format("'{0}':{{type:'terms',limit:-1,field:'{1}'", name, f))
            //        ) + new string('}', pivotQ.Count() * 2);

            //    yield return KV.Create("json.facet", value);
            //}
        }
    }
}
