using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Serialization;
using CodemastersReader;

namespace UdpTelemetryFaker
{
    internal class Program
    {
        private static readonly IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20777);

        private static void Main()
        {
            var latestData = new List<TelemetryPacket>();

            var udpSocket = new UdpClient();
            udpSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            latestData = GetLatestData(latestData);

            while (true)
            {
                foreach (var item in latestData)
                {
                    byte[] bts = StructureToByteArray(item);
                    udpSocket.Send(bts, bts.Length, groupEP);
                    Console.WriteLine(item);
                    Thread.Sleep(TimeSpan.FromSeconds(1.0 / 30.0));
                }
            }
        }

        private static List<TelemetryPacket> GetLatestData(List<TelemetryPacket> latestData)
        {
            FileStream FileStream = File.Open(@"D:\temp\lap.xml", FileMode.Open);
            var XmlSerializer = new XmlSerializer(latestData.GetType());
            latestData = (List<TelemetryPacket>)XmlSerializer.Deserialize(FileStream);
            FileStream.Close();
            return latestData;
        }

        private static byte[] StructureToByteArray(object obj)
        {
            int len = Marshal.SizeOf(obj);
            byte[] arr = new byte[len];
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
    }
}
