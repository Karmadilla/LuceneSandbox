using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Simple;
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
            Random random = new Random();
            string[] words = File.ReadAllLines("words.txt");

            var luceneVersion = LuceneVersion.LUCENE_48;
            var dir = new RAMDirectory();

            //create an analyzer to process the text
            var analyzer = new WhitespaceAnalyzer(luceneVersion);

            //create an index writer
            var indexConfig = new IndexWriterConfig(luceneVersion, analyzer);
            var writer = new IndexWriter(dir, indexConfig);

            //writer.AddDocument(new Document { new TextField("Query", new { Query = "Best pants in chicago" }.Query, Field.Store.YES) });
            //writer.AddDocument(new Document { new TextField("Query", new { Query = "Best pants in detroit" }.Query, Field.Store.YES) });
            //writer.AddDocument(new Document { new TextField("Query", new { Query = "Worst pants in chicago" }.Query, Field.Store.YES) });
            //writer.AddDocument(new Document { new TextField("Query", new { Query = "Worst pants in detroit" }.Query, Field.Store.YES) });
            //writer.AddDocument(new Document { new TextField("Query", new { Query = "hello world pants" }.Query, Field.Store.YES) });

            //writer.Flush(triggerMerge: false, applyAllDeletes: false);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Index");
                Console.WriteLine("2. Search");
                Console.WriteLine("3. Fill Index (bulk index random word sentences)");

                string option = Console.ReadLine().ToString();

                if (option == "1")
                {
                    Console.Clear();
                    Console.Write("Text to index: ");
                    string textToIndex = Console.ReadLine();
                    writer.AddDocument(new Document() { new TextField("Query", new { Query = textToIndex }.Query, Field.Store.YES) });
                    writer.Flush(true, false);
                }
                else if (option == "2")
                {
                    Console.Clear();
                    Console.Write("Text to search: ");
                    string textToSearch = Console.ReadLine();
                    
                    var simpleQueryParser = new Lucene.Net.QueryParsers.Simple.SimpleQueryParser(
                        analyzer,
                        new Dictionary<string, float>() 
                        {
                            { "Query", 1f }
                        }, Operator.OR_OPERATOR);
                    simpleQueryParser.DefaultOperator = Occur.SHOULD;
                    var query = simpleQueryParser.Parse(textToSearch);
                    var searcher = new IndexSearcher(writer.GetReader(false));
                    var topDocs = searcher.Search(query, 10).ScoreDocs;

                    foreach (var hit in topDocs)
                    {
                        var foundDoc = searcher.Doc(hit.Doc);

                        foreach (var item2 in foundDoc.Fields)
                        {
                            Console.WriteLine("value: " + item2.GetStringValue() + " score: " + hit.Score);
                        }
                    }
                    Console.WriteLine("Press Enter to continue");
                    Console.ReadLine();
                }
                else if (option == "3")
                {
                    Console.Clear();
                    Console.Write("How many sentences?: ");
                    string numberOfRandomSentences = Console.ReadLine();

                    int number = Convert.ToInt32(numberOfRandomSentences);

                    for (int i = 0; i < number; i++)
                    {
                        string sentence = GenerateRandomSentence(words, random).ToLower();
                        Console.WriteLine(i + " " + sentence);

                        writer.AddDocument(new Document() { new TextField("Query", new { Query = sentence }.Query, Field.Store.YES) });
                        
                    }
                    writer.Flush(true, false);
                    Console.ReadLine();
                }
            }

            //var simpleQueryParser = new Lucene.Net.QueryParsers.Simple.SimpleQueryParser(analyzer, "Query");
            //simpleQueryParser.DefaultOperator = Occur.SHOULD;
            //var query = simpleQueryParser.Parse("Best pants chicago -in");            

            //// re-use the writer to get real-time updates
            //var searcher = new IndexSearcher(writer.GetReader(applyAllDeletes: true));
            //var hits = searcher.Search(query, top 20).ScoreDocs;

            //foreach (var hit in hits)
            //{
            //    var foundDoc = searcher.Doc(hit.Doc);
            //    foreach (var item2 in foundDoc.Fields)
            //    {
            //        Console.WriteLine("value: " + item2.GetStringValue() + " score: " + hit.Score);
            //    }
            //}
        }

        private static string GenerateRandomSentence(string[] words, Random random)
        {
            int numberOfWords = random.Next(4, 8);
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < numberOfWords; i++)
            {
                string sentence = GetRandomWord(words, random) + " ";
                builder.Append(sentence);
            }
            return builder.ToString().Trim();
        }

        private static string GetRandomWord(string[] words, Random random)
        {
            int word = random.Next(words.Length);
            return words[word];
        }
    }
}