using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt
{
    public class ComponentType
    {
        public string Name { get; set; }
        public IEnumerable<ComponentProperty> Properties { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
