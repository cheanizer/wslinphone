using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Telephony.Views
{
    public partial class FormPlayback : Form
    {
        public string location;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        public FormPlayback()
        {
            InitializeComponent();
        }

        public FormPlayback(string location)
        {
            InitializeComponent();
            this.location = location;
        }

        private void FormPlayback_Load(object sender, EventArgs e)
        {
            if (File.Exists(location))
            {
                btnPlay.Enabled = true;
                slider.Enabled = true;
            }
            else
            {
                MessageBox.Show(this,"Not Found","File Not Found",MessageBoxButtons.OK);
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (audioFile == null)
            {
                audioFile = new AudioFileReader(this.location);
                outputDevice.Init(audioFile);
            }
            TimeSpan time = audioFile.TotalTime;
            int length = (int) time.TotalMilliseconds / 100;
            slider.Maximum = length;
            outputDevice.Play();
            timer1.Start();
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            outputDevice.Dispose();
            outputDevice = null;
            audioFile.Dispose();
            audioFile = null;
            slider.Value = slider.Maximum;
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int position = slider.Value;
            if (slider.Value < slider.Maximum)
            slider.Value = position + 1;
        }
    }
}
