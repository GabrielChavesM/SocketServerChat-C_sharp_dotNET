using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer {
    public partial class Form1 : Form {

        private delegate void UpdateStatusCallback(string strMessage);

        bool connected = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            if (connected) {
                Application.Exit();
                return;
            }

            if (txtIP.Text == string.Empty) {
                MessageBox.Show("Inform the IP address.");
                txtIP.Focus();
                return;
            }

            try {
                // Analising server IP address informed in the textbox
                IPAddress IPaddress = IPAddress.Parse(txtIP.Text);
                int hostPort = (int)numPort.Value;

                // Create a new ChatServer object instance
                Server mainServer = new Server(IPaddress, hostPort);

                // Link event treatment of the event StatusChanged to mainServer_StatusChanged
                Server.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);

                // Starts servicing connections
                mainServer.StartService();

                // Shows that the service for connections have started
                listLog.Items.Add("Server active, waiting for users to connect...");
                listLog.SetSelected(listLog.Items.Count - 1, true);
            } catch (Exception ex) {
                listLog.Items.Add("Connection error: " + ex.Message);
                listLog.SetSelected(listLog.Items.Count - 1, true);
                return;
            }

            connected = true;
            txtIP.Enabled = false;
            numPort.Enabled = false;
            btnStartServer.ForeColor = Color.Red;
            btnStartServer.Text = "Exit";
        }

        public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // Calls the method that updates the form
            this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
        }

        private void UpdateStatus(string strMessage)
        {
            // Updates messages logo
            listLog.Items.Add(strMessage);
            listLog.SetSelected(listLog.Items.Count - 1, true);
        }
    }
}
