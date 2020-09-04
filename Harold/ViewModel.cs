//using Microsoft.Expression.Encoder.Devices;
using NumSharp;
//using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Tensorflow;
using Google.Protobuf;
//using static Tensorflow.Binding;
using ImageReader;
using WebEye.Controls.Wpf;
using GenericViewer;
using Rhino.Geometry;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using MWGeometry;
using System.Windows.Media.Media3D;
using FEModel;
using HelixToolkit.Wpf;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Structure;

namespace Harold
{
    public class ViewModel : ViewModelBase
    {

        //-------------------------------------------
        //               Fields and Properties 
        //--------------------------------------------
        //public Collection<EncoderDevice> VideoDevices { get; set; }
        //public Collection<EncoderDevice> AudioDevices { get; set; }

        public List<WebCameraId> cameras { get; set; } = new List<WebCameraId>();
        public WebCameraId SelectedCamera { get; set; }

        public Pix2Pix p2pConverter;
        
        string pix2pixPath = "";
        public string Pix2pixPath
        {
            get { return pix2pixPath; }
            set { pix2pixPath = value; RaisePropertyChanged(nameof(Pix2pixPath)); }
        }

        string slabPath = "";
        public string SlabPath
        {
            get { return slabPath; }
            set { slabPath = value; RaisePropertyChanged(nameof(SlabPath)); }
        }

        string wallsPath = "";
        public string WallsPath
        {
            get { return wallsPath; }
            set { wallsPath = value; RaisePropertyChanged(nameof(WallsPath)); }
        }

        string columnsPath = "";
        public string ColumnsPath
        {
            get { return columnsPath; }
            set { columnsPath = value; RaisePropertyChanged(nameof(ColumnsPath)); }
        }

        string openingsPath = "";
        public string OpeningsPath
        {
            get { return openingsPath; }
            set { openingsPath = value; RaisePropertyChanged(nameof(OpeningsPath)); }
        }

        StructuralModel myStructure;
        public StructuralModel MyStructure
        {
            get { return myStructure; }
            set
            {
                myStructure = value;
                RaisePropertyChanged(nameof(MyStructure));
            }
        }
        
        GeometricalViewModel my3DView;
        public GeometricalViewModel My3DView
        {
            get { return my3DView; }
            set { my3DView = value; RaisePropertyChanged(nameof(My3DView)); }
        }

        FEView myFEView;
        public FEView MyFEView
        {
            get { return myFEView; }
            set { myFEView = value; RaisePropertyChanged(nameof(MyFEView)); }   
        }

        public PythonOrigin PythonPathOrigin = PythonOrigin.server;

        //-------------------------------------------
        //               Constructor 
        //--------------------------------------------
        public ViewModel()
        {
            //var fm = new FEModel.FEModel();
            p2pConverter = new Pix2Pix();
            myFEView = new FEView();
            myStructure = new StructuralModel();
        }

        //-------------------------------------------
        //               Methods 
        //--------------------------------------------

        public void UpdateStructure()
        {
            BuildView();
            RaisePropertyChanged(nameof(MyStructure));
        }

        // Update the 3D view
        public void BuildView()
        {
            ObservableCollection<Object3D> myObjects = new ObservableCollection<Object3D>();

            GeometricalViewModel myView = new GeometricalViewModel();
            
            double H = myStructure.StoryHeight;
            double T = myStructure.SlabThickness;
            myStructure.RescaleAll();

            for (int i = 0; i < myStructure.NumStories; i++)
            {
                if(myStructure?.Slabs != null)
                {
                    foreach (var myslab in myStructure?.Slabs)
                    {
                        List<MWPoint2D> pts = myslab.Points.Select(s => s).ToList();
                        pts.Add(pts[0]);

                        Object3D slab = new Object3D
                        {
                            Curve = new PolylineCurve(pts.Select(p => new Rhino.Geometry.Point3d(p.X, p.Y, (i + 1) * H))),
                            Vector = new Vector3d(0, 0, 0.3),
                            Color = Color.FromArgb(255, 200, 200, 200),
                            Holes = myStructure.Openings.Select(op => op.Points.Select(p => new Point3D(p.X, p.Y, (i + 1) * H)).ToList()).ToList()
                        };

                        myObjects.Add(slab);
                    }
                }
                
                //columns
                if(myStructure?.Columns != null)
                {
                    foreach (var mycolumn in myStructure?.Columns)
                    {
                        List<MWPoint2D> pts = mycolumn.Points.Select(s => s).ToList();
                        pts.Add(pts[0]);

                        Object3D Acolumn = new Object3D
                        {
                            Curve = new PolylineCurve(pts.Select(p => new Rhino.Geometry.Point3d(p.X, p.Y, i * H + 0.5 * T))),
                            Vector = new Vector3d(0, 0, H),
                            Color = Color.FromArgb(255, 200, 50, 50)
                        };

                        myObjects.Add(Acolumn);
                    }
                }
                
                //walls
                if(myStructure?.Walls != null)
                {
                    foreach (var mywall in myStructure?.Walls)
                    {
                        var points = myStructure.ExpandWall(new MWPoint2D(mywall.Points[0].X, mywall.Points[0].Y), new MWPoint2D(mywall.Points[1].X, mywall.Points[1].Y));
                        //var points = mywall.Select(p => p).ToList();

                        points.Add(points[0]);
                        //Console.WriteLine("new wall");
                        //foreach (var p in points)
                        //    Console.WriteLine("X = {0}, Y = {1}", p.X, p.Y);
                        Object3D wall = new Object3D
                        {
                            Curve = new PolylineCurve(points.Select(p => new Rhino.Geometry.Point3d(p.X, p.Y, i * H + 0.5 * T))),
                            Vector = new Vector3d(0, 0, H),
                            Color = Color.FromArgb(255, 50, 50, 200)
                        };
                        myObjects.Add(wall);

                    }
                }
                
            }
            myView.Objects = myObjects;

            My3DView = myView;
        }

