using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt
{
    public class Component
    {
        public string Manufacturer { get; set; }

        public string Model { get; set; }

        public string InConnector { get; set; }

        public IEnumerable<string> OutConnectors { get; set; }

        public string Description { get; set; }

        public IEnumerable<ComponentProperty> Properties { get; set; }

        public string Type { get; set; }

        public long EnergyConsumption { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Component &&
                ((Component)obj).Model.Equals(Model) &&
                ((Component)obj).Manufacturer.Equals(Manufacturer);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public ComponentProperty this[string index]
        {
            get
            {
                return Properties.Where(property => property.Name.Equals(index)).First();
            }
        }
    }
}
