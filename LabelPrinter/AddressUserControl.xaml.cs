using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace LabelPrinter
{
    /// <summary>
    /// Interaction logic for AddressUserControl.xaml
    /// </summary>
    public partial class AddressUserControl : UserControl
    {
        public AddressUserControl()
        {
            InitializeComponent();
        }

        private void SaveAddress_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            if (b != null)
            {
                MainWindow mainWindow = b.CommandParameter as MainWindow;

                if (mainWindow != null)
                {
                    MainViewModel mainVM = mainWindow.DataContext as MainViewModel;

                    if (mainVM != null)
                    {
                        AddressViewModel avm = b.DataContext as AddressViewModel;

                        mainVM.AllAddresses.Add(new AddressSelectionViewModel { Address = avm.Address });

                        mainVM.SearchText = mainVM.SearchText;

                        XElement addresses = new XElement("Addresses",
                        mainVM.AllAddresses.Select(
                            a =>
                            new XElement("Address",
                                new XCData(a.Address)
                            )
                            ));
                        
                        addresses.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Addresses.xml"));



                    }
                }

            }

        }

        private void ClearAddress_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                AddressViewModel avm = b.DataContext as AddressViewModel;

                if (avm != null)
                {
                    avm.Address = string.Empty;
                }

            }
        }

        private void ButtonsGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            this.ButtonsGrid.Visibility = Visibility.Visible;
        }

        private void ButtonsGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            this.ButtonsGrid.Visibility = Visibility.Hidden;
        }
    }
}
