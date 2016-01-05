using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using SpheroNET;

namespace SpheroTest
{
    class Program
    {

        private static IEnumerable<string> GetOrbBasicLinesFromFile(string filePath)
        {
            var rawLines = File.ReadLines(filePath, Encoding.UTF8);
            var result = new List<string>();
            foreach(var line in rawLines)
            { 
                 if (!string.IsNullOrEmpty(line) && line[0] != '\'')
                 {
                     result.Add(line + "\r");
                 }
            }
            return result;
        }

        static void Main(string[] args)
        {
            SpheroConnector spheroConnector = new SpheroConnector();
            Sphero sphero = null;

            string[] parameters = new string[] { "none" };
            string command = "none";
            while (!string.IsNullOrEmpty(command))
            {
                command = parameters[0];
                switch (command)
                {
                    case "find":
                        spheroConnector.Scan();
                        var deviceNames = spheroConnector.DeviceNames;
                        for (int i = 0; i < deviceNames.Count; i++)
                        { 
                            Console.WriteLine("{0}: {1}", i, deviceNames[i]);
                        }
                        break;
                    case "connect":
                        if (parameters.Length < 2) break;
                        int index = -1;
                        if (int.TryParse(parameters[1], out index))
                        {
                            try
                            {
                                sphero = spheroConnector.Connect(index);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else 
                        {
                            Console.WriteLine("'{0}' is not a valid device index.", parameters[1]);
                        }
                        break;
                    case "close":
                        spheroConnector.Close();
                        break;
                     case "sleep":
                        sphero.Sleep();
                        break;
                    case "setcolor":
                        byte r, g, b;
                        try
                        {
                            r = byte.Parse(parameters[1]);
                            g = byte.Parse(parameters[2]);
                            b = byte.Parse(parameters[3]);
                        }
                        catch 
                        {
                            string name = parameters[1];
                            Color c = Color.FromName(name);
                            r = c.R;
                            g = c.G;
                            b = c.B;
                        }                     
                        sphero.SetRGBLEDOutput(r, g, b);
                        break;
                    case "getresponses":
                        {
                            if (parameters.Length < 2) break;
                            int count;
                            if (int.TryParse(parameters[1], out count))
                            {
                                lock (sphero.Listener.SyncRoot)
                                {
                                    IEnumerable<SpheroResponsePacket> packets = sphero.Listener.GetLastResponsePackets(count);
                                    foreach (var packet in packets)
                                    {
                                        Console.WriteLine(packet);
                                    }
                                }
                            }
                        }
                        break;
                    case "getasync":
                        {       
                            if (parameters.Length < 2) break;
                            int count;
                            if (int.TryParse(parameters[1], out count))
                            {
                                lock (sphero.Listener.SyncRoot)
                                {
                                    IEnumerable<SpheroAsyncPacket> packets = sphero.Listener.GetLastAsyncPackets(count);
                                    foreach (var packet in packets)
                                    {
                                        Console.WriteLine(packet);
                                    }
                                }
                            }
                        }
                        break;
                    case "exit":
                        spheroConnector.Close();
                        return;
                    case "sendprogram":
                        {
                            var area = StorageArea.Temporary;
                            IEnumerable<string> programLines = GetOrbBasicLinesFromFile("orbbasic.txt");
                            foreach (var programLine in programLines)
                            {
                                Console.WriteLine(programLine);
                            }
                            sphero.EraseOrbBasicStorage(area);
                            sphero.SendOrbBasicProgram(area, programLines);
                        }
                        break;
                    case "runprogram":
                        {
                            StorageArea area = StorageArea.Temporary;
                            sphero.ExecuteOrbBasicProgram(area, 10);
                        }
                        break;
                    case "abortprogram":
                        sphero.AbortOrbBasicProgram();
                        break;
                    case "diagnostics":
                        {
                            sphero.PerformLevelOneDiagnostics();
                        }
                        break;
                }
                Console.Write("> ");
                parameters = Console.ReadLine().Split(new char[] { ' ' });
            }
        }
    }
}
