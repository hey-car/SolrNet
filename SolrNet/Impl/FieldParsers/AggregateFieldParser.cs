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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SolrNet.Utils;

namespace SolrNet.Impl.FieldParsers {
    /// <summary>
    /// Aggregates <see cref="ISolrFieldParser"/>s
    /// </summary>
    public class AggregateFieldParser : ISolrFieldParser {
        private readonly IEnumerable<ISolrFieldParser> parsers;
        private readonly ConcurrentDictionary<Tuple<Type, string>, ISolrFieldParser> cachedParserCache;
        private readonly ConcurrentDictionary<Type, bool> cachedHandleableTypes;
        private readonly ConcurrentDictionary<string, bool> cachedHandleableSolrTypes;

        /// <summary>
        /// Aggregates <see cref="ISolrFieldParser"/>s
        /// </summary>
        /// <param name="parsers"></param>
        public AggregateFieldParser(IEnumerable<ISolrFieldParser> parsers) {
            this.parsers = parsers;

            this.cachedParserCache = new ConcurrentDictionary<Tuple<Type, string>, ISolrFieldParser>();
            this.cachedHandleableTypes = new ConcurrentDictionary<Type, bool>();
            this.cachedHandleableSolrTypes = new ConcurrentDictionary<string, bool>();
        }

        /// <inheritdoc />
        public bool CanHandleSolrType(string solrType)
        {
            if (cachedHandleableSolrTypes.ContainsKey(solrType))
                return true;

            if (parsers.Any(p => p.CanHandleSolrType(solrType)))
            {
                cachedHandleableSolrTypes[solrType] = true;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool CanHandleType(Type t)
        {
            if (cachedHandleableTypes.ContainsKey(t))
                return true;

            if (parsers.Any(p => p.CanHandleType(t)))
            {
                cachedHandleableTypes[t] = true;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public object Parse(XElement field, Type t) {
            var parser = GetParser(t, field.Name.LocalName);
            if (parser == null)
                return null;

            return parser.Parse(field, t);
        }

        private ISolrFieldParser GetParser(Type t, string solrType)
        {
            var key = Tuple.Create(t, solrType);
            ISolrFieldParser result;
            if (!cachedParserCache.TryGetValue(key, out result))
            {
                result = parsers.FirstOrDefault(p => p.CanHandleType(t) && p.CanHandleSolrType(solrType));
                cachedParserCache[key] = result;
            }

            return result;
        }
    }
}
