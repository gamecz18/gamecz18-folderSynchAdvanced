using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;


//tohle smazat je to k ničemu
using System.Diagnostics;

namespace folderSynch
{



    enum dateCheck
    {

        createTime,
        changeTime,
        lastOpenTime


    }

    public enum op
    {
        existuji,
        smazat,
        prepsat,
        existujiVelikostJina,
        nexistuji,
        sameHash,
        neznama,
        novejsiOdlisny,
        starsiOdlisny,
        novejsiStejnyObsah,
        starsiStejnyObsah


    }



    class filesInfo
    {

        public int ID { get; set; }
        public string fileName { get; set; }
        public DateTime dateOfLastChange { get; set; }
        public DateTime dateOfLastOpen { get; set; }
        public DateTime dateOfCreation { get; set; }
        public op operace { get; set; }
        public string copyDes { get; set; }
        public bool copy { get; set; }
        public long velikost { get; set; }
        public string hash { get; set; }
        public string cesta { get; set; }
        public bool vybran { get; set; }

        //asi k ničemu niný používá se v old metodach asi
        public filesInfo Clone()
        {
            return new filesInfo
            {
                fileName = this.fileName,
                dateOfLastChange = this.dateOfLastChange,
                operace = this.operace,
                copyDes = this.copyDes,
                copy = this.copy,
                velikost = this.velikost,
                hash = this.hash,
                cesta = this.cesta

            };
        }

    }
    static class synch
    {
        public static List<filesInfo> sourceInfo = new List<filesInfo>();
        public static List<filesInfo> desInfo = new List<filesInfo>();
        private static readonly object zamek = new object();
        
        //to-do přidat stavy sem
        public static readonly op[] opraceNaVlozeni = { op.existuji, 
            op.existujiVelikostJina, op.nexistuji, op.sameHash, 
            op.neznama, op.novejsiStejnyObsah, op.starsiStejnyObsah, op.starsiOdlisny, op.novejsiOdlisny };




        static public void deleteSameFiles(List<filesInfo> files)
        {

            foreach (var item in (files.FindAll(x => x.operace == op.sameHash)))
            {
                files.Remove(item);
            }

        }


