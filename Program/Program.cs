using Networking;
using Networking.Communication;
using System.Net.Sockets;

class Program : INotificationHandler
{
    private static ICommunicator server = CommunicationFactory.GetCommunicator(false, true);
    private int id = 1;

    static async Task Main(string[] args)
    {
        Program program = new Program();

        // Start the server
        string ip = server.Start();
        server.Subscribe("dummy", program, false);

        Console.WriteLine("______________This is server____________");
        Console.WriteLine($"Server running at {ip}");

        string input;
        Console.WriteLine("Type messages to send to the client. Type 'q' to quit.");

        // Keep reading inputs from the console until 'q' is entered
        do
        {
            Console.Write("Server: ");
            input = Console.ReadLine();
            if (input != null && input.Trim().ToLower() != "q")
            {
                server.Send(input, "dummy", null);
            }
        } while (input != null && input.Trim().ToLower() != "q");

        Console.WriteLine("Server shutting down...");
    }

    public void OnDataReceived(string serializedData)
    {
        Console.WriteLine($"Received from client: {serializedData}");
    }

    public void OnClientJoined(TcpClient client, string ip, string port)
    {
        server.AddClient(id++.ToString(), client, ip, port);
        Console.WriteLine($"Client connected: {ip}:{port}");
    }
}
