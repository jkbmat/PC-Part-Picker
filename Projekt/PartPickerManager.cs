using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Projekt
{
    public class PartPickerManager
    {
        public static string[] RequiredParts = { "Power Source", "RAM", "Motherboard", "Disk", "Processor" };

        public Build CurrentBuild { get; set; }
        public ComponentStorage ComponentStorage { get; set; }

        public PartPickerManager(ComponentStorage componentStorage)
        {
            ComponentStorage = componentStorage;
            CurrentBuild = new Build();
        }
    }
}
