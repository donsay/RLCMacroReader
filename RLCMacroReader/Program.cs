// RLCMacroReader by Don Sayler W7OXR
using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RLCMacroReader
{
    class Program
    {
        public static SerialPort sp;
        public static Settings settings;
        public static readonly string _TIMEOUT = "TIMEOUT";
        
        static void Main(string[] args)
        {
           switch (args.Length)
           {
                case 0:
                    Console.WriteLine("please specify options:" + Environment.NewLine);
                    ShowHelp();
                    break;

                case 1:
                    switch (args[0].ToLower())
                    {
                        case "cfg":
                            ShowCommSettings();
                            break;

                        case "f":
                            Console.WriteLine("Usage is: RLCMacroReader f <filename>");
                            return;
                        
                        default:
                            Console.WriteLine("'{0}' is not supported.", args[0]);
                            break;
                    }

                    break;
                
                case 2:
                    switch (args[0].ToLower())
                    {
                        case "cfg":
                            if(!SetCommSettings(args[1]))
                            {
                                string msg = "Invalid comm settings. Usage is" + Environment.NewLine +
                                             "RLCMacroReader cfg <portname,baudrate,parity,databits,stopbits>" + Environment.NewLine +
                                             "for example: RLCMacroReader cfg com4,19200,None,8,1";

                                Console.WriteLine(msg);
                            }

                            break;

                        case "f":
                            ReadMacros(args[1]);
                            break;
                        
                        default:
                            Console.WriteLine("'{0}' is not supported.", args[0]);
                            break;
                    }

                    break;

                default:
                    Console.WriteLine("Unsupported number of arguments. See usage below:" + Environment.NewLine);
                    ShowHelp();
                    break;
            }
        }

        private static bool SetCommSettings(string commsettings)
        {
            bool stat = true;

            string[] p = commsettings.Split(",");

            if (p.Length != 5)
            {
                return false;
            }

            settings = new Settings
            {
                PortName = p[0]
            };

            if (!int.TryParse(p[1], out settings.Baudrate))
            {
                return false;
            }

            // parity need to be fully spelled out, first letter capitalized;
            // so handle the common errors
            switch (p[2])
            {
                case "E":
                case "e":
                case "even":
                    p[2] = "Even";
                    break;

                case "N":
                case "n":
                case "none":
                    p[2] = "None";
                    break;

                case "O":
                case "o":
                case "odd":
                    p[2] = "Odd";
                    break;
            }

            if (!Enum.TryParse<Parity>(p[2], out settings.Parity))
            {
                return false;
            }

            if (!int.TryParse(p[3], out settings.Databits))
            {
                return false;
            }

            if (!Enum.TryParse<StopBits>(p[4], out settings.StopBits))
            {
                return false;
            }

            SaveSettings();
            ShowCommSettings();
            return stat;
        }

        private static void SaveSettings()
        {
            XmlSerializer xmlWriter = new XmlSerializer(typeof(Settings));

            using (FileStream fs = File.Create("settings.xml"))
            {
                xmlWriter.Serialize(fs, settings);
            }
        }

        private static bool LoadSettings()
        {
            bool stat = true;
            settings = new Settings();

            if (File.Exists("settings.xml"))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));

                try
                {
                    using (StreamReader file = new StreamReader("settings.xml"))
                    {
                        settings = (Settings)xmlSerializer.Deserialize(file);
                    }
                }

                catch (Exception ex)
                {
                    stat = false;
                }
            }

            return stat;
        }

        private static void ShowCommSettings()
        {
            string msg = "";

            if (LoadSettings())
            {
                Console.WriteLine("Serial Port Settings:");

                msg = "Portname: " + settings.PortName + Environment.NewLine +
                      "Baudrate: " + settings.Baudrate.ToString() + Environment.NewLine +
                      "Parity: " + settings.Parity.ToString() + Environment.NewLine +
                      "Databits: " + settings.Databits.ToString() + Environment.NewLine +
                      "Stopbits: " + settings.StopBits.ToString();
            }

            else
            {
                msg = "Unable to read comm settings.";
            }

            Console.WriteLine(msg);                 
        }

        private static void ShowDisclaimer()
        {
            string msg = "RLCMacroReader is provided FREE of charge." + Environment.NewLine +
                         "Reasonable effort has been made to insure that it" + Environment.NewLine +
                         "will not have any adverse effects on connected devices." + Environment.NewLine +
                         "However, the author assumes no liability for any malfunction or loss of data." + Environment.NewLine;

            Console.WriteLine(msg);
        }



        private static void ShowHelp()
        {
            Console.WriteLine("RLCMacroReader [options]");

            string msg = "[cfg] - show comm settings" + Environment.NewLine +
                         "[cfg <portname,baudrate,parity,databits,stopbits>] comm settings, ie:com3,19200,None,8,1" + Environment.NewLine +
                         "[f <filename>] file to save macros to";

            Console.WriteLine(msg);
        }

        static void ReadMacros(string filename)
        {
            bool ok = true;

            if (!LoadSettings())
            {
                Console.WriteLine("Cancelled. Could not load comm settings.");
                return;
            }

            sp = new SerialPort(settings.PortName, settings.Baudrate, settings.Parity, settings.Databits, settings.StopBits)
            {
                ReadTimeout = 2500,
            };
          
            try
            {
                ShowDisclaimer();

                // log in
                string msg = "If macro commands are not under a security level," + Environment.NewLine +
                             "just press <Enter> for the next two prompts." + Environment.NewLine +
                             "To exit, enter 'exit' at the user prompt and press <Enter>.";

                Console.WriteLine(msg);

                Console.WriteLine("Please enter user number:");
                string user = Console.ReadLine();

                if (user.ToLower() == "exit")
                {
                    return;
                }

                Console.WriteLine("Please enter password:");
                string pw = Console.ReadLine();

                sp.Open();
                string response;

                // if no user and password, just assume all is OK;
                // otherwise use them to sign in
                if (user.Length > 0 && pw.Length > 0)
                {
                    response = SendReceive(sp, "187 " + user + " " + pw);

                    if (response == _TIMEOUT)
                    {
                        Console.WriteLine("Timeout, controller did not respond.");
                        return;
                    }
                }

                else
                {
                    response = "logged in";
                }

                FileInfo fi = new FileInfo(filename);
                string fname = fi.Name.Remove(fi.Name.IndexOf(fi.Extension));
                fname = fname + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + fi.Extension;

                Console.WriteLine("Here we go. This could take a while.");
                if (ok && response.ToLower().Contains("logged in"))
                {
                    using (StreamWriter sw = new StreamWriter(fname))
                    {
                        // record the Automatic macros
                        sw.WriteLine("Automatic macros...");

                        for (int m = 200; m < 500; m++)
                        {
                            Console.WriteLine("Checking macro {0}...", m);
                            response = SendReceive(sp, "054 " + m.ToString("000"));

                            if (response == _TIMEOUT || response.Contains("Error"))
                            {
                                ok = false;
                            }

                            if (!ok)
                            {
                                Console.WriteLine(response);
                                return;
                            }

                            else if (!response.ToLower().Contains("this macro is 0 percent full"))
                            {
                                sw.WriteLine(response + Environment.NewLine);
                            }
                        }

                        sw.WriteLine("\nUser macros...");
                        
                        if (ok)
                        {
                            for (int m = 500; m < 1000; m++)
                            {
                                Console.WriteLine("Checking macro {0}...", m);
                                response = SendReceive (sp, "054 " + m.ToString("000"));

                                if (response == _TIMEOUT || response.Contains("Error"))
                                {
                                    ok = false;
                                }

                                if (!ok)
                                {
                                    Console.WriteLine(response);
                                    return;
                                }

                                else if (!response.ToLower().Contains("this macro is 0 percent full"))
                                {
                                    sw.WriteLine(response + Environment.NewLine);
                                }
                            }
                        }
                    }

                    response = SendReceive(sp, "189");
                }

                else
                {
                    Console.WriteLine("Unable to sign in to controller.");
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            if (sp.IsOpen)
            {
                sp.Close();
            }
        }

        private static string SendReceive(SerialPort port, string msg)
        {
            string r = "";
            int rxBufSize = 1024;
            byte[] rxBuf = new byte[rxBufSize];
            byte[] data = Encoding.ASCII.GetBytes(msg + '\r' + '\n');

            try
            {
                string RxCmd = "";
                string TxCmd = msg.Substring(0, 3);

                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                sp.Write(data, 0, data.Length);

                Task.Delay(2000).Wait();

                int b;
                int p = 0;
                bool done = false;

                while (!done && (p < rxBufSize))
                {
                    b = port.ReadByte();
                    rxBuf[p] = (byte)b;

                    if (p > 4)
                    {
                        // stop when we see 'DTMF>'
                        if (rxBuf[p] == '>' && rxBuf[p - 1] == 'F' && rxBuf[p - 2] == 'M' && rxBuf[p - 3] == 'T' && rxBuf[p - 4] == 'D')
                        {
                            done = true;
                        }
                    }

                    p++;
                }

                if (p > 0)
                {
                    byte[] buf = new byte[p];
                    Array.Copy(rxBuf, 0, buf, 0, p);
                    r = Encoding.ASCII.GetString(buf);
                    RxCmd = r.Substring(0, 3);

                    // if the command has been returned in the result, take the command out
                    int pos;

                    if (RxCmd == TxCmd)
                    {
                        pos = r.IndexOf('\n', 0);
                        r = r.Substring(pos + 1, r.Length - pos -1);
                    }

                    // now strip off all the termininating stuff
                    pos = r.IndexOf("OK", 0);

                    if (pos > -1)
                    {
                        r = r.Substring(0, pos);
                    }
                }
            }

            catch (TimeoutException)
            {
                r = _TIMEOUT;
            }

            catch (Exception ex)
            {
                r = ex.Message;
            }

            return r;
        }
    }
}
