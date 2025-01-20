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
using ConsoleApp1_Pet.Architecture;
using OpenTK.Mathematics;

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

        [AttributeUsage(AttributeTargets.Property)]
        public class NetworkSyncAttribute : Attribute { }

        public struct NetworkPropertyInfo
        {
            public GOComponent instance;
            public PropertyInfo info;
            public object knownValue;

            public NetworkPropertyInfo(GOComponent instance, PropertyInfo info) : this()
            {
                this.instance = instance;
                this.info = info;
            }
            public void SetValue(object value)
            {
                info.SetValue(instance, value);
            }
            /// <summary>
            /// checks if value is changed, and if changed - stores it in <see cref="knownValue"/>
            /// </summary>
            /// <returns>is value changed</returns>
            public bool Exchange()
            {
                var val = info.GetValue(instance);
                if(val!= knownValue)
                {
                    knownValue = val;
                    return true;
                }
                return false;
            }
        }
        [MessagePackObject]
        public struct PropertyChangeInfo
        {
            [Key(0)]
            public int componentId;
            [Key(1)]
            public int propertyId;
            [Key(2)]
            public object value;

            public PropertyChangeInfo(int componentId, int propertyId, object value)
            {
                this.componentId = componentId;
                this.propertyId = propertyId;
                this.value = value;
            }
            public void Serialize(Stream stream)
            {
                MessagePackSerializer.Serialize( stream, this);
            }
        }
        [MessagePackObject]
        public struct NetworkChangePacket
        {
            [Key(0)]
            public int NetworkIdentityId;
            [Key(1)]
            public List<PropertyChangeInfo> propertyChangeInfos;
            public byte[] Serialize()
            {
                return MessagePackSerializer.Serialize(this);
            }
            public void Serialize(Stream stream)
            {
                MessagePackSerializer.Serialize(stream, this);
            }
            public static NetworkChangePacket Deserialize(byte[] data)
            {
                return MessagePackSerializer.Deserialize<NetworkChangePacket>(data);
            }
            public static NetworkChangePacket Deserialize(Stream data)
            {
                return MessagePackSerializer.Deserialize<NetworkChangePacket>(data);
            }
        }
        public class NetworkIdentity : GOComponent
        {
            //public bool SyncTransform;
            private Matrix4 oldTransform;
            public NetworkSyncBroker client;
            public Dictionary<int, GOComponent> components = new Dictionary<int, GOComponent>();
            // componentId, property ordinal, property value
            public Dictionary<int, NetworkPropertyInfo[]> properties = new Dictionary<int, NetworkPropertyInfo[]>();
           // public Dictionary<int, object[]> valuesCache = new Dictionary<int, object[]>();
            public override void OnInit(GameObject go)
            {
                base.OnInit(go);

                foreach (var v in go.Components)
                {
                    RegisterComponent(v);
                }
                go.OnComponentAdded += OnComponentAdded;
                NetworkSyncBroker.RegisterIdentity(this);
            }

            private void OnComponentAdded(GameObject go, GOComponent comp)
            {
                RegisterComponent(comp);
            }

            private void RegisterComponent(GOComponent v)
            {
                components.Add(v.ID, v);
                
                var inf = v.GetType().GetSyncProperties();
                if (inf.Length > 0)
                {
                    var vals = new NetworkPropertyInfo[inf.Length];
                    int i = 0;
                    foreach (var prop in inf)
                    {
                        vals[i] = new NetworkPropertyInfo(v, prop);

                        i++;

                    }

                    properties.Add(v.ID, vals);
                }
            }

            public void GetChanges(ref List<PropertyChangeInfo> changes)
            {
               // var changes = new List<PropertyChangeInfo>();
                foreach(var p in properties)
                {
                    int index = 0;
                    foreach(var v in p.Value)
                    {
                        if (v.Exchange())
                        {
                            changes.Add(new PropertyChangeInfo(p.Key, index, v.knownValue));
                        }
                        index++;
                    }
                }
                if (oldTransform != transform)
                {
                    oldTransform = transform;
                    changes.Add(new PropertyChangeInfo(-1, -1, oldTransform));
                }
                //return changes;
            }
            public void ApplyChanges(List<PropertyChangeInfo> changes)
            {
                foreach(var c in changes)
                {
                    if (c.propertyId == -1)
                    {
                        this.oldTransform= (Matrix4)c.value;
                        continue;
                    }

                    if(properties.TryGetValue(c.componentId, out var v))
                    {
                        v[c.propertyId].SetValue(c.value);
                    }
                }
            }
        }
    }
    public class NetworkSyncBroker
    {
        public static NetworkSyncBroker instance = new NetworkSyncBroker();
        private Dictionary<int, NetworkIdentity> map = new Dictionary<int, NetworkIdentity>();
        public List<NetworkIdentity> identities = new List<NetworkIdentity>();
        private List<PropertyChangeInfo> Pcache = new List<PropertyChangeInfo>();
        private List<NetworkChangePacket> Ncache = new List<NetworkChangePacket>();


        public List<NetworkChangePacket> GetClientChanges()
        {
            var res = new   List<NetworkChangePacket>();
       
            Ncache.Clear();
            foreach (var v in identities)
            {
                Pcache.Clear();
                v.GetChanges(ref Pcache);
                Ncache.Add(new NetworkChangePacket() { NetworkIdentityId = v.ID, propertyChangeInfos = Pcache });
            }

            return Ncache;
        }
        public void ApplyServerChanges(List<NetworkChangePacket> changes)
        {
            foreach(var v in changes)
            {
                if(map.TryGetValue(v.NetworkIdentityId, out var networkIdentity))
                {
                    networkIdentity.ApplyChanges(v.propertyChangeInfos);
                }
            }           
        }
        public static void RegisterIdentity(NetworkIdentity id)
        {
            id.client = instance;
            instance.map.Add(id.ID, id);
        }
    }
    public static class NetworkReflectionCache
    {
        public static Dictionary<Type, PropertyInfo[]> typeCache = new Dictionary<Type, PropertyInfo[]>();
        public static PropertyInfo[] GetSyncProperties(this Type type)
        {
            if (typeCache.TryGetValue(type, out var propertyInfos))
            {
                return propertyInfos;
            }
            else
            {
                var tt = type.GetProperties(BindingFlags.Instance).Where(x=> x.GetCustomAttribute<NetworkSyncAttribute>()!=null).ToArray();
                typeCache.Add(type, tt);
                return tt;
            }
        }
    }
}
