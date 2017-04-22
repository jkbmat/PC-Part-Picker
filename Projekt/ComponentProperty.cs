using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt
{
    public class ComponentProperty
    {
        public Type Type;
        public string Name;
        public object Value;

        public override bool Equals(object obj)
        {
            if (obj is ComponentProperty == false)
                return false;

            ComponentProperty b = obj as ComponentProperty;

            return Type == b.Type && Name == b.Name && Value == b.Value;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
