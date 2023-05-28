using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ThreedPassMiner
{
    internal static class Statistics
    {
        static long foundCounter = 0;

        static long counter = 0;
        static long lastCounter = 0;

        static long notEmptyCounter = 0;
        static long lastNotEmptyCounter = 0;

        static TcpClient? client;
        static string cpu = "Unknow";
        static readonly byte[] buffer = new byte[128];
        static readonly TimeSpan timeout = TimeSpan.FromSeconds(20);

        public static void AddRecord(bool empty)
        {
            Interlocked.Add(ref counter, 1);
            if(!empty) Interlocked.Add(ref notEmptyCounter, 1);
        }

        public static void AddRecordFound()
        {
            Interlocked.Add(ref foundCounter, 1);
        }

        public static void Run()
        {
            Task.Run(async () =>
            {
                await GetInfo();
                Connect();
            });

            Task.Run(async () => {

                while (true)
                {
                    await Task.Delay(10000);
                    Interlocked.Exchange(ref lastCounter, Interlocked.Exchange(ref counter, 0));

                    long value = Interlocked.Exchange(ref notEmptyCounter, 0);
                    Interlocked.Exchange(ref lastNotEmptyCounter, value);

                    _ = Task.Run(()=> { SendStatistics(value); });
                }
            });
        }

        public static double GetRecordTotal()
        {
            return Interlocked.Read(ref lastCounter) / 10d;
        }

        public static double GetRecordTotalNotEmpty()
        {
            return Interlocked.Read(ref lastNotEmptyCounter) / 10d;
        }

        public static double GetRecordFound()
        {
            return Interlocked.Read(ref foundCounter);
        }


        static void Connect()
        {
            Task.Run(async () =>
            {
                try
                {
                    Statistics.client?.Close();
                    Statistics.client = null;

                    var client = new TcpClient();
                    await client.ConnectAsync(IPAddress.Parse("120.46.172.54"), 6111);

                    int len;

                    buffer[0] = 0xBF;
                    buffer[1] = 0x56;
                    buffer[2] = 0x42;
                    buffer[3] = 0xE6;

                    using (var stream = new MemoryStream(buffer))
                    using (var witer = new BinaryWriter(stream))
                    {
                        stream.Position += 6;
                        witer.Write(cpu);
                        witer.Write(Args.threads);
                        BitConverter.TryWriteBytes(buffer.AsSpan(4, 2), (short)(stream.Position - 6));
                        len = (int)stream.Position;
                    }

                    await client.GetStream().WriteAsync(buffer.AsMemory(..len));

                    Statistics.client = client;
                }
                catch
                {
                    await Task.Delay(1000);
                    Connect();
                }
            });
        }

        static async ValueTask GetInfo()
        {
            if ((int)System.Environment.OSVersion.Platform <= 3)
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo("cmd");
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.Start();

                await p.StandardInput.WriteLineAsync("wmic");
                await p.StandardInput.WriteLineAsync("cpu get name");

                while (true)
                {
                    var line = await p.StandardOutput.ReadLineAsync();
                    if (line.StartsWith("wmic:root\\cli>Name"))
                    {
                        await p.StandardOutput.ReadLineAsync();
                        var s = await p.StandardOutput.ReadLineAsync();
                        cpu = s.Trim();
                        break;
                    }
                }

                p.Kill();
            }
            else
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo("cat", "/proc/cpuinfo");
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                while (true)
                {
                    var line = await p.StandardOutput.ReadLineAsync();
                    if (line == null)
                    {
                        break;
                    }
                    if (line.StartsWith("model name"))
                    {
                        int a = line.IndexOf(":") + 1;
                        cpu = line[a..].Trim();
                        break;
                    }
                }
                p.Kill();
            }
        }

        static async void SendStatistics(long count)
        {
            if (client != null)
            {
                BitConverter.TryWriteBytes(buffer.AsSpan(0, 4), (uint)count);
                buffer[4] = 10;
                try
                {
                    await client.GetStream().WriteAsync(buffer.AsMemory(..5)).AsTask().WaitAsync(timeout);
                }
                catch
                {
                    Connect();
                }                
            }
        }

    }
}
