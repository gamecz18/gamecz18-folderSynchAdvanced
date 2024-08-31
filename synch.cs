using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace folderSynch
{
    enum op
    {
        existuji,
        smazat,
        prepsat
    }



    class filesInfo
    {
        public string fileName { get; set; }
        public DateTime date { get; set; }
        public op operace { get; set; }
        public string copyDes { get; set; }
        public bool copy { get; set; }
        public long velikost { get; set; }

    }
    static class synch
    {
        public static List<filesInfo> sourceInfo = new List<filesInfo>();
        public static List<filesInfo> desInfo = new List<filesInfo>();
        static public void checkFiles(string folder, bool source, List<filesInfo> files)
        {
            
            foreach (var item in Directory.GetFiles(folder))
            {

                if (source)
                {
                    files.Add(new filesInfo
                    {
                        fileName = Path.GetFileName(item),
                        date = File.GetLastWriteTime(item),
                        velikost = new FileInfo(item).Length
                    });
                   
                    continue;

                }

                if (!sourceInfo.Any(x => x.fileName == Path.GetFileName(item)))
                {
                    files.Add(new filesInfo
                    {
                        fileName = Path.GetFileName(item),
                        date = File.GetLastWriteTime(item),
                        operace = op.smazat,
                        velikost = new FileInfo(item).Length
                    });
                    folders.pocetZmen++;
                    continue;
                }

                files.Add(new filesInfo
                {
                    fileName = Path.GetFileName(item),
                    date = File.GetLastWriteTime(item),
                    velikost = new FileInfo(item).Length
                });
                

            }

            //Thread.Sleep(10);


        }

        public static async void copyFiles(ProgressBar pb1)
        {
            Task t1 = Task.Run(() =>
            {
                checkExistance();
            });
            

            do
            {

            } while (t1.Status != TaskStatus.RanToCompletion);

            pb1.Dispatcher.Invoke(() =>
            {
                pb1.Maximum = folders.pocetZmen;
                pb1.Value = 0;
              
            });

            foreach (filesInfo item in desInfo)
            {
                if (item.operace == op.smazat)
                {
                    zvetsitHodnotu(pb1);
                    File.Delete(folders.destinacionFolder + "\\" + item.fileName);
                }
            }
            int chyba = 0;
            foreach (filesInfo item in sourceInfo)
            {
                skok:
                try
                {
                    if (item.copy && item.operace == op.existuji)
                    {
                        zvetsitHodnotu(pb1);
                        File.Copy(folders.sourseFolder + "\\" + item.fileName, item.copyDes + "\\" + item.fileName);
                        File.SetLastWriteTime(item.copyDes + "\\" + item.fileName, item.date);
                    }
                    else if (item.operace == op.prepsat)
                    {
                        zvetsitHodnotu(pb1);
                        File.Copy(folders.sourseFolder + "\\" + item.fileName, item.copyDes + "\\" + item.fileName, true);
                        File.SetLastWriteTime(item.copyDes + "\\" + item.fileName, item.date);
                    }
                }
                catch (Exception err)
                {
                    Thread.Sleep(1000);
                    if (chyba == 15)
                    {
                        chyba = 0;
                       System.Windows.Forms.MessageBox.Show(err.Message, "Error synch ", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Asterisk);
                    }
                    chyba++;
                    goto skok;
                }
                

            }
            MainWindow.Instance.reload();
            //MainWindow.Instance.disEnabElement(true);
            sourceInfo.Clear();
            desInfo.Clear();


        }


        static void zvetsitHodnotu(ProgressBar pb1)
        {
            pb1.Dispatcher.Invoke(() =>
            {
                pb1.Value++;
            });


        }


        static void checkExistance()
        {
            if (desInfo.Count == 0)
            {
                sourceInfo.ForEach(item => { item.copy = true; item.copyDes = folders.destinacionFolder; item.operace = op.existuji; });
                return;

            }
            foreach (var (item, index) in sourceInfo.Select((value, i) => (value, i)))
            {
                if (!desInfo.Any(a => a.fileName == item.fileName))
                {
                    sourceInfo[index].copy = true;
                    sourceInfo[index].operace = op.existuji;
                    sourceInfo[index].copyDes = folders.destinacionFolder;
                    folders.pocetZmen++;

                }
                else
                {
                    if (desInfo.Find(x => x.fileName == item.fileName).velikost != item.velikost)
                    {

                        sourceInfo[index].operace = op.prepsat;
                        sourceInfo[index].copyDes = folders.destinacionFolder;
                        folders.pocetZmen++;
                    }
                }

            }



        }




    }
}
