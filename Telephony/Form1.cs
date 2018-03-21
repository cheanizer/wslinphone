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
using Telephony.Helper;
using Telephony.Models;
using LiteDB;
using System.Media;

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
        private Recorder recorder;
        private CallDirection callDir;
        private string inboundCallerId;
        private LiteDatabase db;
        private LiteCollection<Recording> collection;
        private int duration;
        private CallerIdentity callerIdentity;
        private DTMF dtdt;
        private SoundPlayer ringer;
        
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
            db = new LiteDatabase(@"nwrtelephony");
            collection = db.GetCollection<Recording>("recording");
            dtdt = new DTMF();
            ringer = new SoundPlayer(Properties.Resources.telephone_ring_04);

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
            txtLog.Text += "Welcome " + Environment.NewLine;
            stsStatus.Text = "Disconnected";
            lblHost.Text = Properties.Settings.Default.pbx_hosts;
            lblExt.Text = Properties.Settings.Default.pbx_extension;
            ippbx.extension = Properties.Settings.Default.pbx_extension;
            ippbx.password = Properties.Settings.Default.pbx_password;
            ippbx.hosts = Properties.Settings.Default.pbx_hosts;
            ippbx.callerid = Properties.Settings.Default.pbx_caller;
            
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
                invokeUpdateLog(ex);
                dtdt.setDtmf(ex);
                dtdt.semua();
                phone.MakeCall(ex);
                
            };

            WebSocketKu.Terminate += delegate ()
            {
                if (call != null)
                {
                    phone.TerminateCall(call);
                    callState = false;
                    ringer.Stop();
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

            WebSocketKu.SendDtmf += delegate (string number)
            {
                dtdt.play(int.Parse(number));
                phone.SendDTMFs(call,number);
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

                Console.WriteLine(callState);
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
                        callerIdentity = new CallerIdentity(extension);

                        updateLog("Calling " + callerIdentity.Number);
                        callRinging = true;
                        //phone.MakeCall(callerIdentity.Number);
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
                string pwd = Common.passwordToAsterixs(ippbx.password);
                invokeUpdateLog("Extension:" + ippbx.extension + " password:" + pwd + " hosts:" + ippbx.hosts);

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
                    if (recorder != null)
                    {
                        recorder.stopRecordInput();
                        recorder.stopRecordOutput();
                    }
                    invokeUpdateLog("Call complete");
                    callerIdentity = null;
                    ws.send("call|completed");
                    notifyIcon1.BalloonTipTitle = "Call Completed";
                    notifyIcon1.BalloonTipText = "Call completed";
                    notifyIcon1.ShowBalloonTip(10000000);
                    ringer.Stop();
                };

                phone.CallActiveEvent += delegate (Call call)
                {
                    ringer.Stop();
                    callState = true;
                    callRinging = false;
                    this.call = call;
                    if (this.callerIdentity != null && (this.callerIdentity.Extension != null && this.callerIdentity.Extension.Equals("") == false ))
                    {
                        phone.SendDTMFs(call, callerIdentity.Extension);
                    }
                    
                    invokeUpdateLog("On Call");
                    if (callDir == CallDirection.Inbound)
                    {
                        recorder = new Recorder(inboundCallerId);
                    }else
                    {
                        recorder = new Recorder(txtExt.Text);
                    }
                    recorder.startRecordInput();
                    recorder.startRecordOutput();
                    recorder.CombineComplete += (s, e) => {
                       Recording recording = recorder.getRecording();
                       if (callDir == CallDirection.Inbound)
                        {
                            recording.Direction = Recording.RecDirection.Inbound;
                        }
                        else
                        {
                            recording.Direction = Recording.RecDirection.Outbound;
                        }
                        recording.Status = Recording.RecStatUpload.Unuploaded;
                        invokeUpdateLog("Recording : " + recording.Filename);
                        ws.send("recording|" + recording.Filename);
                        collection.Insert(recording);
                    };
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
                    callState = false;
                    inboundCallerId = Common.getExtensionNumberFromCall(call.GetFrom());
                    invokeUpdateLog("Incoming Call from " + inboundCallerId);
                    ws.send("call|incoming|" + inboundCallerId);
                    notifyIcon1.BalloonTipTitle = "Incoming Call";
                    notifyIcon1.BalloonTipText = "Incoming call from " + inboundCallerId;
                    notifyIcon1.ShowBalloonTip(1);
                    ringer.Play();
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

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.Activate();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void TrayMinimizerForm_Resize(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipTitle = "Minimize to Tray App";
            notifyIcon1.BalloonTipText = "You have successfully minimized your form.";

            if (FormWindowState.Minimized == this.WindowState)
            {
                if (notifyIcon1.Visible == false)
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(30000);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                //notifyIcon1.Visible = false;
            }
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
                            callerIdentity = new CallerIdentity(extension);
                            updateLog("Calling " + callerIdentity.Number);
                            callRinging = true;
                            phone.MakeCall(callerIdentity.Number);
                        }
                    }
                }
            }
            else
            {
                string code = e.KeyData.ToString();
                dtdt.playKeyCode(code);
                if (callState)
                {
                    phone.SendDTMFs(call,dtdt.getNumber(code));
                }
                Console.WriteLine(e.KeyData.ToString());
            }
        }

        private void recordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form rec = new Views.FormRecording();
            rec.Text = "Recording";
            rec.Owner = this;
            rec.ShowDialog(this);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Console.Beep();
        }
    }
}
