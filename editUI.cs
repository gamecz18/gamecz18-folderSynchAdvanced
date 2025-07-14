using IWshRuntimeLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace folderSynch
{




    class ComboBoxHelper
    {
        class ComboBoxFilterItem
        {

            public string Text { get; set; }
            public string Tag { get; set; }
            public int Count { get; set; }
            public bool IsEnabled { get; set; } = true;
        }


        public void InitializeFilterComboBox(ComboBox comboBoxInfomace)
        {
            var filterItems = new List<ComboBoxFilterItem>
    {
        new ComboBoxFilterItem { Text = "📁 Existují", Tag = "existuji", Count = 0 },
        new ComboBoxFilterItem { Text = "⚠️ Existují – jiná velikost", Tag = "existujiVelikostJina", Count = 0 },
        new ComboBoxFilterItem { Text = "❌ Neexistují", Tag = "nexistuji", Count = 0 },
        new ComboBoxFilterItem { Text = "✅ Stejný hash", Tag = "sameHash", Count = 0 },
        new ComboBoxFilterItem { Text = "❓ Neznámé", Tag = "neznama", Count = 0 }
    };

            comboBoxInfomace.ItemsSource = filterItems;
            comboBoxInfomace.DisplayMemberPath = "Text";
            comboBoxInfomace.SelectedValuePath = "Tag";
            comboBoxInfomace.SelectedIndex = 0;
        }



    }

    static class MessageBoxHelper
    {
        public static MessageBoxResult chciSmazatSouboryMessageBox()
        {


            return MessageBox.Show("Přejete si smazat soubory z disku?", "Varování", MessageBoxButton.OKCancel, MessageBoxImage.Question);


        }
    
    
    
    }




    static class ListViewHelper
    {

        public static void smazatStejne(this ListView listView, bool mazatSoubory = false)
        {
          
            if (mazatSoubory  )
            {
                if (MessageBoxHelper.chciSmazatSouboryMessageBox() == MessageBoxResult.OK)
                {
                    folderWork.smazaniSouboruZDisku(synch.sourceInfo.FindAll(x => x.operace == op.sameHash));
                }
                else
                {
                    return;
                }
           
                
            }
            synch.deleteSameFiles(synch.sourceInfo);

            listView.Items.Clear();
            pridaniDoList.addToList(ref listView, synch.sourceInfo);


        }
        public static void smazatZListView(this ListView listView, bool mazatSoubory = false)
        {
            List<pridaniDoList.listItemyPridat> itemyvList = new List<pridaniDoList.listItemyPridat>();
            foreach (pridaniDoList.listItemyPridat item in listView.Items)
            {
                if (item.IsChecked)
                {
                    itemyvList.Add(item);



                }

            }

            
            if (mazatSoubory)
            {
                if (MessageBoxHelper.chciSmazatSouboryMessageBox() == MessageBoxResult.OK)
                {
                    folderWork.smazaniSouboruZDisku(synch.sourceInfo.Where(x => x.vybran).ToList());
                }
                else
                {
                    return;

                }
              
            }
            synch.sourceInfo.RemoveAll(x => x.vybran);

            foreach (var item in itemyvList)
            {
                listView.Items.Remove(item);
            }
        }


    }


}
