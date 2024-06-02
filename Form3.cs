using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PdfiumViewer;
using Spire.Doc;
using Spire.Xls;
using NAudio.Wave;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using LibVLCSharp;

namespace MultiFileViewer
{
    public partial class Form3 : Form
    {
        private WaveOutEvent waveOut;
        private AudioFileReader audioFileReader;
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private VideoView _videoView;

        public Form3()
        {
            InitializeComponent();

            //for video
            var libvlcPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vls");
           // Core.Initialize();
         //   _libVLC = new LibVLC("--plugin-path=" + libvlcPath);

       //     _mediaPlayer = new MediaPlayer(_libVLC);
         //   _videoView = new VideoView { MediaPlayer = _mediaPlayer, Dock = DockStyle.Fill };

            SetupUI();
        }

        private void SetupUI()
        {
            Button playPauseButton = new Button { Text = "Play/Pause" };
            playPauseButton.Click += PlayPauseButton_Click;

            Button stopButton = new Button { Text = "Stop" };
            stopButton.Click += StopButton_Click;

            FlowLayoutPanel controlPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom };
            controlPanel.Controls.Add(playPauseButton);
            controlPanel.Controls.Add(stopButton);

            panelDisplay.Controls.Add(controlPanel);
        }

        private void buttonSelectFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                listBoxFiles.Items.Clear();
                foreach (string file in openFileDialog.FileNames)
                {
                    listBoxFiles.Items.Add(file);
                }
            }
        }

        private void listBoxFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFiles.SelectedItem != null)
            {
                string selectedFile = listBoxFiles.SelectedItem.ToString();
                DisplayFile(selectedFile);
            }
        }

        private void DisplayFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            panelDisplay.Controls.Clear();

            switch (extension)
            {
                case ".pdf":
                    DisplayPdf(filePath);
                    break;
                case ".txt":
                case ".html":
                case ".htm":
                    DisplayTextOrHtml(filePath);
                    break;
                case ".doc":
                case ".docx":
                    DisplayWordDocument(filePath);
                    break;
                case ".xls":
                case ".xlsx":
                    DisplayExcelDocument(filePath);
                    break;
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                case ".gif":
                    DisplayImage(filePath);
                    break;
                case ".mp3":
                case ".wav":
                    PlayAudio(filePath);
                    break;
                case ".mp4":
                case ".avi":
                case ".wmv":
                    PlayVideo(filePath);
                    break;
                default:
                    MessageBox.Show("Unsupported file type.");
                    break;
            }
        }

        private void DisplayPdf(string filePath)
        {
            var pdfViewer = new PdfViewer
            {
                Dock = DockStyle.Fill,
                Document = PdfDocument.Load(filePath)
            };
            panelDisplay.Controls.Add(pdfViewer);
        }

        private void DisplayTextOrHtml(string filePath)
        {
            var webBrowser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                Url = new Uri(filePath)
            };
            panelDisplay.Controls.Add(webBrowser);
        }

        private void DisplayWordDocument(string filePath)
        {
            var document = new Document();
            document.LoadFromFile(filePath);
            var richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
            //    Rtf = document.GetRtfText()
            };
            panelDisplay.Controls.Add(richTextBox);
        }

        private void DisplayExcelDocument(string filePath)
        {
            var workbook = new Workbook();
            workbook.LoadFromFile(filePath);
            var sheet = workbook.Worksheets[0];
            var richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Text = sheet.ExportDataTable().ToString()
            };
            panelDisplay.Controls.Add(richTextBox);
        }

        private void DisplayImage(string filePath)
        {
            var pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile(filePath)
            };
            panelDisplay.Controls.Add(pictureBox);
        }

        private void PlayAudio(string filePath)
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                audioFileReader.Dispose();
            }

            audioFileReader = new AudioFileReader(filePath);
            waveOut = new WaveOutEvent();
            waveOut.Init(audioFileReader);
            waveOut.Play();
        }

        //private void PlayVideo(string filePath)
        //{
        //    _mediaPlayer.Media?.Dispose();
        //    var media = new Media(_libVLC, new Uri(filePath));
        //    _mediaPlayer.Media = media;
        //    panelDisplay.Controls.Add(_videoView);
        //    _videoView.Dock = DockStyle.Fill;
        //    _mediaPlayer.Play();
        //}

        private void PlayVideo(string filePath)
        {
            _mediaPlayer.Media?.Dispose();
            var media = new Media(_libVLC, new Uri(filePath));
            _mediaPlayer.Media = media;

            // إضافة إعدادات إضافية إذا لزم الأمر
            // _mediaPlayer.SetVideoTrack(0);
            // _mediaPlayer.SetAudioTrack(1);

            panelDisplay.Controls.Clear(); // تفريغ المحتويات السابقة
            panelDisplay.Controls.Add(_videoView); // إضافة عنصر عرض الفيديو
            _videoView.Dock = DockStyle.Fill; // تأكد من أن عرض الفيديو ملء الشاشة

            _mediaPlayer.Play();
        }


        private void PlayPauseButton_Click(object sender, EventArgs e)
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
            }
            else
            {
                _mediaPlayer.Play();
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            _mediaPlayer.Stop();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Dispose();
            }

            if (_libVLC != null)
            {
                _libVLC.Dispose();
            }

            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
            }

            if (audioFileReader != null)
            {
                audioFileReader.Dispose();
            }
        }
    }
}
