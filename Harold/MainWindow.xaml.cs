using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
//using OpenCvSharp;
using WebEye.Controls.Wpf;
using System.Drawing;
using Newtonsoft.Json;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Threading;
using Timer = System.Windows.Forms.Timer;
using System.Security.Permissions;
using Window = System.Windows.Window;

namespace Harold
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ViewModel myVM { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            myVM = new ViewModel();
            myVM.cameras = new List<WebCameraId>(Webcam.GetVideoCaptureDevices());
            myVM.SelectedCamera = myVM.cameras[0];
            this.DataContext = myVM;
        }

        private void StartCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Display webcam video
                //WebcamViewer.StartPreview();
                Webcam.StartCapture(myVM.SelectedCamera);
                //myVM.p2pConverter.LoadGraph();
                //vide
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StopCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop the display of webcam video.
            //WebcamViewer.StopPreview();
            Webcam.StopCapture();
        }

        private void StartRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            // Start recording of webcam video to harddisk.
            //WebcamViewer.StartRecording();
        }

        private void StopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop recording of webcam video to harddisk.
            //WebcamViewer.StopRecording();
        }

        private void ScreeningButton_Click(object sender, RoutedEventArgs e)
        {
            InitTimer();
        }
        // bal bal balba
        private Timer timer1;
        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 10000; // in miliseconds
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TakeSnapshotButton_Click(sender, null);
        }

        private void TakeSnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            // Take snapshot of webcam video.
            /// DEBUG
            //string name = @"C:\WebcamSnapshots\test2.jpg"; //@"C:\workspace\Harold\Harold\Resources\pix2pix\samples\3.png"; //
            /// RELEASE
            string name = System.IO.Path.GetTempFileName().Replace(".tmp",".jpg");

            Bitmap newBmp = Webcam.GetCurrentImage();
            newBmp.Save(name);
            
            /// DEBUG PATHS
            //string[] ls = System.IO.Path.GetTempFileName().Split('\\');
            //string pix2pixFilename = @"C:\workspace\Harold\Harold\Resources\pix2pix\outputs\" + ls[ls.Length-1].Replace(".tmp",".png");
            //string textFileName = @"C:\workspace\Harold\Harold\Resources\pix2pix\outputs\" + ls[ls.Length - 1].Replace(".tmp", "_text.txt");

            /// RELEASE PATHS
            string tempName = System.IO.Path.GetTempFileName();
            string pix2pixFilename = tempName.Replace(".tmp", ".png");
            string textFileName = tempName.Replace(".tmp", "_text.txt");

            Console.WriteLine(pix2pixFilename);
            Console.WriteLine("coucou");
            myVM.p2pConverter.ConvertImage(name, pix2pixFilename, textFileName, myVM.PythonPathOrigin);
            myVM.GetStructureFromP2P();

            // Extract elements
            myVM.Pix2pixPath = pix2pixFilename;
            myVM.SlabPath = pix2pixFilename.Replace(".png",string.Empty) + "_slabs.png";
            myVM.WallsPath = pix2pixFilename.Replace(".png", string.Empty) + "_walls.png";
            myVM.ColumnsPath = pix2pixFilename.Replace(".png", string.Empty) + "_columns.png";
            myVM.OpeningsPath = pix2pixFilename.Replace(".png", string.Empty) + "_openings.png";
            
            //string json = File.ReadAllText(pix2pixFilename.Replace(".png","_contours.json"));
            //string old = "[\"";
            //json = json.Replace(old, "[");
            //old = "\"]";
            //json = json.Replace(old, "]");
            //old = "]\"";
            //json = json.Replace(old, "]");
            //old = "\"[";
            //json = json.Replace(old, "[");
            //Console.WriteLine(json);

            //try
            //{
            //    myVM.MyStructure = JsonConvert.DeserializeObject<Structure>(json);
            //    myVM.StructurePreprocess();
            //}
            //catch(Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
            //// Extract text
            ////string text = System.IO.File.ReadAllText(textFileName);
            ////Dictionary<string, object[]> texts = JsonConvert.DeserializeObject<Dictionary<string, object[]>>(text);
            ////Console.WriteLine("text found in the image: {0}", text);
            ////myVM.ProcessText(text);

            myVM.BuildView();
        }

        private void sliderChanged(object sender, RoutedEventArgs e)
        {
            ViewModel vm = (sender as Slider).DataContext as ViewModel;
            if (vm != null) vm.UpdateStructure();
        }
        private void RunFEA_Click(object sender, RoutedEventArgs e)
        {
            myVM.RunFEA();
        }
        
        private void CreateDXF(object sender, RoutedEventArgs e)
        {
            myVM.CreateDXF();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            UCSettings settings = new UCSettings();
            Window w = new Window()
            {
                DataContext = myVM,
                Content = settings,
                SizeToContent = SizeToContent.Height,
                Width=300,
                Title="Settings"
            };
            w.ShowDialog();
        }

    }
}