        static public void checkFilesExis(List<filesInfo> files = null, List<filesInfo> filesSource = null)
        {

            Dictionary<string, List<filesInfo>> hashGroups = null;
            Dictionary<string, List<filesInfo>> fileJmenoGroups = null;

            if (filesSource != null)
            {
                hashGroups = files.GroupBy(x => x.hash).ToDictionary(g => g.Key, g => g.ToList());
                fileJmenoGroups = files.GroupBy(x => x.fileName).ToDictionary(g => g.Key, g => g.ToList());
            }


            op coDelam = op.neznama;
            foreach (var item in filesSource)
            {
                DateTime oldDateLastChange = item.dateOfLastChange;
                DateTime oldDateLastOpen = item.dateOfLastOpen;
                DateTime oldDateLastCreation = item.dateOfCreation;


                DateTime porovnavaneDatum = settings.jakaOperaceData switch
                {
                    dateCheck.changeTime => oldDateLastChange,
                    dateCheck.createTime => oldDateLastCreation,
                    dateCheck.lastOpenTime => oldDateLastOpen,
                    _ => oldDateLastChange
                };

                if (files != null)
                {
                    if (hashGroups != null && hashGroups.TryGetValue(item.hash, out List<filesInfo> hashStejny))
                    {
                        foreach (var item2 in hashStejny)
                        {

                            if (item.fileName != item2.fileName)
                            {
                                continue;
                            }
                            DateTime porovnavaneDatum2 = settings.jakaOperaceData switch
                            {
                                dateCheck.changeTime => item2.dateOfLastChange,
                                dateCheck.createTime => item2.dateOfCreation,
                                dateCheck.lastOpenTime => item2.dateOfLastOpen,
                                _ => item2.dateOfLastChange
                            };
                            switch (settings.dateDetectionDup)
                            {
                                case true when porovnavaneDatum2 > porovnavaneDatum:
                                    item2.operace = op.novejsiStejnyObsah;
                                    break;
                                case true when porovnavaneDatum2 < porovnavaneDatum:
                                    item2.operace = op.starsiStejnyObsah;
                                    break;
                                default:
                                    item2.operace = op.sameHash;
                                    break;
                            }
                            item2.copyDes = item.cesta;





                        }
                        coDelam = op.sameHash;
                    }
                    else if (fileJmenoGroups != null && fileJmenoGroups.TryGetValue(item.fileName, out List<filesInfo> jmenoStejny))
                    {
                        foreach (var item1 in jmenoStejny)
                        {

                            if (item.hash != item1.hash)
                            {
                                continue;
                            }
                            DateTime porovnavaneDatum2 = settings.jakaOperaceData switch
                            {
                                dateCheck.changeTime => item1.dateOfLastChange,
                                dateCheck.createTime => item1.dateOfCreation,
                                dateCheck.lastOpenTime => item1.dateOfLastOpen,
                                _ => item1.dateOfLastChange
                            };


                            switch (settings.dateDetectionDup)
                            {
                                case true when porovnavaneDatum2 > porovnavaneDatum:
                                    item1.operace = op.novejsiOdlisny;
                                    break;
                                case true when porovnavaneDatum2 < porovnavaneDatum:
                                    item1.operace = op.starsiOdlisny;
                                    break;
                                default:
                                    item1.operace = op.existujiVelikostJina;
                                    break;
                            }

                            item1.copyDes = item.cesta;



                        }

                    }

                }
            }
          
            
          
            foreach (var item in files.Where(x => x.operace == op.existuji))
            {
                item.operace = op.nexistuji;
            }
           
            


        }








        static public void checkFilesExisOld(List<filesInfo> files, string folder, List<filesInfo> filesSource = null)
        {
            lock (zamek)
            {
                Dictionary<string, List<filesInfo>> hashGroups = null;
                Dictionary<string, List<filesInfo>> fileJmenoGroups = null;

                if (filesSource != null)
                {
                    hashGroups = filesSource.GroupBy(x => x.hash).ToDictionary(g => g.Key, g => g.ToList());
                    fileJmenoGroups = filesSource.GroupBy(x => x.fileName).ToDictionary(g => g.Key, g => g.ToList());
                }

                foreach (var item in Directory.GetFiles(folder))
                {
                    string hashPomoc = hashing.getHash(item);
                    string jmenoPomoc = Path.GetFileName(item);
                    op coDelam = op.neznama;

                    if (filesSource != null)
                    {
                        if (hashGroups != null && hashGroups.TryGetValue(hashPomoc, out List<filesInfo> hashStejny))
                        {
                            foreach (var item2 in hashStejny)
                            {
                                item2.operace = op.sameHash;
                            }
                            coDelam = op.sameHash;
                        }
                        else if (fileJmenoGroups != null && fileJmenoGroups.TryGetValue(jmenoPomoc, out List<filesInfo> jmenoStejny))
                        {
                            foreach (var item1 in jmenoStejny)
                            {
                                item1.operace = op.existujiVelikostJina;
                            }
                            coDelam = op.existujiVelikostJina;
                        }
                        else
                        {
                            coDelam = op.nexistuji;
                        }
                    }


                    lock (files)
                    {
                        files.Add(new filesInfo
                        {
                            fileName = jmenoPomoc,
                            dateOfLastChange = File.GetLastWriteTime(item),
                            velikost = new FileInfo(item).Length,
                            hash = hashPomoc,
                            operace = coDelam,
                            cesta = item
                        });
                    }
                }
            }
        }


