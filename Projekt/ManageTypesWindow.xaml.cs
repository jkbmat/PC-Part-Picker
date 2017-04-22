using Projekt.Validation;
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

namespace Projekt
{
    /// <summary>
    /// Interaction logic for ManageTypesWindow.xaml
    /// </summary>
    public partial class ManageTypesWindow : Window
    {
        private PartPickerManager Manager { get; set; }

        private Grid PropertyGrid { get; set; }

        private Button AddPropertyButton { get; set; }

        private int propertyRows = 0;

        public ManageTypesWindow(PartPickerManager manager)
        {
            Manager = manager;
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var types = Manager.ComponentStorage.GetComponentTypes();
            TypeList.Items.Clear();
            foreach (var type in types)
            {
                TypeList.Items.Add(type);
            }

            TypeList.SelectedIndex = 0;
            
            TypeGrid.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
        }

        private void CreateTypeGrid()
        {
            propertyRows = 0;

            TypeGrid.RowDefinitions.Clear();
            TypeGrid.Children.Clear();

            var rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            TypeGrid.RowDefinitions.Add(rd);

            var label = new Label();
            label.Content = "Type Name";
            label.Margin = new Thickness(10);
            TypeGrid.Children.Add(label);
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, 0);

            var textbox = new TextBox();
            textbox.Margin = new Thickness(10);
            textbox.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            textbox.Tag = "Name";
            TypeGrid.Children.Add(textbox);
            Grid.SetRow(textbox, 0);
            Grid.SetColumn(textbox, 1);

            var properties = new GroupBox();
            properties.Header = "Properties (empty-named properties will be ignored)";
            properties.Padding = new Thickness(5, 10, 5, 5);
            properties.Content = new Grid();

            PropertyGrid = (Grid)properties.Content;

            var cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            PropertyGrid.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(1, GridUnitType.Star);
            PropertyGrid.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            PropertyGrid.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(1, GridUnitType.Star);
            PropertyGrid.ColumnDefinitions.Add(cd);

            rd = new RowDefinition();
            rd.Height = new GridLength(1, GridUnitType.Star);
            TypeGrid.RowDefinitions.Add(rd);

            TypeGrid.Children.Add(properties);
            Grid.SetRow(properties, 1);
            Grid.SetColumnSpan(properties, 2);

            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            TypeGrid.RowDefinitions.Add(rd);

            var applyButton = new Button();
            applyButton.Content = "Apply";
            applyButton.Margin = new Thickness(0, 10, 10, 10);
            applyButton.Padding = new Thickness(15, 2, 15, 2);
            applyButton.Click += ApplyButton_Click;
            applyButton.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            applyButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            TypeGrid.Children.Add(applyButton);
            Grid.SetColumnSpan(applyButton, 2);
            Grid.SetRow(applyButton, 2);

            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            PropertyGrid.RowDefinitions.Add(rd);

            AddPropertyButton = new Button();
            AddPropertyButton.Content = "Add New Property";
            AddPropertyButton.Padding = new Thickness(15, 2, 15, 2);
            AddPropertyButton.Margin = new Thickness(5);
            AddPropertyButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            AddPropertyButton.Click += AddPropertyButton_Click;
            AddPropertyButton.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            PropertyGrid.Children.Add(AddPropertyButton);
            Grid.SetColumnSpan(AddPropertyButton, 4);
            Grid.SetRow(AddPropertyButton, 0);
            Grid.SetColumn(AddPropertyButton, 0);
        }

        void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ComponentType selected = TypeList.SelectedItem as ComponentType;
            ComponentType created = new ComponentType();
            List<ComponentProperty> props = new List<ComponentProperty>();

            for (int x = 0; x < propertyRows; x++)
            {
                props.Add(new ComponentProperty());
            }

