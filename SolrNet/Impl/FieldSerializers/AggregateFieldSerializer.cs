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
using SolrNet.Exceptions;

namespace SolrNet.Impl.FieldSerializers {
    /// <summary>
    /// Aggregates <see cref="ISolrFieldSerializer"/>s
    /// </summary>
    public class AggregateFieldSerializer : ISolrFieldSerializer {
        private readonly IEnumerable<ISolrFieldSerializer> serializers;
        private readonly ConcurrentDictionary<Type, ISolrFieldSerializer> cachedSerializerCache;

        /// <summary>
        /// Aggregates <see cref="ISolrFieldSerializer"/>s
        /// </summary>
        public AggregateFieldSerializer(IEnumerable<ISolrFieldSerializer> serializers) {
            this.serializers = serializers;
            this.cachedSerializerCache = new ConcurrentDictionary<Type, ISolrFieldSerializer>();
        }

        /// <inheritdoc />
        public bool CanHandleType(Type t) {
            return GetSerializer(t) != null;
        }

        /// <inheritdoc />
        public IEnumerable<PropertyNode> Serialize(object obj) {
            if (obj == null)
                return null;
            var type = obj.GetType();
            var s = GetSerializer(type);
            if (s != null)
                return s.Serialize(obj);
            throw new TypeNotSupportedException(string.Format("Couldn't serialize type '{0}'", type));
        }

        private ISolrFieldSerializer GetSerializer(Type t)
        {
            ISolrFieldSerializer result;
            if (!cachedSerializerCache.TryGetValue(t, out result))
            {
                result = serializers.FirstOrDefault(s => s.CanHandleType(t));
                cachedSerializerCache[t] = result;
            }
            return result;
        }
    }
}
