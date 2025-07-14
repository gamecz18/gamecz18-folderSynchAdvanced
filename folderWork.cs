using folderSynch;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;



namespace folderSynch
{

    struct infoFolder()
    {
        public string cesta { get; set; }

        public string cestaNazev { get; set; }
        public string cestaInside { get; set; }
        public bool zkopitovano = false;


    }

    class hashing
    {

        public static string getHashWholeSync(string cesta)
        {
            using Stream st1 = File.OpenRead(cesta);
            using SHA256 sha256 = SHA256.Create();


            byte[] hash = sha256.ComputeHash(st1);
            return Convert.ToHexString(hash);
        }





        public static string getHashPart(string cesta)
        {
            using Stream file = File.OpenRead(cesta);
            using SHA256 sha256 = SHA256.Create();
            byte[] buffer = new byte[8192];
            var pozice = new[] { 0L, file.Length / 2, Math.Max(0, file.Length - 8192) };

            foreach (var item in pozice)
            {

                file.Seek(item, SeekOrigin.Begin);
                int read = file.Read(buffer);
                sha256.TransformBlock(buffer, 0, read, null, 0);

            }
            sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

            return Convert.ToHexString(sha256.Hash);



        }

        public static string getHash(string cesta)
        {
            // return "s";
            FileInfo fi = new FileInfo(cesta);

            if (settings.wholeHasing)
            {
                return getHashWholeSync(cesta);
            }


            switch (fi.Length)
            {
                case < 1024 * 1024:
                    return getHashWholeSync(cesta);

                default:
                    return getHashPart(cesta);

            }




        }


    }


    static class pridaniDoList
    {
        public class listItemyPridat
        {
            public string nazev { get; set; }
            public string hash { get; set; }
            public string operace { get; set; }
            public string cesta { get; set; }
            public string ikona { get; set; }
            public bool IsChecked { get; set; }
            public string copyDes { get; set; }
            public System.Windows.Media.Brush Pozadi { get; set; }

        }



        static public void addToList(ref ListView lv1, List<filesInfo> lf1)
        {
            lv1.Items.Clear();



            foreach (filesInfo lf in lf1)
            {
                List<string> st = new List<string> { lf.fileName, lf.hash };



                System.Windows.Media.Brush pozadiPomoc = lf.operace switch
                {
                    op.existujiVelikostJina or op.starsiOdlisny or op.novejsiOdlisny => System.Windows.Media.Brushes.Orange,
                    op.nexistuji => System.Windows.Media.Brushes.Yellow,
                    op.sameHash or op.novejsiStejnyObsah or op.starsiStejnyObsah => System.Windows.Media.Brushes.LightGreen,
                    op.neznama => System.Windows.Media.Brushes.Red,
                    _ => System.Windows.Media.Brushes.Transparent
                };


                listItemyPridat item = new listItemyPridat
                {
                    nazev = lf.fileName,
                    hash = lf.hash,
                    operace = prevodOperace(lf.operace).text,
                    ikona = prevodOperace(lf.operace).ikona,
                    cesta = lf.cesta.ToString(),
                    IsChecked = lf.vybran,
                    Pozadi = pozadiPomoc,
                    copyDes = lf.copyDes,





                };



                lv1.Items.Add(item);


            }


        }

        static (string text, string ikona) prevodOperace(op operace)
        {

            return operace switch
            {
                op.existuji => ("Existují", "📁"),
                op.existujiVelikostJina => ("Existují – jiná velikost", "⚠️"),
                op.nexistuji => ("Neexistují", "❌"),
                op.sameHash => ("Stejný hash", "✅"),
                op.neznama => ("Neznámé", "❓"),
                op.novejsiOdlisny => ("Novější - jiný obsah", "❌🆕"),
                op.starsiOdlisny => ("Starší - jiný obsah", "❌⏰"),
                op.novejsiStejnyObsah => ("Novější - stejný obsah", "✅🆕"),
                op.starsiStejnyObsah => ("Starší - stejný obsah", "✅⏰"),
                _ => ("Neznámý stav", "❓")


            };
            /*
            switch (operace)
            {
                case op.existuji:
                    return ("Existují", "📁");
                case op.existujiVelikostJina:
                    return ("Existují – jiná velikost", "⚠️");
                case op.nexistuji:
                    return ("Neexistují", "❌");
                case op.sameHash:
                    return ("Stejný hash", "✅");
                case op.neznama:
                    return ("Neznámé", "❓");
                default:
                    return ("Neznámý stav", "❓");
            }*/

        }
    }



    class pridaniDoTree
    {



        private class node
        {
            public string nazev { get; set; }
            public string icona { get; set; }
            public bool zkopirovano { get; set; } = false;
            public op operace { get; set; }

            public ObservableCollection<node> children { get; set; } = new();
        }





