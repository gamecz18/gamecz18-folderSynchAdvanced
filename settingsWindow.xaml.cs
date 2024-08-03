using System;
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
            if (folders.timeToSynch == null)
            {
                time_to_Synch.SelectedIndex = 14;
            }
            else
            {
                time_to_Synch.SelectedIndex = (folders.timeToSynch/ 60000) - 1;

            }
            if (!string.IsNullOrEmpty(folders.jmenoInstance))
            {
                inputTextBox.Text = folders.jmenoInstance;
            }
            if (folders.bootOnStartup != null)
            {
                bootChechBox.IsChecked = folders.bootOnStartup;
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
            folders.timeToSynch = int.Parse(time_to_Synch.SelectedValue.ToString()) * 60000;
            if (!string.IsNullOrEmpty(inputTextBox.Text))
            {
                folders.jmenoInstance = inputTextBox.Text;
            }
            if (bootChechBox.IsChecked.Value)
            {
                folders.bootOnStartup = true;
                folders.creteBoot();
            }
            else
            {
                folders.bootOnStartup = false;
            }



        }

        private void bootChechBox_Checked(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(folders.destinacionFolder) || string.IsNullOrEmpty(folders.sourseFolder))
            {
                System.Windows.Forms.MessageBox.Show("One folder or more folders are not selected.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                bootChechBox.IsChecked = false;

            }
        }
    }
}
