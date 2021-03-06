using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundMaker.Properties;

namespace SoundMaker
{
    class SoundMaker
    {
        public static NotifyIcon notifyIcon;
        public static ContextMenuStrip menu;
        public static WaveOutEvent waveOutEvent;

        public static double gain = 0.001d;
        public static double frequency = 10;

        public static Thread soundLoopThread;

        static void Main(string[] args)
        {
            Thread notifyThread = new Thread(NotifyIconRenderer);
            notifyThread.Start();

            using var wo = new WaveOutEvent();
            soundLoopThread = new Thread(SoundLoopWorker);
            soundLoopThread.Start();
        }

        static void SoundLoopWorker()
        {
            waveOutEvent = new WaveOutEvent();
            var sample = new SignalGenerator { Frequency = frequency, Gain = gain };
            while (true)
            {
                if (waveOutEvent.PlaybackState != PlaybackState.Playing || sample.Gain != gain || sample.Frequency != frequency)
                {
                    sample.Gain = gain;
                    sample.Frequency = frequency;
                    waveOutEvent.Stop();
                    waveOutEvent.Init(sample.Take(TimeSpan.FromHours(1)));
                    waveOutEvent.Play();
                }
                Thread.Sleep(500);
            }
        }

        static void NotifyIconRenderer()
        {

            var gainTextBox = new ToolStripTextBox() { Text = gain.ToString() };
            gainTextBox.TextChanged += GainTextBox_TextChanged;

            var frequencyTextBox = new ToolStripTextBox() { Text = frequency.ToString() };
            frequencyTextBox.TextChanged += FrequencyTextBox_TextChanged;

            menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripLabel("Gain:"));
            menu.Items.Add(gainTextBox);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripLabel("Frequency:"));
            menu.Items.Add(frequencyTextBox);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Add to autostart", null, AddToAutostart_Click);
            menu.Items.Add("Exit", null, MenuExit_Click);

            notifyIcon = new NotifyIcon
            {
                BalloonTipTitle = "Sound Maker - Uliczny Denser",
                Icon = Resources.icon,
                ContextMenuStrip = menu,
                Text = "Sound Maker",
                Visible = true
            };

            Application.Run();
        }

        static void GainTextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is ToolStripTextBox textBox)
            {
                var isDouble = double.TryParse(textBox.Text, out double value);
                if (isDouble)
                {
                    gain = value;
                }
            }
        }

        static void FrequencyTextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is ToolStripTextBox textBox)
            {
                var isDouble = double.TryParse(textBox.Text, out double value);
                if (isDouble)
                {
                    frequency = value;
                }
            }
        }

        static void AddToAutostart_Click(object sender, EventArgs e)
        {
            RegistryKey registryKeyk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var existsInAutostart = registryKeyk.GetValue("Sound Maker") != null;

            if (existsInAutostart)
            {
                registryKeyk.DeleteValue("Sound Maker");
                notifyIcon.BalloonTipText = "Removed from Autostart";
            }
            else
            {
                registryKeyk.SetValue("Sound Maker", Application.ExecutablePath);
                notifyIcon.BalloonTipText = "Added to Autostart";
            }

            notifyIcon.ShowBalloonTip(500);
        }

        static void MenuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }
    }
}
