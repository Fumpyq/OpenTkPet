using MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static ConsoleApp1_Pet.Server.TcpServer;

namespace ConsoleApp1_Pet.Server
{
    public class TcpClientSide
    {
        private TcpClient _client;

        public async Task Connect(string serverAddress, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(serverAddress, port);
            Console.WriteLine($"Connected to server at {serverAddress}:{port}.");
        }
        public void StartPingSender()
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                sw.Restart();
                SendMessage(new tmPing() { ping =sw.Elapsed.TotalMilliseconds});
                sw.Stop();
               // Console.WriteLine();
            }
        }
        public void SendMessage (TcpMessage message)
        {
            if (_client == null || !_client.Connected)
            {
                Console.WriteLine("Client not connected!");
                return;
            }

            try
            {
                using (MemoryStream buffer = new MemoryStream())
                {
                    //using (NetworkStream stream = _client.GetStream())
                    //{

                        // Serialize message
                        byte[] data = MessagePackSerializer.Serialize((object)message);


                        // Send the length of the message as the first four bytes
                        byte[] lengthBytes = BitConverter.GetBytes(data.Length);
                        byte[] typeBytes = BitConverter.GetBytes((int)message.type);



                        buffer.Write(lengthBytes, 0, lengthBytes.Length);
                        buffer.Write(typeBytes, 0, typeBytes.Length);
                        buffer.Write(data, 0, data.Length);
                        buffer.WriteTo(_client.GetStream());
                        _client.GetStream().Flush();

                        Console.WriteLine($"Sent: {message}");


                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _client?.Close();
            _client = null;
            Console.WriteLine("Disconnected from server.");
        }
    }
    [MessagePackObject]
    public class tmPing : TcpMessage
    {
        [IgnoreMember]
        public override MessageType type => MessageType.ping;

        [Key(0)]
        public double ping = 153;
    }

    public abstract class TcpMessage
    {
        [IgnoreMember]
        public abstract MessageType type { get; }
    }

}
