using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using pml.file.writer;
using System.IO;
using System.Text.RegularExpressions;
using msra.nlp.tr.feature;
using System.Collections;
using pml.type;

namespace msra.nlp.tr.share
{
    public class Pipeline
    {
        string digital = "65535";
        static Regex regex2 = new Regex(@"^\d+$");
        static String pattern = @"\b[\w\d-']+\b";
        static Regex regex = new Regex(pattern);
        public void ExtractWordTable()
        {
            FileReader reader = new LargeFileReader(DefaultPaths.TRAIN_DATA_FILE);
            FileStream fileStream = new FileStream(DefaultPaths.wordTableFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            string line;
            string[] array;
            string word;
            MatchCollection mc;
            Dictionary<String, int> wordTable = new Dictionary<string, int>();


            while ((line = reader.ReadLine()) != null)
            {
                array = line.Split('\t');
                mc = regex.Matches(array[2]);
                foreach (Match match in mc)
                {
                    word = match.Groups[0].Value;
                    if (regex2.IsMatch(word))
                    {
                        word = this.digital;
                    }
                    if (!wordTable.ContainsKey(word))
                    {
                        wordTable[word] = wordTable.Count + 1;
                        streamWriter.WriteLine(word + "\t" + wordTable.Count);
                    }
                }
            }
            reader.Close();
            streamWriter.Close();
            fileStream.Close();
        }

        FeatureExtractor extractor = new FeatureExtractor();

        public void ExtractFeature()
        {
            Console.Error.WriteLine("Extract feature of train data...");
            Extract(DefaultPaths.TRAIN_DATA_FILE, DefaultPaths.TRAIN_FEATURE_FILE);
            Console.Error.WriteLine("Train feature done!");
            Console.Error.WriteLine("Extract feature of test data...");
            Extract(DefaultPaths.TEST_DATA_FILE, DefaultPaths.TEST_FEATURE_FILE);
            Console.Error.WriteLine("Test feature done!");

        }

        public void Extract(String source, String des)
        {
            Dictionary<int, int> numByType = new Dictionary<int, int>(16);
            FileReader reader = new LargeFileReader(source);
            FileStream fileStream = new FileStream(des, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            string line;
            string[] array;
            Pair<int, IList> pair;

            int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                count++;
                if (count % 1000 == 0)
                {
                    Console.Error.WriteLine(count + " items processed!");
                }
                array = line.Split('\t');
                try
                {
                    pair = extractor.ExtractFeatureWithLable(array);
                }
                catch (Exception)
                {
                    continue;
                }
                int num = 0;
                try
                {
                    num = numByType[pair.first];
                }
                catch (Exception)
                {
                    num = 0;
                }
                if (num > 100000)   // limit the item num up to 100,000
                {
                    continue;
                }
                streamWriter.Write(pair.first);
                List<Pair<int, int>> feature = (List<Pair<int, int>>)pair.second;

                for (int i = 0; i < feature.Count; i++)
                {
                    streamWriter.Write("\t" + feature[i].first + ":" + feature[i].second);
                }
                streamWriter.Write("\n");
                numByType[pair.first] = ++num;
            }
            reader.Close();
            streamWriter.Close();
            fileStream.Close();
            Console.WriteLine(numByType);
            Console.WriteLine("");
        }
    }
}
