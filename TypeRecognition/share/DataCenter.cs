using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pml.file.reader;
using pml.type;

namespace msra.nlp.tr.share
{
    public class DataCenter
    {
        

        /*The word table of the train data
         */ 
        static Dictionary<String, int> word2index = null;

        /* get the word table
         */ 
        public static Dictionary<String, int> GetWordTable()
        {
            if (word2index == null)
            {
                LoadWordTable();
            }
            return word2index;
        }

        /*Get size of the word table
         */ 
        public static int GetWordTableSize()
        {
            if (word2index == null)
            {
                LoadWordTable();
            }
            return word2index.Count;
        }

        public static bool InsertToWordTable(Pair<String,int> pair)
        {
            if(word2index == null)
            {
                LoadWordTable();
            }
            if(word2index.ContainsKey(pair.first))
            {
                return false;
            }
            else
            {
                word2index[pair.first] = pair.second;
                return true;
            }
        }

        public static int GetWordIndex(String word)
        {
            if (word2index == null)
            {
                LoadWordTable();
            }
            int index;
            try
            {
                index = word2index[word];
                return index;
            }catch(Exception)
            {
                word2index.Add(word, word2index.Count + 1);
                return word2index.Count;
            }
        }

        private static void LoadWordTable()
        {
            FileReader fileReader = null;
            try
            {
                if (GlobalParameter.wordTableFilePath != null)
                {
                    fileReader = new LargeFileReader(GlobalParameter.wordTableFilePath);
                }
                else
                {
                    fileReader = new LargeFileReader(DefaultPaths.wordTableFilePath);
                }
            }catch(Exception e)
            {
                if (GlobalParameter.wordTableFilePath != null)
                {
                    Console.Error.WriteLine("Cannot open word table file with the path: " + GlobalParameter.wordTableFilePath);
                }
                else
                {
                    Console.Error.WriteLine("Cannot open word table file with the path: " + DefaultPaths.wordTableFilePath);
                }
            }
            String line;
            String[] array;
            word2index = new Dictionary<string, int>();

            while((line = fileReader.ReadLine())!=null)
            {
                try{
                    array = line.Split('\t');
                    word2index[array[0]] = int.Parse(array[1]);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            fileReader.Close();
        }


        private DataCenter() { }
    }
}