            foreach (Control control in TypeGrid.Children)
            {
                if ((string)control.Tag == "Name")
                {
                    created.Name = ((TextBox)control).Text;
                    if (created.Name == "")
                    {
                        MessageBox.Show(this, "Type Name is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    break;
                }
            }

            foreach (Control control in PropertyGrid.Children)
            {
                if (control is TextBox && (string)control.Tag == "Name")
                    props[Grid.GetRow(control)].Name = ((TextBox)control).Text;

                if (control is ComboBox && (string)control.Tag == "Type")
                    props[Grid.GetRow(control)].Type = ((ComboBox)control).SelectedIndex == 0 ? typeof(string) : typeof(double);
            }

            if(selected == null)
            {
                if(Manager.ComponentStorage.GetTypeByName(created.Name) != null)
                {
                    MessageBox.Show(this, created.Name + " already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                props = props.Where(prop => prop.Name != "").ToList();
                created.Properties = props;
                Manager.ComponentStorage.AddType(created);
                Window_Initialized(null, null);
                return;
            }

            int i = 0;
            bool changedOK = false;
            foreach (ComponentProperty property in selected.Properties)
            {
                if(!props[i].Equals(property) && !changedOK)
                {
                    MessageBoxResult messageBoxResult = MessageBox
                        .Show("One of the existing properties has changed. If you proceed, all properties of existing elements of this type will be reset to blank state. Do you wish to proceed?", "Are you sure?", System.Windows.MessageBoxButton.YesNo);

                    if (messageBoxResult != MessageBoxResult.Yes)
                        return;
    
                    changedOK = true;
                }

                i++;
            }

            props = props.Where(prop => prop.Name != "").ToList();
            created.Properties = props;

            Manager.ComponentStorage.EditType(selected, created, changedOK);
            Window_Initialized(null, null);
        }

        void AddPropertyButton_Click(object sender, RoutedEventArgs e)
        {
            AddPropertyRow();
        }

        private void AddPropertyRow()
        {
            var rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            PropertyGrid.RowDefinitions.Add(rd);

            Grid.SetRow(AddPropertyButton, propertyRows + 1);

            var label = new Label();
            label.Margin = new Thickness(5);
            label.Content = "Name";
            PropertyGrid.Children.Add(label);
            Grid.SetRow(label, propertyRows);
            Grid.SetColumn(label, 0);

            var textbox = new TextBox();
            textbox.Tag = "Name";
            textbox.Margin = new Thickness(5);
            textbox.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            PropertyGrid.Children.Add(textbox);
            Grid.SetRow(textbox, propertyRows);
            Grid.SetColumn(textbox, 1);

            label = new Label();
            label.Margin = new Thickness(5);
            label.Content = "Type";
            PropertyGrid.Children.Add(label);
            Grid.SetRow(label, propertyRows);
            Grid.SetColumn(label, 2);

            var comboBox = new ComboBox();
            comboBox.Margin = new Thickness(5);
            comboBox.Tag = "Type";
            PropertyGrid.Children.Add(comboBox);
            Grid.SetRow(comboBox, propertyRows);
            Grid.SetColumn(comboBox, 3);
            comboBox.Items.Add("String");
            comboBox.Items.Add("Number");
            comboBox.SelectedIndex = 0;

            propertyRows++;
        }

        private void AddPropertyRow(ComponentProperty property)
        {
            AddPropertyRow();

            foreach (Control control in PropertyGrid.Children)
        	{
                if (Grid.GetRow(control) == propertyRows - 1)
                {
                    if (control is TextBox)
                    {
                        ((TextBox)control).Text = property.Name;
                    }
                    if (control is ComboBox && property.Type == typeof(double))
                    {
                        ((ComboBox)control).SelectedIndex = 1;
                    }
                }
	        }
        }

        private void TypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonRemove.IsEnabled = TypeList.SelectedItem != null && !PartPickerManager.RequiredParts.Contains(TypeList.SelectedItem.ToString());

            if (TypeList.SelectedItem == null)
                return;

            CreateTypeGrid();

            ComponentType selected = TypeList.SelectedItem as ComponentType;

            foreach (Control control in TypeGrid.Children)
            {
                if ((string)control.Tag == "Name")
                {
                    ((TextBox)control).Text = selected.Name;
                    if (PartPickerManager.RequiredParts.Contains(selected.Name))
                        ((TextBox)control).IsEnabled = false;
                    break;
                }
            }


            propertyRows = 0;

            foreach (var property in selected.Properties)
            {
                AddPropertyRow(property);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TypeList.UnselectAll();
            CreateTypeGrid();
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to remove "+TypeList.SelectedItem.ToString()+"?\nALL COMPONENTS IN DATABASE WITH THIS TYPE WILL ALSO BE REMOVED!!", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
                Manager.ComponentStorage.RemoveType(TypeList.SelectedItem as ComponentType);

            Window_Initialized(null, null);
        }
    }
}
