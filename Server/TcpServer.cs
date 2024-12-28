using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using MessagePack;
using System.Collections.Frozen;
using static ConsoleApp1_Pet.Server.TcpServer;
using System.Reflection;
using System.ComponentModel;
using ConsoleApp1_Pet.Scripts;

namespace ConsoleApp1_Pet.Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageHandlerAttribute : Attribute
    {
        public MessageType MessageType { get; }

        public MessageHandlerAttribute(MessageType messageType)
        {
            MessageType = messageType;
        }
    }
    public class TcpServer
    {
        private TcpListener _listener;
        private ConcurrentDictionary<Guid, TcpClient> _connectedClients = new ConcurrentDictionary<Guid, TcpClient>();
        private CancellationTokenSource _cts;
        private ConsoleWindow console;

        public FrozenDictionary<MessageType, MessageHandler> messageHandlers;


        public enum MessageType
        {
           // unspecified =2,
            ping = 1,
            helloWorld=2
        }
        private void RegisterHandlers()
        {
            //Get all methods in the current type
            var types =
                AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(x => x.GetTypes().Where(t => Attribute.IsDefined(t, typeof(MessageHandlerAttribute))))
                                .ToArray();


            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var dict = new Dictionary<MessageType, MessageHandler>();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<MessageHandlerAttribute>();
                if (attribute != null)
                {
                    
                    if (dict.TryGetValue(attribute.MessageType,out var hh))
                    {
                        Console.WriteLine($"Warning: {attribute.MessageType} already implemented by {hh.ToString()}");
                        continue;
                    }
                        if (type.IsAssignableFrom(typeof(MessageHandler)))
                        {
                            Console.WriteLine($"Warning: method {type.Name} decorated with MessageHandler attribute must be of type MessageHandler");
                            continue;
                        }
                    try
                    {
                        var classInstance =(MessageHandler) Activator.CreateInstance(type);
                        dict.Add(attribute.MessageType, classInstance);
                    }
                    catch (Exception ex) { Console.WriteLine($"Err: canot create instance of {type.Name}: {ex.ToString()}"); }

                    //// Check if method is decorated with MessageHandlerAttribute
                    //var attribute = method.GetCustomAttribute<MessageHandlerAttribute>();
                    //if (attribute != null)
                    //{
                    //    if (method.ReturnType != typeof(Task))
                    //    {
                    //        Console.WriteLine($"Warning: method {method.Name} decorated with MessageHandler attribute must be asynchronous and return Task");
                    //        continue;
                    //    }
                    //    var parameters = method.GetParameters();
                    //    if (parameters.Length != 1 || parameters[0].ParameterType != typeof(object))
                    //    {
                    //        Console.WriteLine($"Warning: method {method.Name} decorated with MessageHandler attribute must accept only one parameter type of object");
                    //        continue;
                    //    }
                    //    //Creates a delegate for a method that takes object as argument and returns Task
                    //    var handler = (Func<object, Task>)Delegate.CreateDelegate(typeof(Func<object, Task>), this, method);

                    //    _messageHandlers[attribute.MessageType] = handler;
                    //    Console.WriteLine($"Registered handler {method.Name} for {attribute.MessageType}");
                    //}
                }
            }
            messageHandlers = dict.ToFrozenDictionary();
            
        }

        //public void RegisterHandlers()
        //{
        //    var dict = new Dictionary<MessageType, MessageHandler>()
        //    {
        //        {MessageType.ping, new mh_Ping()}
        //    };
           
        //}


        public abstract class MessageHandler
        {
            public abstract void Handle(byte[] body);
        }
        public abstract class TypedMessageHandler<T> : MessageHandler
        {
         
            public abstract void Handle(T message);
            public override void Handle(byte[] body)
            {
                Handle(MessagePackSerializer.Deserialize<T>(body));
            }
        }

        [MessageHandler(MessageType.ping)]
        public class mh_Ping : TypedMessageHandler<tmPing>
        {
            public override void Handle(tmPing message)
            {
                Console.WriteLine("ping message is:"+message.ping);
            }
        }

        public void Start(int port)
        {
          
            RegisterHandlers();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            ConsoleWindow window = new ConsoleWindow();
            ScriptManager.AddScript(window);
            console = window;
            console.WriteLine($"Server started on port {port}.");

            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {

                        TcpClient client = await _listener.AcceptTcpClientAsync(_cts.Token);
                        _ = HandleClientAsync(client);

                    }
                }
                catch (OperationCanceledException)
                {
                    // server is stopping 
                }
                finally
                {
                    console.WriteLine("Server Stopped!");
                }
            });
        }
           

        


        private async Task HandleClientAsync(TcpClient client)
        {
            Guid clientId = Guid.NewGuid();
            _connectedClients.TryAdd(clientId, client);
            console.WriteLine($"Client connected (ID: {clientId}). Active clients: {_connectedClients.Count}");

            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {

                            // Read the length of the message
                            byte[] MessageHead = new byte[8];
                            int bytesRead = await stream.ReadAsync(MessageHead, 0, MessageHead.Length, _cts.Token);
                            if (bytesRead != 8 || _cts.Token.IsCancellationRequested)
                            {
                                break; // Connection closed
                            }
                            int messageLength = BitConverter.ToInt32(MessageHead.AsSpan().Slice(0,4));
                            int messageType = BitConverter.ToInt32(MessageHead.AsSpan().Slice(4,4));

                            // Read the message itself
                            byte[] messageBuffer = new byte[messageLength];
                            bytesRead = await stream.ReadAsync(messageBuffer, 0, messageLength, _cts.Token);
                            if (bytesRead != messageLength || _cts.Token.IsCancellationRequested)
                            {
                                break; // Invalid message
                            }

                            if( Enum.IsDefined(typeof(MessageType), messageType))
                            {
                                MessageType mt = (MessageType)messageType;
                                if (messageHandlers.TryGetValue(mt, out var mh))
                                {
                                    mh.Handle(messageBuffer);
                                }
                            }
                            else
                            {
                                console.WriteLine($"Received unknown message from {clientId}");
                            }

                           

                            // Deserialize using MessagePack
                          //  object message = MessagePackSerializer.Deserialize<object>(messageBuffer);

                           // console.WriteLine($"Received from {clientId}: {message}");
                            //// Construct the response
                            //var response = new
                            //{
                            //    Status = "OK",
                            //    Message = "Server received message: " + message.ToString()
                            //};

                            //// Serialize using MessagePack
                            //byte[] responseBytes = MessagePackSerializer.Serialize(response);

                            //// Send the length of the message as the first four bytes
                            //byte[] responseLengthBytes = BitConverter.GetBytes(responseBytes.Length);
                            //await stream.WriteAsync(responseLengthBytes, 0, responseLengthBytes.Length);
                            //await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                        }
                        catch (Exception ex)
                        {
                            // Handle exceptions with more care
                            console.WriteLine($"Error reading data from {clientId}: {ex.Message}");
                            break; // close connection
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                console.WriteLine($"Error handling client {clientId}: {ex.Message}");
            }
            finally
            {
                _connectedClients.TryRemove(clientId, out _);
                client.Close();
                console.WriteLine($"Client disconnected (ID: {clientId}). Active clients: {_connectedClients.Count}");
            }
        }


        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
        }
    }
}
