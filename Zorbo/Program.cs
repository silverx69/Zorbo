using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using Zorbo.Chat;
using Zorbo.Chat.Messages;
using Zorbo.Chat.Server;
using Zorbo.Data;
using Zorbo.Net;
using Zorbo.Net.Messages;

namespace Zorbo
{
    [Message(9999)]
    public class TestObject
    {
        public Guid GuidValue { get; set; }

        public string StringValue { get; set; }

        public List<NestedTestObject> NestedValues { get; set; }
    }

    public class NestedTestObject
    {
        public int IntValue { get; set; }

        public IPAddress IPValue { get; set; } = new([127, 0, 0, 1]);

        public List<int> ListValue { get; set; }

        public string StringValue { get; set; }

        [BinaryIgnore, JsonIgnore]
        public string IgnoredProp { get; set; } = "Ignored";
    }

    internal class Program
    {
        static ChatServer server;
        static ZorboSocket listener;

        // create somewhat complex custom object
        static readonly TestObject OBJ1 = new() {
            GuidValue = Guid.NewGuid(),
            StringValue = "TestString",
            NestedValues = [
                new NestedTestObject() {
                    IntValue = 1,
                    StringValue = "NestedTestString1",
                    ListValue = [11, 44, 902, 4232, 23232]
                },
                new NestedTestObject() {
                    IntValue = 2,
                    StringValue = "NestedTestString2",
                    ListValue = [11, 44, 902, 4232, 23232]
                },
                new NestedTestObject() {
                    IntValue = 3,
                    StringValue = "NestedTestString3",
                    ListValue = [11, 44, 902, 4232, 23232]
                }
            ]
        };

        static void Main(string[] args) => Run().Wait();

        static async Task Run() {

            await TestZorboSocketListener();
            await TestZorboChatServer();
            await TestZorboSocketUDP();


            Console.Read();
        }

        static async Task<X509Certificate2> GenerateTestCert(string name) {
            return Certificates.Generate(new CertGenerationOptions() {
                Name = name,
                //Address = await Network.GetPublicAddress(),
                LocalAddresses = Network.GetLocalAddresses(),
                PrivateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.pfx"),
                PublicFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.pub.cer")
            });
        }

        static async Task TestZorboChatServer() {
            server = new ChatServer();
            await server.StartAsync();

            Console.WriteLine("Chat server is running.");
            Console.WriteLine("Chat client connecting...");

            var client = new ZorboSocket();

            client.Connected += ChatClient_Connected;
            client.Rejected += ChatClient_Rejected;
            client.Received += ChatClient_Received;
            client.Exception += ChatClient_Exception;
            client.Disconnected += ChatClient_Disconnected;

            client.CertificateCallback = Certificates.SelfSignedValidationCallback;

            //client.IsSecureSocket = true;
            //client.Connect(IPAddress.IPv6Loopback, server.LocalEndPoint.Port);

            client.Connect(new Uri("tcps://[::1]:" + server.LocalEndPoint.Port));
        }

