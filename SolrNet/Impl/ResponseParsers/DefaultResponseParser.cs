﻿using System;
using System.Xml.Linq;

namespace SolrNet.Impl.ResponseParsers {
    public class DefaultResponseParser<T>: ISolrAbstractResponseParser<T> {
        private readonly AggregateResponseParser<T> parser;

        public DefaultResponseParser(ISolrDocumentResponseParser<T> docParser) {
            parser = new AggregateResponseParser<T>(new ISolrAbstractResponseParser<T>[] {
                new ResultsResponseParser<T>(docParser),
                new HeaderResponseParser<T>(),
                new FacetsResponseParser<T>(),
                new FacetsNewResponseParser<T>(),
                new HighlightingResponseParser<T>(),
                new MoreLikeThisResponseParser<T>(docParser),
                new SpellCheckResponseParser<T>(),
                new StatsResponseParser<T>(),
                new CollapseResponseParser<T>(),
                new GroupingResponseParser<T>(docParser),
                new CollapseExpandResponseParser<T>(docParser),
                new ClusterResponseParser<T>(),
                new TermsResponseParser<T>(),
                new MoreLikeThisHandlerMatchResponseParser<T>(docParser),
                new InterestingTermsResponseParser<T>(),
				new TermVectorResultsParser<T>(),
                new DebugResponseParser<T>()
            });
        }

        /// <inheritdoc />
        public void Parse(XDocument xml, AbstractSolrQueryResults<T> results) {
            parser.Parse(xml, results);
        }
    }
}
