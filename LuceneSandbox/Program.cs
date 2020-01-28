using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var luceneVersion = LuceneVersion.LUCENE_48;
            var dir = new RAMDirectory();

            //create an analyzer to process the text
            var analyzer = new SimpleAnalyzer(luceneVersion);

            ////create an index writer
            var indexConfig = new IndexWriterConfig(luceneVersion, analyzer);
            var writer = new IndexWriter(dir, indexConfig);

            writer.AddDocument(new Document { new TextField("Query", new { Query = "Best pants in chicago" }.Query, Field.Store.YES) });
            writer.AddDocument(new Document { new TextField("Query", new { Query = "Best pants in detroit" }.Query, Field.Store.YES) });
            writer.AddDocument(new Document { new TextField("Query", new { Query = "Worst pants in chicago" }.Query, Field.Store.YES) });
            writer.AddDocument(new Document { new TextField("Query", new { Query = "Worst pants in detroit" }.Query, Field.Store.YES) });
            writer.AddDocument(new Document { new TextField("Query", new { Query = "hello world pants" }.Query, Field.Store.YES) });

            writer.Flush(triggerMerge: false, applyAllDeletes: false);

            // search with BooleanQuery
            //var boolQuery = new Lucene.Net.Search.BooleanQuery();
            //boolQuery.Add(new TermQuery(new Term("Query", "york")), Occur.SHOULD);
            //boolQuery.Add(new TermQuery(new Term("Query", "new")), Occur.SHOULD);

            var simpleQueryParser = new Lucene.Net.QueryParsers.Simple.SimpleQueryParser(analyzer, "Query");
            simpleQueryParser.DefaultOperator = Occur.SHOULD;            
            var query = simpleQueryParser.Parse("chicago -in");            

            // re-use the writer to get real-time updates
            var searcher = new IndexSearcher(writer.GetReader(applyAllDeletes: true));
            var hits = searcher.Search(query, 20 /* top 20 */).ScoreDocs;

            foreach (var hit in hits)
            {
                var foundDoc = searcher.Doc(hit.Doc);
                foreach (var item2 in foundDoc.Fields)
                {
                    Console.WriteLine("value: " + item2.GetStringValue() + " score: " + hit.Score);
                }
            }
        }
    }
}