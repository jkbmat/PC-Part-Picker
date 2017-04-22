using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows;

namespace Projekt
{
    [ComVisible(true)]
    public class WebController
    {
        private PartPickerManager Manager { get; set; }
        private WebBrowser Browser { get; set; }

        private string connectorLeftImage;
        private string connectorRightImage;

        private bool updatingSize = false;

        public WebController(WebBrowser browser, PartPickerManager manager)
        {
            Browser = browser;
            Manager = manager;

            connectorLeftImage = System.IO.Path.GetTempFileName();
            Properties.Resources.connector_left.Save(connectorLeftImage);
            connectorRightImage = System.IO.Path.GetTempFileName();
            Properties.Resources.connector_right.Save(connectorRightImage);
        }

        public bool IsEmpty()
        {
            return Manager.CurrentBuild.IsEmpty();
        }

        public void RefreshBrowser()
        {
            var ps = Manager.CurrentBuild.GetPartsByType("Power Source").First();

            Browser.InvokeScript("setView", BuildBrowser(ps));
            Browser.InvokeScript("setPower", new object[] { ps.EnergyConsumption, Manager.CurrentBuild.RecursiveCalculateConsumption(ps) - ps.EnergyConsumption });
            Browser.InvokeScript("setMissingComponents", String.Join(", ", Manager.CurrentBuild.GetMissingComponents()));
        }

        public void InitializeBrowser()
        {
            string source = Properties.Resources.BlankBrowser;
            source = source.Replace("<![CDATA[STYLESHEET]]>", Properties.Resources.BrowserStyle);
            source = source.Replace("<![CDATA[JAVASCRIPT]]>", Properties.Resources.jquery_1_11_3_min + Properties.Resources.BrowserScript);

            Browser.ObjectForScripting = this;
            Browser.NavigateToString(source);
        }

        public void FindPowerSource()
        {
            var searchWindow = new SearchWindow(true, Manager, null, null, "Power Source");

            if (searchWindow.ShowDialog() == true && searchWindow.SelectedItem != null)
            {
                Manager.CurrentBuild.Connect(searchWindow.SelectedItem);

                RefreshBrowser();
            }
        }

        public void FindComponentsByInConnector(string inConnector, string oldManufacturer, string oldModel)
        {
            var searchWindow = new SearchWindow(true, Manager, null, inConnector, null);

            if (searchWindow.ShowDialog() == true && searchWindow.SelectedItem != null)
            {
                Manager.CurrentBuild.Connect(Manager.CurrentBuild.BuildStorage.GetComponent(oldManufacturer, oldModel), searchWindow.SelectedItem);

                RefreshBrowser();
            }
        }

        public void RemoveComponent(string manufacturer, string model)
        {
            using (var a = File.CreateText("a.html")) { a.Write((Browser.Document as mshtml.IHTMLDocument2).body.outerHTML); }

            Manager.CurrentBuild.RemoveComponent(Manager.CurrentBuild.BuildStorage.GetComponent(manufacturer, model));
        }

        public void Print()
        {
            mshtml.IHTMLDocument2 doc = Browser.Document as mshtml.IHTMLDocument2;

            doc.execCommand("Print", true, null);
        }

        public void UpdateSize(double w, double h)
        {
            if (updatingSize)
                return;

            updatingSize = true;

            Browser.Width = w;
            Browser.Height = h;

            updatingSize = false;
        }

        public void DetectUpdateSize()
        {
            RowDefinition row = ((MainWindow)Window.GetWindow(Browser)).Grid.RowDefinitions[1];
            ColumnDefinition col = ((MainWindow)Window.GetWindow(Browser)).Grid.ColumnDefinitions[0];
            UpdateSize(col.ActualWidth, row.ActualHeight);
        }

        private string BuildBrowser(Component component)
        {
            string newBrowser;

            newBrowser = "<li>";

            newBrowser += "<div class='component' inconnector='" + component.InConnector + "' manufacturer='" + component.Manufacturer + "' model='" + component.Model + "'>";
            newBrowser += "<small><strong>" + component.Type + "</strong>";

            newBrowser += ((component.EnergyConsumption > 0) ? " consuming " + component.EnergyConsumption : " producing " + -component.EnergyConsumption) + " W</small><br/>";
            newBrowser += "<h2><strong>" + component.Manufacturer + "</strong> " + component.Model + "</h2><br/><br/>";
            newBrowser += "<button class='toggle-details' onclick='toggleDetails(\"" + component.Manufacturer + "\", \"" + component.Model + "\")'>▾ Details</button>";
            newBrowser += "<button onclick='remove(\"" + component.Manufacturer + "\", \"" + component.Model + "\")'>Remove</button>";
            newBrowser += "<div class='details'><ul>";
            newBrowser += "<em>" + component.Description + "</em><br/><br/>";

            foreach (var property in component.Properties)
            {
                newBrowser += "<strong>" + property.Name + ": </strong>";
                newBrowser += property.Value + "<br/>";
            }

            newBrowser += "</ul></div>";
            newBrowser += "</div>";

            foreach (var connector in component.OutConnectors.Distinct())
            {
                int count = 0;

                foreach (var part in Manager.CurrentBuild.BuildStorage.GetConnected(component, connector))
	            {
                    newBrowser += "<img class='connector_img' src='" + connectorLeftImage + "'/>" + connector + "<img class='connector_img' src='" + connectorRightImage + "'/>";
                    newBrowser += "<ul>";
                    newBrowser += BuildBrowser(part);
                    newBrowser += "</ul>";

                    count++;
	            }

                while (count < component.OutConnectors.Count(con => con.Equals(connector)))
                {
                    newBrowser += "<img class='connector_img' src='" + connectorLeftImage + "'/>" + connector + "<img class='connector_img' src='" + connectorRightImage + "'/><br/>";
                    newBrowser += "<ul><li><button onclick='addComponent(\""+component.Manufacturer+"\", \""+component.Model+"\", \""+connector+"\")'>Connect</button></li></ul>";
                    count++;
                }
            }

            newBrowser += "</li>";

            return newBrowser;
        }

    }
}
