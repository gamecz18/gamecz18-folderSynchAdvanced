using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static folderSynch.pridaniDoList;
using Forms = System.Windows.Forms;
using Path = System.IO.Path;

namespace folderSynch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //slouží pro pracovaní s tímto oknem u jiných knihove(UI prvky)
        public static MainWindow Instance;
        //slouží pro vytvoření notifikační ikony
        private readonly Forms.NotifyIcon _nf;
        private string selectedRadioButton = "";
        private GridViewColumnHeader posleniHodnota = null;
        int jakaOperace = 0;
        public MainWindow()
        {
            InitializeComponent();






            Instance = this;
            ComboBoxHelper comboBoxHelper = new ComboBoxHelper();
            comboBoxHelper.InitializeFilterComboBox(comboBoxInfomace);
            folders.loadSettings();
            synchBox.IsEnabled = false;
            //slouží pro pracovaní s tímto oknem u jiných knihove(UI prvky)

            //notifikační ikona
            _nf = new Forms.NotifyIcon();
            _nf.Icon = new System.Drawing.Icon("images/icon.ico");
            _nf.Text = "Folder Synch APP";
            _nf.ContextMenuStrip = new Forms.ContextMenuStrip();
            if (!string.IsNullOrEmpty(settings.jmenoInstance))
            {
                _nf.ContextMenuStrip.Items.Add(settings.jmenoInstance);
            }

            _nf.ContextMenuStrip.Items.Add("Stop", null, NotifyIcon_Click);
            checkBoxSynchFolders.IsChecked = settings.synchAllFoldes;
            if (settings.bootOnStartup)
            {
                synchOnBack();
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (tabBasic.IsSelected)
            {
                sourceCount.Visibility = Visibility.Visible;
                sourcePath.Visibility = Visibility.Visible;
                stackPrava.Visibility = Visibility.Visible;
                saveSesButton.IsEnabled = true;
                loadSesButton.IsEnabled = true;
                synchOnBackButton.IsEnabled = true;
                synchButton.Content = "Synchronizovat";
                synchBox.Visibility = Visibility.Visible;
                prubeh.Visibility = Visibility.Visible;


            }
            else
            {
                synchButton.Content = "Hledat soubory";
                saveSesButton.IsEnabled = false;
                loadSesButton.IsEnabled = false;
                synchOnBackButton.IsEnabled = false;
                sourceCount.Visibility = Visibility.Hidden;
                sourcePath.Visibility = Visibility.Hidden;
                stackPrava.Visibility = Visibility.Hidden;
                synchBox.Visibility = Visibility.Hidden;
                prubeh.Visibility = Visibility.Hidden;

            }


        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
            cts1.Cancel();
            _nf.Visible = false;


        }

        public delegate void MethodInvoker();


        protected override void OnClosing(CancelEventArgs e)
        {
            _nf.Dispose();
            base.OnClosing(e);
        }




        private async void buttonSec_Click(object sender, RoutedEventArgs e)
        {

            if ((tab.SelectedItem as TabItem).Header.ToString() == "Hledání")
            {
                if (folderWork.selecFolder(ref settings.sourseFolder))
                {
                    return;
                }
                int pocet = 0;
                sourceFilesViewFind.Items.Clear();
                sourcePath2.Content = $"Cesta source: {settings.sourseFolder}";
                if (settings.sourseFolder == null) return;
                /*await Task.Run(() =>
                {

                    foreach (var item in Directory.GetFiles(settings.sourseFolder))
                    {
                        pocet++;
                        listItemyPridat lIP = new listItemyPridat
                        {
                            nazev = Path.GetFileName(item)
                        };
                        this.sourceFilesViewFind.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                            () => { sourceFilesViewFind.Items.Add(lIP); });
                    }
                });*/
            }
            else
            {

                if (folderWork.selecFolder(ref settings.sourseFolder))
                {
                    return;
                }
                int pocet = 0;
                sourceFilesView.Items.Clear();
                if (settings.sourseFolder == null) return;
                await Task.Run(() =>
                {

                    foreach (var item in Directory.GetFiles(settings.sourseFolder))
                    {
                        pocet++;
                        listItemyPridat lIP = new listItemyPridat
                        {
                            nazev = Path.GetFileName(item)
                        };
                        this.sourceFilesView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                            () => { sourceFilesView.Items.Add(lIP); });
                    }
                });
                sourceCount.Content = $"Počet s.: {pocet}";
                sourcePath.Content = $"Cesta: : {settings.sourseFolder}";
            }


        }

        private async void buttonDes_Click(object sender, RoutedEventArgs e)
        {

            if ((tab.SelectedItem as TabItem).Header.ToString() == "Hledání")
            {
                if (folderWork.selecFolder(ref settings.destinacionFolder))
                {

                    return;
                }
                int pocet = 0;
                desPath2.Content = $"Cesta des.: {settings.destinacionFolder}";
                //desctiFilesViewFind.Items.Clear();
                if (settings.destinacionFolder == null) return;
                await Task.Run(() =>
                {

                    /*foreach (var item in Directory.GetFiles(settings.destinacionFolder))
                    {
                        pocet++;
                        listItemyPridat lIP = new listItemyPridat
                        {
                            nazev = Path.GetFileName(item)
                        };

                        /* this.desctiFilesViewFind.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                             () => { desctiFilesViewFind.Items.Add(lIP); });

                    }*/
                });

            }
            else
            {

                if (folderWork.selecFolder(ref settings.destinacionFolder))
                {

                    return;
                }
                int pocet = 0;
                desctiFilesView.Items.Clear();
                if (settings.destinacionFolder == null) return;
                await Task.Run(() =>
                {

                    foreach (var item in Directory.GetFiles(settings.destinacionFolder))
                    {
                        pocet++;

                        listItemyPridat lIP = new listItemyPridat
                        {
                            nazev = Path.GetFileName(item)
                        };
                        this.desctiFilesView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                            () => { desctiFilesView.Items.Add(lIP); });

                    }
                });
                desCount.Content = $"Počet s.: {pocet}";
                desPath.Content = $"Cesta: : {settings.destinacionFolder}";

            }
        }


        private void synchButton_Click(object sender, RoutedEventArgs e)
        {

            if ((tab.SelectedItem as TabItem).Header.ToString() == "Hledání")
            {
                sourceFilesViewFind.Items.Clear();
                //desctiFilesViewFind.Items.Clear();
                disEnabElement(false);
                hledani();
            }
            else
            {
                sych();
            }



        }


        async void hledani()
        {

            synch.sourceInfo.Clear();
            synch.desInfo.Clear();


            if (string.IsNullOrEmpty(settings.destinacionFolder) || string.IsNullOrEmpty(settings.sourseFolder))
            {
                System.Windows.Forms.MessageBox.Show("One folder or more folders are not selected.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            searchFolder.listFoldersSource.Clear();
            searchFolder.listFoldersDes.Clear();
            //slouží pro načtení všech des a source složek
            Task T1 = new Task(() =>
            {

                searchFolder.findFolders(settings.sourseFolder, searchFolder.listFoldersSource, settings.sourseFolder, synch.sourceInfo);

            });
            Task T2 = new Task(() =>
            {

                searchFolder.findFolders(settings.destinacionFolder, searchFolder.listFoldersDes, settings.destinacionFolder, synch.desInfo);

            });
            T1.Start();
            T2.Start();
            //aby vše proběhlo synchroně

            await Task.Run(() =>
            {
                do
                {

                } while (T1.Status.Equals(TaskStatus.Running) || T2.Status.Equals(TaskStatus.Running));
            });

            

            



            Thread.Sleep(333);
            string baseSourceFolder = settings.sourseFolder;
            string baseDesFolder = settings.destinacionFolder;







            /* await Task.Run(() =>
             {
                 Parallel.ForEach(searchFolder.listFoldersDes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                 item =>
              {
                  //this.prubeh.Dispatcher.Invoke(() => { prubeh.Value = 0; }, System.Windows.Threading.DispatcherPriority.Normal);




                  try
                  {


                      synch.checkFilesExisOld(synch.desInfo, item.cesta);




                  }
                  catch (System.Exception err)
                  {

                      Forms.MessageBox.Show(err.Message, "Nastala chybu u synch", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Asterisk);
                      disEnabElement(true);
                  }

              });
             });


             await Task.Run(() =>
             { Parallel.ForEach(searchFolder.listFoldersSource, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },

              item =>
              {
                  try
                  {
                       synch.checkFilesExisOld(synch.sourceInfo, item.cesta, synch.desInfo);

                  }
                  catch (System.Exception err)
                  {

                      Forms.MessageBox.Show(err.Message, "Nastala chybu u synch", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Asterisk);
                      disEnabElement(true);
                  }
              });
             });*/



            synch.checkFilesExis(synch.sourceInfo, synch.desInfo);





            // pridaniDoList.addToList(ref desctiFilesViewFind, synch.desInfo);
            TreeViewSource.Items.Clear();
           // pridaniDoTree.pridaniDoTreeView(ref TreeViewSource, synch.sourceInfo);
            pridaniDoList.addToList(ref sourceFilesViewFind, synch.sourceInfo);
            synch.desInfo.Clear();
            AutoResizeColumns(sourceFilesViewFind);
            // AutoResizeColumns(desctiFilesViewFind);
            this.synchBox.Dispatcher.Invoke(() => { synchBox.IsEnabled = false; }, System.Windows.Threading.DispatcherPriority.Normal);
            this.synchlabel.Dispatcher.Invoke(() => { synchlabel.Foreground = new SolidColorBrush(Colors.Gray); }, System.Windows.Threading.DispatcherPriority.Normal);




            disEnabElement(true);
            folders.zjisiPocetSouborů(synch.sourceInfo);



        }


        

        private void AutoResizeColumns(ListView listView)
        {
            if (listView.View is GridView gridView)
            {
                foreach (var column in gridView.Columns)
                {

                    column.Width = 0;
                    column.Width = double.NaN;
                }
            }
        }

        async void sych()
        {

            if (string.IsNullOrEmpty(settings.destinacionFolder) || string.IsNullOrEmpty(settings.sourseFolder))
            {
                System.Windows.Forms.MessageBox.Show("One folder or more folders are not selected.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }


            disEnabElement(false);

            this.synchBox.Dispatcher.Invoke(() => { synchBox.IsEnabled = true; }, System.Windows.Threading.DispatcherPriority.Normal);
            this.synchlabel.Dispatcher.Invoke(() => { synchlabel.Foreground = new SolidColorBrush(Colors.Red); }, System.Windows.Threading.DispatcherPriority.Normal);



            if (settings.synchAllFoldes)
            {

                searchFolder.listFoldersSource.Clear();
                searchFolder.listFoldersDes.Clear();
                //slouží pro načtení všech des a source složek
                Task T1 = new Task(() =>
                {

                    searchFolder.findFolders(settings.sourseFolder, searchFolder.listFoldersSource, settings.sourseFolder);

                });
                Task T2 = new Task(() =>
                {

                    searchFolder.findFolders(settings.destinacionFolder, searchFolder.listFoldersDes, settings.destinacionFolder);

                });
                T1.Start();
                T2.Start();
                //aby vše proběhlo synchroně

                await Task.Run(() =>
                {
                    do
                    {

                    } while (T1.Status.Equals(TaskStatus.Running) || T2.Status.Equals(TaskStatus.Running));
                });


                Thread.Sleep(333);
                string baseSourceFolder = settings.sourseFolder;
                string baseDesFolder = settings.destinacionFolder;
                bool deleteTry = true;
                foreach (var item in searchFolder.listFoldersDes.Where(x => !searchFolder.listFoldersSource.Any(p => p.cestaInside.Contains(x.cestaInside))))
                {

                    if (Directory.Exists((baseDesFolder + item.cestaInside)))
                    {
                        try
                        {
                            DirectoryInfo df1 = new DirectoryInfo((baseDesFolder + item.cestaInside));
                            df1.Attributes = FileAttributes.Normal;
                            Directory.Delete((baseDesFolder + item.cestaInside), true);
                        }
                        catch (Exception err)
                        {
                            if (deleteTry)
                            {
                                MessageBox.Show(err.Message + " Folder: " + (baseDesFolder + item.cestaInside), "Error in deteting folders", MessageBoxButton.OK, MessageBoxImage.Stop);
                                deleteTry = !deleteTry;
                            }

                        }

                    }

                    //searchFolder.listFoldersDes.Remove(item);
                }
                await Task.Run(() =>
                {
                    foreach (var item in searchFolder.listFoldersSource)
                    {
                        this.prubeh.Dispatcher.Invoke(() => { prubeh.Value = 0; }, System.Windows.Threading.DispatcherPriority.Normal);



                        if (!Directory.Exists(baseDesFolder + item.cestaInside))
                        {
                            Directory.CreateDirectory(baseDesFolder + item.cestaInside);
                            DirectoryInfo df1 = new DirectoryInfo(baseDesFolder + item.cestaInside);
                            df1.Attributes = new DirectoryInfo(item.cesta).Attributes;


                        }// to-do opravit crash pri vybrani drive jednotky
                        try
                        {
                            settings.destinacionFolder = baseDesFolder + item.cestaInside;
                            DirectoryInfo df2 = new DirectoryInfo(baseDesFolder + item.cestaInside);
                            df2.CreationTime = new DirectoryInfo(item.cesta).CreationTime;
                            df2.LastWriteTime = new DirectoryInfo(item.cesta).LastWriteTime;
                        }
                        catch (Exception)
                        {


                        }

                        this.desPath.Dispatcher.Invoke(() =>
                        {
                            desPath.Content = settings.destinacionFolder;
                        }, System.Windows.Threading.DispatcherPriority.Normal);
                        settings.sourseFolder = item.cesta;
                        Thread.Sleep(250);


                        try
                        {
                            folders.pocetZmen = 0;
                            synch.checkFiles(settings.sourseFolder, true, synch.sourceInfo);
                            synch.checkFiles(settings.destinacionFolder, false, synch.desInfo);
                            //await Task.Run(() => synch.copyFiles(prubeh));
                            synch.copyFiles(prubeh);

                            /* JumpItem:
                                 if (t1.Status != TaskStatus.RanToCompletion)
                                 {
                                     goto JumpItem;
                                 }
                               */
                            // Thread.Sleep(550);


                        }
                        catch (System.Exception err)
                        {

                            Forms.MessageBox.Show(err.Message, "Nastala chybu u synch", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Asterisk);
                            disEnabElement(true);
                        }



                        /*skok:
                        if (t1.Status != TaskStatus.RanToCompletion)
                        {
                            goto skok;
                        }*/
                        //synch.desInfo.Clear();
                        //synch.sourceInfo.Clear();

                    }
                });

                this.synchBox.Dispatcher.Invoke(() => { synchBox.IsEnabled = false; }, System.Windows.Threading.DispatcherPriority.Normal);
                this.synchlabel.Dispatcher.Invoke(() => { synchlabel.Foreground = new SolidColorBrush(Colors.Gray); }, System.Windows.Threading.DispatcherPriority.Normal);




                disEnabElement(true);
            }
            else
            {


                await Task.Run(async () =>
                {

                    try
                    {
                        folders.pocetZmen = 0;
                        synch.checkFiles(settings.sourseFolder, true, synch.sourceInfo);
                        synch.checkFiles(settings.destinacionFolder, false, synch.desInfo);
                        await Task.Run(() => synch.copyFiles(prubeh));
                        disEnabElement(true);


                    }
                    catch (System.Exception err)
                    {

                        Forms.MessageBox.Show(err.Message, "Nastala chybu u synch", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Asterisk);
                        disEnabElement(true);
                    }

                });
            }





        }
        public void disEnabElement(bool operand)
        {
            this.synchButton.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
            {
                synchButton.IsEnabled = operand;
            });
            this.synchButton.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
            {
                buttonDes.IsEnabled = operand;
            });
            this.synchButton.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
            {
                buttonSec.IsEnabled = operand;
            });
            this.synchButton.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
            {
                synchOnBackButton.IsEnabled = operand;
            });




        }



        public async void reload()
        {

            if (string.IsNullOrEmpty(settings.destinacionFolder)) return;
            this.desctiFilesView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                      () =>
                      {
                          desctiFilesView.Items.Clear();
                      });
            int pocet = 0;
            await Task.Run(() =>
            {

                foreach (var item in Directory.GetFiles(settings.destinacionFolder))
                {
                    pocet++;
                    listItemyPridat lIP = new listItemyPridat
                    {
                        nazev = Path.GetFileName(item)
                    };
                    this.desctiFilesView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        () => { desctiFilesView.Items.Add(lIP); });

                }
            });
            this.desCount.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, () => { desCount.Content = $"Počet s.: {pocet}"; });


        }
        private void reloadButton_Click(object sender, RoutedEventArgs e)
        {
            //znovu načte listbox
            reload();

        }

        private void saveSesButton_Click(object sender, RoutedEventArgs e)
        {
            //uloží nastavení složek
            folders.saveSettings();
        }


        void createIcon()
        {

            //_nf.Click += NotifyIcon_Click;
            //zobrazí ikonu
            _nf.Visible = true;
            synchOnBackgourd();

            //this.Visibility = Visibility.Visible;
        }

        void cancleIcon()
        {
            _nf.Visible = false;
        }


        private void synchOnBackButton_Click(object sender, RoutedEventArgs e)
        {

            synchOnBack();
        }
        void synchOnBack()
        {

            if (string.IsNullOrEmpty(settings.destinacionFolder) || string.IsNullOrEmpty(settings.sourseFolder))
            {
                System.Windows.Forms.MessageBox.Show("One folder or more folders are not selected.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }
            this.Visibility = Visibility.Collapsed;
            createIcon();
        }


        CancellationToken ct1 = new CancellationToken();
        CancellationTokenSource cts1 = new CancellationTokenSource();
        void synchOnBackgourd()
        {
            Task.Run(async () =>
            {


                while (true)
                {
                    sych();

                    Thread.Sleep(settings.timeToSynch);
                    if (ct1.IsCancellationRequested)
                    {
                        ct1.ThrowIfCancellationRequested();
                    }

                }


            }, cts1.Token);



        }

        private async void loadSesButton_Click(object sender, RoutedEventArgs e)
        {
            folders.loadSettings();
            if (string.IsNullOrEmpty(settings.sourseFolder))
            {


                sourceCount.Content = $"Počet s. ";
                sourcePath.Content = $"Cesta:  ";


            }
            else
            {
                sourceFilesView.Items.Clear();
                int pocet = 0;
                await Task.Run(() =>
                {

                    foreach (var item in Directory.GetFiles(settings.sourseFolder))
                    {
                        pocet++;

                        this.sourceFilesView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                            () => { sourceFilesView.Items.Add(Path.GetFileName(item)); });
                    }
                });
                sourceCount.Content = $"Počet s.: {pocet}";
                sourcePath.Content = $"Cesta: : {settings.sourseFolder}";

            }
            if (string.IsNullOrEmpty(settings.destinacionFolder))
            {
                desctiFilesView.Items.Clear();
                desCount.Content = $"Počet s. ";
                desPath.Content = $"Cesta:  ";
            }
            else
            {
                reload();
            }





        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            settingsWindow st1 = new settingsWindow();

            if (st1.ShowDialog() == true)
            {
                MainWindow mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                this.Close();

            }
        }

        private void checkBoxSynchFolders_Checked(object sender, RoutedEventArgs e)
        {
            settings.synchAllFoldes = checkBoxSynchFolders.IsChecked.Value;
        }

        private void deleteSame_Click(object sender, RoutedEventArgs e)
        {
            sourceFilesViewFind.smazatStejne();
        }


        void GridViewColumnZmenaZareniClickedHandler(object sender, RoutedEventArgs e)
        {
            var kliknutiZapati = e.OriginalSource as GridViewColumnHeader;

            bool opak = false;
            if (kliknutiZapati != null)
            {
                // Ignorovat kliknutí na padding (prázdné místo)
                if (kliknutiZapati.Role != GridViewColumnHeaderRole.Padding)
                {
                    // Odstranit šipku z předchozího záhlaví
                    if (posleniHodnota != null && posleniHodnota != kliknutiZapati)
                    {
                        posleniHodnota.Column.HeaderTemplate = null;
                    }


                    // Přepnout šipku na aktuálním záhlaví
                    if (kliknutiZapati.Column.HeaderTemplate == Resources["HeaderTemplateArrowUp"] as DataTemplate)
                    {
                        kliknutiZapati.Column.HeaderTemplate = Resources["HeaderTemplateArrowDown"] as DataTemplate;
                        opak = true;
                    }
                    else
                    {
                        kliknutiZapati.Column.HeaderTemplate = Resources["HeaderTemplateArrowUp"] as DataTemplate;
                        opak = false;
                    }








                }

                sourceFilesViewFind.Items.Clear();

                switch (kliknutiZapati.Content)
                {
                    case "Nazev":
                        if (opak)
                        {
                            pridaniDoList.addToList(ref sourceFilesViewFind, synch.sourceInfo.OrderBy(x => x.fileName).ToList());
                        }
                        else
                        {
                            pridaniDoList.addToList(ref sourceFilesViewFind, synch.sourceInfo.OrderBy(x => x.fileName).Reverse().ToList());
                        }


                        break;

                    case "Operace":



                        pridaniDoList.addToList(ref sourceFilesViewFind, synch.sourceInfo.OrderBy(x => x.operace == synch.opraceNaVlozeni[jakaOperace] ? 0 : 1).ThenBy(x => x.operace).ToList());


                        break;
                    case "Cesta":
                        if (opak)
                        {
                            pridaniDoList.addToList(ref sourceFilesViewFind, synch.sourceInfo.OrderBy(x => x.cesta).ToList());
                        }
                        else
                        {
                            pridaniDoList.addToList(ref sourceFilesViewFind, synch.sourceInfo.OrderBy(x => x.cesta).Reverse().ToList());
                        }


                        break;
                    case "Vybrat":

                        if (opak)
                        {
                            pridaniDoList.addToList(ref sourceFilesViewFind, synch.sourceInfo.OrderBy(x => x.vybran).ToList());
                        }
                        else
                        {
                            pridaniDoList.addToList(ref sourceFilesViewFind, synch.sourceInfo.OrderBy(x => x.vybran).Reverse().ToList());
                        }


                        break;
                    default:

                        pridaniDoList.addToList(ref sourceFilesViewFind, synch.sourceInfo);
                        break;
                }



                jakaOperace++;
                if (synch.opraceNaVlozeni.Length <= jakaOperace)
                {
                    jakaOperace = 0;
                }
                posleniHodnota = kliknutiZapati;

            }
        }


        op zjistiOperaci(string tag)
        {
            switch (tag)
            {
                case "existuji":
                    return op.existuji;
                    break;
                case "existujiVelikostJina":
                    return op.existujiVelikostJina;
                    break;
                case "nexistuji":
                    return op.nexistuji;
                    break;
                case "sameHash":
                    return op.sameHash;
                    break;
                case "neznama":
                    return op.neznama;
                    break;
                default:
                    return 0;
                    break;


            }
        }


        private void selectButton_Click(object sender, RoutedEventArgs e)
        {

            op operace = zjistiOperaci(comboBoxInfomace.SelectedValue.ToString() ?? "neznama");

            foreach (pridaniDoList.listItemyPridat item in sourceFilesViewFind.Items)
            {

                var originalItem = synch.sourceInfo.FirstOrDefault(x => x.cesta == item.cesta && x.fileName == item.nazev);

                if (originalItem != null && originalItem.operace == operace)
                {
                    originalItem.vybran = true;
                    item.IsChecked = true;
                }
            }


            sourceFilesViewFind.Items.Refresh();



        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                selectedRadioButton = rb.Tag.ToString();

            }
        }

        private void unselectButton_Click(object sender, RoutedEventArgs e)
        {
            op operace = zjistiOperaci(comboBoxInfomace.SelectedValue.ToString() ?? "neznama");

            foreach (pridaniDoList.listItemyPridat item in sourceFilesViewFind.Items)
            {

                var originalItem = synch.sourceInfo.FirstOrDefault(x => x.cesta == item.cesta && x.fileName == item.nazev);

                if (originalItem != null && originalItem.operace == operace)
                {
                    originalItem.vybran = false;
                    item.IsChecked = false;
                }
            }
            sourceFilesViewFind.Items.Refresh();
        }

        private void deleteSelected_Click(object sender, RoutedEventArgs e)
        {



            sourceFilesViewFind.smazatZListView();

        }

        private void copySelected_Click(object sender, RoutedEventArgs e)
        {
            using (Forms.FolderBrowserDialog folderDialog = new Forms.FolderBrowserDialog())
            {
                folderDialog.Description = "Vyberte cílovou složku pro zkopírovaní souborů";
                folderDialog.ShowNewFolderButton = true;
                if (folderDialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    foreach (var item in synch.sourceInfo)
                    {//to-do zabezpečit kopirovani
                        File.Copy(item.cesta, folderDialog.SelectedPath + "\\" + item.fileName);


                    }


                }
            }
        }

        private void sourceFilesViewFind_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is pridaniDoList.listItemyPridat item)
            {

                var itemvListu = synch.sourceInfo.FirstOrDefault(x => x.cesta == item.cesta && x.fileName == item.nazev && x.hash == item.hash);


                if (itemvListu != null)
                {
                    itemvListu.vybran = item.IsChecked;
                }
            }
        }

        private void deleteSelectedFromDisk_Click(object sender, RoutedEventArgs e)
        {
            sourceFilesViewFind.smazatZListView(true);
        }

        private void deleteSameFromDisk_Click(object sender, RoutedEventArgs e)
        {
            sourceFilesViewFind.smazatStejne(true);
        }


        void test(TreeViewItem childItem, int pocet)
        {
            childItem.Header = "dsd";
            childItem.Items.Add("test");
            if (pocet >= 50)
            {
                return;
            }

            TreeViewItem dite = new TreeViewItem();
            dite.Header = "dite";
            dite.Items.Add("dite");
            childItem.Items.Add(dite);
            test(dite, pocet+=1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /* TreeViewItem rootItem = new TreeViewItem();
             rootItem.Header = "📁 Root";

             TreeViewItem childItem = new TreeViewItem();
             childItem.Header = "📄 file.txt";
             rootItem.Items.Add(childItem);
             childItem.Items.Add("sdsd");
             childItem.Items.Add("sdsd");
             test(childItem, 0);
             TreeViewSource.Items.Add(rootItem);*/

            pridaniDoTree.pridaniDoTreeView(ref TreeViewSource);
        }
    }
}




