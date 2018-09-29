using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SocketAsyncLib;

namespace SocketAsyncTest
{
    
    public partial class Form1 : Form
    {
        private delegate void SetText(string message);
        public Form1()
        {
            InitializeComponent();
            
            SocketServer server = new SocketServer("192.168.0.254", 55000);
            server.ReceiveEvent += Server_ReceiveEvent;      
            server.Start();
        }

        private void Server_ReceiveEvent(object sender, EventArgs e)
        {
            ReceiveEventArgs rea = e as ReceiveEventArgs;
            string data = Encoding.GetEncoding(936).GetString(rea.Receive);
            AddText(data);
            
        }
        private void AddText(string message)
        {
            if (this.rtb.InvokeRequired)
            {
                this.rtb.BeginInvoke(new SetText(AddText), new object[] { message });
            }else
            {
                this.rtb.AppendText(message + "\n");
                this.rtb.ScrollToCaret();
            }
        }
    }
}