        static public async Task checkFilesExisOld(List<filesInfo> files, string folder, List<filesInfo> filesSource = null, bool source = false)
        {
            foreach (var item in Directory.GetFiles(folder))
            {
                string hashPomoc = hashing.getHash(item);

                op coDelam = op.neznama;

                if (filesSource != null)
                {
                    List<filesInfo> InfoTemp = filesSource.Select(file => file.Clone()).ToList();
                    List<filesInfo> fi1 = InfoTemp.FindAll(x => x.hash == hashPomoc);

                    if (fi1.Any())
                    {
                        foreach (var item2 in fi1)
                        {
                            item2.operace = op.sameHash;
                            coDelam = op.sameHash;
                            InfoTemp.Remove(item2);
                        }
                    }
                    else
                    {
                        fi1 = InfoTemp.FindAll(x => x.fileName == Path.GetFileName(item) && x.hash != hashPomoc);
                        foreach (var item1 in fi1)
                        {
                            item1.operace = op.existujiVelikostJina;
                            coDelam = op.existujiVelikostJina;
                            InfoTemp.Remove(item1);
                        }
                    }
                }

                files.Add(new filesInfo
                {

                    fileName = Path.GetFileName(item),
                    dateOfLastChange = File.GetLastWriteTime(item),
                    velikost = new FileInfo(item).Length,
                    hash = hashPomoc,
                    operace = coDelam,
                    cesta = item
                });
            }
        }

        static public void checkFiles(string folder, bool source, List<filesInfo> files)
        {

            foreach (var item in Directory.GetFiles(folder))
            {

                if (source)
                {
                    files.Add(new filesInfo
                    {
                        fileName = Path.GetFileName(item),
                        dateOfLastChange = File.GetLastWriteTime(item),
                        dateOfCreation = File.GetCreationTime(item),
                        dateOfLastOpen = File.GetLastAccessTime(item),
                        velikost = new FileInfo(item).Length

                    });

                    continue;

                }

                if (!sourceInfo.Any(x => x.fileName == Path.GetFileName(item)))
                {
                    files.Add(new filesInfo
                    {
                        fileName = Path.GetFileName(item),
                        dateOfLastChange = File.GetLastWriteTime(item),
                        operace = op.smazat,
                        velikost = new FileInfo(item).Length

                    });
                    folders.pocetZmen++;
                    continue;
                }

                files.Add(new filesInfo
                {
                    fileName = Path.GetFileName(item),
                    dateOfLastChange = File.GetLastWriteTime(item),
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
                    File.Delete(settings.destinacionFolder + "\\" + item.fileName);
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
                        File.Copy(settings.sourseFolder + "\\" + item.fileName, item.copyDes + "\\" + item.fileName);
                        File.SetLastWriteTime(item.copyDes + "\\" + item.fileName, item.dateOfLastChange);
                    }
                    else if (item.operace == op.prepsat)
                    {
                        zvetsitHodnotu(pb1);
                        File.Copy(settings.sourseFolder + "\\" + item.fileName, item.copyDes + "\\" + item.fileName, true);
                        File.SetLastWriteTime(item.copyDes + "\\" + item.fileName, item.dateOfLastChange);
                    }
                }
                catch (Exception err)
                {
                    Thread.Sleep(1000);
                    if (chyba >= 15)
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
                sourceInfo.ForEach(item => { item.copy = true; item.copyDes = settings.destinacionFolder; item.operace = op.existuji; });
                return;

            }
            foreach (var (item, index) in sourceInfo.Select((value, i) => (value, i)))
            {
                if (!desInfo.Any(a => a.fileName == item.fileName))
                {
                    sourceInfo[index].copy = true;
                    sourceInfo[index].operace = op.existuji;
                    sourceInfo[index].copyDes = settings.destinacionFolder;
                    folders.pocetZmen++;

                }
                else
                {
                    if (desInfo.Find(x => x.fileName == item.fileName).velikost != item.velikost)
                    {

                        sourceInfo[index].operace = op.prepsat;
                        sourceInfo[index].copyDes = settings.destinacionFolder;
                        folders.pocetZmen++;
                    }
                }

            }



        }




    }
}
