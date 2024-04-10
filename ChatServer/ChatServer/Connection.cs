using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer {

    // This class handles connections, there will be as many as the instances of the connected user
    internal class Connection {
        
        TcpClient tcpClient;

        // Thread that will send information to client
        private Thread thrSender;
        private StreamReader srReceptor;
        private StreamWriter swSender;
        private string currentUser;
        private string strResponse;

        // The constructor from the class that handles the TCP conection
        public Connection(TcpClient tcpCon)
        {
            tcpClient = tcpCon;

            // The thread that accepts the client waits the message
            thrSender = new Thread(AcceptClient);
            thrSender.IsBackground = true;

            // The thread calls the AcceptClient() method
            thrSender.Start();
        }
        private void CloseConnection()
        {
            // Closes open objects
            tcpClient.Close();
            srReceptor.Close();
            swSender.Close();
        }

        // Occurs when a new client is accepted
        private void AcceptClient()
        {
            srReceptor = new StreamReader(tcpClient.GetStream());
            swSender = new StreamWriter(tcpClient.GetStream());

            // Read client account information
            currentUser = srReceptor.ReadLine();

            // We have an response from the client
            if (currentUser != "") {
                // Stores the user name in the hash table
                if (Server.htUsers.Contains(currentUser)) {
                    // 0 => means not connected
                    swSender.WriteLine("0|This username alreadye exists.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                } else if (currentUser == "Administrator") {
                    // 0 => means not connected
                    swSender.WriteLine("0|This username alreadye exists.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                } else {
                    // 1 => succesfully connected
                    swSender.WriteLine("1");
                    swSender.Flush();

                    // Includes user in hash table and start listening its messages
                    Server.IncludeUser(tcpClient, currentUser);
                }
            } else {
                CloseConnection();
                return;
            }

            try {
                // Continues waiting for user message
                while ((strResponse = srReceptor.ReadLine()) != "") {
                    // If so, remove it
                    if (strResponse == null) {
                        Server.RemoveUser(tcpClient);
                    } else {
                        // Sends a message for all the other users
                        Server.SendMessage(currentUser, strResponse);
                    }
                }
            } catch {
                // If there is a problem with this user, disconnect
                Server.RemoveUser(tcpClient);
            }
        }
    }
}
