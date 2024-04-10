using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer {

    // This delegate is necessary to specify the parameters that we are passing with the event
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
    internal class Server {

        // Those hash table stores the users and its connections (accessed/consulted by usr), 30 limited users/connections
        public static Hashtable htUsers = new Hashtable(30);
        public static Hashtable htConections = new Hashtable(30);
        // Store the IP Address
        private IPAddress IPaddress;
        private int hostPort;
        private TcpClient tcpClient;

        // The event and its argument will notify the form when a user connects, disconnects, sends a message, ... 
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;

        // The constructor sets the IP address to the one returned by the object instantiation
        public Server(IPAddress address, int port)
        {
            IPaddress = address;
            hostPort = port;
        }

        // This thread will handle the listener and its connections
        private Thread thrListener;

        // TCP object that will listen to connections
        private TcpListener tlsClient;

        // Will tell the while loop to keep monitoring connections, while true the server runs
        bool ServRunning = false;

        // Includes user on hash table
        public static void IncludeUser(TcpClient tcpUser, string strUsername)
        {
            // First will include the name and connection associated with both hash tables
            Server.htUsers.Add(strUsername, tcpUser);
            Server.htConections.Add(tcpUser, strUsername);

            // Informs the new connection for all the users and server form
            SendMessageAdmin(htConections[tcpUser] + " joined...");
        }

        // Remove users from hash table
        public static void RemoveUser(TcpClient tcpUser)
        {
            if (htConections[tcpUser] != null) {
                // Firstly show information and show it to users about connection
                SendMessageAdmin(htConections[(tcpUser)] + " left...");

                // Remove user from hash table
                Server.htUsers.Remove(Server.htConections[tcpUser]);
                Server.htConections.Remove(tcpUser);
            }
        }

        // This is called when we need the event StatusChanged
        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChangedEventHandler statusHandler = StatusChanged;

            if (statusHandler != null) {
                // Invokes delegate
                statusHandler(null, e);
            }
        }

        public static void SendMessageAdmin(string Message)
        {
            StreamWriter swSenderSender;

            // Displays first in the application
            e = new StatusChangedEventArgs("Administrator: " + Message);
            OnStatusChanged(e);

            // Create an array of TCP clients with the size of existing clients
            TcpClient[] tcpClients = new TcpClient[Server.htUsers.Count];

            // Copy TcpClient objetcs to the array
            Server.htUsers.Values.CopyTo(tcpClients, 0);

            // Scroll through TCP client list
            for (int i = 0 ; i < tcpClients.Length ; i++) {
                // Try to send an message to all the clients
                try {
                    // If the message is empty or null connection, exit
                    if (Message.Trim() == "" || tcpClients[i] == null) {
                        continue;
                    }

                    // Send message to user on this loop
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine("Administrator: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                } catch {
                    // If the user does not exist, remove it
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        // Send messages from one user to all
        public static void SendMessage(string Origin, string Message)
        {
            StreamWriter swSenderSender;

            // Firstly displays message on the application
            e = new StatusChangedEventArgs(Origin + " said: " + Message);
            OnStatusChanged(e);

            // Create an array of TCP clients with the size of existing clients
            TcpClient[] tcpClients = new TcpClient[Server.htUsers.Count];

            // Copy TcpClient objetcs to the array
            Server.htUsers.Values.CopyTo(tcpClients, 0);

            // Scroll through TCP client list
            for (int i = 0 ; i < tcpClients.Length ; i++) {
                // Try to send an message to all the clients
                try {
                    // If the message is empty or null connection, exit
                    if (Message.Trim() == "" || tcpClients[i] == null) {
                        continue;
                    }

                    // Send message to user on this loop
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine(Origin + " said: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                } catch {
                    // If the user does not exist, remove it
                    RemoveUser(tcpClients[i]);
                }
            }
        }
        public void StartService()
        {
            try {
                // Get the IP
                IPAddress localIP = IPaddress;
                int localPort = hostPort;

                // Create an TCP listener object using server IP and defined ports
                tlsClient = new TcpListener(localIP, localPort);

                // Start TCP listener and hear its connections
                tlsClient.Start();

                // While loop checks if server is running before checking the connections
                ServRunning = true;

                // Start a new thread hosts the listener
                thrListener = new Thread(MaintainService);
                thrListener.IsBackground = true;
                thrListener.Start();
            } catch (Exception ex) {

            }
        }

        private void MaintainService()
        {
            // While the server is running
            while (ServRunning) {
                // Accept pendent connection
                tcpClient = tlsClient.AcceptTcpClient();

                // Create a new instance of the connection
                Connection newConnection = new Connection(tcpClient);
            }
        }
    }
}
