using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PartPickerManager manager;
        private WebController webController;

        public MainWindow()
        {
            InitializeComponent();

            manager = new PartPickerManager(new XMLComponentStorage("PartPickerDatabase.ppd", true));
            webController = new WebController(Browser, manager);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            webController.InitializeBrowser();
        }

        private void NewBuild_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to start a new build? All unsaved changes will be lost.", "New Build", System.Windows.MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            manager.CurrentBuild = new Build();
            webController.InitializeBrowser();
        }

        private void LoadBuild_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Part Picker Build Files|*.ppb";

            if (openDialog.ShowDialog() == true)
            {
                manager.CurrentBuild = new Build(openDialog.FileName);
                webController.InitializeBrowser();
            }
        }

        private void SaveBuild_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Part Picker Build Files|*.ppb";
            saveDialog.DefaultExt = "ppb";
            saveDialog.AddExtension = true;

            if (saveDialog.ShowDialog() == true)
                manager.CurrentBuild.Save(saveDialog.FileName);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            webController.DetectUpdateSize();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var search = new SearchWindow(manager);
            search.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            webController.Print();
        }

    }
}
