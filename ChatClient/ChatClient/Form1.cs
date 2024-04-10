using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient {
    public partial class Form1 : Form {

        // Handles user name
        private string UserName;
        private StreamWriter stwSender;
        private StreamReader strReader;
        private TcpClient tcpServer;
        // Necessary to update the form with messages from the other thread
        private delegate void UpdateLogCallBack(string strMessage);
        // Necessary to define the form for the "disconnected" state from the other thread
        private delegate void ExitConnectionCallBack(string strReason);
        private Thread messageThread;
        private IPAddress IPaddress;
        private int hostPort;
        private bool Connected;

        public Form1()
        {
            // When exiting the application: disconnect
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // If its not connected, wait to connect
            if (!Connected) {
                // Start connection
                InitializeConnection();
            } else {
                // If connected, disconnect
                ExitConnection("Disconnected by the user.");
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If "Enter" was pressed
            if (e.KeyChar == (char)13) {
                SendMessage();
            }
        }

        private void InitializeConnection()
        {
            try {
                // Handles IP address informed in a object IPAddress
                IPaddress = IPAddress.Parse(txtServerIP.Text);
                // Handles the host port number
                hostPort = (int)numHostPort.Value;
                // Start a new TCP connection with server chat
                tcpServer = new TcpClient();
                tcpServer.Connect(IPaddress, hostPort);
                // Helps verifying if we are connected
                Connected = true;
                // Prepare form
                UserName = txtUser.Text;
                // Disable and able the appropriated camps
                txtServerIP.Enabled = false;
                numHostPort.Enabled = false;
                txtUser.Enabled = false;
                txtMessage.Enabled = true;
                btnSend.Enabled = true;
                btnConnect.ForeColor = Color.Red;
                btnConnect.Text = "Disconnect";
                // Send the user name to server
                stwSender = new StreamWriter(tcpServer.GetStream());
                stwSender.WriteLine(txtUser.Text);
                stwSender.Flush();

                // Start the thread needed to receive messages and new communications
                messageThread = new Thread(new ThreadStart(ReceiveMessage));
                messageThread.IsBackground = true;
                messageThread.Start();
                labelStatus.Invoke(new Action(() => {
                    labelStatus.ForeColor = Color.Green;
                    labelStatus.Text = $"Connected to chat server {IPaddress}:{hostPort}";
                }));
            } catch (Exception ex) {
                labelStatus.Invoke(new Action(() => {
                    labelStatus.ForeColor = Color.Red;
                    labelStatus.Text = "Error when trying to connect to the server: \n" + ex.Message;
                }));
            }
        }

        private void ReceiveMessage()
        {
            // Receive the message from server
            strReader = new StreamReader(tcpServer.GetStream());
            string ConReply = strReader.ReadLine();

            // If the first character is 1, connected successfully
            if (ConReply[0] == '1') {
                // Updates the form to inform that it is connected
                this.Invoke(new UpdateLogCallBack(this.UpdateLog), new object[] {"Connected Successfully!"});
            } else {
                // If the first char is not 1, connection failed
                string Reason = "Not connected: ";
                // Extracts the reason of the reply message. The reason starts in the 3rd char
                Reason += ConReply.Substring(2, ConReply.Length - 2);
                // Updates the form informing the reason for the connection to fail
                this.Invoke(new ExitConnectionCallBack(this.ExitConnection), new object[] { Reason });
                // Leaves method
                return;
            }

            // While connected, reads the lines coming from the server
            while (Connected) {
                // Display message on TextBox
                this.Invoke(new UpdateLogCallBack(this.UpdateLog), new object[] { strReader.ReadLine() });
            }
        }
        private void UpdateLog(string strMessage)
        {
            // Annex text to the end of each line
            txtLog.AppendText(strMessage + "\r\n");
        }
        
        private void SendMessage()
        {
            // Send messages to server
            if (txtMessage.Lines.Length >= 1) {
                stwSender.WriteLine(txtMessage.Text);
                stwSender.Flush();
                txtMessage.Lines = null;
            }
            txtMessage.Text = "";
        }
        private void ExitConnection(string Reason)
        {
            // Closes connection to server
            // Show reason to close server
            txtLog.AppendText(Reason + "\r\n");
            // Disable and able the appropriated camps in form
            txtServerIP.Enabled = true;
            numHostPort.Enabled = true;
            txtUser.Enabled = true;
            txtMessage.Enabled = false;
            btnSend.Enabled = false;
            btnConnect.ForeColor = Color.Green;
            btnConnect.Text = "Connect";

            // Close the objects
            Connected = false;
            stwSender.Close();
            strReader.Close();
            tcpServer.Close();
            labelStatus.Invoke(new Action(() => {
                labelStatus.ForeColor = Color.Green;
                labelStatus.Text = $"Disconnected from chat server {IPaddress}:{hostPort}";
            }));
        }

        // Handles the event to exit the app
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected) {
                // Exit all connections, streams, etc ...
                Connected = false;
                stwSender.Close();
                strReader.Close();
                tcpServer.Close();
                labelStatus.Invoke(new Action(() => {
                    labelStatus.ForeColor = Color.Green;
                    labelStatus.Text = $"Disconnected from chat server {IPaddress}:{hostPort}";
                }));
            }
        }
    }
}
