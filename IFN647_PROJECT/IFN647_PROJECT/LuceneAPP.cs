using System;
using System.Linq;
using Lucene.Net.Analysis; // for Analyser
using Lucene.Net.Documents; // for Document
using Lucene.Net.Index; //for Index Writer
using Lucene.Net.Store; //for Directory
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search; // for IndexSearcher
using Lucene.Net.QueryParsers;  // for QueryParser
using Lucene.Net.Analysis.Snowball;
using System.Collections.Generic;


namespace IFN647_PROJECT
{
    public class LuceneAPP
    {
       public static Directory IndexDirectory;
        public static Analyzer analyzer;
         IndexWriter writer;
        public static IndexSearcher searcher;
        public static QueryParser parser;
        public static Query query;
        PorterStemmerAlgorithm.PorterStemmer myStemmer; 
        Dictionary<string, int> tokenCount;
        public string[] stopWords = { "a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with" };
        CustomerSimilarity similarity = new CustomerSimilarity();
        public static Field title_field;
        public static Field author_field;
        public static string id = "ID";
        public static string TITLE = "TItle";
        public static string AUTHOR = "Author";
        public static string BIB = "BiblInfo";
        public static string ABS = "Abstract";








        public void OpenIndex(string index_path)
        {
            IndexDirectory = FSDirectory.Open(index_path);
           
        }
        public void CreatAnalyzer_writer()
        {
            //analyzer = new SnowballAnalyzer(Lucene.Net.Util.Version.LUCENE_30, "English");
            analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            IndexWriter.MaxFieldLength mfl = new IndexWriter.MaxFieldLength(IndexWriter.DEFAULT_MAX_FIELD_LENGTH);
            writer = new IndexWriter(IndexDirectory, analyzer, true, mfl);
            writer.SetSimilarity(similarity);
            searcher = new IndexSearcher(LuceneAPP.IndexDirectory);
            searcher.Similarity = writer.Similarity;

        }
        // Method Read_directory() will get all path of file  that under the folder "folder_path"
        // then, these path as input to method Deal_indexing()
        public void Read_directory(string folder_path)
        {
            System.IO.DirectoryInfo root = new System.IO.DirectoryInfo(folder_path);
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder 
            try
            {
                files = root.GetFiles("*.*");
            }

            catch (UnauthorizedAccessException e)
            {
                System.Console.WriteLine(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                // Process every file
                foreach (System.IO.FileInfo fi in files)
                {
                    string name = fi.FullName;
                    Deal_indexing(name);

                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                // Resursive call for each subdirectory.
                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    string name = dirInfo.FullName;
                    Read_directory(name);

                }
            }
        }
        public void Deal_indexing(string file_path)
        {
            /*used to split text according to DOCID, 
            Title, Author, Bibliographic information and abstract*/
            string[] delims = { ".I",".T" ,".A",".B",".W"};           
            System.IO.StreamReader reader = new System.IO.StreamReader(file_path);
            string lines = reader.ReadToEnd();
            string[] lines_split = lines.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            // remove abstract string from 0 to length of title, so we can get abstract without title 
            lines_split[4] = lines_split[4].Remove(0,lines_split[1].Length);
            //creating 5 field for DOCID, Title, Author, Bibliographic information and abstract
            //need analyzed: document id. 
            Field id_field = new Field(id,lines_split[0],Field.Store.YES,Field.Index.NOT_ANALYZED);    
            title_field = new Field(TITLE, lines_split[1], Field.Store.YES, Field.Index.ANALYZED,Field.TermVector.WITH_POSITIONS_OFFSETS);
             author_field = new Field(AUTHOR, lines_split[2], Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            Field bil_field = new Field(BIB, lines_split[3], Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            Field abs_field = new Field(ABS, lines_split[4], Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            title_field.Boost = indexForm1.titleboost;
            author_field.Boost = indexForm1.authorboost;
            Document doc = new Document();           
            doc.Add(id_field);
            doc.Add(title_field);
            doc.Add(author_field);
            doc.Add(bil_field);
            doc.Add(abs_field);
            writer.AddDocument(doc);
            
        }
       
        
        
        public void CreatParser()
        {
            

            parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, new[] { "Abstract", "TItle", "ID" }, analyzer);

        }
        public TopDocs Searching(string input_query)
        {
           // input_query = input_query.ToLower();
             query = parser.Parse(input_query);
           
            TopDocs results = searcher.Search(query, 500);
            
            return results;

        }
        public void CleanUpindex()
        {
            writer.Optimize();
            writer.Flush(true, true, true);
            writer.Dispose();
        }
        public void CleanUpSearch()
        {
           
            searcher.Dispose();
        }
        //if user choose preprocessing, this method will do following steps: tokenization, remove stop word and 
        public string preprocessing(string text)
        {
            string[] separators = { ",", ".", "!", "?", ";", ":", "-", " ", "\n", "\"", "'" };
            string[] query_token=text.ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries);
            myStemmer = new PorterStemmerAlgorithm.PorterStemmer();
            List<string> filteredTokens = new List<string>();
            for (int i = 0; i < query_token.Length; i++)
            {
                string token = query_token[i];
                if (!stopWords.Contains(token) && (token.Length > 2))
                    filteredTokens.Add(token);
            }
            filteredTokens.ToArray<string>();
            string processed = "";
            foreach (var word in filteredTokens)
            {
                processed += myStemmer.stemTerm(word)+" ";
            }
            return processed;
            


        }





    }
    
}
