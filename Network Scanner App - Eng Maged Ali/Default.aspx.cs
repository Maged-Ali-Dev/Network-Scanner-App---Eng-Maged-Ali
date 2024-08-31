using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace Network_Scanner_App___Eng_Maged_Ali
{
    public partial class Default : System.Web.UI.Page
    {
        private static Dictionary<string, string> previousLatencies = new Dictionary<string, string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initial page load logic if needed
            }
        }


        protected async void btnScan_Click(object sender, EventArgs e)
        {
            await UpdateDeviceData();
        }

        private async Task UpdateDeviceData()
        {
            string routerIP = GetDefaultGateway();
            string localIP = GetLocalIPAddress();

            if (routerIP != null)
            {
                string subnet = GetSubnet(routerIP);
                if (subnet != null)
                {
                    List<Device> devices = await ScanNetwork(subnet, routerIP, localIP);
                    BindDevicesToGrid(devices);
                }
                else
                {
                    // Handle subnet detection failure
                    // You could add an error message or logging here
                }
            }
            else
            {
                // Handle router IP detection failure
                // You could add an error message or logging here
            }
        }

        static string GetDefaultGateway()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties properties = networkInterface.GetIPProperties();
                    foreach (GatewayIPAddressInformation gateway in properties.GatewayAddresses)
                    {
                        if (gateway.Address.AddressFamily == AddressFamily.InterNetwork) // Only IPv4
                        {
                            return gateway.Address.ToString();
                        }
                    }
                }
            }
            return null;
        }

        static string GetLocalIPAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return unicastAddress.Address.ToString();
                        }
                    }
                }
            }
            return null;
        }

        static string GetSubnet(string routerIP)
        {
            string[] ipParts = routerIP.Split('.');
            if (ipParts.Length == 4)
            {
                return $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.";
            }
            return null;
        }

        async Task<List<Device>> ScanNetwork(string subnet, string routerIP, string localIP)
        {
            List<Device> devices = new List<Device>();
            List<Task> tasks = new List<Task>();

            // Add Router first
            devices.Add(new Device
            {
                IPAddress = routerIP,
                HostName = "Router",
                Latency = await MeasureLatency(routerIP),
                MacAddress = GetMacAddress(routerIP)
            });

            // Add Local Device second
            devices.Add(new Device
            {
                IPAddress = localIP,
                HostName = "This Device",
                Latency = await MeasureLatency(localIP),
                MacAddress = GetMacAddress(localIP)
            });

            // Scan remaining devices
            for (int i = 1; i < 255; i++)
            {
                string ip = $"{subnet}{i}";
                if (ip == routerIP || ip == localIP)
                    continue;

                tasks.Add(Task.Run(async () =>
                {
                    if (await IsDeviceOnline(ip))
                    {
                        var device = new Device
                        {
                            IPAddress = ip,
                            HostName = GetHostName(ip),
                            Latency = await MeasureLatency(ip),
                            MacAddress = GetMacAddress(ip)
                        };

                        if (device.HostName == "Unknown")
                        {
                            device.HostName = GetNetBiosName(ip);
                        }

                        lock (devices)
                        {
                            devices.Add(device);
                        }

                        if (previousLatencies.ContainsKey(ip) &&
                            int.TryParse(previousLatencies[ip].Replace(" ms", ""), out int previousLatency) &&
                            int.TryParse(device.Latency.Replace(" ms", ""), out int currentLatency) &&
                            currentLatency > previousLatency)
                        {
                            Console.WriteLine($"Latency increased for {ip}: {previousLatency} ms -> {currentLatency} ms");
                        }

                        previousLatencies[ip] = device.Latency;
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return devices;
        }

        async Task<bool> IsDeviceOnline(string ip)
        {
            try
            {
                using (var ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ip, 1000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        async Task<string> MeasureLatency(string ip)
        {
            const int measurements = 5;
            List<long> latencies = new List<long>();

            try
            {
                using (var ping = new Ping())
                {
                    for (int i = 0; i < measurements; i++)
                    {
                        PingReply reply = await ping.SendPingAsync(ip, 1000);
                        if (reply.Status == IPStatus.Success)
                        {
                            latencies.Add(reply.RoundtripTime);
                        }
                        await Task.Delay(100);
                    }
                }

                if (latencies.Count > 0)
                {
                    long averageLatency = (long)latencies.Average();
                    return $"{averageLatency} ms";
                }
                else
                {
                    return "Request timed out";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        string GetHostName(string ip)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                return hostEntry.HostName;
            }
            catch (SocketException)
            {
                return "Unknown";
            }
        }

        string GetNetBiosName(string ip)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "nbtstat";
                process.StartInfo.Arguments = $"-A {ip}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string line in lines)
                {
                    if (line.Contains("<20>") || (line.Contains("<00>") && line.Contains("UNIQUE")))
                    {
                        return line.Substring(0, line.IndexOf("<")).Trim();
                    }
                }
            }
            catch (Exception)
            {
                // Handle any exceptions related to the process
            }
            return "Unknown";
        }

        string GetMacAddress(string ipAddress)
        {
            if (ipAddress == GetLocalIPAddress())
            {
                return GetLocalMacAddress();
            }
            else
            {
                try
                {
                    var arpProcess = new Process();
                    arpProcess.StartInfo.FileName = "arp";
                    arpProcess.StartInfo.Arguments = "-a";
                    arpProcess.StartInfo.RedirectStandardOutput = true;
                    arpProcess.StartInfo.UseShellExecute = false;
                    arpProcess.StartInfo.CreateNoWindow = true;
                    arpProcess.Start();

                    string output = arpProcess.StandardOutput.ReadToEnd();
                    arpProcess.WaitForExit();

                    string[] lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && parts[0] == ipAddress)
                        {
                            return parts[1];
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions if needed
                    Console.WriteLine(ex.Message);
                }
            }

            return "Unknown";
        }

        string GetLocalMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic.GetPhysicalAddress().ToString();
                }
            }
            return "Unknown";
        }

        private void BindDevicesToGrid(List<Device> devices)
        {
            // Example usage in the code-behind
            gridDevices.DataSource = devices;
            gridDevices.DataBind();

        }
    }

    public class Device
    {
        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Latency { get; set; }
        public string MacAddress { get; set; }
    }
}
