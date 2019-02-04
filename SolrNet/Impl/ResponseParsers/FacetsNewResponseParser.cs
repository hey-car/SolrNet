#region license
// Copyright (c) 2007-2010 Mauricio Scheffer
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SolrNet.Impl.FieldParsers;
using SolrNet.Utils;

namespace SolrNet.Impl.ResponseParsers {
    /// <summary>
    /// Parses facets from query response
    /// </summary>
    /// <typeparam name="T">Document type</typeparam>
    public class FacetsNewResponseParser<T> : ISolrAbstractResponseParser<T> {        

        public void Parse(XDocument xml, AbstractSolrQueryResults<T> results) {
            var mainFacetNode = xml.Element("response")
                .Elements("lst")
                .FirstOrDefault(X.AttrEq("name", "facets"));
            if (mainFacetNode != null) {
                //results.FacetQueries = ParseFacetQueries(mainFacetNode);
                //results.FacetFields = ParseFacetFields(mainFacetNode);
				results.FacetPivots = ParseFacetPivots(mainFacetNode);
            }
        }

        /// <summary>
        /// Parses facet queries results
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public IDictionary<string, int> ParseFacetQueries(XElement node) {
            var d = new Dictionary<string, int>();

            foreach (var fieldNode in node.Elements("lst"))
            {
                var name = fieldNode.Attribute("name").Value;
                if (name.StartsWith("query."))
                {
                    name = name.Substring(6);
                    d[name] = Int32.Parse(fieldNode.Elements("int").First(X.AttrEq("name", "count")).Value);
                }
            }


            var facetQueries = node.Elements("lst")
                .Where(X.AttrEq("name", "facet_queries"))
                .Elements();
            foreach (var fieldNode in facetQueries) {
                var key = fieldNode.Attribute("name").Value;
                var value = Convert.ToInt32(fieldNode.Value);
                d[key] = value;
            }
            return d;
        }

        /// <summary>
        /// Parses facet fields results
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public IDictionary<string, ICollection<KeyValuePair<string, int>>> ParseFacetFields(XElement node) {
            var d = new Dictionary<string, ICollection<KeyValuePair<string, int>>>();

            foreach (var fieldNode in node.Elements("lst")) {
                var name = fieldNode.Attribute("name").Value;
                if (name.StartsWith("field."))
                {
                    name = name.Substring(6);

                    d[name] = fieldNode.Elements("arr")
                                       .Where(X.AttrEq("name", "buckets"))
                                       .Elements("lst").Select(n =>
                                           new KeyValuePair<string,int>(
                                               n.Elements().First(X.AttrEq("name", "val")).Value,
                                               int.Parse(n.Elements("int").First(X.AttrEq("name", "count")).Value))
                                           ).ToArray();
                }                
            }
            return d;
        }

		/// <summary>
		/// Parses facet pivot results
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public IDictionary<string, IList<Pivot>> ParseFacetPivots(XElement node)
		{
			var d = new Dictionary<string, IList<Pivot>>();

            foreach (var fieldNode in node.Elements("lst")) {
                var name = fieldNode.Attribute("name").Value;
                if (name.StartsWith("pivot."))
                {
                    name = name.Substring(6);
                    
                    d[name] = fieldNode.Elements("arr")
                                       .Where(X.AttrEq("name", "buckets"))
                                       .Elements("lst").Select(ParsePivotNode).ToArray();
                }
            }
            return d;
		}

		public Pivot ParsePivotNode(XElement node)
		{
			Pivot pivot = new Pivot();

            //pivot.Field = node.Elements("str").First(X.AttrEq("name", "field")).Value;
            pivot.Value = node.Elements().First(X.AttrEq("name", "val")).Value;
            pivot.Count = int.Parse(node.Elements("int").First(X.AttrEq("name", "count")).Value);

            var childPivotNodes = node.Elements("lst").ToList();
			if (childPivotNodes.Count > 0)
			{
				pivot.HasChildPivots = true;
				pivot.ChildPivots = new List<Pivot>();

				foreach (var childNode in childPivotNodes.Elements("arr").Where(X.AttrEq("name", "buckets")).SelectMany(e => e.Elements("lst")))
				{
					pivot.ChildPivots.Add(ParsePivotNode(childNode));
				}
			}

            return pivot;
		}

    }
}