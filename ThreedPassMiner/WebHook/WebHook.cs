using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ThreedPassMiner
{
    class WebHookParams {
        public string url;
        public string body;
        public string headers;
        public bool   httpEncode;
        public string dingEncrypt;
    }

    class WebHook
    {
        static readonly Regex regex = new Regex("(.+)[：:=]{1}(.+)");
        public static WebHookParams[]? webHookParams;

        internal static void Notice(string msg)
        {
            if (webHookParams != null)
            {
                Parallel.ForEach(webHookParams, p =>
                {
                    try { NoticeImpl(msg, p); }
                    catch { }
                });
            }
        }

        static string NoticeImpl(string msg, WebHookParams p)
        {
            using (var web = new WebClient() { Encoding = Encoding.UTF8 })
            {
                var url = p.url.Replace("#msg#", HttpUtility.UrlEncode(msg));

                if (url.Contains("dingtalk.com") && !string.IsNullOrEmpty(p.dingEncrypt))
                {
                    var timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                    string stringToSign = timestamp + "\n" + p.dingEncrypt;
                    byte[] signData = null;
                    using (var mac = new HMACSHA256(Encoding.UTF8.GetBytes(p.dingEncrypt)))
                        signData = mac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                    string sign = HttpUtility.UrlEncode(Convert.ToBase64String(signData));
                    url += $"&timestamp={timestamp}&sign={sign}";
                }

                var body = p.body.Replace("#msg#",
                    !p.httpEncode ? msg.Replace(@"\", @"\\") : HttpUtility.UrlEncode(msg.Replace(@"\", @"\\"))
                    );

                var matches = regex.Matches(p.headers);
                foreach (Match match in matches)
                    web.Headers.Add(match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim());

                if (string.IsNullOrWhiteSpace(body))
                    return web.DownloadString(url);
                else
                    return web.UploadString(url, body);
            }
        }
    }
}
