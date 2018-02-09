using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using sipdotnet;
using WebSocketSharp;
using System.Timers;

namespace Telephony
{
    
    public partial class Form1 : Form
    {
        private Account account;
        private Phone phone;
        private String logs;
        private Ippbx ippbx;
        private bool callState;
        private bool callRinging;
        private Call call;
        private bool connectState;
        private WebSocket wss;
        private WebSocketKu ws;
        private enum CallDirection {
            None,
            Inbound,
            Outbound
        };

        private CallDirection callDir;
        
        public Form1()
        {
            InitializeComponent();
            logs = "";
            callState = false;
            callRinging = false;
            connectState = false;
            ippbx = new Ippbx();
            callDir = CallDirection.None;
            this.ShowInTaskbar = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (! this.connectState)
            {
                connect();
            }
            else
            {
                disconnect();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void broadcastStatus(object source, ElapsedEventArgs e)
        {
            if (phone == null || (phone != null && connectState == false))
            {
                ws.send("status|disconnected");
            }else if (phone != null && connectState == true)
            {
                ws.send("status|connected");
            }
            else
            {
                ws.send("status|disconnected");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;

            statusStrip1.Text = "okebage";
            //Console.WriteLine(folder + "/recorded");
            Console.WriteLine(folder);
            txtLog.Text += "Welcome " + Environment.NewLine;
            lblHost.Text = Properties.Settings.Default.pbx_hosts;
            lblExt.Text = Properties.Settings.Default.pbx_extension;
            


            ippbx.extension = Properties.Settings.Default.pbx_extension;
            ippbx.password = Properties.Settings.Default.pbx_password;
            ippbx.hosts = Properties.Settings.Default.pbx_hosts;
            ippbx.callerid = Properties.Settings.Default.pbx_caller;
            //button2.Enabled = false;


            ws = new WebSocketKu();
            ws.start();
            lblSocket.Text = WebSocketKu.address;
            WebSocketKu.MessageRecived += delegate (string me)
            {
                Console.WriteLine(me);
                invokeUpdateLog("WS Command " + me);
            };


            WebSocketKu.Register += delegate (string host, string ext, string passwrd)
            {
                invokeUpdateLog("registering: " + ext + "@" + host);
                ippbx.extension = ext;
                ippbx.hosts = host;
                ippbx.password = passwrd;
                connect();
            };

            WebSocketKu.Call += delegate (string ex)
            {
                phone.MakeCallAndRecord(ex, folder + "\recorded\testingx.wav");
            };

            WebSocketKu.Terminate += delegate ()
            {
                if (call != null)
                {
                    phone.TerminateCall(call);
                    callState = false;
                }
            };

            WebSocketKu.Recieve += delegate()
            {
                if (call != null && callDir == CallDirection.Inbound && callState == false)
                {
                    callRinging = false;
                    phone.ReceiveCall(call);
                }
            };


            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(broadcastStatus);
            aTimer.Interval = 2000;

            aTimer.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string extension = txtExt.Text;
            Console.WriteLine(callDir);
            System.Diagnostics.Debug.WriteLine("di clik");
            if (this.callDir == CallDirection.None || this.callDir == CallDirection.Outbound)
            {
                if (callState || callRinging)
                {
                    phone.TerminateCall(call);
                    callState = false;
                }
                else
                {
                    if (string.IsNullOrEmpty(extension))
                    {
                        MessageBox.Show("Extension cannot be empty", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        updateLog("Calling " + extension);
                        callRinging = true;
                        phone.MakeCall(extension);
                    }
                }
            }else if (callDir == CallDirection.Inbound && callState == false)
            {
                phone.ReceiveCall(call);
                callState = true;
            }else if (callDir == CallDirection.Inbound && callState == true)
            {
                callState = false;
                phone.TerminateCall(call);
            }
        }

        private void create_call()
        { 
}

        private void disconnect()
        {
            phone.Disconnect();
            this.connectState = false;
            updateLog("Disconnected");
            button2.Enabled = false;
            if (!connectState)
            {
                stsStatus.Text = "Disconnected";
            }
        }

        private void connect()
        {
            if (string.IsNullOrEmpty(ippbx.hosts) || string.IsNullOrEmpty(ippbx.extension) || string.IsNullOrEmpty(ippbx.password))
            {
                MessageBox.Show("IPPBX parameter not complete", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                invokeUpdateLog("Registering ...");
                invokeUpdateLog("Extension:" + ippbx.extension + " password:" + ippbx.password + " hosts:" + ippbx.hosts);

                account = new Account(ippbx.extension, ippbx.password, ippbx.hosts);
                phone = new Phone(account);
                phone.PhoneConnectedEvent += delegate ()
                {
                    invokeUpdateLog("connected");
                    ws.send("register|success");
                    Properties.Settings.Default["pbx_hosts"] = ippbx.hosts;
                    Properties.Settings.Default["pbx_extension"] = ippbx.extension;
                    Properties.Settings.Default["pbx_password"] = ippbx.password;
                    Properties.Settings.Default["pbx_caller"] = ippbx.hosts;
                    Properties.Settings.Default.Save();
                    invokeUpdateTelepohyInfo();
                };

                phone.PhoneDisconnectedEvent += delegate ()
                {
                    invokeUpdateLog("Disconnected");
                    ws.send("register|disconnect");
                };

                phone.CallCompletedEvent += delegate (Call call)
                {
                    callRinging = false;
                    callDir = CallDirection.None;
                    invokeUpdateLog("Call complete");
                    ws.send("call|completed");
                };

                phone.CallActiveEvent += delegate (Call call)
                {
                    callState = true;
                    callRinging = false;
                    this.call = call;
                    invokeUpdateLog("On Call");
                    ws.send("call|active");
                };

                phone.LoadEvent += delegate (Call call)
                {
                    callRinging = true;
                    this.call = call;
                    invokeUpdateLog("Ringging");
                    ws.send("call|ringing");
                };

                phone.IncomingCallEvent += delegate (Call call)
                {
                    this.call = call;
                    callRinging = true;
                    callDir = CallDirection.Inbound;
                    
                    invokeUpdateLog("Incoming Call from " + call.GetFrom());
                    ws.send("call|incoming|" + call.GetFrom());
                };

                phone.Connect();
                this.connectState = true;
                if (button2.InvokeRequired)
                {
                    lblExt.Invoke(new MethodInvoker(delegate { button2.Enabled = true; }));
                }
                else
                {
                    button2.Enabled = true;
                }
            }
        }

        private void telephonyToolStripMenuItem_Click(object sender, EventArgs e)
        {
           formSettingExt form = new formSettingExt();

            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                ippbx = form.Ippbx;
                lblHost.Text = ippbx.hosts;
                lblExt.Text = ippbx.extension;

                connect();
            }
        }

        private void invokeUpdateTelepohyInfo()
        {
            if (lblExt.InvokeRequired)
            {
                lblExt.Invoke(new MethodInvoker(delegate { updateTelephonyInfo(); }));
            }
            else
            {
                updateTelephonyInfo();
            }
        }

        private void updateTelephonyInfo()
        {
            lblHost.Text = ippbx.hosts;
            lblExt.Text = ippbx.extension;
            if (connectState)
            {
                stsStatus.Text = "Connected";
            }
        }

        private void invokeUpdateLog(string text)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new MethodInvoker(delegate { updateLog(text); }));
            }
            else
            {
                updateLog(text);
            }
        }

        private void updateLog(string text)
        {
            var time = DateTime.Now.ToString("yy-MM-d hh:mm:ss");
            txtLog.Text +="[" + time +"] " + text + Environment.NewLine;
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ws.send("nudes");
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            ws.send("nudes");
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void TrayMinimizerForm_Resize(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipTitle = "Minimize to Tray App";
            notifyIcon1.BalloonTipText = "You have successfully minimized your form.";

            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
                
            }
            
        }

        private void groupBox1_Enter_1(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void txtExt_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Enter)
            {
                if (!this.connectState)
                {
                    MessageBox.Show("Not connected to pbx server","Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    return;
                }
                if (this.callDir == CallDirection.None || this.callDir == CallDirection.Outbound)
                {
                    var extension = txtExt.Text;
                    if (callState || callRinging)
                    {
                        phone.TerminateCall(call);
                        callState = false;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(extension))
                        {
                            MessageBox.Show("Extension cannot be empty", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            updateLog("Calling " + extension);
                            callRinging = true;
                            phone.MakeCall(extension);
                        }
                    }
                }
            }
        }
    }
}
