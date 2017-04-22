using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Projekt
{
    /// <summary>
    /// An improved XMLStorage for saving builds
    /// </summary>
    public class BuildStorage : XMLComponentStorage
    {
        public List<Connector> Connectors { get; private set; }

        /// <summary>
        /// Creates a new build storage
        /// </summary>
        public BuildStorage() : base(null, false)
        {
            Connectors = new List<Connector>();
        }


        /// <summary>
        /// Creates a new build storage from file
        /// </summary>
        /// <param name="fileName">file to load</param>
        public BuildStorage(string fileName) : base(fileName, false)
        {
            Connectors = LoadConnectors().ToList();
        }

        /// <summary>
        /// Connects two components
        /// </summary>
        /// <param name="from">Existing component</param>
        /// <param name="to">Component to connect</param>
        public void Connect(Component from, Component to)
        {
            if (!ConnectableOut(from, to.InConnector))
                throw new InvalidOperationException("The component you are connecting to has no free connector "+to.InConnector);

            AddComponent(to);
            AddConnector(from, to);
        }

        /// <summary>
        /// Removes a component from build
        /// </summary>
        /// <param name="component"></param>
        new public void RemoveComponent(Component component)
        {
            if(component.Type != "Power Source")
                RemoveConnector(Connectors.Where(con => con.To.Equals(component)).First());

            foreach (var connector in component.OutConnectors.Distinct())
            {
                foreach (var part in GetConnected(component, connector).ToList())
                {
                    RemoveComponent(part);
                }
            }
            base.RemoveComponent(component);
        }

        /// <summary>
        /// Returns all components connected to a specific connector in a component
        /// </summary>
        /// <param name="from">Source component</param>
        /// <param name="connectorType">Connector type</param>
        /// <returns>Collection of components that are connected to the Source component on <paramref name="connectorType"/></returns>
        public IEnumerable<Component> GetConnected(Component from, string connectorType)
        {
            return Connectors
                .Where
                (
                    connector => connector.Type.Equals(connectorType) && connector.From.Equals(from)
                )
                .Select(connector => connector.To);
        }

        /// <summary>
        /// Disconnects a connector
        /// </summary>
        /// <param name="connector"></param>
        private void Disconnect(Connector connector)
        {
            RemoveConnector(connector);

            foreach (var connectorType in connector.To.OutConnectors.Distinct())
            {
                foreach (var component in GetConnected(connector.To, connectorType))
                {
                    Disconnect(new Connector() { From = connector.To, To = component });
                }
            }

            base.RemoveComponent(connector.To);
        }

        /// <summary>
        /// Creates a new connector
        /// </summary>
        /// <param name="from">Source component</param>
        /// <param name="to">Destination component</param>
        private void AddConnector(Component from, Component to)
        {
            Connectors.Add(new Connector() { From = from, To = to });

            Doc.Root.Add
            (
                new XElement
                (
                    "connector",

                    new XElement
                    (
                        "from",

                        new XElement("manufacturer", from.Manufacturer),
                        new XElement("model", from.Model)
                    ),
                    new XElement
                    (
                        "to",

                        new XElement("manufacturer", to.Manufacturer),
                        new XElement("model", to.Model)
                    ) 
                )
            );
        }

        /// <summary>
        /// Removes a connector
        /// </summary>
        /// <param name="connector">connector to remove</param>
        private void RemoveConnector(Connector connector)
        {
            Connectors.Remove(connector);

            Doc
                .Descendants("connector")
                .Where
                (
                    con =>
                        connector.From.Manufacturer.Equals(con.Element("from").Element("manufacturer").Value) &&
                        connector.From.Model.Equals(con.Element("from").Element("model").Value) &&
                        connector.To.Manufacturer.Equals(con.Element("to").Element("manufacturer").Value) &&
                        connector.To.Model.Equals(con.Element("to").Element("model").Value)
                )
                .Remove();
        }

        /// <summary>
        /// Returns a collection of all connectors
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Connector> LoadConnectors()
        {
            return Doc
                .Descendants("connector")
                .Select
                (
                    connector => new Connector()
                    {
                        From = GetComponent(connector.Element("from").Element("manufacturer").Value, connector.Element("from").Element("model").Value),
                        To = GetComponent(connector.Element("to").Element("manufacturer").Value, connector.Element("to").Element("model").Value),
                    }
                );
        }

        /// <summary>
        /// Tests whether there is a free outconnector on a component
        /// </summary>
        /// <param name="from">Source component</param>
        /// <param name="connectorType">Connector type to test</param>
        /// <returns>whether there is a free outconnector on a component</returns>
        private bool ConnectableOut(Component from, string connectorType)
        {
            return from.OutConnectors.Count(connector => connector.Equals(connectorType)) > GetConnected(from, connectorType).Count();
        }
    }
}
