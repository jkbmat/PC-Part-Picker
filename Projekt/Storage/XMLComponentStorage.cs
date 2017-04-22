using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Projekt
{
    public class XMLComponentStorage : ComponentStorage
    {
        private string FileName { get; set; }
        protected XDocument Doc { get; set; }

        /// <summary>
        /// Vytvori novy Storage, ktory uklada komponenty do XML suboru
        /// </summary>
        /// <param name="fileName">Nazov suboru, kam sa ma ukladat</param>
        /// <param name="autoSave">Ulozit data automaticky do suboru pri vypnuti aplikacie?</param>
        public XMLComponentStorage(string fileName, bool autoSave)
        {
            FileName = fileName;

            try
            {
                Doc = XDocument.Load(fileName);
            }
            catch(Exception)
            {
                Doc = XDocument.Parse(Properties.Resources.BlankDatabase);
            }

            if (autoSave)
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(Save);
        }

        /// <summary>
        /// Vrati vsetky komponenty, ktore sa vedia zapojit do konektoru
        /// </summary>
        /// <param name="connectorType">typ konektoru</param>
        /// <returns>zoznam komponentov</returns>
        public IEnumerable<Component> GetComponentsByInConnector(string connectorType)
        {
            return Doc
                .Descendants("component")
                .Where
                (
                    component =>
                        component.Descendants("in-connector").First().Attribute("type").Value.Equals(connectorType)
                )
                .Select(CreateComponent);
        }

        /// <summary>
        /// Vrati vsetky komponenty daneho typu
        /// </summary>
        /// <param name="type">typ komponentov</param>
        /// <returns>zoznam komponentov</returns>
        public IEnumerable<Component> GetComponentsByType(string type)
        {
            return Doc
                .Descendants("component")
                .Where
                (
                    component =>
                        component.Attribute("type").Value.Equals(type)
                )
                .Select(CreateComponent);
        }

        /// <summary>
        /// Prida novy komponent
        /// </summary>
        /// <param name="component">komponent, ktory sa ma pridat</param>
        public void AddComponent(Component component)
        {
            Doc.Root
                .Add
                (
                    new XElement
                    (
                        "component",


                        new XAttribute("type", component.Type),


                        new XElement("description", component.Description),

                        new XElement("energy-consumption", component.EnergyConsumption.ToString()),

                        new XElement
                        (
                            "in-connector",

                            new XAttribute("type", component.InConnector == null ? "null" : component.InConnector)
                        ),

                        new XElement("manufacturer", component.Manufacturer),

                        new XElement("model", component.Model),

                        component.Properties.Select
                        (
                            property =>
                                new XElement
                                (
                                    "property",
                                    property.Value,
                                    
                                    new XAttribute("name", property.Name),
                                    new XAttribute("type", property.Type == typeof(string) ? "string" : "number")
                                )
                        ),

                        component.OutConnectors.Select(connector => new XElement("out-connector", new XAttribute("type", connector)))

                    )
                );
        }

        /// <summary>
        /// Odstrani komponent
        /// </summary>
        /// <param name="component">komponent, ktory sa ma odstranit</param>
        public void RemoveComponent(Component component)
        {
            var toRemove = GetComponentElement(component.Manufacturer, component.Model);

            if(toRemove == null)
            {
                throw new InvalidOperationException("The component you are trying to remove isn't in the storage");
            }

            toRemove.Remove();
        }

        /// <summary>
        /// Ulozi dokument do suboru
        /// </summary>
        /// <param name="fileName">subor</param>
        public virtual void Save(string fileName)
        {
            Doc.Save(fileName);
        }

        /// <summary>
        /// Vrati vsetky typy komponentov v Storagei
        /// </summary>
        /// <returns>Typy komponentov</returns>
        public IEnumerable<ComponentType> GetComponentTypes()
        {
            return Doc.Descendants("type").Select(CreateType);
        }

        /// <summary>
        /// Vrati vsetky rozne vstupne connectory v storagei
        /// </summary>
        /// <returns>vsetky rozne vstupne connectory v storagei</returns>
        public IEnumerable<string> GetInConnectors()
        {
            var ret = Doc
                .Descendants("in-connector")
                .Select
                (
                    connector => connector
                        .Attribute("type").Value
                )
                .Distinct()
                .ToList();

            ret.Remove("null");

            return ret;
        }

        /// <summary>
        /// Vrati vsetky rozne connectory v Storagei
        /// </summary>
        /// <returns>Connectory</returns>
        public IEnumerable<string> GetConnectors()
        {
            return Doc
                .Descendants("out-connector")
                .Select
                (
                    connector => connector
                        .Attribute("type").Value
                )
                .Concat(GetInConnectors())
                .Distinct();
        }

        /// <summary>
        /// Vrati vsetky rozne connectory v Storagei
        /// </summary>
        /// <returns>Connectory</returns>
        public IEnumerable<Component> Query(string search, string inConnector, string type)
        {
            return Doc
                .Descendants("component")
                .Where
                (
                    component =>
                    {
                        bool s, c, t;
                        s = c = t = true;

                        if (search != "")
                            s = SearchComponentXML(component, search);
                        if (inConnector != "")
                            c = component.Descendants("in-connector").First().Attribute("type").Value.Equals(inConnector);
                        if (type != "")
                            t = component.Attribute("type").Value.Equals(type);

                        return s && c && t;
                    }
                )
                .Select(CreateComponent);
        }

        /// <summary>
        /// Vrati komponent identifikovany podla modelu a vyrobcu
        /// </summary>
        /// <param name="manufacturer">vyrobca</param>
        /// <param name="model">model</param>
        /// <returns>vyhladany Component</returns>
        public Component GetComponent(string manufacturer, string model)
        {
            return CreateComponent(GetComponentElement(manufacturer, model));
        }

        /// <summary>
        /// Vrati typ podla nazvu
        /// </summary>
        /// <param name="name">Nazov typu</param>
        /// <returns>Hladany typ</returns>
        public ComponentType GetTypeByName(string name)
        {
            return CreateType(GetTypeElement(name));
        }

        /// <summary>
        /// Odstrani typ
        /// </summary>
        /// <param name="type">Typ, ktory sa ma odstranit</param>
        public void RemoveType(ComponentType type)
        {
            Doc.Descendants("type")
                .Where
                (
                    typeNode =>
                        typeNode.Attribute("name").Value == type.Name
                )
                .Remove();

            Doc.Descendants("component")
                .Where
                (
                    componentNode =>
                        componentNode.Attribute("type").Value == type.Name
                )
                .Remove();
        }

        /// <summary>
        /// Prida novy typ
        /// </summary>
        /// <param name="type">typ</param>
        public void AddType(ComponentType type)
        {
            Doc.Root
                .Add
                (
                    new XElement
                        (
                            "type",

                            new XAttribute("name", type.Name),

                            type.Properties
                                .Select
                                (
                                    prop =>
                                        new XElement
                                            (
                                                "property",

                                                new XAttribute("name", prop.Name),
                                                new XAttribute("type", prop.Type == typeof(string) ? "string" : "number")
                                            )
                                )
                        )
                );
        }

        /// <summary>
        /// Upravi typ
        /// </summary>
        /// <param name="old">Stary typ</param>
        /// <param name="editTo">Novy typ</param>
        /// <param name="changed">Maju sa premazat properties existujucich komponentov tohto typu?</param>
        public void EditType(ComponentType old, ComponentType editTo, bool changed)
        {
            var node = Doc.Descendants("type").First(type => type.Attribute("name").Value == old.Name);
            var affectedComponents = Doc.Descendants("component").Where(comp => comp.Attribute("type").Value == old.Name);

            if(old.Name != editTo.Name)
            {
                node.Attribute("name").Value = editTo.Name;
                foreach(var comp in affectedComponents)
                {
                    comp.Attribute("type").Value = editTo.Name;
                }
            }

            var newProps = editTo.Properties.Except(old.Properties);

            if(changed)
            {
                Doc.Descendants("component").Where(comp => comp.Attribute("type").Value == editTo.Name).Descendants("property").Remove();
                newProps = editTo.Properties;
            }

            node.Descendants().Remove();

            foreach (var prop in newProps)
            {
                node
                    .Add
                    (
                        new XElement(
                            "property",

                            new XAttribute("name", prop.Name),
                            new XAttribute("type", prop.Type == typeof(string) ? "string" : "number")
                        )
                    );
            }
        }


        /// <summary>
        /// Vrati XElement komponentu podla vyrobcu a modelu
        /// </summary>
        /// <param name="manufacturer">vyrobca</param>
        /// <param name="model">model</param>
        /// <returns>XElement komponentu</returns>
        protected XElement GetComponentElement(string manufacturer, string model)
        {
            return Doc.Descendants("component")
                .Where
                (
                    component =>
                        component.Elements("manufacturer").First().Value.Equals(manufacturer) &&
                        component.Elements("model").First().Value.Equals(model)
                ).FirstOrDefault();
        }

        /// <summary>
        /// Z XElementu komponentu vytvori Component
        /// </summary>
        /// <param name="component">XElement komponentu</param>
        /// <returns>vytvoreny Component</returns>
        protected virtual Component CreateComponent(XElement component)
        {
            if (component == null)
                return null;

            var ret = new Component()
            {
                Description = component.Descendants("description").First().Value,

                EnergyConsumption = Convert.ToInt32(component.Descendants("energy-consumption").First().Value),

                InConnector = component.Descendants("in-connector").First().Attribute("type").Value,

                Manufacturer = component.Descendants("manufacturer").First().Value,

                Model = component.Descendants("model").First().Value,

                OutConnectors = component.Descendants("out-connector").Select(connector => connector.Attribute("type").Value),

                Properties = component.Descendants("property")
                    .Select
                    (
                        property =>
                            new ComponentProperty()
                            {
                                Name = property.Attribute("name").Value,
                                Type = property.Attribute("type").Value == "number" ? typeof(double) : typeof(string),
                                Value = property.Value
                            }
                    ),

                Type = component.Attribute("type").Value
            };

            return ret;
        }

        /// <summary>
        /// Ulozi stav databazy do pamati
        /// </summary>
        private void Save(object sender, EventArgs e)
        {
            Save(FileName);
        }

        /// <summary>
        /// Vrati XElement daneho typu
        /// </summary>
        /// <param name="name">nazov typu</param>
        /// <returns>XElement typu</returns>
        private XElement GetTypeElement(string name)
        {
            return Doc.Descendants("type").FirstOrDefault(type => type.Attribute("name").Value.Equals(name));
        }

        /// <summary>
        /// Vytvori ComponentType objekt z XElementu
        /// </summary>
        /// <param name="type">XElement</param>
        /// <returns>ComponentType</returns>
        private ComponentType CreateType(XElement type)
        {
            if (type == null)
            {
                return null;
            }

            return new ComponentType()
                        {
                            Name = type.Attribute("name").Value,
                            Properties = type.Descendants("property")
                                .Select
                                (
                                    property =>
                                        new ComponentProperty()
                                        {
                                            Name = property.Attribute("name").Value,
                                            Type = property.Attribute("type").Value == "string" ? typeof(string) : typeof(double)
                                        }
                                )
                        };
        }

        /// <summary>
        /// Vyhlada text v XElemente komponentu
        /// </summary>
        /// <param name="component">komponent</param>
        /// <param name="search">hladany text</param>
        /// <returns>Nasiel sa text?</returns>
        private bool SearchComponentXML(XElement component, string search)
        {
            return component.Element("manufacturer").Value.ToLower().Contains(search.ToLower()) ||
                component.Element("model").Value.ToLower().Contains(search.ToLower()) ||
                component.Element("description").Value.ToLower().Contains(search.ToLower());
        }

    }
}
