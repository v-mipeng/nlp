using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using msra.nlp.tr.share;
using System.Collections;
using pml.type;

namespace msra.nlp.tr.feature
{
    public class FeatureExtractor
    {
        public FeatureExtractor() { }


        public Pair<int,IList> ExtractFeatureWithLable(String[] input)
        {
            Pair<int, IList> pair = new Pair<int, IList>();
            pair.first = GetTypeValue(input[1]);
            pair.second = ExtractFeature(new string[] { input[0], input[2] });
            return pair;
        }

        /*   Extract feature from the input
         *   The input should contains three items:
         *      Mention surface:   the surface text of the mention
         *      Mention context:   the context contains the mention
         *   The output is an int array store the features value:
         *      Mention surface:    [s0 e0]
         *      Mention Shape:      [s0 e0] 
         *                          [AA, A., (A|a)'(s), d-d, dd]
         *      Mention length
         *      Last token
         *      Next token
         *      Matched entity type: -1(no match)  : TODO
         *          organization.organization
                    people.person
                    location.location
                    commerce.consumer_product	business.product_category	medicine.drug	food.food	food.beverage	computer.computer	automotive.model	computer.software
                    sports.sport
                    medicine.disease
                    time.event
                    book.written_work
                    film.film
                    broadcast.content
                    visual_art.artwork 
         * 
         */
        public IList  ExtractFeature(String[] input)
        {
            int wordTableSize = DataCenter.GetWordTableSize();
            IList  feature = new List<Pair<int, int>>();
            String[] words = input[0].Split(' ');
            int offset = 0;
            // mention length
            feature.Add(new Pair<int,int>(offset++,words.Length));
            // mention shape: s0 e0
            feature.Add(new Pair<int, int>(offset++, GetWordShape(words[0])));
            feature.Add(new Pair<int, int>(offset++, GetWordShape(words[words.Length - 1])));
            // mention surface: s0 e0
            offset = feature.Count;
            feature.Add(new Pair<int, int>(offset+DataCenter.GetWordIndex(words[0]),1));
            offset += DataCenter.GetWordTableSize();
            feature.Add(new Pair<int, int>(offset+DataCenter.GetWordIndex(words[words.Length - 1]),1));
            // last token
            offset += DataCenter.GetWordTableSize();
            String lastToken =    GetLastToken(input[1], input[0]);
            if(lastToken != null)
            {
                feature.Add(new Pair<int,int>(offset+DataCenter.GetWordIndex(lastToken),1));
            }
            // next token
            offset += DataCenter.GetWordTableSize();
             String nextToken =    GetNextToken(input[1], input[0]);
            if(nextToken !=null)
            {
                 feature.Add(new Pair<int,int>(offset+DataCenter.GetWordIndex(nextToken),1));
            }
            // TODO: type of the matched entity
            return feature;
        }


        Dictionary<string, int> type2index = null;

        /* Make the type-->value map : define the interest type and others
         */ 
        void MakeTypeMap()
        {
            String[] types = new String[] { "organization.organization", "people.person", "location.location", "commerce.consumer_product", "sports.sport", "medicine.disease", "time.event", "book.written_work", "film.film", "broadcast.content", "visual_art.artwork","others"};
            type2index = new Dictionary<string, int>();

            for (int i = 0; i < types.Length; i++)
            {
                type2index[types[i]] = i;
            }
        }

        /* Get the mapped int value of type
         */ 
        int GetTypeValue(String type)
        {
           if(type2index == null)
           {
               MakeTypeMap();
           }
           try
           {
               int value = type2index[type];
               return value;
           }
           catch (Exception)
           {
               return type2index["others"];
           }

        }



        /*Get last token of the mention
         */ 
        static String pattern = @",[^,]*,";
        static Regex regex = new Regex(pattern);
        String[] seperator = new string[] { " ", "\t" };
        String GetLastToken(String context,String mention)
        {
            String head = context.Substring(0,context.IndexOf(mention));
            head = regex.Replace(head, " ").TrimEnd();
            String lastToken = null;
            for (int i = head.Length-1; i >= 0; i--)
            {
                  if(head[i]==' ' || head[i]=='\t')
                  {
                      lastToken = head.Substring(i + 1);
                      break;
                  }
            }
            return lastToken;
        }
    
        /* Get next token of the mention
         */ 
        String GetNextToken(String context, String mention)
        {
            String tail = context.Substring(context.LastIndexOf(mention) + mention.Length);
            tail = tail.TrimStart();
            if(tail.StartsWith(",") || tail.Length == 0)
            {
                return null;
            }
            else
            {
                int index;
                if ((index = tail.IndexOf(' ')) != -1)
                {
                    return tail.Substring(0, index);
                }
                else
                {
                    return tail;
                }
            }
        }

        /* [Aa or aa,AA, A., (A|a)'(s), d-d, dd]
        */ 
        int GetWordShape(String word)
        {
            if (IsNormal(word))
            {
                return 0;
            }
            else if (IsNumber(word))
            {
                return 5;
            }
            else if (IsAllUpCase(word))
            {
                return 1;
            }
            else if(IsShortName(word))
            {
                return 2;
            }
            else if (IsConnectNumber(word))
            {
                return 4;
            }
            else
            {
                return 0;
            }
            
        }

        /* Is shape: Aa, aa or other normal format
         */ 
        bool IsNormal(String input)
        {
           if(input.Length == 1)
           {
               return true;
           }
           else
           {
               for(int i = 1; i<input.Length-1; i++)
               {
                   if(input[i]<'a' || input[i] > 'z')
                   {
                       return false;
                   }
               }
               return true;
           }
        }

        bool IsAllUpCase(String input)
        {
            if(input.Length == 1)   // don't consider one character word like "A"
            {
                return false;
            }
            foreach(char item in input)
            {
                if(item>'Z' || item < 'A')
                {
                    return false;
                }
            }
            return true;
        }
        internal bool IsFirstUpCase(string input)
        {
            if(input[0] > 'A' && input[0] < 'Z')
            {
                for(int i = 1; i<input.Length-1; i++)
                {
                    if(input[i] > 'z'|| input[i] < 'a')
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /* Is shape: Mr.
         */
        bool IsShortName(String input)
        {
            if(input.EndsWith(".") && input[0] < 'Z' && input[0] > 'A')
            {
                return true;
            }
            return false;
        }

        /* Is shape : Jason's or Jasons'
         */ 
        static String pattern2 = @"\w*'(s)?";
        Regex regex2 = new Regex(pattern2);
        bool IsPossessive(String input)
        {
            if (regex2.IsMatch(input))
            {
                return true;
            }
            return false;
        }

        /* Is shape: dd
         */
        static string pattern3 = @"^\d*$";
        Regex regex3 = new Regex(pattern3);
        bool IsNumber(string input)
        {
            if (regex3.IsMatch(input))
            {
                return true;
            }
            return false;
        }

        /* Is shape: d-d
       */
        static string pattern4 = @"^\d*-\d*$";
        Regex regex4 = new Regex(pattern4);
        bool IsConnectNumber(string input)
        {
            if (regex4.IsMatch(input))
            {
                return true;
            }
            return false;
        }
    }
}
