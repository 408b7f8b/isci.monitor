using System;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using isci.Allgemein;
using isci.Daten;
using isci.Beschreibung;
using System.Reflection.Emit;

namespace isci.monitor
{
    class Program
    {
        public static void ClearVisibleRegion()
        {
            int cursorTop = Console.CursorTop;
            int cursorLeft = Console.CursorLeft;
            for(int y = Console.WindowTop; y < Console.WindowTop + Console.WindowHeight; y++) {
                Console.SetCursorPosition(Console.WindowLeft, y);
                Console.Write(new string(' ', Console.WindowWidth));
            }

            Console.SetCursorPosition(Console.WindowLeft, Console.WindowTop);
        }

        static void DarstellungAktualisieren(object state)
        {
            System.Console.Clear();
            //ClearVisibleRegion();
            //Console.SetCursorPosition(Console.WindowLeft, Console.WindowTop);

            foreach (var eintrag in structure.dateneinträge)
            {
                string ausgabe = eintrag.Key + " [" + zeitstempel[eintrag.Value.Identifikation] + "]: " + eintrag.Value.WertSerialisieren();
                /* for (int i = 0; i < Console.WindowWidth-ausgabe.Length; ++i)
                {
                    ausgabe += " ";
                } */
                Console.WriteLine(ausgabe);                
            }
        }

        static void WerteAktualisieren(object state)
        {
            var aenderungen = structure.Lesen();
            structure.Zustand.WertAusSpeicherLesen();

            var zeitstempel_tmp = DateTime.Now.ToString("O");
            foreach (var aenderung in aenderungen)
            {
                zeitstempel[aenderung] = zeitstempel_tmp;
                structure[aenderung].aenderungExtern = false;
            }
        }

        static void Neustarten(object source, System.IO.FileSystemEventArgs e)
        {
            neustarten = true;
        }

        static bool neustarten;

        static Datenstruktur structure;

        static Dictionary<string, string> zeitstempel;

        static void Main(string[] args)
        {
            if (args.Length < 5) {
                Console.WriteLine("Parameter für Start:");
                Console.WriteLine("1.: Name des Automatisierungssystems");
                Console.WriteLine("2.: Lokaler Ordner der Datenstrukturen");
                Console.WriteLine("3.: Lokaler Ordner der Datenmodelle");
                Console.WriteLine("4.: Pause zwischen Darstellungsaktualisierungen in ms");
                Console.WriteLine("5.: Pause zwischen Lesezugriffen in ms");
                Console.WriteLine("Beispiel:");
                Console.WriteLine("./isci.monitor Demonstrator ~/Datenstrukturen ~/Anwendungen/Demonstrator/Datenmodelle 500 10");
                System.Environment.Exit(0);
            }

            isci.Logger.aktiv = false;

            start:
            
            //Erstellung des Zugriffs auf die dateibasierte Datenstruktur unter Nutzung der Parametrierung.
            structure = new Datenstruktur(args[1], "monitor", args[0]);

            //Hinzufügen aller als Dateien gespeicherte Datenmodelle im Standardordner.
            structure.DatenmodelleEinhängenAusOrdner(args[2]);
            //Logischer Start der Datenstruktur.
            structure.Start();

            var watcher = new System.IO.FileSystemWatcher();
            watcher.Path = args[2];
            watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.DirectoryName;
            watcher.Filter = "*.json"; // Filter for JSON files

            watcher.Changed += Neustarten;
            watcher.Created += Neustarten;
            watcher.Deleted += Neustarten;
            watcher.Renamed += Neustarten;

            watcher.EnableRaisingEvents = true;

            zeitstempel = new Dictionary<string, string>();
            var zeitstempel_tmp = DateTime.Now.ToString("O");
            foreach (var eintrag in structure.dateneinträge)
            {
                zeitstempel.Add(eintrag.Value.Identifikation, zeitstempel_tmp);
            }

            var functionXTimer = new System.Threading.Timer(DarstellungAktualisieren, null, 0, System.Int32.Parse(args[3]));
            var functionYTimer = new System.Threading.Timer(WerteAktualisieren, null, 0, System.Int32.Parse(args[4]));

            //Arbeitsschleife
            while (!neustarten)
            {
                //Detektieren ob sich an den Dateien im Ordner was geändert hat
                System.Threading.Thread.Sleep(10000);
            }

            functionXTimer.Dispose();
            functionYTimer.Dispose();

            neustarten = false;
            goto start;
        }
    }
}