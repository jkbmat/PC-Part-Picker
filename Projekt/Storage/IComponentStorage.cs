using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projekt
{
    /// <summary>
    /// Ulozisko komponent
    /// </summary>
    public interface ComponentStorage
    {
        /// <summary>
        /// Vrati vsetky komponenty, ktore sa vedia zapojit do konektoru
        /// </summary>
        /// <param name="connectorType">typ konektoru</param>
        /// <returns>zoznam komponentov</returns>
        IEnumerable<Component> GetComponentsByInConnector(string connectorType);

        /// <summary>
        /// Vrati vsetky komponenty daneho typu
        /// </summary>
        /// <param name="type">typ komponentov</param>
        /// <returns>zoznam komponentov</returns>
        IEnumerable<Component> GetComponentsByType(string type);

        /// <summary>
        /// Prida novy komponent
        /// </summary>
        /// <param name="component">komponent, ktory sa ma pridat</param>
        void AddComponent(Component component);

        /// <summary>
        /// Odstrani komponent
        /// </summary>
        /// <param name="component">komponent, ktory sa ma odstranit</param>
        void RemoveComponent(Component component);

        /// <summary>
        /// Vrati komponent identifikovany podla modelu a vyrobcu
        /// </summary>
        /// <param name="manufacturer">vyrobca</param>
        /// <param name="model">model</param>
        /// <returns>vyhladany Component</returns>
        Component GetComponent(string manufacturer, string model);

        /// <summary>
        /// Vrati vsetky typy komponentov v Storagei
        /// </summary>
        /// <returns>Typy komponentov</returns>
        IEnumerable<ComponentType> GetComponentTypes();

        /// <summary>
        /// Vrati vsetky rozne vstupne connectory v storagei
        /// </summary>
        /// <returns>vsetky rozne vstupne connectory v storagei</returns>
        IEnumerable<string> GetInConnectors();

        /// <summary>
        /// Vyhlada komponenty podla zadanych parametrov
        /// </summary>
        /// <param name="search">Fraza pretextove vyhladavanie</param>
        /// <param name="inConnector">Vstupny connector</param>
        /// <param name="type">Typ komponentu</param>
        /// <returns>Kolekcia komponentov splnajucich parametre</returns>
        IEnumerable<Component> Query(string search, string inConnector, string type);

        /// <summary>
        /// Vrati typ podla nazvu
        /// </summary>
        /// <param name="name">Nazov typu</param>
        /// <returns>Hladany typ</returns>
        ComponentType GetTypeByName(string name);

        /// <summary>
        /// Vrati vsetky rozne connectory v Storagei
        /// </summary>
        /// <returns>Connectory</returns>
        IEnumerable<string> GetConnectors();

        /// <summary>
        /// Prida novy typ
        /// </summary>
        /// <param name="type">typ</param>
        void AddType(ComponentType type);

        /// <summary>
        /// Upravi typ
        /// </summary>
        /// <param name="old">Stary typ</param>
        /// <param name="editTo">Novy typ</param>
        /// <param name="changed">Maju sa premazat properties existujucich komponentov tohto typu?</param>
        void EditType(ComponentType old, ComponentType editTo, bool changed);

        /// <summary>
        /// Odstrani typ
        /// </summary>
        /// <param name="type">Typ, ktory sa ma odstranit</param>
        void RemoveType(ComponentType type);
    }
}
