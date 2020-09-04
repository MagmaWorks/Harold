using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageReader;

namespace Harold
{
    /// <summary>
    /// Interaction logic for UCSettings.xaml
    /// </summary>
    public partial class UCSettings : UserControl
    {
        public UCSettings()
        {
            InitializeComponent();
        }

        private void localCB_Clicked(object sender, RoutedEventArgs e)
        {
            ViewModel vm = (this.DataContext as ViewModel);
            if(vm.PythonPathOrigin == PythonOrigin.server)
            {
                localCB.IsChecked = true;
                serverCB.IsChecked = false;

                vm.PythonPathOrigin = PythonOrigin.local;
            }
            else
            {
                localCB.IsChecked = true;
            }
        }

        private void serverCB_Clicked(object sender, RoutedEventArgs e)
        {
            ViewModel vm = (this.DataContext as ViewModel);
            if (vm.PythonPathOrigin == PythonOrigin.local)
            {
                localCB.IsChecked = false;
                serverCB.IsChecked = true;

                vm.PythonPathOrigin = PythonOrigin.server;
            }
            else
            {
                serverCB.IsChecked = true;
            }
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Close();
        }

        //private void IPAddressChanged(object sender, RoutedEventArgs e)
        //{
        //    ViewModel vm = (this.DataContext as ViewModel);
        //    vm.p2pConverter.IPAddress = (sender as TextBlock).Text;
        //}
    }
}
