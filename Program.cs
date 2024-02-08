using System;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using isci.Allgemein;
using isci.Daten;
using isci.Beschreibung;
using Newtonsoft.Json.Linq;

namespace isci.monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            //Erstellung des Zugriffs auf die dateibasierte Datenstruktur unter Nutzung der Parametrierung.
            var structure = new Datenstruktur("/media/ramdisk", "monitor", "Demonstrator");

            //Hinzufügen aller als Dateien gespeicherte Datenmodelle im Standardordner.
            structure.DatenmodelleEinhängenAusOrdner("/OrdnerDatenmodelle");
            //Logischer Start der Datenstruktur.
            structure.Start();
            
            //Arbeitsschleife
            while(true)
            {
                System.Console.Clear();
                structure.Lesen();

                foreach (var eintrag in structure.dateneinträge)
                {
                    System.Console.WriteLine(eintrag.Key + ": " + eintrag.Value.WertSerialisieren());
                }

                System.Threading.Thread.Sleep(1000);                 
            }
        }
    }
}