using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace CCam
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            get_listCam();
            getAvailablePorts();
            toolStripComboBox4.SelectedIndex = 1;
        }
        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox1.Text == "- Refresh -")
            {
                get_listCam();
            }
        }
        private void toolStripComboBox3_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox3.Text == "- Refresh -")
            {
                getAvailablePorts();
            }
        }
        void getAvailablePorts()
        {
            toolStripComboBox3.Items.Clear();
            String[] ports = SerialPort.GetPortNames();
            toolStripComboBox3.Items.AddRange(ports);
            toolStripComboBox3.Items.Add("- Refresh -");
            toolStripComboBox3.SelectedIndex = 0;
        }

        private FilterInfoCollection webcam = null;
        private VideoCaptureDevice cam = null;

        private void get_listCam()
        {
            try
            {
                toolStripComboBox1.Items.Clear();
                webcam = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                foreach (FilterInfo Vid in webcam)
                {
                    toolStripComboBox1.Items.Add(Vid.Name);
                }
                toolStripComboBox1.Items.Add("- Refresh -");
                toolStripComboBox1.SelectedIndex = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("Error, Please refresh the Video Device List!");
            }

        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (toolStripButton1.Text == "Start")
                {
                    cam.VideoResolution = cam.VideoCapabilities[toolStripComboBox2.SelectedIndex];
                    cam.NewFrame += Cam_NewFrame;
                    cam.Start();
                    toolStripButton1.Text = "Stop";
                }
                else
                {
                    if (cam.IsRunning)
                    {
                        cam.NewFrame -= Cam_NewFrame;
                        Thread.Sleep(200);
                        cam.Stop();
                    }
                    toolStripButton1.Text = "Start";
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error! Can not start the device.");
            }
        }
        bool capture = false;
        Image CapImage = null;
        private void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
                    pictureBox1.Image = new Bitmap(eventArgs.Frame);
                    if (capture)
                    {
                        if (CapImage != null) CapImage.Dispose();
                        CapImage = new Bitmap(eventArgs.Frame);
                        capture = false;
                        Capture.Text = "Captured";
                        saveToolStripButton.Text = "Save";
                    }
                }
                catch (Exception)
                {

                }
                GC.Collect(); //Without this, memory goes nuts
            });
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (toolStripComboBox1.Text == "- Refresh -")
                {
                    get_listCam();
                }
                else
                {
                    if (cam != null) cam = null;
                    cam = new VideoCaptureDevice(webcam[toolStripComboBox1.SelectedIndex].MonikerString);
                    toolStripComboBox2.Items.Clear();
                    for (int index = 0; index < cam.VideoCapabilities.Count(); index++)
                    {
                        string selRes = cam.VideoCapabilities[index].FrameSize.Height.ToString() + "x" + cam.VideoCapabilities[index].FrameSize.Width.ToString();
                        toolStripComboBox2.Items.Add(selRes);
                    }
                    toolStripComboBox2.SelectedIndex = 0;
                }
            }
            catch (Exception)
            {
            }
        }

        string path = "";
        int index = 0;
        private void Capture_Click(object sender, EventArgs e)
        {
            capture = true;
            Capture.Text = "Capturing...";
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            if (path != "")
            {
                if (CapImage != null)
                {
                    saveToolStripButton.Text = "Saving...";
                    CapImage.Save(path + "/" + toolStripTextBox1.Text + "-" + index.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    index++;
                    toolStripLabel3.Text = "- " + index.ToString();
                    Capture.Text = "Capture";
                    saveToolStripButton.Text = "Saved";
                }
            }
            else
            {
                MessageBox.Show("Select Path!");
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select Output Folder";
            if(fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.SelectedPath;
                this.Text = "CCam | Path: " + path;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (toolStripButton2.Text == "Connect")
                {
                    serialPort1.PortName = toolStripComboBox3.Text;
                    serialPort1.BaudRate = Convert.ToInt32(toolStripComboBox4.Text);
                    serialPort1.ReadTimeout = 1000;
                    serialPort1.WriteTimeout = 1000;
                    serialPort1.DataReceived += SerialPort1_DataReceived;
                    serialPort1.Open();
                    toolStripButton2.Text = "Disconnect";
                }
                else
                {
                    serialPort1.Close();
                    toolStripButton2.Text = "Connect";
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Unauthorizzed Accessed");
            }
        }
        string data = "";
        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                data = sp.ReadLine();
                this.Invoke(new EventHandler(deCode));
            }
            catch (TimeoutException)
            {
                serialPort1.DiscardInBuffer();
            }
        }
        private void deCode(object sender, EventArgs e)
        {
            try
            {
                char[] spliter = { ':', '=', ']', '\r','\n'};
                string[] datas = data.Split(spliter);
                if (datas.Length > 1)
                {
                    toolStripTextBox1.Text = datas[0];
                    if (datas[0] == "[CI" || datas[0] == "CI")
                    {
                        toolStripTextBox1.Text = datas[1];
                        Capture.PerformClick();
                        timer1.Start();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(!capture)
            {
                if (CapImage != null)
                {
                    saveToolStripButton.Text = "Saving...";
                    CapImage.Save(path + "/" + toolStripTextBox1.Text + "-" + index.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    index++;
                    toolStripLabel3.Text = "- " + index.ToString();
                    Capture.Text = "Capture";
                    saveToolStripButton.Text = "Saved";
                    timer1.Stop();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cam != null)
                if (cam.IsRunning)
                {
                    cam.NewFrame -= Cam_NewFrame;
                    Thread.Sleep(100);
                    cam.Stop();
                }
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            help hh = new help();
            hh.ShowDialog();
        }
    }
}
