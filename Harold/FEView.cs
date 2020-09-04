using HelixToolkit.Wpf;
using MWGeometry;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using FEModel;
using Structure;

namespace Harold
{
    public class FEView : ViewModelBase
    {
        public FEModel.FEModel fem;
        FEResults results;
        public FEResults Results
        {
            get { return results; }
            set { results = value; RaisePropertyChanged(nameof(Results)); }
        }
        //public List<MWPoint3D> MeshPoints;
        //public List<MWVector3D> MeshDisps;
        //public List<int[]> MeshT3;
        //public List<int[]> MeshL2;
        public List<Point3D> DeformedMesh { get; set; }
        //double maxDeflection = 0;
        //public double MaxDeflection
        //{
        //    get { return maxDeflection; }
        //    set { maxDeflection = value; RaisePropertyChanged(nameof(MaxDeflection)); }
        //}
        //double maxDrift = 0;
        //public double MaxDrift
        //{
        //    get { return maxDrift; }
        //    set { maxDrift = value; RaisePropertyChanged(nameof(MaxDrift)); }
        //}

        Model3D mesh;
        public Model3D Mesh
        {
            get { return mesh; }
            set { mesh = value; RaisePropertyChanged(nameof(Mesh)); }
        }

        double amp = 0;
        public double Amp
        {
            get { return amp; }
            set
            {
                amp = value;
                AmpEff = Math.Pow(10, amp);
                BuildMesh();
                RaisePropertyChanged(nameof(Amp));
            }
        }

        double ampEff;
        public double AmpEff
        {
            get { return ampEff; }
            set { ampEff = value; RaisePropertyChanged(nameof(AmpEff)); }
        }

        private void GetDeformedMesh()
        {
            DeformedMesh = results.PointsPos.Zip(results.PointsDisps, (p, v) => new Point3D(p.X + AmpEff * v.X, p.Y + AmpEff * v.Y, p.Z + AmpEff * v.Z)).ToList();
        }

        public void BuildMesh()
        {
            GetDeformedMesh();
            var modelGroup = new Model3DGroup();
            MeshBuilder meshBuilder = new MeshBuilder(false, true);
            //List<Point3D> pointDisps = DeformedMesh.Select(p => new Point3D(p.X, p.Y, p.Z)).ToList();
            DeformedMesh.ForEach(p =>
            {
                meshBuilder.Positions.Add(p);
                meshBuilder.TextureCoordinates.Add(new System.Windows.Point());
            });
            results.T3.ForEach(t =>
            {
                meshBuilder.AddTriangle(new List<int> { t[0], t[1], t[2] });
                meshBuilder.AddCylinder(DeformedMesh[t[0]], DeformedMesh[t[1]], 0.05, 4);
                meshBuilder.AddCylinder(DeformedMesh[t[1]], DeformedMesh[t[2]], 0.05, 4);
                meshBuilder.AddCylinder(DeformedMesh[t[0]], DeformedMesh[t[2]], 0.05, 4);
            });
            //fem.modelGeo.Beams.ForEach(b =>
            //{
            //    meshBuilder.AddCylinder(new Point3D(b.Start.X, b.Start.Y, b.Start.Z), new Point3D(b.End.X, b.End.Y, b.End.Z), 0.1, 8);
            //});
            results.L2.ForEach(l =>
            {
                var p1 = DeformedMesh[l[0]];
                var p2 = DeformedMesh[l[1]];
                meshBuilder.AddCylinder(p1, p2, 0.1, 8);
            });

            var color = Color.FromArgb(150, 200, 0, 0);
            var mesh = new GeometryModel3D(meshBuilder.ToMesh(), MaterialHelper.CreateMaterial(color));
            mesh.BackMaterial = mesh.Material;
            modelGroup.Children.Add(mesh);
            Mesh = modelGroup;
        }
        
        public void Run(ref StructuralModel structure)
        {
            FEModel.Geometry geom = new FEModel.Geometry();
            double H = structure.StoryHeight;
            for (int i = 0; i < structure.NumStories; i++)
            //for(int i = 0; i < 2; i++)
            {
                for (int j = 0; j < structure.Slabs.Count; j++)
                {
                    List<MWPoint2D> pts = structure.Slabs[j].Points;
                    Shell s = new Shell(pts.Select(p => new MWPoint3D(p.X, p.Y, (i + 1) * H)).ToList(), structure.SlabThickness, ShellType.Slab);
                    s.Holes = structure.Openings.Select(o => o.Points.Select(p => new MWPoint3D(p.X, p.Y, (i + 1) * H)).ToList()).ToList();
                    geom.Shells.Add(s);
                }
                for (int j = 0; j < structure.Columns.Count; j++)
                {
                    var col = structure.Columns[j];
                    MWPoint2D center = new MWPoint2D(0.5 * (col.Points[0].X + col.Points[2].X), 0.5 * (col.Points[0].Y + col.Points[2].Y));
                    List<MWPoint2D> secPts = col.Points.Select(p => new MWPoint2D(p.X - center.X, p.Y - center.Y)).ToList();
                    geom.Beams.Add(new Beam(new MWPoint3D(center.X, center.Y, i * structure.StoryHeight),
                                            new MWPoint3D(center.X, center.Y, (i + 1) * structure.StoryHeight),
                                            secPts));
                }
                for (int j = 0; j < structure.Walls.Count; j++)
                {
                    List<MWPoint2D> wall = structure.Walls[j].Points;
                    List<MWPoint3D> wallPts = new List<MWPoint3D>()
                    {
                        new MWPoint3D(wall[0].X, wall[0].Y, i*structure.StoryHeight),
                        new MWPoint3D(wall[1].X, wall[1].Y, i*structure.StoryHeight),
                        new MWPoint3D(wall[1].X, wall[1].Y, (i+1)*structure.StoryHeight),
                        new MWPoint3D(wall[0].X, wall[0].Y, (i+1)*structure.StoryHeight),
                    };
                    geom.Shells.Add(new Shell(wallPts, structure.WallThickness, ShellType.Wall));
                }
            }

            // Loads
            FEModel.Loads loads = new Loads()
            {
                LL = structure.sLoad.LL,
                SDL = structure.sLoad.SDL,
                CLAD = structure.sLoad.CLAD
            };

            fem = new FEModel.FEModel(geom, loads);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Results = fem.Analyze();
            
            TimeSpan ts = sw.Elapsed;
            Console.WriteLine("meshing time = {0}s", ts.TotalSeconds);
            BuildMesh();
            ts = sw.Elapsed;
            Console.WriteLine("display time = {0}s", ts.TotalSeconds);
            sw.Stop();
        }
    }
}
