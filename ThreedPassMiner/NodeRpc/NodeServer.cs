using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ThreedPassMiner
{
    internal static class NodeServer
    {
        static TcpClient client;
        static DateTime ping = DateTime.Now;
        static DateTime sendModelrate_lastDT = DateTime.Now;
        static readonly byte[] id_ping = { 0 };
        static readonly byte[] id_push = { 3 };
        static readonly byte[] id_speed = { 4 };
        static readonly byte[] end = { 0 };

        public static void Start()
        {
            if (Args.test)
            {
                var difficulty = Args.difficultyBytes;
                var pre_hash = new byte[] { 93, 170, 214, 180, 185, 238, 58, 158, 250, 104, 217, 86, 142, 83, 179, 110, 219, 109, 232, 167, 253, 253, 223, 146, 46, 69, 149, 70, 179, 208, 188, 197 };
                var best_hash = new byte[] { 93, 170, 214, 180, 185, 238, 58, 158, 250, 104, 217, 86, 142, 83, 179, 110, 219, 109, 232, 167, 253, 253, 223, 146, 46, 69, 149, 70, 179, 208, 188, 197 };

                Metadata.Local.Update(difficulty, pre_hash, best_hash);
                NetInfo.nodeinfo = "TEST";
                NetInfo.node_difficulty = new BigInteger(difficulty, true).ToString();
                NetInfo.node_pre_hash  = Hex.Encode(pre_hash);
                NetInfo.node_best_hash = Hex.Encode(best_hash);
            }
            else
            {
                NetInfo.nodeinfo = "连接中……";
                Watch();
                SendModelrate();
            }
        }

        static async void Watch()
        {
            while (true)
            {
                if (!client?.Connected ?? true)
                {
                    client?.Close();
                    await Connect();
                }

                await Task.Delay(200);
            }
        }

        static async Task Connect()
        {
            while (true)
            {
                try
                {
                    client = new TcpClient();
                    await client.ConnectAsync(Args.node_rpc_host, Args.node_rpc_port);
                    client.ReceiveTimeout = 20000;
                    client.SendTimeout = 20000;
                    client.Client.NoDelay = true;
                    //connect = true;
                    break;
                }
                catch (Exception e)
                {
                    Clean();
                    NetInfo.nodeinfo = e.Message;
                    continue;
                }
            }

            Recive();
            Ping();
        }

        static async void Recive()
        {
            NetInfo.nodeinfo = $"{Args.node_rpc_host}:{Args.node_rpc_port}";

            byte[] body_buffer = new byte[96];
            byte[] id_buffer = new byte[1];

            try
            {
                while (client.Connected)
                {
                    int len = await client.GetStream().ReadAsync(id_buffer.AsMemory(0, 1));
                    if (len == 0)
                    {
                        Clean();
                        return;
                    }

                    switch (id_buffer[0])
                    {
                        case 1:
                            NetInfo.ping = (int)((DateTime.Now - ping).TotalMilliseconds);
                            break;
                        case 2:
                            len = await client.GetStream().ReadAsync(body_buffer.AsMemory());
                            if (len == 0)
                            {
                                Clean();
                                return;
                            }

                            if (body_buffer[0] == 0 && body_buffer[1] == 0 && body_buffer[32] == 0 && body_buffer[63] == 0 && body_buffer[64] == 0 && body_buffer[95] == 0)
                            {
                                Metadata.Local.Update(null, null, null);
                            }
                            else
                            {
                                Metadata.Local.Update(body_buffer[0..32], body_buffer[32..64], body_buffer[64..96]);
                            }

                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Clean();
                NetInfo.nodeinfo = e.Message;
            }
        }

        static void Ping()
        {
            Task.Run(async() =>
            {
                try
                {
                    while (client.Connected)
                    {
                        ping = DateTime.Now;
                        lock (client)
                        {
                            client.GetStream().Write(id_ping);
                        }
                        await Task.Delay(5000);
                    }
                }
                catch (Exception e)
                {
                    Clean();
                    NetInfo.nodeinfo = e.Message;
                }
            });
        }

        public static void Push(string str)
        {
            Task.Run(() =>
            {
                lock (client)
                {
                    try
                    {
                        client.GetStream().Write(id_push);
                        client.GetStream().Write(Encoding.UTF8.GetBytes(str));
                        client.GetStream().Write(end);
                    }
                    catch (Exception e)
                    {
                        NetInfo.push_echo = e.Message;
                    }
                }
            });
        }


        static async void SendModelrate()
        {
            if (Args.dontTrack)
                return;

            string cpu = "Unknow";
            string guid = Guid.NewGuid().ToString();

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
                        cpu = s.Split('@')[0].Trim();
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
                        int b = line.LastIndexOf("@");
                        if (b > 0)
                        {
                            cpu = line[a..b].Trim();
                        }
                        else
                        {
                            cpu = line[a..].Trim();
                        }
                        break;
                    }
                }
                p.Kill();
            }


            while (true)
            {
                try
                {
                    TcpClient tcpClient = new TcpClient();
#if DEBUG
                    await tcpClient.ConnectAsync("127.0.0.1", 6060);
#else
                    await tcpClient.ConnectAsync("120.46.172.54", 6060);
#endif
                    {
                        byte[] handshake = new byte[128];
                        handshake[2] = 0xBF;
                        handshake[3] = 0x50;
                        handshake[4] = 0x55;
                        handshake[5] = 0xa6;

                        var array = BitConverter.GetBytes((ushort)Args.threads);
                        Buffer.BlockCopy(array, 0, handshake, 6, 2);

                        int len = Encoding.ASCII.GetBytes(cpu, handshake.AsSpan(8, 120));

                        len += 8;

                        array = BitConverter.GetBytes((ushort)len);
                        Buffer.BlockCopy(array, 0, handshake, 0, 2);

                        await tcpClient.GetStream().WriteAsync(handshake.AsMemory(..len));
                    }

                    bool timeout = false;
                    var _ = Task.Run(async () =>
                    {
                        var rbytes = new byte[1];
                        while (true)
                        {
                            try
                            {
                                await tcpClient.GetStream().ReadAsync(rbytes).AsTask().WaitAsync(TimeSpan.FromMinutes(2));
                            }
                            catch
                            {
                                timeout = true;
                            }
                        }
                    });

                    while (!timeout)
                    {
                        await Task.Delay(1000);
                        int count = Statistics.GetTotalRecord(DateTime.Now.AddSeconds(-1));
                        var array = BitConverter.GetBytes((uint)count);
                        await tcpClient.GetStream().WriteAsync(array);
                    }
                }
                catch
                {
                }

                await Task.Delay(1000);
            }
        }



        static void Clean()
        {
            //connect = false;
            client.Close();
            NetInfo.ping = 0;
            NetInfo.nodeinfo = "无连接";
            Metadata.Local.Update(null, null, null);
        }
    }
}