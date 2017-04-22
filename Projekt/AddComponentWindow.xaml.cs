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
using System.Windows.Shapes;
using WPFTextBoxAutoComplete;

using Projekt.Validation;

namespace Projekt
{
    /// <summary>
    /// Interaction logic for AddComponentWindow.xaml
    /// </summary>
    public partial class AddComponentWindow : Window
    {
        public object NoneConnector { get; private set; }
        public List<ComboBox> OutConnectors { get; set; }
        public bool IsEdit { get; set; }

        public Dictionary<string, TextBox> Properties { get; set; }

        public Component NewComponent { get; set; }

        private PartPickerManager Manager { get; set; }


        private Grid TypeGrid;

        public AddComponentWindow(PartPickerManager manager, bool isEdit = false) : this()
        {
            Manager = manager;
            OutConnectors = new List<ComboBox>();

            Properties = new Dictionary<string, TextBox>();

            TypeGrid = new Grid();
            WindowGrid.Children.Add(TypeGrid);

            var cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            TypeGrid.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(1, GridUnitType.Star);
            TypeGrid.ColumnDefinitions.Add(cd);

            Grid.SetColumn(TypeGrid, 1);
            Grid.SetColumnSpan(TypeGrid, 2);
            Grid.SetRow(TypeGrid, 7);

            IsEdit = isEdit;

            TextBoxEnergy.PreviewTextInput += TextBoxValidation.NumericalTextBoxCheck;

            LoadTypes();

            InConnector.Items.Add("None (use this for the Power Source)");
            NoneConnector = InConnector.Items[InConnector.Items.Count - 1];

            FillConnectors(InConnector);
        }

        public AddComponentWindow()
        {
            InitializeComponent();
        }

        private void LoadTypes()
        {
            var types = Manager.ComponentStorage.GetComponentTypes();

            ComboBoxTypes.Items.Clear();

            foreach (var type in types)
            {
                ComboBoxTypes.Items.Add(type);
                if (type.Name == "Power Source")
                {
                    ComboBoxTypes.SelectedItem = type;
                }
            }

        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxEnergy.Text == "")
            {
                ShowError("Energy Consumption is required");
                return;
            }

            var energy = Math.Abs(Convert.ToInt64(TextBoxEnergy.Text));
            if (ComboBoxTypes.SelectedItem.ToString().Equals("Power Source"))
                energy = -energy;

            var props = Manager.ComponentStorage.GetTypeByName(ComboBoxTypes.SelectedItem.ToString()).Properties.ToList();
            for (int i = 0; i < props.Count(); i++)
            {
                props[i].Value = Properties[props[i].Name].Text;
                if (props[i].Type == typeof(double))
                    props[i].Value = Convert.ToDouble(props[i].Value);
            }

            var component = new Component()
            {
                Description = TextBoxDescription.Text,
                EnergyConsumption = energy,
                Manufacturer = TextBoxManufacturer.Text,
                Model = TextBoxModel.Text,
                Type = ComboBoxTypes.SelectedItem.ToString(),
                OutConnectors = OutConnectors.Where(con => con.Text != "").Select(con => con.Text),
                InConnector = InConnector.SelectedIndex == 0 ? "null" : InConnector.Text,
                Properties = props
            };

            if (component.Manufacturer == "")
            {
                ShowError("Manufacturer is required");
                return;
            }

            if (component.Model == "")
            {
                ShowError("Model is required");
                return;
            }

            if (component.InConnector == "")
            {
                ShowError("Input Connector is required");
                return;
            }

            if (!IsEdit && Manager.ComponentStorage.GetComponent(component.Manufacturer, component.Model) != null)
            {
                ShowError(component.Manufacturer + " " + component.Model + " already exists.");
                return;
            }

            NewComponent = component;
            Close();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(this, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Connector_GotFocus(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;
        }

        public void ButtonAddNewOutConnector_Click(object sender, RoutedEventArgs e)
        {
            var row = Grid.GetRow(ButtonAddNewOutConnector);

            var rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            GridOutConnectors.RowDefinitions.Insert(row, rd);

            Grid.SetRow(ButtonAddNewOutConnector, row + 1);

            var outConnectorComboBox = new ComboBox();
            outConnectorComboBox.IsEditable = true;
            FillConnectors(outConnectorComboBox);
            outConnectorComboBox.GotFocus += Connector_GotFocus;
            outConnectorComboBox.Margin = new Thickness(10);

            OutConnectors.Add(outConnectorComboBox);

            GridOutConnectors.Children.Add(outConnectorComboBox);
            
            Grid.SetRow(outConnectorComboBox, row);
        }

        private void FillConnectors(ComboBox cb)
        {
            var connectors = Manager.ComponentStorage.GetConnectors();
            foreach (var connector in connectors)
            {
                cb.Items.Add(connector);
            }
        }

        private void ComboBoxTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxTypes.Items.Count == 0)
                return;

            TypeGrid.Children.Clear();
            TypeGrid.RowDefinitions.Clear();
            Properties.Clear();

            var properties = Manager.ComponentStorage.GetTypeByName(ComboBoxTypes.SelectedItem.ToString()).Properties.ToList();

            for (int i = 0; i < properties.Count(); i++)
            {
                var rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                TypeGrid.RowDefinitions.Add(rd);
                
                var label = new Label();
                label.Content = properties[i].Name;
                label.Margin = new Thickness(10);
                TypeGrid.Children.Add(label);
                Grid.SetRow(label, i);
                Grid.SetColumn(label, 0);

                var textbox = new TextBox();
                textbox.Margin = new Thickness(10);
                textbox.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                if(properties[i].Type == typeof(double))
                    textbox.PreviewTextInput += TextBoxValidation.NumericalTextBoxCheck;
                TypeGrid.Children.Add(textbox);
                Grid.SetRow(textbox, i);
                Grid.SetColumn(textbox, 1);

                Properties.Add(properties[i].Name, textbox);
            }
        }

        private void ButtonManageTypes_Click(object sender, RoutedEventArgs e)
        {
            var manageTypesWindow = new ManageTypesWindow(Manager);
            manageTypesWindow.ShowDialog();
            LoadTypes();
        }
    }
}