        static public void pridaniDoTreeView(ref TreeView treeView, List<filesInfo> fi = null)
        {
            for (int i = 0; i <= 5; i++)
            {


                node rootNode = new node
                {
                    nazev = "Root",
                    icona = "root_icon", // zde by měla být cesta k ikoně
                    operace = op.neznama
                };

                for (int j = 0; j < 5; j++)
                {
                    rootNode.children.Add(new node
                    {
                        nazev = $"Child {j + 1}",
                        icona = "child_icon", // zde by měla být cesta k ikoně
                        operace = op.neznama

                    });
                    
                }
                
                treeView.Items.Add(rootNode);
            }
       
            /* foreach (var item in fi)
             {

             }*/
        }



    }


    static class searchFolder
    {
        public static List<infoFolder> listFoldersSource = new List<infoFolder>();
        public static List<infoFolder> listFoldersDes = new List<infoFolder>();
        static public async Task findFolders(string cesta, List<infoFolder> listFolders, string mainCesta, List<filesInfo> files = null)
        {
            if (files != null)
            {
                Parallel.ForEach(Directory.GetFiles(cesta), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, file =>
                {

                    string hashPomoc = hashing.getHash(file);
                    string jmenoPomoc = Path.GetFileName(file);
                    lock (files)
                    {
                        files.Add(new filesInfo
                        {
                            fileName = jmenoPomoc,
                            dateOfLastChange = File.GetLastWriteTime(file),
                            dateOfCreation = File.GetCreationTime(file),
                            dateOfLastOpen = File.GetLastAccessTime(file),
                            velikost = new FileInfo(file).Length,
                            hash = hashPomoc,
                            cesta = file
                        });
                    }
                });
            }

            foreach (string item in Directory.GetFileSystemEntries(cesta))
            {


                FileAttributes fa1 = File.GetAttributes(item);
                if ((fa1 & FileAttributes.Directory) == FileAttributes.Directory)
                {

                    string[] pomoc = item.Split(new string[] { @"\" }, StringSplitOptions.None);
                    string pomocS = item.Substring(item.IndexOf(new DirectoryInfo(mainCesta).Name));
                    listFolders.Add(new infoFolder
                    {
                        cesta = item,
                        cestaNazev = "\\" + pomoc[pomoc.Length - 1],
                        cestaInside = (pomocS).Substring(pomocS.IndexOf("\\"))


                    });

                    findFolders(item, listFolders, mainCesta, files);
                }





            }
            if (cesta == mainCesta)
            {
                string[] pomoch = cesta.Split(new string[] { @"\" }, StringSplitOptions.None);
                listFolders.Add(new infoFolder { cesta = cesta, cestaNazev = "\\" + pomoch[pomoch.Length - 1], cestaInside = "" });
            }
            return;


        }



    }


    static class folders
    {

        public static int pocetZmen = 0;



        static public void zjisiPocetSouborů(List<filesInfo> sourceInfo)
        {

            MainWindow.Instance.countSameHash.Content = sourceInfo.Count(x => x.operace == op.sameHash || x.operace == op.novejsiStejnyObsah || x.operace == op.starsiStejnyObsah);
            MainWindow.Instance.countExist.Content = sourceInfo.Count(x => x.operace == op.existuji);
            MainWindow.Instance.countDifferentSize.Content = sourceInfo.Count(x => x.operace == op.existujiVelikostJina || x.operace == op.starsiOdlisny || x.operace == op.novejsiOdlisny);
            MainWindow.Instance.countNotExist.Content = sourceInfo.Count(x => x.operace == op.nexistuji);
            MainWindow.Instance.countUnknown.Content = sourceInfo.Count(x => x.operace == op.neznama);



        }

        public static void creteBoot()
        {
            string cesta = Directory.GetCurrentDirectory();
            // vytvozi se objekt 
            IWshRuntimeLibrary.WshShell w1 = new IWshRuntimeLibrary.WshShell();
            //dalsi objekt // ktery se urci cesta kam se ma zastupce dat
            IWshRuntimeLibrary.IWshShortcut s1 = (IWshRuntimeLibrary.IWshShortcut)w1.CreateShortcut($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Microsoft\Windows\Start Menu\Programs\Startup\{settings.jmenoInstance}.lnk");
            //zaspuce otevre
            s1.TargetPath = $@"{cesta}\folderSynch.exe";
            s1.WorkingDirectory = $@"{cesta}";
            //ulozi
            s1.Save();
            Clipboard.SetText($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Microsoft\Windows\Start Menu\Programs\Startup");
            System.Windows.Forms.MessageBox.Show($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Microsoft\Windows\Start Menu\Programs\Startup", "Spouštěcí cesta byla uložena do schránky");



        }



        public static void saveSettings()
        {
            Clipboard.SetText(settings.jsemCesta);
            System.Windows.Forms.MessageBox.Show(settings.jsemCesta, "Cesta byla uložena do schránky");

            SettingsData nastaveni = new SettingsData
            {
                SourseFolder = settings.sourseFolder,
                DestinacionFolder = settings.destinacionFolder,
                TimeToSynch = settings.timeToSynch,
                JmenoInstance = settings.jmenoInstance,
                BootOnStartup = settings.bootOnStartup,
                SynchAllFoldes = settings.synchAllFoldes,
                WholeHasing = settings.wholeHasing



            };
            //serializace do jsonu
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true // pro lepší čitelnost

            };
            string JsonString = JsonSerializer.Serialize(nastaveni, options);

            File.WriteAllText(settings.jsemCesta + "\\" + "setting.txt", JsonString);





        }


        public static void saveSettingsOld()
        {
            Clipboard.SetText(settings.jsemCesta);
            System.Windows.Forms.MessageBox.Show(settings.jsemCesta, "Cesta byla uložena do schránky");


            using (StreamWriter st1 = new StreamWriter(settings.jsemCesta + "\\" + "setting.txt"))
            {



                st1.WriteLine($"Folders out:* {settings.sourseFolder} ");
                st1.WriteLine($"Folders in:* {settings.destinacionFolder}");
                st1.WriteLine($"Time to Synch:* {settings.timeToSynch}");
                st1.WriteLine($"Instance:* {settings.jmenoInstance}");
                st1.WriteLine($"Statup:* {settings.bootOnStartup}");
                st1.WriteLine($"Synch Folders:* {settings.synchAllFoldes}");



            }




        }
        public static void loadSettings()
        {       //pokud cesta existuje
            if (File.Exists(settings.jsemCesta + "\\" + "setting.txt"))
            {
                try
                {

                    string jsonString = File.ReadAllText(settings.jsemCesta + "\\" + "setting.txt");



                    SettingsData nastaveni = JsonSerializer.Deserialize<SettingsData>(jsonString);

                    if (nastaveni != null)
                    {
                        settings.sourseFolder = nastaveni.SourseFolder;
                        settings.destinacionFolder = nastaveni.DestinacionFolder;
                        settings.timeToSynch = nastaveni.TimeToSynch;
                        settings.jmenoInstance = nastaveni.JmenoInstance;
                        settings.bootOnStartup = nastaveni.BootOnStartup;
                        settings.synchAllFoldes = nastaveni.SynchAllFoldes;
                        settings.wholeHasing = nastaveni.WholeHasing;
                        if (!string.IsNullOrEmpty(settings.sourseFolder))
                        {
                            MainWindow.Instance.sourcePath.Content = $"Cesta: : {settings.sourseFolder}";
                            MainWindow.Instance.sourcePath2.Content = $"Cesta: : {settings.sourseFolder}";
                        }
                        if (!string.IsNullOrEmpty(settings.destinacionFolder))
                        {
                            MainWindow.Instance.desPath.Content = $"Cesta: : {settings.destinacionFolder}";
                            MainWindow.Instance.desPath2.Content = $"Cesta: : {settings.destinacionFolder}";

                        }
                    }



                }
                catch (Exception err)
                {

                    System.Windows.Forms.MessageBox.Show(err.Message, "Error with reading file", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }

        }

        public static void loadSettingsOld()
        {       //pokud cesta existuje
            if (File.Exists(settings.jsemCesta + "\\" + "setting.txt"))
            {
                try
                {


                    //udělá se stream reader a ten poté přečte celý soubor 
                    using (StreamReader st1 = new StreamReader(settings.jsemCesta + "\\" + "setting.txt"))
                    {
                        string line;
                        string[] parts = new string[6];
                        int pomoc = 0;
                        while ((line = st1.ReadLine()) != null)
                        {
                            //splitne a druhá čast se uloži
                            parts[pomoc] = line.Split(new string[] { ":*" }, System.StringSplitOptions.None)[1];
                            pomoc++;
                        }
                        settings.sourseFolder = parts[0].Trim();
                        settings.destinacionFolder = parts[1].Trim();
                        settings.timeToSynch = int.Parse(parts[2].Trim());
                        settings.jmenoInstance = parts[3].Trim();
                        settings.bootOnStartup = bool.Parse(parts[4].Trim());
                        settings.synchAllFoldes = bool.Parse(parts[5].Trim());

                    }
                    if (!string.IsNullOrEmpty(settings.sourseFolder))
                    {
                        MainWindow.Instance.sourcePath.Content = $"Cesta: : {settings.sourseFolder}";
                    }
                    if (!string.IsNullOrEmpty(settings.destinacionFolder))
                    {
                        MainWindow.Instance.desPath.Content = $"Cesta: : {settings.destinacionFolder}";
                    }


                }
                catch (Exception err)
                {

                    System.Windows.Forms.MessageBox.Show(err.Message, "Error with reading file", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }

        }

    }

}
static class folderWork
{


    static public void smazaniSouboruZDisku(List<filesInfo> infoOsouborech)
    {
        foreach (filesInfo item in infoOsouborech)
        {
            FileSystem.DeleteFile(item.cesta, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }



    }



    static public bool selecFolder(ref string s)
    {

        var op = new OpenFolderDialog();

        if (op.ShowDialog().Value == true)
        {
            s = op.FolderName;
            return false;
        }
        return true;

    }
}

