using System;
using System.Net;
using System.Net.WebSockets;
using System.Numerics;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ThreedPassMiner
{
    internal static class NodeServer
    {
        //static readonly byte[] sendBuffer = Encoding.ASCII.GetBytes("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"poscan_getMeta\"}");
        //static byte[] recvBuffer = new byte[1024];

        public static void Start()
        {
            if (Args.test)
            {
                var difficulty = Args.difficultyBytes;
                var pre_hash  = new byte[] { 93, 170, 214, 180, 185, 238, 58, 158, 250, 104, 217, 86, 142, 83, 179, 110, 219, 109, 232, 167, 253, 253, 223, 146, 46, 69, 149, 70, 179, 208, 188, 197 };
                var best_hash = new byte[] { 93, 170, 214, 180, 185, 238, 58, 158, 250, 104, 217, 86, 142, 83, 179, 110, 219, 109, 232, 167, 253, 253, 223, 146, 46, 69, 149, 70, 179, 208, 188, 197 };

                Metadata.Local.Update(difficulty, pre_hash, best_hash);
                NetInfo.nodeinfo = "TEST";
                NetInfo.node_difficulty = $"[{string.Join(", ", difficulty)}]";
                NetInfo.node_pre_hash   = $"[{string.Join(", ", pre_hash  )}]";
                NetInfo.node_best_hash  = $"[{string.Join(", ", best_hash )}]";
            }
            else
            {
                new Thread(Loop) { IsBackground = true }.Start();
            }
        }

        //static async void Connect()
        //{
        //    try
        //    {
        //        ws = new ClientWebSocket();
        //        await ws.ConnectAsync(new Uri(url), CancellationToken.None);
        //        SendLoop();
        //        RecvLoop();
        //    }
        //    catch (Exception e)
        //    {
        //        Clean();
        //        NetInfo.nodeinfo = e.InnerException?.Message ?? e.Message;
        //        Log.LogError(e);
        //        await Task.Delay(1000);
        //        Connect();
        //    }


        //}

        //static async void RecvLoop()
        //{
        //    try
        //    {
        //        while (ws.State == WebSocketState.Open)
        //        {
        //            string json;
        //            try
        //            {
        //                var result = await ws.ReceiveAsync(recvBuffer, CancellationToken.None);
        //                json = Encoding.ASCII.GetString(recvBuffer, 0, result.Count);
        //            }
        //            catch (Exception e)
        //            {
        //                Clean();
        //                NetInfo.nodeinfo = e.InnerException?.Message ?? e.Message;
        //                Log.LogError(e);
        //                await Task.Delay(1000);
        //                break;
        //            }

        //            try
        //            {
        //                var jo = JObject.Parse(json);
        //                int? id = (int?)jo["id"];
        //                switch (id)
        //                {
        //                    case 1:
        //                        {
        //                            string? result = (string?)jo["result"];
        //                            if (result == null || result.Length != 192)
        //                            {
        //                                Clean();
        //                                NetInfo.nodeinfo = url;
        //                            }
        //                            else
        //                            {
        //                                var difficulty = Hex.Decode(result.Substring( 0, 64));
        //                                var pre_hash   = Hex.Decode(result.Substring(64, 64));

        //                                if (Metadata.Update(difficulty, pre_hash))
        //                                {
        //                                    Deque.Clear();

        //                                    NetInfo.nodeinfo = url;
        //                                    NetInfo.node_difficulty = $"[{string.Join(", ", difficulty)}]";
        //                                    NetInfo.node_pre_hash   = $"[{string.Join(", ", pre_hash)}]";
        //                                }
        //                            }
        //                        }
        //                        break;
        //                    case 2:
        //                        {
        //                            string? result = (string?)jo["result"];
        //                            Statistics.AddRecord(!(result == null || result != "0"));
        //                            NetInfo.push_echo = $"[{DateTime.Now}] {json}";
        //                        }
        //                        break;
        //                    default:
        //                        NetInfo.push_echo = $"[{DateTime.Now}] {json}";
        //                        break;
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Clean();
        //                NetInfo.nodeinfo = e.InnerException?.Message ?? e.Message;
        //                Log.LogError(e);
        //            }
        //        }
        //        Connect();
        //    }
        //    catch (Exception e)
        //    {
        //        Clean();
        //        NetInfo.nodeinfo = e.InnerException?.Message ?? e.Message;
        //        Log.LogError(e);
        //        await Task.Delay(1000);
        //        Connect();
        //    }
        //}

        //static async void SendLoop()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            await Task.Delay(200);
        //            await ws.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
        //        }
        //        catch
        //        {

        //        }
        //    }
        //}

        //static async void Loop(ClientWebSocket ws)
        //{
        //    try
        //    {
        //        while (ws.State == WebSocketState.Open)
        //        {
        //            await Task.Delay(200);

        //            string json;
        //            try
        //            {
        //                await ws.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
        //                var result = await ws.ReceiveAsync(recvBuffer, CancellationToken.None);
        //                json = Encoding.ASCII.GetString(recvBuffer,0, result.Count);
        //            }
        //            catch (Exception e)
        //            {
        //                Clean();
        //                NetInfo.nodeinfo = e.Message;
        //                Log.LogError(e);
        //                await Task.Delay(1000);
        //                break;
        //            }

        //            try
        //            {
        //                var jo = JObject.Parse(json);
        //                string? result = (string?)jo["result"];
        //                if (result == null || result.Length != 192)
        //                {
        //                    Clean();
        //                    NetInfo.nodeinfo = url;
        //                }
        //                else
        //                {
        //                    var difficulty = Hex.Decode(result.Substring(0, 64));
        //                    var pre_hash = Hex.Decode(result.Substring(64, 64));

        //                    if (Metadata.Update(difficulty, pre_hash))
        //                    {
        //                        Deque.Clear();

        //                        NetInfo.nodeinfo = url;
        //                        NetInfo.node_difficulty = $"[{string.Join(", ", difficulty)}]";
        //                        NetInfo.node_pre_hash = $"[{string.Join(", ", pre_hash)}]";
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Clean();
        //                NetInfo.nodeinfo = e.Message;
        //                Log.LogError(e);
        //            }
        //        }
        //        Connect();
        //    }
        //    catch (Exception e)
        //    {
        //        Clean();
        //        NetInfo.nodeinfo = e.Message;
        //        Log.LogError(e);
        //        await Task.Delay(1000);
        //        Connect();
        //    }
        //}

        static void Loop()
        {
            while (true)
            {
                Thread.Sleep(200);
                Todo();
            }
        }

        static void Todo()
        {
            try
            {
                string url = $"http://{Args.node_rpc_host}:{Args.node_rpc_port}";
                string json = "";
                using (var web = new WebClient())
                {
                    web.Headers["Content-Type"] = "application/json; charset=utf-8";
                    json = web.UploadString(url, "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"poscan_getMeta\"}");
                }

                NetInfo.nodeinfo = url;

                var jo = JObject.Parse(json);
                string? result = (string?)jo["result"];
                if (result == null)
                {
                    Clean();
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        var difficulty = Hex.Decode(result.AsSpan(  0, 64));
                        var pre_hash   = Hex.Decode(result.AsSpan( 64, 64));
                        var best_hash  = Hex.Decode(result.AsSpan(128, 64));

                        Metadata.Local.Update(difficulty, pre_hash, best_hash);

                        NetInfo.node_difficulty = $"{new BigInteger(difficulty, true)} [{string.Join(", ", difficulty)}]";
                        NetInfo.node_pre_hash   = $"[{string.Join(", ", pre_hash)}]";
                        NetInfo.node_best_hash  = $"[{string.Join(", ", best_hash)}]";
                    }
                    else
                    {
                        NetInfo.node_difficulty = null;
                        NetInfo.node_pre_hash   = null;
                        NetInfo.node_best_hash  = null;
                        Metadata.Local.Update(null, null, null);
                    }
                }
            }
            catch (Exception e)
            {
                Clean();
                NetInfo.nodeinfo = e.Message;
            }
        }

        static void Clean()
        {
            NetInfo.node_difficulty = null;
            NetInfo.node_pre_hash   = null;
            NetInfo.node_best_hash  = null;
            Metadata.Local .Update(null, null, null);
        }

        //public static async void Send(string msg)
        //{
        //    try
        //    {
        //        await ws.SendAsync(Encoding.ASCII.GetBytes(msg), WebSocketMessageType.Text, true, CancellationToken.None);
        //    }
        //    catch
        //    {
        //    }
        //}

    }
}