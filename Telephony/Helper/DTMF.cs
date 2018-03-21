using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Timers;


namespace Telephony.Helper
{
    class DTMF
    {
        public string dtmf;
        Dictionary<string, int> dic;
        List<SoundPlayer> players;
        string[] codes;
        public Timer timer;
        private static int index = 0;
        public DTMF()
        {
            timer = new Timer();
            timer.Interval = 210;
            players = new List<SoundPlayer> {
                new SoundPlayer(Properties.Resources._0),
                new SoundPlayer(Properties.Resources._1),
                new SoundPlayer(Properties.Resources._2),
                new SoundPlayer(Properties.Resources._3),
                new SoundPlayer(Properties.Resources._4),
                new SoundPlayer(Properties.Resources._5),
                new SoundPlayer(Properties.Resources._6),
                new SoundPlayer(Properties.Resources._7),
                new SoundPlayer(Properties.Resources._8),
                new SoundPlayer(Properties.Resources._9)
            };
            codes = new string[]{ "D0","D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9" };
            dic = new Dictionary<string, int>();
            dic.Add("D0", 0);
            dic.Add("D1", 1);
            dic.Add("D2", 2);
            dic.Add("D3", 3);
            dic.Add("D4", 4);
            dic.Add("D5", 5);
            dic.Add("D6", 6);
            dic.Add("D7", 7);
            dic.Add("D8", 8);
            dic.Add("D9", 9);

            timer.Elapsed += new ElapsedEventHandler((s, e) => {
                
                var lenght = this.dtmf.Length;
                if (lenght > 1)
                {
                    var ind = System.Threading.Interlocked.Increment(ref index);
                    if (ind <= lenght)
                    {
                        Console.WriteLine(this.dtmf[ind - 1]);
                        players[(int) Char.GetNumericValue(this.dtmf[ind - 1])].Play();
                    }
                    else
                    {
                        timer.Stop();
                        timer.Enabled = false;
                    }
                }
                else
                {
                    timer.Stop();
                    timer.Enabled = false;
                }
                
            });
        }

        public void setDtmf(string dtmf)
        {
            double num;
            var isNumeric = double.TryParse(dtmf, out num);
            if (isNumeric)
            {
                this.dtmf = dtmf;
            }
            else
            {
                throw new InvalidOperationException("DTMF must be number");
            }
        }

        public void semua()
        {
            if (this.dtmf != null && this.dtmf.Equals("") == false)
            {
                DTMF.index = 0;
                timer.Enabled = true;
                timer.Start();
            }
        }

        public void play(int number)
        {
            players[number].Play();
        }

        public void playKeyCode(string keycode)
        {
            if (codes.Contains(keycode))
            {
                
                players[dic[keycode]].Play();
            }

            
        }

        public string getNumber(string keycode)
        {
            if (codes.Contains(keycode))
                return dic[keycode].ToString();
            else return "";
        }
    }
}
