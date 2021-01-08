using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightspeedSmartAgentUninstaller
{
    class Program
    {
        static void die(string rs)
        {
            ConsoleColor c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("");
            Console.WriteLine(rs);

            Console.ForegroundColor = c;

            Console.ReadKey();
            Environment.Exit(0);
        }

        static bool pStart(string path, string args = "", bool admin = false, bool vis = true)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = path;
            
            if (!String.IsNullOrEmpty(args))
            {
                psi.Arguments = args;
            }

            if (admin)
            {
                psi.UseShellExecute = true;
                psi.Verb = "runas";
            }

            if (!vis)
            {
                psi.WindowStyle = ProcessWindowStyle.Hidden;
            }

            try
            {
                Process.Start(psi);

                return true;
            } catch (Exception)
            {
                return false;
            }
        }

        static void Main(string[] args)
        {
            Console.Clear();
            Console.ResetColor();

            WindowsPrincipal pr = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            if (!pr.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine("Requesting admin rights...");

                if (pStart(Assembly.GetExecutingAssembly().Location, admin: true))
                {
                    Environment.Exit(0);
                } else
                {
                    die("Admin is required. Press any key to exit.");
                }
            }

            Console.WriteLine("Attempting to start looking... (If stuck on looking, you're not. It takes a while.)");

            if (pStart("C:\\Windows\\System32\\cmd.exe", "/C wmic.exe product where \"name like 'Lightspeed Smart Agent'\" call uninstall", vis: false))
            {
                Console.WriteLine("Looking... (May take a while. The computer will reboot automatically if uninstall succeeds [Maybe].)");
            } else
            {
                die("Failed to start looking. Press any key to exit.");
            }

            bool loc = false;
            bool suc = false;
            bool wmics = false;

            while (!wmics)
            {
                wmics = false;

                foreach (var i in Process.GetProcesses())
                {
                    if (i.ProcessName.ToLower() == "wmic")
                    {
                        wmics = true;
                    }
                }

                Thread.Sleep(40);
            }

            wmics = false;

            while (!loc)
            {
                wmics = false;

                foreach (var i in Process.GetProcesses())
                {
                    if (i.ProcessName.ToLower() == "wmic")
                    {
                        wmics = true;
                        continue;
                    }

                    if ((i.MainWindowTitle.ToLower() == "lightspeed smart agent setup" || (i.MainWindowTitle.ToLower().Contains("lightspeed") && i.MainWindowTitle.ToLower() != Process.GetCurrentProcess().MainWindowTitle.ToLower())) && i.Id != Process.GetCurrentProcess().Id)
                    {
                        loc = true;

                        Console.WriteLine("Found! Trying to uninstall...");

                        try
                        {
                            i.Kill();
                            suc = true;
                            break;
                        } catch (Exception)
                        {
                            suc = false;
                        }
                    }
                }

                if (!wmics)
                {
                    loc = true;
                    break;
                }

                Thread.Sleep(40);
            }

            if (suc)
            {
                ConsoleColor c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine("\nUninstall succeeded! Attempting to reboot...");
                Console.ReadKey();

                Environment.Exit(0);
            } else
            {
                die("Uninstall Failed. Press any key to exit.");
            }
        }
    }
}