        public void GetStructureFromP2P()
        {
            myStructure = new StructuralModel();
            myStructure.PicColumns = p2pConverter.Columns;
            myStructure.PicWalls = p2pConverter.Walls;
            myStructure.PicOpenings = p2pConverter.Openings;
            myStructure.PicSlabs = p2pConverter.Slabs;
            //myStructure.pictureScale = p2pConverter.Scaling;
            StructurePreprocess();
        }
        public void RunFEA()
        {
            bool lic = FEModel.FEModel.CheckLicense();
            if(lic) myFEView.Run(ref myStructure);
        }

        public void CreateDXF()
        {
            ExportDXF.CreateDXF(ref myStructure);
        }

        public void StructurePreprocess()
        {
            myStructure.ReInitialize();
            myStructure.TransformPoints();
            if (myStructure.Slabs != null) myStructure.ProcessSlabs();
            if (myStructure.Columns != null) myStructure.ProcessColumns();
            if (myStructure.Walls != null) myStructure.ProcessWalls();
            if (myStructure.Openings != null) myStructure.ProcessOpenings();
        }

        public void ProcessText(string text)
        {
            text = text.ToLower();

            getNumStories(text);

        }

        public void getNumStories(string text)
        {
            List<string> words = new List<string>() { "levels", "levls", "lvels", "evels", "vels", "vls", "evel", "levers" };
            bool levelsMentioned = false;
            string word = "";
            foreach (var w in words)
            {
                if (text.Contains(w))
                {
                    levelsMentioned = true;
                    word = w;
                    break;
                }
            }

            if (levelsMentioned)
            {
                text = text.Replace('&', '6');
                int idx = text.IndexOf(word);
                string substring = text.Substring(Math.Max(idx - 5, 0), Math.Min(word.Length, 10));
                myStructure.NumStories = Convert.ToInt32(FindNumber(substring, 1));
            }
            else
                myStructure.NumStories = 1;
        }

        public void getLoads(string text)
        {
            if (text.Contains("ll"))
            {
                int idx = text.IndexOf("ll");
                string substring = text.Substring(idx, Math.Min(text.Length - idx, 8));
                myStructure.sLoad.LL = FindNumber(substring, myStructure.sLoad.LL);
            }
            if (text.Contains("sdl"))
            {
                int idx = text.IndexOf("sdl");
                string substring = text.Substring(idx, Math.Min(text.Length - idx, 8));
                myStructure.sLoad.SDL = FindNumber(substring, myStructure.sLoad.SDL);
            }
            if (text.Contains("clad"))
            {
                int idx = text.IndexOf("clad");
                string substring = text.Substring(idx, Math.Min(text.Length - idx, 8));
                myStructure.sLoad.CLAD = FindNumber(substring, myStructure.sLoad.CLAD);
            }
        }

        public void getSlabThickness(string text)
        {
            string s = "rc slab";
            if(text.Contains(s))
            {
                int idx = text.IndexOf(s);
                string substring = text.Substring(idx, Math.Min(text.Length - idx, 10));
                myStructure.SlabThickness = FindNumber(substring, myStructure.SlabThickness);
            }
        }

        public double FindNumber(string s, double def)
        {
            string b = String.Empty;
            for (int i = 0; i < s.Length; i++)
                if (Char.IsDigit(s[i]))
                    b += s[i];

            if (b.Length > 0)
                return int.Parse(b);
            return def;
        }

        

    }

    public class Structure
    {
        

    }

    
}
