 

Overview

This C# ASP.NET application is a  Network Scanner  that scans the local network, identifying all active devices, their IP addresses, hostnames, latency, and MAC addresses. The application periodically updates this information, displaying it in a DataGridView with a modern UI design.

 Key Components

1.  Form1 Class : This is the main form of the application, which handles UI rendering, device scanning, and displaying the results.
2.  Device Class : Represents a device on the network, with properties such as `IPAddress`, `HostName`, `Latency`, and `MacAddress`.

 Initialization and UI Setup

 1. Constructor: `public Form1()`
-  Purpose : Initializes the form, sets up the DataGridView for displaying the network devices, and starts a timer to periodically scan the network.
-  Key Functions :
  - `InitializeComponent()`: Standard method to initialize WinForms components.
  - `InitializeDataGridView()`: Configures the design of the DataGridView.
  - `InitializeTimer()`: Sets up and starts a timer that triggers the network scan every 6 seconds.

 2. `InitializeDataGridView()`
-  Purpose : Sets up the DataGridView with a modern design and adds columns for displaying IP Address, Host Name, Latency, and MAC Address.
-  Key Customizations :
  - Alternating row colors for better readability.
  - Flat style buttons with a custom color scheme.
  - Custom header design with a dark blue background and white text.
  - Columns are automatically resized to fit the content.

 Timer and Button Click Events

 1. `InitializeTimer()`
-  Purpose : Creates a timer that triggers the `UpdateDeviceData()` method every 6 seconds to keep the device data up to date.
-  Details :
  - The timer is started immediately upon form initialization.
  - Every tick (6 seconds), the network is rescanned.

 2. `btnScan_Click_1(object sender, EventArgs e)`
-  Purpose : Manually triggers a network scan when the user clicks the "Scan" button.
-  Details :
  - Calls the `UpdateDeviceData()` method to perform a scan.

 Network Scanning Logic

 1. `UpdateDeviceData()`
-  Purpose : Orchestrates the entire network scanning process.
-  Flow :
  - Retrieves the router's IP address using `GetDefaultGateway()`.
  - Retrieves the local device's IP address using `GetLocalIPAddress()`.
  - Determines the subnet from the router's IP address using `GetSubnet()`.
  - Initiates the scanning of the network by calling `ScanNetwork(subnet, routerIP, localIP)`.

 2. `GetDefaultGateway()`
-  Purpose : Retrieves the default gateway (router) IP address of the network.
-  Details :
  - Iterates over all network interfaces to find the one that is up and running.
  - Retrieves the gateway address from the interface properties.

 3. `GetLocalIPAddress()`
-  Purpose : Retrieves the local machine's IP address.
-  Details :
  - Similar to `GetDefaultGateway()`, it checks all active network interfaces and returns the IP address of the local machine.

 4. `GetSubnet(string routerIP)`
-  Purpose : Determines the subnet of the network based on the router's IP address.
-  Details :
  - Splits the router IP address into its components and returns the first three parts to form the subnet (e.g., `192.168.1.`).

 5. `ScanNetwork(string subnet, string routerIP, string localIP)`
-  Purpose : Scans the entire network for devices, including the router, the local machine, and other devices.
-  Flow :
  -  Router and Local Device : Adds them first to the list with their IP, hostname, latency, and MAC address.
  -  Other Devices : Iterates over all possible IP addresses in the subnet (1-254), skipping the router and local IPs.
  -  Parallel Tasks : Each device scan runs in parallel using `Task.Run()`, improving performance.

 6. `IsDeviceOnline(string ip)`
-  Purpose : Checks if a device is online by pinging its IP address.
-  Details :
  - Sends a ping and waits for a reply.
  - If the ping succeeds, the device is considered online.

 7. `MeasureLatency(string ip)`
-  Purpose : Measures the network latency to a device.
-  Details :
  - Sends 5 ping requests to the device.
  - Averages the round-trip times to determine the latency.
  - Handles timeouts and exceptions gracefully.

 8. `GetHostName(string ip)`
-  Purpose : Retrieves the hostname of a device using DNS.
-  Details :
  - Uses `Dns.GetHostEntry(ip)` to get the host entry and returns the hostname.
  - If the hostname cannot be determined, it returns "Unknown".

 9. `GetNetBiosName(string ip)`
-  Purpose : Retrieves the NetBIOS name of a device if the hostname is unknown.
-  Details :
  - Executes the `nbtstat` command with the `-A` argument to query the NetBIOS name.
  - Parses the output to extract the name.

 10. `GetMacAddress(string ipAddress)`
-  Purpose : Retrieves the MAC address of a device.
-  Details :
  - If the IP address is the local machine's, it calls `GetLocalMacAddress()`.
  - For other devices, it executes the `arp` command to get the MAC address.
  - Validates and formats the MAC address.

 11. `GetLocalMacAddress()`
-  Purpose : Retrieves the MAC address of the local machine.
-  Details :
  - Iterates over all network interfaces, matching the IP address to find the correct interface.
  - Converts the MAC address to a string.

 Displaying the Results

 1. `BindDevicesToGrid(List<Device> devices)`
-  Purpose : Binds the list of devices to the DataGridView for display.
-  Details :
  - Clears the existing rows in the DataGridView.
  - Iterates over the `devices` list, adding each device to the DataGridView with its IP, hostname, latency, and MAC address.
 
