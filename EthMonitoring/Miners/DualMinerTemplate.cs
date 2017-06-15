﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EthMonitoring
{

    class EthMonJsonTemplate
    {
        public int id { get; set; }
        public string error { get; set; }
        public List<string> result { get; set; }
    }
    class DualMinerTemplate
    {
        LogWriter logger = new LogWriter();

        public Stats getStats(string _host, int _port)
        {
            Stats stats = new Stats()
            {
                online = false,
                hashrates = new List<string>(),
                dcr_hashrates = new List<string>(),
                temps = new List<string>(),
                fan_speeds = new List<string>(),
                power_usage = new List<string>(),
                type = 0
            };

            try
            {
                var clientSocket = new System.Net.Sockets.TcpClient();

                if (clientSocket.ConnectAsync(_host, _port).Wait(5000))
                {
                    string get_menu_request = "{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat1\"}";
                    NetworkStream serverStream = clientSocket.GetStream();
                    byte[] outStream = System.Text.Encoding.ASCII.GetBytes(get_menu_request);
                    serverStream.Write(outStream, 0, outStream.Length);
                    serverStream.Flush();

                    byte[] inStream = new byte[clientSocket.ReceiveBufferSize];
                    serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
                    string _returndata = System.Text.Encoding.ASCII.GetString(inStream);

                    EthMonJsonTemplate result = JsonConvert.DeserializeObject<EthMonJsonTemplate>(_returndata);

                    stats.version = result.result[0]; // Version

                    string[] miner_stats = result.result[2].Split(';');
                    stats.total_hashrate = miner_stats[0];
                    stats.accepted = Int32.Parse(miner_stats[1]);
                    stats.rejected = Int32.Parse(miner_stats[2]);


                    string[] hashrates = result.result[3].Split(';'); // ETH Hashrates

                    for(int i = 0; i < hashrates.Length; i++)
                    {
                        stats.hashrates.Add(hashrates[i]);
                    }

                    string[] dcr_hashrates = result.result[5].Split(';'); // DCR Hashrates

                    for (int i = 0; i < dcr_hashrates.Length; i++)
                    {
                        stats.dcr_hashrates.Add(dcr_hashrates[i]);
                    }

                    // Temps and fan speeds
                    string[] temp = result.result[6].Split(';');

                    int temp_row = 0;
                    for (int i = 0; i < temp.Length; i++)
                    {
                        stats.temps.Add(temp[i]);
                        stats.fan_speeds.Add(temp[i + 1]);
                        i++;
                        temp_row++;
                    }
                    
                    // Close socket
                    clientSocket.Close();
                    clientSocket = null;

                    stats.online = true; // Online
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                logger.LogWrite("Host socket exception: " + ex.Message);
                logger.LogWrite("Stacktrace: " + ex.StackTrace);

                stats.ex = ex;
            }

            return stats;
        }

    }
}