        private static async Task ChatClient_Connected(ZorboSocket sender, ConnectEventArgs e) {
            Console.WriteLine("Chat client connected.");

            sender.Receive();
            sender.Send(new ClientLogin() {
                Guid = Guid.NewGuid(),
                Username = "SilverX",
                Age = 99,
                Country = Country.Canada,
                Region = "Awkward",
                Status = "Thanks to denial, I'm immortal.",
                Flags = ClientSupportFlags.ALL
            });

            sender.Send(new ClientPublic() { 
                Message = "This is a test of the database history system."
            });

            var testHandler = new HttpClientHandler() {
                ServerCertificateCustomValidationCallback = Certificates.SelfSignedValidationCallback
            };

            var testClient = new HttpClient(testHandler);
            var post = new StringContent("This is a POST request.");
            try {
                var response = await testClient.PostAsync(new Uri("https://[::1]:" + server.LocalEndPoint.Port), post);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    Console.WriteLine("Response: {0}", content);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            finally {
                testClient.Dispose();
            }
        }

        private static Task ChatClient_Rejected(ZorboSocket sender, RejectedEventArgs e) {
            Console.WriteLine("Chat client rejected.");
            return Task.CompletedTask;
        }

        private static Task ChatClient_Received(ZorboSocket sender, MessageEventArgs e) {
            Console.WriteLine("{0} {1}", (MessageId)e.Id, JsonSerializer.Serialize(e.Message));
            return Task.CompletedTask;
        }

        private static Task ChatClient_Exception(ZorboSocket sender, ExceptionEventArgs e) {
            Console.WriteLine("Chat client threw exception: {0}", e.Exception.Message);
            return Task.CompletedTask;
        }

        private static async Task ChatClient_Disconnected(ZorboSocket sender, DisconnectEventArgs e) {
            Console.WriteLine("Chat client disconnected.");
            await sender.DisposeAsync();
        }

        static async Task TestZorboSocketListener() {
            // start listener as ZorboSocket
            listener = new ZorboSocket();

            // communicate using TLS
            listener.IsSecureSocket = true;
            listener.Certificate = await GenerateTestCert("ZorboTest");

            // communicate with websockets only
            //listener.IsWebSocket = true;

            listener.Accepted += Listener_Accepted;
            listener.Rejected += Listener_Rejected;
            listener.Exception += Listener_Exception;

            listener.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
            listener.Listen();

            TestTcpClientToZorboSocket();
            TestWebSocketToZorboSocket();
            TestZorboSocketToZorboSocket();
        }

        static async void TestTcpClientToZorboSocket() {
            // start client as TcpClient
            using var tcpClient = new TcpClient();

            await tcpClient.ConnectAsync(IPAddress.IPv6Loopback, listener.LocalEndPoint.Port);

            Console.WriteLine("TcpClient connected successfully.");

            Stream stream = tcpClient.GetStream();

            if (listener.IsSecureSocket) {
                // accept self-signed certificates
                var sslStream = new SslStream(stream, false, Certificates.SelfSignedValidationCallback);

                sslStream.AuthenticateAsClient("::1");

                stream = sslStream;
            }

            var converter = new MessageConverter();

            // get serialized data
            using var writer = new ZBinaryWriter();
            await converter.WriteAsync(writer, OBJ1, MessageType.Binary);

            // write data as a websocket frame
            var frame = new Frame() {
                OpCode = OpCode.Binary,
                IsMasked = true,
                Payload = await writer.ToArrayAsync()
            };

            writer.Clear();
            await FrameReader.WriteAsync(writer, frame);

            // send from TcpClient to ZorboSocket
            await stream.WriteAsync(await writer.ToArrayAsync());

            // read echo from ZorboSocket
            byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
            int count = await stream.ReadAsync(buffer, default);

            // read frame data
            using var reader = new ZBinaryReader(buffer, 0, count);
            frame = await FrameReader.ReadAsync(reader);

            // load payload into binary reader
            reader.Clear();
            await reader.BaseStream.WriteAsync(frame.Payload);
            reader.Position = 0;

            // read payload data
            var message = await converter.ReadAsync(reader, MessageType.Binary);

            Console.WriteLine("TcpClient received message.");
            Console.WriteLine(JsonSerializer.Serialize(message));

            ArrayPool<byte>.Shared.Return(buffer);

            await stream.DisposeAsync();
        }

        static async void TestWebSocketToZorboSocket() {
            // start client as .net websocket
            using var webSocket = new ClientWebSocket();

            // accept self-signed certificates
            webSocket.Options.RemoteCertificateValidationCallback = Certificates.SelfSignedValidationCallback;

            await webSocket.ConnectAsync(new($"wss://[::1]:{listener.LocalEndPoint.Port}"), default);

            Console.WriteLine("WebSocket connected successfully.");

            var converter = new MessageConverter();

            using var writer = new ZBinaryWriter();
            await converter.WriteAsync(writer, OBJ1, MessageType.Binary);

            // send from .net websocket to ZorboSocket
            await webSocket.SendAsync(await writer.ToArrayAsync(), WebSocketMessageType.Binary, true, default);

            // read echo from ZorboSocket
            byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
            var result = await webSocket.ReceiveAsync(buffer, default);

            // websockets already communicate in frames
            // read payload data
            using var messageReader = new ZBinaryReader(buffer, 0, result.Count);
            var message = await converter.ReadAsync(messageReader, (MessageType)result.MessageType);

            // print
            Console.WriteLine("WebSocket received message.\r\n{0}", JsonSerializer.Serialize(message));

            ArrayPool<byte>.Shared.Return(buffer);
        }

        static async void TestZorboSocketToZorboSocket() {
            // start client as zorbosocket
            var client = new ZorboSocket();

            client.Connected += Client_Connected;
            client.Rejected += Client_Rejected;
            client.Received += Client_Received;
            client.Exception += Client_Exception;
            client.Disconnected += Client_Disconnected;

            // accept self-signed certificates
            client.CertificateCallback = Certificates.SelfSignedValidationCallback;

            client.Connect(new Uri($"wss://[::1]:{listener.LocalEndPoint.Port}"));
        }

        static Task Listener_Accepted(ZorboSocket sender, AcceptEventArgs e) {
            Console.WriteLine("Server accepted successfully.");

            var serverClient = e.Socket;
            serverClient.Received += Listener_Received;
            serverClient.Exception += Listener_Exception;
            serverClient.Disconnected += Listener_Disconnected;

            serverClient.Receive();
            return Task.CompletedTask;
        }

        static Task Listener_Rejected(ZorboSocket sender, RejectedEventArgs e) {
            Console.WriteLine("Server rejected connection. Reason: {0}", e.Exception.Message);
            return Task.CompletedTask;
        }

        static Task Listener_Exception(ZorboSocket sender, ExceptionEventArgs e) {
            Console.WriteLine("Server threw exception: {0}", e.Exception.Message);
            return Task.CompletedTask;
        }

        static int dc_count = 0;
        static async Task Listener_Disconnected(ZorboSocket sender, DisconnectEventArgs e) {
            Console.WriteLine($"Server disconnect detected. ({++dc_count})");
            await sender.DisposeAsync();
        }

        static Task Listener_Received(ZorboSocket sender, MessageEventArgs e) {
            // print
            Console.WriteLine("Server received message.\r\n{0}", JsonSerializer.Serialize(e.Message));

            // echo client message
            sender.Send(e.Message, e.MessageType);
            return Task.CompletedTask;
        }

        static Task Client_Connected(ZorboSocket sender, ConnectEventArgs e) {
            Console.WriteLine("ZorboSocket connected successfully.");
            // send from ZorboSocket to ZorboSocket
            sender.Receive();
            sender.Send(OBJ1, MessageType.Text);
            return Task.CompletedTask;
        }

        static Task Client_Rejected(ZorboSocket sender, RejectedEventArgs e) {
            Console.WriteLine("ZorboSocket connection failed: {0}", e.Exception.Message);
            return Task.CompletedTask;
        }

        static Task Client_Exception(ZorboSocket sender, ExceptionEventArgs e) {
            Console.WriteLine("ZorboSocket threw exception: {0}", e.Exception.Message);
            return Task.CompletedTask;
        }

        static async Task Client_Disconnected(ZorboSocket sender, DisconnectEventArgs e) {
            Console.WriteLine("ZorboSocket disconnected.");
            await sender.DisposeAsync();
        }

        static async Task Client_Received(ZorboSocket sender, MessageEventArgs e) {
            // read echo from ZorboSocket
            // print
            Console.WriteLine("ZorboSocket received message.\r\n{0}", JsonSerializer.Serialize(e.Message));

            // Test: disconnect, wait 5 seconds and connect again
            // validates reuse and exposes exceptions caused during close/dispose
            //
            // remove disconnect event so the socket doesn't get disposed
            sender.Disconnected -= Client_Disconnected;
            sender.Disconnect();

            await Task.Delay(5000);

            // resubscribe to disconnect event, in case we disconnect for some other reason
            sender.Disconnected += Client_Disconnected;
            sender.Connect(new IPEndPoint(IPAddress.IPv6Loopback, listener.LocalEndPoint.Port));
        }

        static async Task TestZorboSocketUDP() {

            var clientA = new ZorboSocket(SocketProtocol.Udp);

            clientA.Bind(new(IPAddress.IPv6Any, 0));
            clientA.Received += UdpClient_Received;
            clientA.Exception += UdpClient_Exception;
            clientA.Receive();

            var clientB = new ZorboSocket(SocketProtocol.Udp);

            clientB.Bind(new(IPAddress.IPv6Any, 0));
            clientB.Received += UdpClient_Received;
            clientB.Exception += UdpClient_Exception;
            clientB.Receive();

            var clientC = new ZorboSocket(SocketProtocol.Udp);

            clientC.Bind(new(IPAddress.IPv6Any, 0));
            clientC.Received += UdpClient_Received;
            clientC.Exception += UdpClient_Exception;
            clientC.Receive();

            var testPacket = new ServerPublic() { 
                Id = 0,
                Sender = "SilverX",
                Message = "Testing standard ZorboSocket message conversion over UDP"
            };

            var endpointA = new IPEndPoint(IPAddress.IPv6Loopback, clientA.LocalEndPoint.Port);

            clientB.Send(testPacket, endpointA);
            clientC.Send(testPacket, endpointA);

        }

        static async Task UdpClient_Received(ZorboSocket sender, MessageEventArgs e) {
            Console.WriteLine(
                "UDP Message from {0}:\r\n{1}{2}", 
                e.RemoteEndPoint,
                e.Message.GetType(),
                JsonSerializer.Serialize(e.Message));
        }

        static async Task UdpClient_Exception(ZorboSocket sender, ExceptionEventArgs e) {
            Console.WriteLine("UDP Exception from {0}: {1}", e.RemoteEndPoint, e.Exception.Message);
        }
    }
}
