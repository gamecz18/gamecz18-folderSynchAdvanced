using System;
using System.IO;
using System.Windows;

namespace folderSynch
{
    /// <summary>
    /// Interakční logika pro settingsWindow.xaml
    /// </summary>
    public partial class settingsWindow : Window
    {
        public settingsWindow()
        {
            InitializeComponent();

            for (int i = 1; i != 16; i++)
            {
                time_to_Synch.Items.Add(i).ToString();

            }
            if (settings.timeToSynch == null)
            {
                time_to_Synch.SelectedIndex = 14;
            }
            else
            {
                time_to_Synch.SelectedIndex = (settings.timeToSynch / 60000) - 1;

            }
            if (!string.IsNullOrEmpty(settings.jmenoInstance))
            {
                inputTextBox.Text = settings.jmenoInstance;
            }
            if (settings.bootOnStartup != null)
            {
                bootChechBox.IsChecked = settings.bootOnStartup;
            }

            if (settings.bootOnStartup != null)
            {
                foldersSynch.IsChecked = settings.synchAllFoldes;
            }
          
           
            
           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            saveSeting();
            folders.saveSettings();
            DialogResult = true;
            this.Close();
        }


        void saveSeting()
        {
            settings.timeToSynch = int.Parse(time_to_Synch.SelectedValue.ToString()) * 60000;
            //jmeno instance v liste
            if (!string.IsNullOrEmpty(inputTextBox.Text))
            {
                settings.jmenoInstance = inputTextBox.Text;
            }
            if (foldersSynch.IsChecked.Value)
            {
                settings.synchAllFoldes = true;
            }
            else
            {
                settings.synchAllFoldes = false;
            }


            //
            if (bootChechBox.IsChecked.Value)
            {
                settings.bootOnStartup = true;
                folders.creteBoot();
            }
            else
            {
                settings.bootOnStartup = false;
            }
            
          



        }

        private void bootChechBox_Checked(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(settings.destinacionFolder) || string.IsNullOrEmpty(settings.sourseFolder))
            {
                System.Windows.Forms.MessageBox.Show("One folder or more folders are not selected.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                bootChechBox.IsChecked = false;
                
            }
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(settings.jsemCesta + "\\" + "setting.txt");
            DialogResult = true;
            this.Close();
        }
    }
}
