using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
        public MainWindow()
        {
            InitializeComponent();
            
            Instance = this;
            folders.loadSettings();
            synchBox.IsEnabled = false;
            //slouží pro pracovaní s tímto oknem u jiných knihove(UI prvky)
          
            //notifikační ikona
            _nf = new Forms.NotifyIcon();
            _nf.Icon = new System.Drawing.Icon("images/icon.ico");
            _nf.Text = "Folder Synch APP";
            _nf.ContextMenuStrip = new Forms.ContextMenuStrip();
            if (!string.IsNullOrEmpty(folders.jmenoInstance))
            {
                _nf.ContextMenuStrip.Items.Add(folders.jmenoInstance);
            }

            _nf.ContextMenuStrip.Items.Add("Stop", null, NotifyIcon_Click);
            checkBoxSynchFolders.IsChecked = folders.synchAllFoldes;
            if (folders.bootOnStartup)
            {
                synchOnBack();
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
            if (folderWork.selecFolder(ref folders.sourseFolder))
            {
                return;
            }
            int pocet = 0;
            sourceFilesView.Items.Clear();
            if (folders.sourseFolder == null) return;
            await Task.Run(() =>
            {

                foreach (var item in Directory.GetFiles(folders.sourseFolder))
                {
                    pocet++;

                    this.sourceFilesView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        () => { sourceFilesView.Items.Add(Path.GetFileName(item)); });
                }
            });
            sourceCount.Content = $"Počet s.: {pocet}";
            sourcePath.Content = $"Cesta: : {folders.sourseFolder}";
        }

        private async void buttonDes_Click(object sender, RoutedEventArgs e)
        {

            if (folderWork.selecFolder(ref folders.destinacionFolder))
            {

                return;
            }
            int pocet = 0;
            desctiFilesView.Items.Clear();
            if (folders.destinacionFolder == null) return;
            await Task.Run(() =>
            {

                foreach (var item in Directory.GetFiles(folders.destinacionFolder))
                {
                    pocet++;

                    this.desctiFilesView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        () => { desctiFilesView.Items.Add(Path.GetFileName(item)); });

                }
            });
            desCount.Content = $"Počet s.: {pocet}";
            desPath.Content = $"Cesta: : {folders.destinacionFolder}";

        }


        private void synchButton_Click(object sender, RoutedEventArgs e)
        {


            sych();


        }

        async void sych()
        {

            if (string.IsNullOrEmpty(folders.destinacionFolder) || string.IsNullOrEmpty(folders.sourseFolder))
            {
                System.Windows.Forms.MessageBox.Show("One folder or more folders are not selected.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }


            disEnabElement(false);
            synchBox.IsEnabled = true;
            synchlabel.Foreground = new SolidColorBrush(Colors.Red);
            
            if (folders.synchAllFoldes)
            {

                searchFolder.listFoldersSource.Clear();
                searchFolder.listFoldersDes.Clear();
                //slouží pro načtení všech des a source složek
                Task T1 = new Task(() =>
                {

                    searchFolder.findFolders(folders.sourseFolder, searchFolder.listFoldersSource, folders.sourseFolder);

                });
                Task T2 = new Task(() =>
                {

                    searchFolder.findFolders(folders.destinacionFolder, searchFolder.listFoldersDes, folders.destinacionFolder);

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
                string baseSourceFolder = folders.sourseFolder;
                string baseDesFolder = folders.destinacionFolder;
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
                        catch(Exception err )
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
                            

                        }
                        folders.destinacionFolder = baseDesFolder + item.cestaInside;
                        DirectoryInfo df2 = new DirectoryInfo(baseDesFolder + item.cestaInside);
                        df2.CreationTime = new DirectoryInfo(item.cesta).CreationTime;
                        df2.LastWriteTime = new DirectoryInfo(item.cesta).LastWriteTime;
                        this.desPath.Dispatcher.Invoke(() =>
                        {
                            desPath.Content = folders.destinacionFolder;
                        }, System.Windows.Threading.DispatcherPriority.Normal);
                            folders.sourseFolder = item.cesta;
                        Thread.Sleep(250);


                        try
                        {
                            folders.pocetZmen = 0;
                            synch.checkFiles(folders.sourseFolder, true, synch.sourceInfo);
                            synch.checkFiles(folders.destinacionFolder, false, synch.desInfo);
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
                synchlabel.Foreground = new SolidColorBrush(Colors.Gray);
                synchBox.IsEnabled = false;
                disEnabElement(true);
            }
            else
            {


                await Task.Run(async () =>
                {

                    try
                    {
                        folders.pocetZmen = 0;
                        synch.checkFiles(folders.sourseFolder, true, synch.sourceInfo);
                        synch.checkFiles(folders.destinacionFolder, false, synch.desInfo);
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

            if (string.IsNullOrEmpty(folders.destinacionFolder)) return;
            this.desctiFilesView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                      () =>
                      {
                          desctiFilesView.Items.Clear();
                      });
            int pocet = 0;
            await Task.Run(() =>
            {

                foreach (var item in Directory.GetFiles(folders.destinacionFolder))
                {
                    pocet++;

                    this.desctiFilesView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        () => { desctiFilesView.Items.Add(Path.GetFileName(item)); });

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

            if (string.IsNullOrEmpty(folders.destinacionFolder) || string.IsNullOrEmpty(folders.sourseFolder))
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

                    Thread.Sleep(folders.timeToSynch);
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
            if (string.IsNullOrEmpty(folders.sourseFolder))
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

                    foreach (var item in Directory.GetFiles(folders.sourseFolder))
                    {
                        pocet++;

                        this.sourceFilesView.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                            () => { sourceFilesView.Items.Add(Path.GetFileName(item)); });
                    }
                });
                sourceCount.Content = $"Počet s.: {pocet}";
                sourcePath.Content = $"Cesta: : {folders.sourseFolder}";

            }
            if (string.IsNullOrEmpty(folders.destinacionFolder))
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
            folders.synchAllFoldes = checkBoxSynchFolders.IsChecked.Value;
        }
    }


}
