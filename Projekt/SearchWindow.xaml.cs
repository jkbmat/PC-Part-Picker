using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Projekt
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        public Component SelectedItem { get; private set; }
        public bool Result { get; private set; }

        private PartPickerManager Manager;
        private Timer FinishedTypingTimer;
        private IEnumerable<IGrouping<string, Component>> SearchResults;

        public SearchWindow(PartPickerManager manager)
        {
            Result = false;

            Manager = manager;
            FinishedTypingTimer = new Timer(500);
            FinishedTypingTimer.Elapsed += new ElapsedEventHandler((x, y) => { FinishedTypingTimer.Stop(); Dispatcher.Invoke(DoSearch); });

            InitializeComponent();

            Type.Items.Add("---");
            InConnector.Items.Add("---");
            Type.SelectedIndex = 0;
            InConnector.SelectedIndex = 0;

            foreach (var type in Manager.ComponentStorage.GetComponentTypes())
            {
                Type.Items.Add(type.ToString());
            }

            foreach (var connector in Manager.ComponentStorage.GetInConnectors())
            {
                InConnector.Items.Add(connector);
            }

            AddButton.Visibility = System.Windows.Visibility.Collapsed;
        }

        public SearchWindow(bool enableAdd, PartPickerManager manager, string search, string inConnector, string type) : this(manager)
        {
            if (search != null)
            {
                Search.Text = search;
                Search.IsEnabled = false;
            }

            if (inConnector != null)
            {
                if(!InConnector.Items.Contains(inConnector))
                    InConnector.Items.Add(inConnector);

                InConnector.SelectedItem = inConnector;
                InConnector.IsEnabled = false;
            }

            if (type != null)
            {
                Type.SelectedItem = type;
                Type.IsEnabled = false;
            }

            if (enableAdd)
                AddButton.Visibility = System.Windows.Visibility.Visible;
        }



        private void SearchWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            InConnector.SelectionChanged += QueryChanged;
            Type.SelectionChanged += QueryChanged;

            DoSearch();
        }

        private void QueryChanged(object sender, SelectionChangedEventArgs e)
        {
            DoSearch();
        }

        private void DoSearch()
        {
            SearchResults = Manager.ComponentStorage
                .Query
                (
                    Search.Text,
                    InConnector.SelectedItem.Equals("---") ? "" : InConnector.SelectedItem.ToString(),
                    Type.SelectedItem.Equals("---") ? "" : Type.SelectedItem.ToString()
                ).GroupBy(component => component.Type).ToList();

            SearchTabs.Items.Clear();
            foreach (var type in SearchResults)
            {
                var tab = new TabItem();
                tab.Header = type.Key;
                
                SearchTabs.Items.Add(tab);
            }

            SearchTabs.SelectedIndex = 0;
            SelectionButtons(null, null);
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            FinishedTypingTimer.Stop();
            FinishedTypingTimer.Start();
        }

        private void SearchTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source.GetType() != typeof(TabControl))
                return;

            CreateTabContent();
            SelectionButtons(null, null);
        }

        private void CreateTabContent()
        {
            TabItem selectedTab = SearchTabs.SelectedItem as TabItem;

            if (selectedTab == null)
            {
                return;
            }

            var results = SearchResults
                .First
                (
                    type =>
                        type.Key.Equals(selectedTab.Header)
                );
            
            var dataGrid = new DataGrid();
            dataGrid.IsReadOnly = true;
            dataGrid.HorizontalGridLinesBrush = Brushes.LightGray;
            dataGrid.VerticalGridLinesBrush = Brushes.LightGray;
            dataGrid.RowHeaderWidth = 0;
            
            var style = new System.Windows.Style(typeof(DataGridCell));
            style.Setters.Add(new Setter(DataGrid.BorderThicknessProperty, new Thickness(0)));
            dataGrid.CellStyle = style;

            dataGrid.SelectedCellsChanged += new SelectedCellsChangedEventHandler(SelectionButtons);
            

            DataGridTextColumn col = new DataGridTextColumn();
            col.Header = "Manufacturer";
            col.Binding = new Binding("Manufacturer");
            dataGrid.Columns.Add(col);

            col = new DataGridTextColumn();
            col.Header = "Model";
            col.Binding = new Binding("Model");
            dataGrid.Columns.Add(col);


            if (selectedTab.Header.ToString() != "Power Source")
            {
                col = new DataGridTextColumn();
                col.Header = "Connector";
                col.Binding = new Binding("InConnector");
                dataGrid.Columns.Add(col);
            }

            col = new DataGridTextColumn();
            col.Header = "Energy Usage";
            col.Binding = new Binding("EnergyConsumption");
            dataGrid.Columns.Add(col);

            col = new DataGridTextColumn();
            col.Header = "Description";
            col.Binding = new Binding("Description");
            dataGrid.Columns.Add(col);

            foreach (var property in Manager.ComponentStorage.GetTypeByName(selectedTab.Header.ToString()).Properties)
            {
                col = new DataGridTextColumn();
                col.Header = property.Name;
                col.Binding = new Binding("["+property.Name+"]");
                
                dataGrid.Columns.Add(col);
            }

            foreach (var component in results)
            {
                dataGrid.Items.Add(component);
            }

            selectedTab.Content = dataGrid;
        }

        private void SelectionButtons(object sender, EventArgs e)
        {
            SelectedItem = ((DataGrid)SearchTabs.SelectedContent) == null ? null : ((DataGrid)SearchTabs.SelectedContent).SelectedItem as Component;

            bool setTo = SearchTabs.SelectedContent != null && SelectedItem != null;

            AddButton.IsEnabled = setTo;
            EditButton.IsEnabled = setTo;
            RemoveButton.IsEnabled = setTo;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Result = true;
            Close();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Component selected = ((DataGrid)SearchTabs.SelectedContent).SelectedItem as Component;

            MessageBoxResult messageBoxResult = System.Windows.MessageBox
                .Show
                (
                    "Are you sure you want to remove "+selected.Manufacturer+" "+selected.Model+"?",
                    "Delete Confirmation",
                    System.Windows.MessageBoxButton.YesNo
                );

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Manager.ComponentStorage.RemoveComponent(selected);
                ((DataGrid)SearchTabs.SelectedContent).Items.Remove(selected);

                if (((DataGrid)SearchTabs.SelectedContent).Items.Count == 0)
                    SearchTabs.Items.RemoveAt(SearchTabs.SelectedIndex);
            }

        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddComponentWindow(Manager);
            addWindow.ShowDialog();

            if (addWindow.NewComponent != null)
                Manager.ComponentStorage.AddComponent(addWindow.NewComponent);

            DoSearch();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Component selected = ((DataGrid)SearchTabs.SelectedContent).SelectedItem as Component;

            var addWindow = new AddComponentWindow(Manager, true);

            addWindow.Title = "Edit Component...";
            addWindow.ButtonAdd.Content = "Edit Component";

            addWindow.TextBoxManufacturer.Text = selected.Manufacturer;
            addWindow.TextBoxModel.Text = selected.Model;
            addWindow.TextBoxDescription.Text = selected.Description;
            addWindow.TextBoxEnergy.Text = Math.Abs(selected.EnergyConsumption).ToString();
            addWindow.InConnector.SelectedItem = selected.InConnector == "null" ? addWindow.NoneConnector : selected.InConnector;

            foreach (ComponentType type in addWindow.ComboBoxTypes.Items)
            {
                if (type.Name.Equals(selected.Type))
                {
                    addWindow.ComboBoxTypes.SelectedItem = type;
                }
            }

            foreach (var outConnector in selected.OutConnectors)
            {
                addWindow.ButtonAddNewOutConnector_Click(null, null);
                addWindow.OutConnectors.Last().Text = outConnector;
            }

            foreach (var property in selected.Properties)
            {
                addWindow.Properties[property.Name].Text = property.Value.ToString();
            }

            addWindow.ShowDialog();

            if (addWindow.NewComponent != null)
            {
                Manager.ComponentStorage.RemoveComponent(selected);
                Manager.ComponentStorage.AddComponent(addWindow.NewComponent);
                var index = SearchTabs.SelectedIndex;
                DoSearch();
                SearchTabs.SelectedIndex = index;
            }
        }
    }
}
