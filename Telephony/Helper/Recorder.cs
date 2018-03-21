using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;
using System.IO;
using NAudio.Wave.SampleProviders;
using NAudio.Utils;
using NAudio.CoreAudioApi;
using Telephony.Models;

namespace Telephony.Helper
{
    class Recorder
    {
        public WasapiCapture eventInput;
        public WaveFileWriter writerInput;
        public WasapiLoopbackCapture eventOutput;
        public WaveFileWriter writerOutput;
        public string outputFolder;
        public string inputFilePath;
        public string outputFilepath;
        public string inputPrefix = "_in";
        public string outputPrefix = "_out";
        public string combinePrefix = "_all";
        public bool inputDone = false;
        public bool outputDone = false;
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string Location { get; set; }
        public string Filename { get; set; }
        public string FilenameOut { get; set; }
        public string FilenameCom { get; set; }

        public Recording recording;

        public event EventHandler CombineComplete;
        

        public Recorder(string prefix = "")
        {
            Prefix = prefix;
            initRecording();

            outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "recording");
            Console.WriteLine(outputFolder);
            Directory.CreateDirectory(outputFolder);
            Name = getSessionName();
            Location = outputFolder;
            eventInput = new WasapiCapture();
            writerInput = null;
            writerOutput = null;
            eventOutput = new WasapiLoopbackCapture();

            eventInput.DataAvailable += (s, a) => {
                writerInput.Write(a.Buffer, 0, a.BytesRecorded);
                if (writerInput.Position > eventInput.WaveFormat.AverageBytesPerSecond * (60 * 30))
                {
                    eventInput.StopRecording();
                }
            };
            eventInput.RecordingStopped += (s, a) =>
            {
                writerInput?.Dispose();
                writerInput = null;
                inputDone = true;
                combineAll();
            };

            eventOutput.DataAvailable += (s, a) => {
                writerOutput.Write(a.Buffer, 0, a.BytesRecorded);
                if (writerOutput.Position > eventOutput.WaveFormat.AverageBytesPerSecond * (60 * 30))
                {
                    eventOutput.StopRecording();
                }
            };

            eventOutput.RecordingStopped += (s,a) => {
                writerOutput?.Dispose();
                writerOutput = null;
                outputDone = true;
                combineAll();
            };
        }

        public void combineAll()
        {
            if (inputDone == true && outputDone == true)
            {
                concatInOut();
                inputDone = outputDone = false;
                //CompletedCombineEvent();
            }
        }

        public void startRecordInput()
        {
            recording.StartAt = DateTime.Now;
            if (Prefix != null && Prefix.Equals("") == false)
                Filename = Prefix + "_" + Name + inputPrefix + ".wav";
            else Filename = Name + inputPrefix + ".wav";
            inputFilePath = Path.Combine(outputFolder, Filename);
            writerInput = new WaveFileWriter(inputFilePath, eventInput.WaveFormat);
            eventInput.StartRecording();
        }

        public void stopRecordInput()
        {
            recording.StopAt = DateTime.Now;
            eventInput.StopRecording();
        }

        public void startRecordOutput()
        {
            recording.StartAt = DateTime.Now;

            if (Prefix != null && Prefix.Equals("") == false)
                FilenameOut = Prefix + "_" + Name + outputPrefix + ".wav";
            else FilenameOut = Name + outputPrefix + ".wav";
            outputFilepath = Path.Combine(outputFolder, FilenameOut);
            writerOutput = new WaveFileWriter(outputFilepath,eventOutput.WaveFormat);
            eventOutput.StartRecording();
        }

        public void stopRecordOutput()
        {
            recording.StopAt = DateTime.Now;
            eventOutput.StopRecording();
        }

        public string getSessionName()
        {
            int epoch = Common.getEpoch();
            return epoch.ToString();
        }


        public void concatInOut()
        {
            if (outputFilepath != null && outputFilepath.Equals("") == false && inputFilePath != null && inputFilePath.Equals("") == false)
            {
                using (var input = new AudioFileReader(inputFilePath))
                using (var output = new AudioFileReader(outputFilepath))
                {
                   
                    if (Prefix != null && Prefix.Equals("") == false)
                        FilenameCom = Prefix + "_" + Name + combinePrefix + ".wav";
                    else FilenameCom = Name + combinePrefix + ".wav";
                    var combineFilepath = Path.Combine(outputFolder, FilenameCom);
                    Console.WriteLine(combineFilepath);
                    var gabung = new MixingSampleProvider(new[] { input, output });
                    try {
                        WaveFileWriter.CreateWaveFile16(combineFilepath, gabung);
                    }catch (Exception e)
                    {
                        Console.WriteLine("failed delete");
                    }
                    
                    this.recording.Filename = FilenameCom;
                    this.recording.Location = combineFilepath;
                    TimeSpan x = this.recording.StartAt - this.recording.StopAt;
                    this.recording.Duration = x.TotalSeconds;
                    EventHandler cc = CombineComplete;
                    try
                    {
                        input?.Dispose();
                        output?.Dispose();
                        File.Delete(inputFilePath);
                        File.Delete(outputFilepath);
                    }catch(Exception e)
                    {
                        Console.WriteLine("failed delete");
                    }
                    
                    if (cc != null)
                    {
                        cc(this, EventArgs.Empty);
                    }
                }
            }
        }

        public void initRecording()
        {
            recording = new Recording();
            recording.CallerId = Prefix;
        }

        public Recording getRecording()
        {
            return recording;
        }

    }
}
