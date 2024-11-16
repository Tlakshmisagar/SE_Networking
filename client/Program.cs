using Grpc.Net.Client;
using GrpcServer;
using Network.ClientServices;
using Networking;
using Networking.Communication;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace client
{
    public class Program : INotificationHandler
    {
        static void Main(string[] args)
        {
            Console.WriteLine("______________This is client____________");

            Program program = new Program();
            ICommunicator client = CommunicationFactory.GetCommunicator(true, true);

            // Subscribe to the topic for communication
            client.Subscribe("dummy", program, false);

            Console.Write("Enter server IP: ");
            string ip = Console.ReadLine();

            Console.Write("Enter server Port: ");
            string port = Console.ReadLine();

            // Start the client connection
            string res = client.Start(ip, port);
            Console.WriteLine($"Client connected to server: {res}");

            Console.WriteLine("Type messages to send to the server. Type 'q' to quit.");

            string input;
            // Keep reading inputs from the console until 'q' is entered
            do
            {
                Console.Write("Client: ");
                input = Console.ReadLine();
                if (input != null && input.Trim().ToLower() != "q")
                {
                    client.Send(input, "dummy", null);
                }
            } while (input != null && input.Trim().ToLower() != "q");

            Console.WriteLine("Client shutting down...");
        }

        // Handle data received from the server
        public void OnDataReceived(string serializedData)
        {
            Console.WriteLine($"Received from server: {serializedData}");
        }
    }
}
