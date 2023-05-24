using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OsuParsers.Replays;

namespace OsuReplayv0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOsu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".osu files | *.osu";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                tbInfoOsu.Text = path + "\n" + fileName;

                Debug.WriteLine(tbInfoOsu.Text);

                string[] lines;

                try
                {
                    lines = File.ReadAllLines(path);
                }
                catch (IOException)
                {
                    throw;
                }

                foreach (string line in lines)
                {
                    Debug.WriteLine(line);
                }
            }
            else
            {
                // didnt pick anything
            }
        }

        private void btnOsr_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".osr files| *.osr";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                tbInfoOsr.Text = path + "\n" + fileName;

                Debug.WriteLine(tbInfoOsr.Text);

                BinaryReader replayReader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

                Debug.WriteLine("GameMode: " + replayReader.ReadByte());
                Debug.WriteLine("GameVersion: " + replayReader.ReadInt32());

                if (replayReader.ReadByte() != 0x0b)
                {
                    Debug.WriteLine("MD5 Hash: (null)");
                }
                else
                {
                    int result = 0;
                    int shift = 0;
                    while (true)
                    {
                        byte b = replayReader.ReadByte();

                        Debug.WriteLine(b);
                        bool msb = b < 0b_1000_0000;
                        b &= 0b_0111_1111;

                        result |= b << shift;

                        if (msb)
                        {
                            break;
                        }
                        shift += 7;
                    }
                    Debug.WriteLine(result);
                    char[] chars = replayReader.ReadChars(result);
                    string str = string.Join("", chars);

                    Debug.WriteLine("MD5 Hash: " + str);
                }

                if (replayReader.ReadByte() != 0x0b)
                {
                    Debug.WriteLine("Player name: (null)");
                }
                else
                {
                    int result = 0;
                    int shift = 0;
                    while (true)
                    {
                        byte b = replayReader.ReadByte();

                        Debug.WriteLine(b);
                        bool msb = b < 0b_1000_0000;
                        b &= 0b_0111_1111;

                        result |= b << shift;

                        if (msb)
                        {
                            Debug.WriteLine("msb reached");
                            break;
                        }
                        shift += 7;
                    }
                    Debug.WriteLine(result);
                    char[] chars = replayReader.ReadChars(result);
                    string str = string.Join("", chars);

                    Debug.WriteLine("Player name: " + str);
                }

                if (replayReader.ReadByte() != 0x0b)
                {
                    Debug.WriteLine("MD5 Hash (replay): (null)");
                }
                else
                {
                    int result = 0;
                    int shift = 0;
                    while (true)
                    {
                        byte b = replayReader.ReadByte();

                        Debug.WriteLine(b);
                        bool msb = b < 0b_1000_0000;
                        b &= 0b_0111_1111;

                        result |= b << shift;

                        if (msb)
                        {
                            break;
                        }
                        shift += 7;
                    }
                    char[] chars = replayReader.ReadChars(result);
                    string str = string.Join("", chars);

                    Debug.WriteLine("MD5 Hash (replay): " + str);
                }

                Debug.WriteLine("# of 300s: " + replayReader.ReadUInt16());
                Debug.WriteLine("# of 100s: " + replayReader.ReadUInt16());
                Debug.WriteLine("# of 50s: " + replayReader.ReadUInt16());
                Debug.WriteLine("# of Gekis: " + replayReader.ReadUInt16());
                Debug.WriteLine("# of Katus: " + replayReader.ReadUInt16());
                Debug.WriteLine("# of Misses: " + replayReader.ReadUInt16());
                Debug.WriteLine("Total Score: " + replayReader.ReadInt32());
                Debug.WriteLine("Highest Combo: " + replayReader.ReadUInt16());
                Debug.WriteLine("Full Combo: " + (replayReader.ReadByte() == 0b_1 ? "Yes" : "No")); // TODO: double check ==
                Debug.WriteLine("Mods : " + replayReader.ReadInt32());

                if (replayReader.ReadByte() != 0x0b)
                {
                    Debug.WriteLine("Life bar: (null)");
                }
                else
                {
                    int result = 0;
                    int shift = 0;
                    while (true)
                    {
                        byte b = replayReader.ReadByte();

                        Debug.WriteLine(b);
                        bool msb = b < 0b_1000_0000;
                        b &= 0b_0111_1111;

                        result |= b << shift;

                        if (msb)
                        {
                            break;
                        }
                        shift += 7;
                    }
                    Debug.WriteLine("result: " + result);
                    char[] chars = replayReader.ReadChars(result);
                    string str = string.Join("", chars);

                    Debug.WriteLine("Life bar: " + str);
                }

                long timeStamp = replayReader.ReadInt64();
                Debug.WriteLine("Time stamp: " + timeStamp);

                DateTime centuryBegin = new DateTime(2001, 1, 1);
                DateTime currentDate = DateTime.Now;

                long elapsedTicks = timeStamp - centuryBegin.Ticks;
                TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

                DateTime finalDate = centuryBegin + elapsedSpan;

                TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
                finalDate = TimeZoneInfo.ConvertTimeFromUtc(finalDate, localTimeZone);

                string formattedDate = finalDate.ToString("g");

                Debug.WriteLine("Formatted date: " + formattedDate);

                int bytesToRead = replayReader.ReadInt32();
                Debug.WriteLine("Length of compressed replay data: " + bytesToRead);

                // TODO: store byte array
                byte[] byteArr;
                byteArr = replayReader.ReadBytes(bytesToRead);

                Debug.WriteLine("Online Score ID: " + replayReader.ReadUInt64());
                try
                {
                    Debug.WriteLine("Additional Mod Info: " + replayReader.ReadDouble());
                }
                catch (Exception ex) when (ex is IOException || ex is EndOfStreamException) 
                {
                    Debug.WriteLine("Additional Mod Info: (null)");
                    //throw;
                }
            }
            else
            {
                // didnt pick anything
            }
        }
    }
}
