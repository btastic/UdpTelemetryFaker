using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using F1Telemetry;
using MessagePack;

namespace UdpTelemetryFaker
{
    internal class Program
    {
        private static readonly IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20777);

        private static List<BinaryPacket> GetLatestData()
        {
            var targetFile = @"data\16014670386178318361.f1s";

            var previousData = File.ReadAllBytes(targetFile);
            var packets = LZ4MessagePackSerializer.Deserialize<List<BinaryPacket>>(previousData);

            return packets;
        }

        private static void Main()
        {
            TimeSpan lastPackageTimeSpan = new TimeSpan();

            using (var udpSocket = new UdpClient())
            {
                udpSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                var latestData = GetLatestData();

                while (true)
                {
                    for (int i = 0; i < latestData.Count; i++)
                    {
                        var item = latestData[i];

                        if (!lastPackageTimeSpan.Equals(default(TimeSpan)))
                        {
                            Thread.Sleep(TimeSpan.FromTicks(item.TimeSpan.Ticks - lastPackageTimeSpan.Ticks).Milliseconds);
                        }

                        udpSocket.Send(item.Data, item.Data.Length, groupEP);

                        if (i == latestData.Count - 1)
                        {
                            lastPackageTimeSpan = default(TimeSpan);
                        }
                        else
                        {
                            lastPackageTimeSpan = item.TimeSpan;
                        }
                    }
                }
            }
        }
    }
}