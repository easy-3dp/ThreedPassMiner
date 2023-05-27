using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace ThreedPassMiner
{
    internal static class NodeServer
    {
        static TcpClient client;
        static DateTime ping = DateTime.Now;
        static readonly byte[] id_ping = { 0 };
        static readonly byte[] id_push = { 3 };
        static readonly byte[] id_speed = { 4 };
        static readonly byte[] end = { 0 };
        static byte[] buffer;

        public static void Start()
        {
            if (Args.test)
            {
                var difficulty = Args.difficultyBytes;
                var pre_hash = new byte[] { 93, 170, 214, 180, 185, 238, 58, 158, 250, 104, 217, 86, 142, 83, 179, 110, 219, 109, 232, 167, 253, 253, 223, 146, 46, 69, 149, 70, 179, 208, 188, 197 };
                var best_hash = new byte[] { 93, 170, 214, 180, 185, 238, 58, 158, 250, 104, 217, 86, 142, 83, 179, 110, 219, 109, 232, 167, 253, 253, 223, 146, 46, 69, 149, 70, 179, 208, 188, 197 };

                var bytes = new byte[96];
                difficulty.CopyTo(bytes.AsSpan(0..32));
                pre_hash.CopyTo(bytes.AsSpan(32..64));
                best_hash.CopyTo(bytes.AsSpan(64..96));

                Metadata.Update(bytes);
                NetInfo.nodeinfo = "TEST";
                NetInfo.node_difficulty = new BigInteger(difficulty, true).ToString();
                NetInfo.node_pre_hash  = Hex.Encode(pre_hash);
                NetInfo.node_best_hash = Hex.Encode(best_hash);
            }
            else
            {
                buffer = Args.isSolo ? new byte[96] : new byte[209];
                NetInfo.nodeinfo = "连接中……";
                Connect();
                Ping();
            }
        }

        static async void Connect()
        {
            try
            {
                client?.Close();
                client = new TcpClient();
                await client.ConnectAsync(Args.node_rpc_host, Args.node_rpc_port);
                client.Client.NoDelay = true;
                NetInfo.nodeinfo = $"{Args.node_rpc_host}:{Args.node_rpc_port}";
                Recive();
            }
            catch (Exception e)
            {
                Clean();
                NetInfo.nodeinfo = e.Message;
                await Task.Delay(1000);
                Connect();
            }
        }

        static async void Recive()
        {
            await Task.Yield();

            try
            {
                while (client.Connected)
                {
                    int len = await client.GetStream().ReadAsync(buffer.AsMemory(0, 1));
                    if (len == 0)
                    {
                        throw new Exception("Connect break");
                    }

                    switch (buffer[0])
                    {
                        default:
                            throw new Exception("ERROR DATA");
                        case 1:
                            NetInfo.ping = (int)((DateTime.Now - ping).TotalMilliseconds);
                            break;
                        case 2:
                            len = 0;
                            do len += await client.GetStream().ReadAsync(buffer.AsMemory(len..));
                            while (len < buffer.Length);
                            Metadata.Update(buffer);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Clean();
                NetInfo.nodeinfo = e.Message;
            }
            finally
            {
                await Task.Delay(1000);
                Connect();
            }
        }

        static void Ping()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        lock (client)
                        {
                            ping = DateTime.Now;
                            client.GetStream().Write(id_ping);
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        await Task.Delay(5000);
                    }
                }
            });
        }

        public static void Push(string str)
        {
            Task.Run(() =>
            {
                try
                {
                    lock (client)
                    {
                        client.GetStream().Write(id_push);
                        client.GetStream().Write(Encoding.UTF8.GetBytes(str));
                        client.GetStream().Write(end);
                    }
                }
                catch (Exception e)
                {
                    NetInfo.push_echo = e.Message;
                }
            });
        }

        static void Clean()
        {
            //connect = false;
            client.Close();
            NetInfo.ping = 0;
            NetInfo.nodeinfo = "无连接";
            Metadata.Close();
        }
    }
}