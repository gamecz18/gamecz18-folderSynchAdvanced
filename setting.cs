using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace folderSynch
{
    class settings
    {

        public static string sourseFolder = null;
        public static string destinacionFolder = null;

        public static int timeToSynch = 900000;
        public static string jmenoInstance { get; set; }
        
        public static bool bootOnStartup = false;
        public static bool synchAllFoldes = false;
        public static bool wholeHasing = false;
        public static bool dateDetectionDup = true;
        public static dateCheck jakaOperaceData = dateCheck.createTime;
        public static string jsemCesta { get { return Directory.GetCurrentDirectory(); }}

    }
    public class SettingsData
    {
        public string SourseFolder { get; set; }
        public string DestinacionFolder { get; set; }
        public int TimeToSynch { get; set; }
        public string JmenoInstance { get; set; }
        public bool BootOnStartup { get; set; }
        public bool SynchAllFoldes { get; set; }
        public bool WholeHasing { get; set; }
    }
}
