using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace RLCMacroReader
{
    class Program
    {
        public static SerialPort sp;
        
        
        static void Main(string[] args)
        {
            Console.WriteLine("Serial port name:");
            string portname = Console.ReadLine();

            Console.WriteLine("Please enter user number:");
            string user = Console.ReadLine();

            Console.WriteLine("Please enter password:");
            string pw = Console.ReadLine();

            sp = new SerialPort(portname, 19200, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 2500
            };

            try
            {
                sp.Open();
                string response = SendReceive(sp, "187 " + user + " " + pw + '\n');

                using (StreamWriter sw = new StreamWriter("macros.txt"))
                {
                    sw.WriteLine("Automatic macros...");

                    for (int m = 200; m < 500; m++)
                    {
                        Console.WriteLine("Checking macro {0}...", m);
                        response = SendReceive(sp, "054 " + m.ToString("000") + '\n');

                        if (!response.ToLower().Contains("this macro is 0 percent full"))
                        {
                            sw.WriteLine(response + Environment.NewLine);
                        }
                    }

                    sw.WriteLine("\nUser macros...");

                    for (int m = 500; m < 1000; m++)
                    {
                        Console.WriteLine("Checking macro {0}...", m);
                        response = SendReceive(sp, "054 " + m.ToString("000") + '\n');

                        if (!response.ToLower().Contains("this macro is 0 percent full"))
                        {
                            sw.WriteLine(response + Environment.NewLine);
                        }
                    }
                }

                response = SendReceive(sp, "189" + '\n');
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
            byte[] data = Encoding.ASCII.GetBytes(msg);

            try
            {
                sp.Write(data, 0, data.Length);

                Task.Delay(250).Wait();

                if (port.BytesToRead > 0)
                {
                    byte[] buf = new byte[port.BytesToRead];
                    port.Read(buf, 0, port.BytesToRead);
                    r = Encoding.ASCII.GetString(buf);
                }
            }

            catch (Exception ex)
            {

            }

            return r;
        }
    }


}
