using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using UnityEngine;

namespace UTool.Utility
{
    public static partial class UUtility
    {
        public static byte[] ToUTF8(this string stringData) => Encoding.UTF8.GetBytes(stringData);
        public static string ToUTF8String(this byte[] UTF8Data) => Encoding.UTF8.GetString(UTF8Data);

        public static string ToBase64(this byte[] Base64Data) => Convert.ToBase64String(Base64Data);
        public static byte[] ToBase64Byte(this string stringData) => Convert.FromBase64String(stringData);

        public static string GetLocalIPAddress()
        {
            string localIpAddress = "127.0.0.1";
            try
            {
                string hostName = Dns.GetHostName();
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);

                foreach (IPAddress ipAddress in hostEntry.AddressList)
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIpAddress = ipAddress.ToString();
                        break;
                    }
            }
            catch (Exception ex)
            {
                NetLog($"Error Getting Local IP Address: {ex.Message}");
            }

            return localIpAddress;
        }

        public static async Task<List<string>> GetLocalNetworkIPAddresses()
        {
            List<string> ips = new List<string>();

            string localIpAddress = GetLocalIPAddress();
            string[] ipParts = localIpAddress.Split('.');
            string baseIpAddress = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}";

            List<Task> pingTasks = new List<Task>();

            for (int i = 0; i <= 255; i++)
            {
                if (ipParts[3] == i.ToString())
                    continue;

                string targetIpAddress = $"{baseIpAddress}.{i}";

                Task pingTask = new Task(() =>
                {
                    System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                    PingReply reply = null;

                    try
                    {
                        reply = ping.Send(targetIpAddress, 100);

                        if (reply.Status == IPStatus.Success)
                            ips.Add(targetIpAddress);
                    }
                    catch { }
                });

                pingTasks.Add(pingTask);
            }

            pingTasks.ForEach(pt => pt.Start());
            await Task.WhenAll(pingTasks);

            return ips;
        }

        public static void NetLog(int logType, string message, bool log = true)
            => Log(logType, message, log);

        public static void NetLog(string message, bool log = true)
            => Log(0, message, log);

        public static void NetWarn(string message, bool log = true)
            => Log(1, message, log);

        public static void NetError(string message, bool log = true)
            => Log(2, message, log);

        private static void Log(int logType, string message, bool log)
        {
            string logMsg = $"[UNetComm] : {message}";

            switch (logType)
            {
                case 0:
                    if (log)
                        Debug.Log(logMsg);
                    break;

                default:
                case 1:
                    if (log)
                        Debug.LogWarning(logMsg);
                    break;

                case 2:
                    Debug.LogError(logMsg);
                    break;
            }
        }
    }
}