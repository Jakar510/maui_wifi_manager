﻿using Plugin.MauiWifiManager.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.System;

namespace Plugin.MauiWifiManager
{
    /// <summary>
    /// Interface for Wi-FiNetworkService
    /// </summary>
    public class WifiNetworkService : IWifiNetworkService
    {
        public WifiNetworkService()
        {
        }

        /// <summary>
        /// Connect Wi-Fi
        /// </summary>
        public async Task<NetworkData> ConnectWifi(string ssid, string password)
        {
            NetworkData networkData = new NetworkData();
            var credential = new PasswordCredential();
            credential.Password = password;
            WiFiAdapter adapter;
            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed)
            {
                Console.WriteLine("No Wi-Fi Access Status");
            }
            else
            {
                var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                if (result.Count >= 1)
                {
                    adapter = await WiFiAdapter.FromIdAsync(result[0].Id);
                    if (adapter != null)
                    {
                        await adapter.ScanAsync();
                        WiFiAvailableNetwork wiFiAvailableNetwork = null;
                        foreach (var network in adapter.NetworkReport.AvailableNetworks)
                        {
                            if (network.Ssid == ssid)
                            {
                                wiFiAvailableNetwork = network;
                                break;
                            }
                        }
                        if (wiFiAvailableNetwork != null)
                        {
                            var status = await adapter.ConnectAsync(wiFiAvailableNetwork, WiFiReconnectionKind.Automatic, credential);
                            if (status.ConnectionStatus == WiFiConnectionStatus.Success)
                            {
                                ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
                                var hostname = NetworkInformation.GetHostNames().FirstOrDefault(hn => hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId == InternetConnectionProfile?.NetworkAdapter.NetworkAdapterId);
                                networkData.Ssid = hostname.ToString();
                                Console.WriteLine("OK");
                            }
                            else if (status.ConnectionStatus == WiFiConnectionStatus.InvalidCredential)
                            {
                                Console.WriteLine("Invalid Credential");
                            }
                            else if (status.ConnectionStatus == WiFiConnectionStatus.Timeout)
                            {
                                Console.WriteLine("Timeout");
                            }
                        }
                    }
                }
            }
            return networkData;
        }

        /// <summary>
        /// Disconnect Wi-Fi
        /// </summary>
        public async void DisconnectWifi(string ssid)
        {
            WiFiAdapter adapter;
            var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
            if (result.Count >= 1)
            {
                adapter = await WiFiAdapter.FromIdAsync(result[0].Id);
                adapter.Disconnect();
            }
        }

        /// <summary>
        /// Get Network Info
        /// </summary>
        public Task<NetworkData> GetNetworkInfo()
        {
            NetworkData networkData = new NetworkData();
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (InternetConnectionProfile?.ProfileName != null)
            {
                networkData.Ssid = InternetConnectionProfile?.ProfileName;
            }
            return Task.FromResult(networkData);
        }

        /// <summary>
        /// Open Wi-Fi Setting
        /// </summary>
        public async Task<bool> OpenWifiSetting()
        {
            return await Launcher.LaunchUriAsync(new Uri("ms-settings:network-wifi"));
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {

        }

        public async Task<List<NetworkData>> ScanWifiNetworks()
        {
            List<NetworkData> wifiNetworks = new List<NetworkData>();

            var accessStatus = await WiFiAdapter.RequestAccessAsync();
            if (accessStatus == WiFiAccessStatus.Allowed)
            {
                var result = await WiFiAdapter.FindAllAdaptersAsync();
                if (result.Count > 0)
                {
                    var wifiAdapter = result[0];
                    var availableNetworks = wifiAdapter.NetworkReport.AvailableNetworks;

                    foreach (var network in availableNetworks)
                    {
                        wifiNetworks.Add(new NetworkData
                        {
                            Ssid = network.Ssid,
                            Bssid = network.Bssid
                        });
                    }
                }
            }
            return wifiNetworks;
        }
    }
}
