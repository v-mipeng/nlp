using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr.share
{
    /* The parameters within this class can be assigned by the user
     * however, they are not rewritable once assigned.
     */ 
    class GlobalParameter
    {

        internal static String wordTableFilePath = null;
        internal static String TRAIN_DATA_FILE = null;
        internal static String DEVELOP_DATA_FILE = null;
        internal static String TEST_DATA_FILE = null;
        internal static String TRAIN_FEATURE_FILE = null;
        internal static String TEST_FEATURE_FILE = null;
        
        private GlobalParameter()
        {

        }
    }
}
