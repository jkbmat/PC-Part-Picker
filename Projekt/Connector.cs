using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt
{
    public class Connector
    {
        public Component From { get; set; }
        public Component To { get; set; }

        public string Type
        {
            get
            {
                return this.To.InConnector;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            if (this.From == null && ((Connector)obj).From == null && this.To.Equals(((Connector)obj).To))
                return true;

            if(this.From == null || ((Connector)obj).From == null)
                return false;

            return this.From.Equals(((Connector)obj).From) &&
                this.To.Equals(((Connector)obj).To);
        }

        public override int GetHashCode()
        {
            return From.GetHashCode() * To.GetHashCode() * 3;
        }
    }
}
