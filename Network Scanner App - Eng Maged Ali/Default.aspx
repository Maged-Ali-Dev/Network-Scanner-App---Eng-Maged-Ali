<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Network_Scanner_App___Eng_Maged_Ali.Default" Async="true" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Network Scanner</title>
    <style>
        body {
            background-color: #f4f4f4;
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
            color: #333;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
            text-align: center;
        }
        .button {
            background-color: #007bff;
            color: white;
            padding: 15px 30px;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            font-size: 18px;
            transition: background-color 0.3s ease;
        }
        .button:hover {
            background-color: #0056b3;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            background-color: #fff;
        }
        table, th, td {
            border: 1px solid #ddd;
        }
        th, td {
            padding: 12px;
            text-align: left;
        }
        th {
            background-color: #007bff;
            color: white;
        }
        @media (max-width: 768px) {
            th, td {
                display: block;
                width: 100%;
                box-sizing: border-box;
                text-align: right;
            }
            th {
                background-color: #0056b3;
            }
            td::before {
                content: attr(data-label);
                font-weight: bold;
                display: inline-block;
                width: 100%;
                text-align: left;
                margin-right: 10px;
            }
            td {
                display: flex;
                justify-content: space-between;
                margin-bottom: 10px;
            }
        }
        /* Progress Indicator Styles */
        .loading-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(255, 255, 255, 0.7);
            display: flex;
            align-items: center;
            justify-content: center;
            display: none;
            z-index: 1000;
        }
        .spinner {
            border: 8px solid #f3f3f3;
            border-radius: 50%;
            border-top: 8px solid #007bff;
            width: 50px;
            height: 50px;
            animation: spin 1s linear infinite;
        }
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
    </style>
</head>
<body>
    <div class="container">
        <form id="form1" runat="server">
            <asp:Button ID="btnScan" runat="server" Text="Scan" CssClass="button" OnClick="btnScan_Click" OnClientClick="showLoading(); return true;" />
            <asp:GridView ID="gridDevices" runat="server" AutoGenerateColumns="false" CssClass="responsive-table">
                <Columns>
                    <asp:BoundField DataField="IPAddress" HeaderText="IP Address" SortExpression="IPAddress" />
                    <asp:BoundField DataField="HostName" HeaderText="Host Name" SortExpression="HostName" />
                    <asp:BoundField DataField="Latency" HeaderText="Latency" SortExpression="Latency" />
                    <asp:BoundField DataField="MacAddress" HeaderText="MAC Address" SortExpression="MacAddress" />
                </Columns>
            </asp:GridView>
        </form>
    </div>
    <div id="loadingOverlay" class="loading-overlay">
        <div class="spinner"></div>
    </div>
    <script>
        function showLoading() {
            document.getElementById('loadingOverlay').style.display = 'flex';
        }

        function hideLoading() {
            document.getElementById('loadingOverlay').style.display = 'none';
        }

        // Ensure loading overlay is hidden when page is fully loaded
        window.addEventListener('load', hideLoading);
    </script>
</body>
</html>

