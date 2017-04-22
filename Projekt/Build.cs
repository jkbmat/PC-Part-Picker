using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Projekt
{
    /// <summary>
    /// Reprezentuje ucelenu sadu komponentov
    /// </summary>
    public class Build
    {
        public BuildStorage BuildStorage { get; set; }

        /// <summary>
        /// Vytvori novy Build
        /// </summary>
        public Build()
        {
            BuildStorage = new BuildStorage();
        }

        /// <summary>
        /// Vytvori novy Build podla uz ulozeneho
        /// </summary>
        /// <param name="fileName">subor, v ktorom je ulozeny build</param>
        public Build(string fileName)
        {
            BuildStorage = new BuildStorage(fileName);
        }

        public bool IsEmpty()
        {
            return GetPartsByType("Power Source").Count() == 0;
        }

        /// <summary>
        /// Skontroluje, ci je Build korektny
        /// </summary>
        /// <returns>je Build korektny?</returns>
        public bool IsCorrect()
        {
            return IsInPowerLimit() && HasCorrectParts();
        }

        /// <summary>
        /// Prida do Buildu novy komponent
        /// </summary>
        /// <param name="connectTo">komponent, na ktory sa ma novy komponent napojit</param>
        /// <param name="component">novy komponent</param>
        public void Connect(Component from, Component to)
        {
            BuildStorage.Connect(from, to);
        }

        /// <summary>
        /// Pripoji novy komponent do prazdneho buildu
        /// </summary>
        /// <param name="part">Power Source komponent</param>
        public void Connect(Component part)
        {
            BuildStorage.AddComponent(part);
        }

        /// <summary>
        /// Vrati vsetky komponenty typu <paramref name="type"/>
        /// </summary>
        /// <param name="type">typ hladanych komponentov</param>
        /// <returns>kolekcia vyhladanych komponent</returns>
        public IEnumerable<Component> GetPartsByType(string type)
        {
            return BuildStorage.GetComponentsByType(type);
        }

        /// <summary>
        /// Zisti, ci sa v Builde nachadza komponent
        /// </summary>
        /// <param name="component">komponent</param>
        /// <returns>nachadza sa komponent v Builde?</returns>
        public bool HasComponent(Component component)
        {
            return BuildStorage.GetComponent(component.Manufacturer, component.Model) != null;
        }

        /// <summary>
        /// Odstrani komponent z Buildu
        /// </summary>
        /// <param name="component"></param>
        public void RemoveComponent(Component component)
        {
            BuildStorage.RemoveComponent(component);
        }

        /// <summary>
        /// Vrati pozadovane komponenty, ktore sa nenachadzaju v builde
        /// </summary>
        /// <returns>Kolekcia nazvov pozadovanych komponentov</returns>
        public IEnumerable<string> GetMissingComponents()
        {
            var ret = new List<string>();

            foreach (var part in PartPickerManager.RequiredParts)
            {
                if (GetPartsByType(part).Count() == 0)
                {
                    ret.Add(part);
                }
            }

            return ret;
        }

        /// <summary>
        /// Ulozi Build do suboru
        /// </summary>
        /// <param name="fileName">subor, do ktoreho sa ma Build ulozit</param>
        public void Save(string fileName)
        {
            BuildStorage.Save(fileName);
        }

        /// <summary>
        /// Zisti, ci ma build vsetky potrebne suciastky
        /// </summary>
        /// <returns>ma build vsetky potrebne suciastky?</returns>
        private bool HasCorrectParts()
        {
            return GetMissingComponents().Count() == 0;
        }

        /// <summary>
        /// Zisti, ci build nepresiahol limit spotreby energie
        /// </summary>
        /// <returns>ci build nepresiahol limit spotreby energie</returns>
        private bool IsInPowerLimit()
        {
            var powerSource = GetPartsByType("Power Source").First();

            return RecursiveCalculateConsumption(powerSource) <= 0;
        }

        /// <summary>
        /// Rekurzivna funkcia na pocitanie spotreby energie
        /// </summary>
        /// <param name="component">aktualny komponent</param>
        /// <returns>spotreba komponentu <paramref name="component"/> a vsetkych podkomponentov</returns>
        public long RecursiveCalculateConsumption(Component component)
        {
            long sum = component.EnergyConsumption;

            foreach (var connector in component.OutConnectors)
            {
                foreach (var part in BuildStorage.GetConnected(component, connector))
                {
                    sum += RecursiveCalculateConsumption(part);
                }
            }

            return sum;
        }

    }
}
