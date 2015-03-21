using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Tools.Communication;

namespace Controller
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private static Controller controller;
        private static bool exit = false;

        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            log.InfoFormat("Start controller");

            controller = new Controller();

            HandleConsoleInput();
        }

        private static void HandleConsoleInput()
        {
            DataColumn key = new DataColumn("Key", typeof(char));
            DataColumn entry = new DataColumn("Entry", typeof(string));
            DataColumn action = new DataColumn("Action", typeof(Action));

            DataTable menu = new DataTable();
            menu.Columns.AddRange(new [] { key, entry, action });
            menu.Rows.Add("1", "Test communication", (Action)ComTest);
            //menu.Rows.Add("2", "Stop Shuttle Storage In controller", (Action)StopShuttleStorageIn);
            //menu.Rows.Add("3", "Start Shuttle Storage In controller", (Action)StartShuttleStorageIn);
            //menu.Rows.Add("4", "Listen for Shuttle Storage In status changes", (Action)ListenShuttleEvents);
            //menu.Rows.Add("5", "Listen for Conveyor Storage Lane 1 status changes", (Action)ListenConveyorEvents);
            menu.Rows.Add("x", "Exit", (Action)Exit);

            while (!exit)
            {
                Console.WriteLine();
                foreach (DataRow row in menu.Rows)
                {
                    Console.WriteLine("{0} {1}", row.Field<char>(key), row.Field<string>(entry));
                }

                Console.WriteLine();
                Console.Write("Please choose a menu option: ");

                char input = Console.ReadKey().KeyChar;
                Console.WriteLine();

                Action a = menu.AsEnumerable()
                    .Where(row => row.Field<char>(key)
                        .ToString().Equals(
                        input.ToString(), StringComparison.OrdinalIgnoreCase))
                    .Select(row => row.Field<Action>(action))
                    .FirstOrDefault();

                if (a == null)
                {
                    Console.WriteLine("No entry {0} in menu, choose again", input);
                    continue;
                }

                a.Invoke();

            }

        }

        private static void ComTest()
        {
            controller.Start();

            Console.WriteLine("Com test, s for start ...");
            string action;
            while ((action = Console.ReadLine()) != null)
            {
                switch (action)
                {
                    case "exit":
                    case "x":
                        exit = true;
                        break;
                    case "start":
                    case "s":
                        controller.SendTestData();
                        break;
                    case "stop":
                        break;
                    default:
                        Console.WriteLine("Invalid command");
                        break;
                }

                if (exit)
                {
                    return;
                }
            }

        }

        private static void Exit()
        {
            exit = true;
        }
    }
}
