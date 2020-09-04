using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Karamba.Geometry;
using KarambaCommon;
using Karamba.CrossSections;
using Karamba.Utilities;
using Karamba.Materials;
using Karamba.Supports;
using Karamba.Loads;
using Karamba.Elements;
using Karamba.Algorithms;
using Karamba.Models;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using MWGeometry;
using MathNet.Numerics.LinearAlgebra;
using System.Windows.Forms;

namespace FEModel
{
    public class FEModel
    {
        public Geometry modelGeo { get; set; }
        public Loads modelLoads { get; set; }

        public double beamDisc = 1;

        public FEModel(Geometry geom, Loads loads)
        {
            modelGeo = geom;
            modelLoads = loads;
        }


        public FEResults Analyze()
        {
            var license_path = Karamba.Licenses.License.licensePath();
            var license = Karamba.Licenses.License.getLicense();
            var has_expired = Karamba.Licenses.License.has_expired();
            var license_type = Karamba.Licenses.License.licenseType();

            var k3d = new Toolkit();
            var logger = new MessageLogger();

            // create beam elements
            var lines = new List<Line3>();
            var colSecs = new List<CroSec>();
            for (int i = 0; i < modelGeo.Beams.Count; i++)
            {
                Beam b = modelGeo.Beams[i];
                double l = Math.Max(Points.Distance(b.Section[0], b.Section[1]), Points.Distance(b.Section[1], b.Section[2]));
                double w = Math.Min(Points.Distance(b.Section[0], b.Section[1]), Points.Distance(b.Section[1], b.Section[2]));
                Vector3 v = new Vector3((b.End.X - b.Start.X) / beamDisc, (b.End.Y - b.Start.Y) / beamDisc, (b.End.Z - b.Start.Z) / beamDisc);
                var nodeI = new Point3(b.Start.X, b.Start.Y, b.Start.Z);
                for (int beamInd = 0; beamInd < beamDisc; ++beamInd)
                {
                    var nodeK = new Point3(nodeI.X + v.X, nodeI.Y + v.Y, nodeI.Z + v.Z);
                    lines.Add(new Line3(nodeI, nodeK));
                    colSecs.Add(k3d.CroSec.Box(height: l, uf_thick: w, lf_width: w));
                    nodeI = nodeK;
                }

            }

            List<ShellMesh> shellMeshes = CreateMesh();
            List<Mesh3> slabMeshes = shellMeshes.Where(sm => sm.Shell.type == ShellType.Slab).Select(s => s.Mesh).ToList();
            List<Mesh3> wallMeshes = shellMeshes.Where(sm => sm.Shell.type == ShellType.Wall).Select(s => s.Mesh).ToList();

            // materials
            FemMaterial C4050 = new FemMaterial_Isotrop("concrete", "C40/50", 35e6, 14.58e6, 14.58e6, 25, 40e3, 0, System.Drawing.Color.Blue);

            // cross-sections
            List<CroSec> slabSecs = slabMeshes.SelectMany(s => new List<CroSec> { k3d.CroSec.ShellConst(height: 0.250) }).ToList(); ; // be careful with units (most likely) in cm !!
            List<CroSec> wallSecs = wallMeshes.SelectMany(s => new List<CroSec> { k3d.CroSec.ShellConst(height: 0.300) }).ToList(); ; // be careful with units (most likely) in cm !!

            colSecs.ForEach(c => c.setMaterial(C4050));
            slabSecs.ForEach(c => c.setMaterial(C4050));
            wallSecs.ForEach(c => c.setMaterial(C4050));
            // FE elements
            var slabElements = k3d.Part.MeshToShell(slabMeshes, new List<string>(), slabSecs, logger, out List<Point3> outSlabPoints);
            var wallElements = k3d.Part.MeshToShell(wallMeshes, new List<string>(), wallSecs, logger, out List<Point3> outWallPoints);
            var beamElements = k3d.Part.LineToBeam(lines, new List<string>(), colSecs, logger, out List<Point3> outColPoints);

            List<BuilderElement> elements = new List<BuilderElement>(slabElements);
            elements.AddRange(wallElements);
            elements.AddRange(beamElements);

            // ------ LOADS ---------
            List<Load> loads = new List<Load>();
            // mesh loads on slabs
            List<Load> slabLoads = new List<Load>();
            for (int i = 0; i < slabMeshes.Count; i++)
                slabLoads.Add(k3d.Load.MeshLoad(new List<Vector3> { new Vector3(0, 0, -2.5) }, slabMeshes[i]));
            //loads.AddRange(slabLoads);
            // gravity
            GravityLoad gravity = new GravityLoad(new Vector3(0, 0, -1), 0);
            loads.Add(gravity);

            // supports
            var colSupports = modelGeo.Beams.Where(b => b.Start.Z == 0)
                                .Select(b => k3d.Support.Support(new Point3(b.Start.X, b.Start.Y, 0), k3d.Support.SupportFixedConditions)).ToList();
            var wallSupports = wallMeshes.SelectMany(m => m.Vertices.Where(v => v.Z == 0)
                                         .Select(v => k3d.Support.Support(new Point3(v.X, v.Y, v.Z), k3d.Support.SupportFixedConditions))).ToList();
            var supports = colSupports;
            supports.AddRange(wallSupports);

            // assemble the model
            var model = k3d.Model.AssembleModel(elements, supports, loads, out string info, out double mass, out Point3 cog, out string msg, out bool runtimeWarning);
            
            // calculate the model
            model = k3d.Algorithms.AnalyzeThI(model, out var maxDisp, out var outG, out var outComp, out var warning);

            Karamba.Results.NodalDisp.solve(model, 0, out var trans, out var rot);

            // OUTPUTS
            
            List<MWVector3D> pointDisps = new List<MWVector3D>();
            List<MWPoint3D> points = new List<MWPoint3D>();
            for (int i = 0; i < model.nodes.Count; i++)
            {
                var n0 = model.nodes[i].pos;
                var n1 = trans[i];
                points.Add(new MWPoint3D(n0.X, n0.Y, n0.Z));
                pointDisps.Add(new MWVector3D(n1.X / 10.0, n1.Y / 10.0, n1.Z / 10.0));
            }
            List<int[]> t3 = new List<int[]>();
            List<int[]> l2 = new List<int[]>();
            for (int i = 0; i < model.elems.Count; i++)
            {
                if (model.elems[i] is ModelShell e)
                {
                    var m = e.mesh;
                    for (int j = 0; j < m.Faces.Count; j++)
                    {
                        t3.Add(new int[] {e.fe_node_ind[m.Faces[j][0]],
                                               e.fe_node_ind[m.Faces[j][1]],
                                               e.fe_node_ind[m.Faces[j][2]]});
                    }

                }
                if(model.elems[i] is ModelBeam b)
                {
                    l2.Add(new int[] { b.fe_node_ind[0], b.fe_node_ind[1] });
                }
            }

            FEResults fer = new FEResults()
            {
                PointsPos = points.Select(p => new MWPoint3D(p.X, p.Y, p.Z)).ToList(),
                PointsDisps = pointDisps.Select(p => new MWVector3D(p.X, p.Y, p.Z)).ToList(),
                T3 = t3,
                L2 = l2,
                MaxDeflection = pointDisps.Max(p => Math.Abs(p.Z)) * 1e3,
                MaxDrift = pointDisps.Max(d => Math.Sqrt(d.X * d.X + d.Y * d.Y)) * 1e3,
                NumberElems = model.elems.Count
            };

            return fer;
            //return (points.Select(p => new MWPoint3D(p.X, p.Y, p.Z)).ToList(), t3);
        }

        private void Assemble(List<ShellMesh> shellMeshes)
        {
            
        }

        private List<ShellMesh> CreateMesh()
        {
            // list of meshes
            //List<Mesh3> meshes = new List<Mesh3>();
            List<ShellMesh> shellMeshes = new List<ShellMesh>();

            // create shell elements
            var slabs = modelGeo.Shells.Where(s => s.type == ShellType.Slab).ToList();
            var walls = modelGeo.Shells.Where(s => s.type == ShellType.Wall).ToList();

            // we mesh the walls
            List<List<MWPoint3D>> wallContourEdges = new List<List<MWPoint3D>>();
            for (int i = 0; i < walls.Count; i++)
            {
                var res = meshWall(walls[i], wallContourEdges);
                //meshes.Add(res.Item1);
                wallContourEdges.AddRange(res.Item2);
                shellMeshes.Add(new ShellMesh(walls[i], res.Item1));
            }


            //we mesh the slabs
            for (int i = 0; i < slabs.Count; i++)
            {
                var m = meshSlab(slabs[i], wallContourEdges);
                //meshes.Add(m);
                shellMeshes.Add(new ShellMesh(slabs[i], m));
            }

            return shellMeshes;
        }

        private (Mesh3,List<List<MWPoint3D>>) meshWall(Shell wall, List<List<MWPoint3D>> existingPts = null)
        {
            // create mesh using Triangle.NET library
            Shell w = wall.Clone();

            // pre addition of points on the contours
            List<MWPoint3D> refinedPts = new List<MWPoint3D>();
            for (int i = 0; i < w.Points.Count; i++)
            {
                int k = i == w.Points.Count - 1 ? 0 : i + 1;
                MWPoint3D p0 = w.Points[i];
                MWPoint3D p1 = w.Points[k];
                MWVector3D vec = new MWVector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
                vec = vec.Normalised();
                double dist = Points.Distance3D(p0, p1);
                int n = Convert.ToInt32(dist / 0.75) + 1;
                double inc = dist / n;
                refinedPts.Add(p0);
                for (int j = 1; j < n; j++)
                {
                    MWPoint3D ptAdded = new MWPoint3D(p0.X + j * vec.X * inc, p0.Y + j * vec.Y * inc, p0.Z + j * vec.Z * inc);
                    refinedPts.Add(ptAdded);
                }
            }
            w.Points = refinedPts;

            if (existingPts != null)
            {
                List<MWPoint3D> exPts = existingPts.SelectMany(l => l).ToList();
                for (int i=0; i < exPts.Count; i++)
                {
                    for(int j = 0; j < w.Points.Count; j++)
                    {
                        int k = j == w.Points.Count - 1 ? 0 : j + 1;
                        if (PointIsOnLine(w.Points[j], w.Points[k], exPts[i]))
                        {
                            w.Points.Insert(k, exPts[i]);
                            j = 0;
                        }
                    }
                }
            }
            Polygon pol = new Polygon();
            var res = NormalizeShell(w);
            var vertices = res.Item1;
            pol.Add(new Contour(res.Item1));
            for(int i = 0; i < vertices.Count; i++)
            {
                int k = i == res.Item1.Count - 1 ? 0 : i + 1;
                if (Math.Sqrt(Math.Pow(vertices[i].X - vertices[k].X, 2) + Math.Pow(vertices[i].Y - vertices[k].Y, 2)) < 0.15)
                    pol.Add(new Segment(vertices[i], vertices[k]), false);
            }
            GeoTransform transform = res.Item2;

            double maxL = getMaxDim(w);
            double maxarea = 0.1 / maxL;

            Configuration config = new Configuration();
            QualityOptions qo = new QualityOptions()
            {
                MinimumAngle = 20,
                MaximumArea = maxarea,
            };

            Mesh MyMesh = (new GenericMesher()).Triangulate(pol, qo) as Mesh;
            
            List<Point3> meshPts = deNormalizeMesh(MyMesh.Vertices, wall, transform);

            // extracting the contour edges for future wall and slab meshing
            double xmax = MyMesh.Vertices.Max(p => p.X);
            double xmin = MyMesh.Vertices.Min(p => p.X);
            double ymax = MyMesh.Vertices.Max(p => p.Y);
            double ymin = MyMesh.Vertices.Min(p => p.Y);

            List<List<int>> edgesPtsIdx = MyMesh.Edges.Select(e =>
                new List<int>()
                {
                    MyMesh.Vertices.ToList().IndexOf(MyMesh.Vertices.Single(p => p.ID == e.P0)),
                    MyMesh.Vertices.ToList().IndexOf(MyMesh.Vertices.Single(p => p.ID == e.P1))
                }).ToList();


            List<Edge> edges = MyMesh.Edges.ToList();
            List<List<MWPoint3D>> TBEdges = new List<List<MWPoint3D>>();
            List<Vertex> lv = MyMesh.Vertices.ToList();
            for (int i = 0; i < edges.Count; i++)
            {
                bool b1 = lv[edgesPtsIdx[i][0]].X == xmin && lv[edgesPtsIdx[i][1]].X == xmin;
                bool b2 = lv[edgesPtsIdx[i][0]].X == xmax && lv[edgesPtsIdx[i][1]].X == xmax;
                bool b3 = lv[edgesPtsIdx[i][0]].Y == ymin && lv[edgesPtsIdx[i][1]].Y == ymin;
                bool b4 = lv[edgesPtsIdx[i][0]].Y == ymax && lv[edgesPtsIdx[i][1]].Y == ymax;
                if (b1 || b2 || b3 || b4)
                {
                    TBEdges.Add(new List<MWPoint3D>() { new MWPoint3D(meshPts[edgesPtsIdx[i][0]].X, meshPts[edgesPtsIdx[i][0]].Y, meshPts[edgesPtsIdx[i][0]].Z),
                                                        new MWPoint3D(meshPts[edgesPtsIdx[i][1]].X, meshPts[edgesPtsIdx[i][1]].Y, meshPts[edgesPtsIdx[i][1]].Z)});
                }
            }

            return (new Mesh3(meshPts, MyMesh.Triangles.Select(t => 
                                new Face3(MyMesh.Vertices.ToList().IndexOf(t.GetVertex(0)),
                                          MyMesh.Vertices.ToList().IndexOf(t.GetVertex(1)),
                                          MyMesh.Vertices.ToList().IndexOf(t.GetVertex(2)))).ToList()),
                    TBEdges);
        }

        // Returns true of the points p0 is on the closed line formed by p1 and p2
        // returns false if p0 equals p1 or p2
        private bool PointIsOnLine(MWPoint3D p1, MWPoint3D p2, MWPoint3D p0)
        {
            double tol = 1e-5;
            if (Math.Abs(p0.X - p1.X) < tol && Math.Abs(p0.Y - p1.Y) < tol && Math.Abs(p0.Z - p1.Z) < tol)
                return false;
            if (Math.Abs(p0.X - p2.X) < tol && Math.Abs(p0.Y - p2.Y) < tol && Math.Abs(p0.Z - p2.Z) < tol)
                return false;
            MWVector3D v1 = new MWVector3D(p0.X - p1.X, p0.Y - p1.Y, p0.Z - p1.Z);
            MWVector3D v2 = new MWVector3D(p0.X - p2.X, p0.Y - p2.Y, p0.Z - p2.Z);
            MWVector3D vp = Vectors3D.VectorialProduct(v1, v2);
            bool aligned = Math.Sqrt(vp.X * vp.X + vp.Y * vp.Y + vp.Z * vp.Z) < 1e-4;
            if (aligned)
                return Points.Distance3D(p0, p1) + Points.Distance3D(p0, p2) - Points.Distance3D(p1, p2) < 1e-3;
            else
                return false;
            
        }

        private double getMaxDim(Shell w)
        {
            double xmin = w.Points.Min(p => p.X);
            double xmax = w.Points.Max(p => p.X);
            double ymin = w.Points.Min(p => p.Y);
            double ymax = w.Points.Max(p => p.Y);
            double zmin = w.Points.Min(p => p.Z);
            double zmax = w.Points.Max(p => p.Z);

            return Math.Max(Math.Sqrt(Math.Pow(xmax - xmin, 2) + Math.Pow(ymax - ymin, 2)), zmax - zmin);
        }
        
        private Mesh3 meshSlab(Shell slab, List<List<MWPoint3D>> walls)
        {
            Shell s = slab;
            double z = s.Points.Min(p => p.Z);
            List<List<MWPoint3D>> wallSegments = new List<List<MWPoint3D>>();
            foreach (var w in walls)
            {
                if (Math.Abs(w[0].Z - z) < 1e-5 && Math.Abs(w[1].Z - z) < 1e-5)
                    wallSegments.Add(w);
            }

            // we reorder the walls according to their center X coordinate then center Y coordinate
            wallSegments = wallSegments.OrderBy(seg => 0.5 * (seg[0].X + seg[1].X)).ToList();
            //wallSegments = wallSegments.OrderBy(seg => 0.5 * (seg[0].Y + seg[1].Y)).ToList();
            foreach (var w in wallSegments)
                s.IncludedSegments.Add(new List<MWPoint3D>() { new MWPoint3D(w[0].X, w[0].Y, w[0].Z), new MWPoint3D(w[1].X, w[1].Y, w[1].Z) });

            List<MWPoint2D> s2D = s.Points.Select(p => new MWPoint2D(p.X, p.Y)).ToList();
            foreach(var b in modelGeo.Beams)
            {
                if(Polygons.isInside(s2D, new MWPoint2D(b.Start.X, b.Start.Y)))
                {
                    if (b.Start.Z == z)
                    {
                        if (!s.IncludedVertices.Contains(b.Start)) s.IncludedVertices.Add(b.Start);
                    }
                    else if(b.End.Z == z)
                    {
                        if (!s.IncludedVertices.Contains(b.End)) s.IncludedVertices.Add(b.End);
                    }
                }
            }

            // create mesh using Triangle.NET library
            Polygon pol = new Polygon();
            var res = NormalizeShell(s);
            GeoTransform transform = res.Item2;
            var holesN = s.Holes.Select(h => ApplyGeoTransform(h, transform)).ToList();
            var segN = s.IncludedSegments.Select(seg => ApplyGeoTransform(seg, transform)).ToList();
            var cols = ApplyGeoTransform(s.IncludedVertices, transform);
            pol.Add(new Contour(res.Item1));

            // holes are added
            for (int j = 0; j < s.Holes.Count; j++) 
                pol.Add(new Contour(holesN[j]), true);

            // segments coming from wall meshes are added
            List<Vertex> addedVertices = new List<Vertex>();
            // WARNING : Here are just added the segments with both endpoints new. It might not work every time.
            // If a problem is encountered, it might be worth adding the points as vertices and not as segments.
            for (int j = 0; j < segN.Count; j++)
            {
                //if (!addedVertices.Contains(new Vertex(segN[j][0].X, segN[j][0].Y)) && !addedVertices.Contains(new Vertex(segN[j][1].X, segN[j][1].Y)))
                if (!ContainsVertex(addedVertices,segN[j][0]) && !ContainsVertex(addedVertices, segN[j][1]))
                {
                    pol.Add(new Segment(segN[j][0], segN[j][1]), true);
                    addedVertices.Add(segN[j][0]);
                    addedVertices.Add(segN[j][1]);
                }
                //else if (addedVertices.Contains(new Vertex(segN[j][0].X, segN[j][0].Y)) && addedVertices.Contains(new Vertex(segN[j][1].X, segN[j][1].Y)))
                else if (ContainsVertex(addedVertices, segN[j][0]) && ContainsVertex(addedVertices, segN[j][1]))
                {
                    //Console.WriteLine("Both endpoints already added...");
                }
                //else if (addedVertices.Contains(new Vertex(segN[j][0].X, segN[j][0].Y)))
                else if (ContainsVertex(addedVertices, segN[j][0]))
                {
                    //pol.Add(new Segment(segN[j][0], segN[j][1]), 1);
                    //addedVertices.Add(segN[j][1]);
                }
                //else if (addedVertices.Contains(new Vertex(segN[j][1].X, segN[j][1].Y)))
                else if (ContainsVertex(addedVertices, segN[j][1]))
                {
                    //pol.Add(new Segment(segN[j][0], segN[j][1]), 0);
                    //addedVertices.Add(segN[j][0]);
                }
            }

            // vertices coming from columns are added
            cols.ForEach(c => pol.Add(c));

            //pol.Add(new Segment(new Vertex(segN[0][0].X, segN[0][0].Y), new Vertex(segN[0][1].X, segN[0][1].Y)), true);
            //pol.Add(new Segment(new Vertex(segN[2][0].X, segN[2][0].Y), new Vertex(segN[2][1].X, segN[2][1].Y)), true);
            double maxL = getMaxDim(s);
            double maxarea = 0.1 / maxL;

            Configuration config = new Configuration();
            QualityOptions qo = new QualityOptions()
            {
                MinimumAngle = 15,
                MaximumArea = maxarea
            };
            //ConstraintOptions co = new ConstraintOptions()
            //{
            //    SegmentSplitting = 1
            //};

            Mesh MyMesh = (new GenericMesher()).Triangulate(pol, qo) as Mesh;
            List<Point3> meshPts = deNormalizeMesh(MyMesh.Vertices, s, transform);
            // 
            return new Mesh3(meshPts, MyMesh.Triangles.Select(t => 
                                new Face3(MyMesh.Vertices.ToList().IndexOf(t.GetVertex(0)),
                                          MyMesh.Vertices.ToList().IndexOf(t.GetVertex(1)),
                                          MyMesh.Vertices.ToList().IndexOf(t.GetVertex(2)))).ToList());
        }

        /// <summary>
        /// Normalizes the points coordinates in the plane defined by the shell, and returns the holes and wall segments coordinates 
        /// in this new reference system
        /// </summary>
        /// <param name="s"></param>
        /// <param name="segments"></param>
        /// <returns></returns>
        public static (List<Vertex>, GeoTransform) NormalizeShell(Shell s)
        {
            MWPoint3D X0 = s.Points[0];

            // we choose a point p0 and make sure the origin is part of the plane of the shell
            List<MWPoint3D> cPoints = s.Points.Select(p => new MWPoint3D(p.X - X0.X, p.Y - X0.Y, p.Z - X0.Z)).ToList();
            List<List<MWPoint3D>> cHolesPoints = s.Holes.Select(lp => lp.Select(p => new MWPoint3D(p.X - X0.X, p.Y - X0.Y, p.Z - X0.Z)).ToList()).ToList();
            List<List<MWPoint3D>> cSegPoints = s.IncludedSegments.Select(lp => lp.Select(p => new MWPoint3D(p.X - X0.X, p.Y - X0.Y, p.Z - X0.Z)).ToList()).ToList();
            MWPoint3D p0 = new MWPoint3D(0, 0, 0);

            // we look for 2 points such that p0, p1 and p2 are not aligned
            MWPoint3D p1 = cPoints[1];
            MWPoint3D p2 = cPoints[2];
            for(int i = 2; i < cPoints.Count; i++)
            {
                p2 = cPoints[i];
                double dp = Math.Abs((p2.X - p0.X) * (p1.X - p0.X) + (p2.Y - p0.Y) * (p1.Y - p0.Y) + (p2.Z - p0.Z) * (p1.Z - p0.Z));
                if (dp < Points.Distance3D(p0, p1) * Points.Distance3D(p0, p2) - 1e-4)
                    break;
            }

            // we extract two normal vectors defining the plan of the shell
            MWVector3D v = new MWVector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            v = v.Normalised();
            MWVector3D w0 = new MWVector3D(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);
            MWVector3D n = Vectors3D.VectorialProduct(v, w0);
            n = n.Normalised();
            MWVector3D w = Vectors3D.VectorialProduct(v, n);
            w = w.Normalised();

            Matrix<double> P = Matrix<double>.Build.DenseOfRowArrays(new[] { v.X, v.Y, v.Z }, 
                                                                     new[] { w.X, w.Y, w.Z }, 
                                                                     new[] { n.X, n.Y, n.Z });

            List<MWPoint2D> newPts = cPoints.Select(p =>
            {
                MWVector3D vp = new MWVector3D(p.X, p.Y, p.Z);
                double x = Vectors3D.ScalarProduct(vp, v);
                double y = Vectors3D.ScalarProduct(vp, w);
                double z = Vectors3D.ScalarProduct(vp, n);
                //Console.WriteLine("z = {0} (should be 0)", z);
                return new MWPoint2D(x, y);
            }).ToList();

            //List<List<MWPoint2D>> newHolesPts = cHolesPoints.Select(lp => lp.Select(p =>
            //{
            //    MWVector3D vp = new MWVector3D(p.X, p.Y, p.Z);
            //    double x = Vectors3D.ScalarProduct(vp, v);
            //    double y = Vectors3D.ScalarProduct(vp, w);
            //    double z = Vectors3D.ScalarProduct(vp, n);
            //    //Console.WriteLine("z = {0} (should be 0)", z);
            //    return new MWPoint2D(x, y);
            //}).ToList()).ToList();

            //List<List<MWPoint2D>> newSegPts = cSegPoints.Select(lp => lp.Select(p =>
            //{
            //    MWVector3D vp = new MWVector3D(p.X, p.Y, p.Z);
            //    double x = Vectors3D.ScalarProduct(vp, v);
            //    double y = Vectors3D.ScalarProduct(vp, w);
            //    double z = Vectors3D.ScalarProduct(vp, n);
            //    //Console.WriteLine("z = {0} (should be 0)", z);
            //    return new MWPoint2D(x, y);
            //}).ToList()).ToList();

            if (newPts.Any(p => double.IsNaN(p.X)))
                Console.WriteLine("NaN");

            double x0 = newPts.Min(p => p.X);
            double y0 = newPts.Min(p => p.Y);
            double x1 = newPts.Max(p => p.X);
            double y1 = newPts.Max(p => p.Y);

            double l = Math.Max(x1 - x0, y1 - y0);

            GeoTransform trans = new GeoTransform()
            {
                P = P,
                X0 = X0,
                Xmin = new MWPoint2D(x0, y0),
                Xmax = new MWPoint2D(x1, y1)
            };
            
            return (newPts.Select(p => new Vertex((p.X - x0) / l, (p.Y - y0) / l)).ToList(),
                trans
                );
        }

        public static List<Vertex> ApplyGeoTransform(List<MWPoint3D> points, GeoTransform t)
        {
            List<MWPoint3D> pts0 = points.Select(p => new MWPoint3D(p.X - t.X0.X, p.Y - t.X0.Y, p.Z - t.X0.Z)).ToList();
            List<MWPoint2D> pts1 = pts0.Select(p =>
            {
                Vector<double> v = t.P.Multiply(Vector<double>.Build.DenseOfArray(new[] { p.X, p.Y, p.Z }));
                return new MWPoint2D(v[0], v[1]);
            }).ToList();
            double l = Math.Max(t.Xmax.X - t.Xmin.X, t.Xmax.Y - t.Xmin.Y);
            double x0 = t.Xmin.X;
            double y0 = t.Xmin.Y;
            return pts1.Select(p => new Vertex((p.X - x0) / l, (p.Y - y0) / l)).ToList();
        }

        public static List<Point3> deNormalizeMesh(ICollection<Vertex> lv, Shell s, GeoTransform t)
        {
            MWPoint3D p0 = s.Points[0];

            // we choose a point p0 and make sure the origin is part of the plane of the shell
            //List<MWPoint3D> cPoints = s.Points.Select(p => new MWPoint3D(p.X - p0.X, p.Y - p0.Y, p.Z - p0.Z)).ToList();
            //p0 = new MWPoint3D(0, 0, 0);

            //// we look for 2 points such that p0, p1 and p2 are not aligned
            //MWPoint3D p1 = cPoints[1];
            //MWPoint3D p2 = cPoints[2];
            //for (int i = 2; i < cPoints.Count; i++)
            //{
            //    p2 = cPoints[i];
            //    double dp = Math.Abs((p2.X - p0.X) * (p1.X - p0.X) + (p2.Y - p0.Y) * (p1.Y - p0.Y) + (p2.Z - p0.Z) * (p1.Z - p0.Z));
            //    if (dp < Points.Distance3D(p0, p1) * Points.Distance3D(p0, p2) - 1e-4)
            //        break;
            //}

            //// we extract two normal vectors (v,w) defining the plan of the shell and a normal n
            //MWVector3D v = new MWVector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            //v = v.Normalised();
            //MWVector3D w0 = new MWVector3D(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);
            //MWVector3D n = Vectors3D.VectorialProduct(v, w0);
            //n = n.Normalised();
            //MWVector3D w = Vectors3D.VectorialProduct(v, n);
            //w = w.Normalised();

            //List<MWPoint2D> newPts = cPoints.Select(p =>
            //{
            //    MWVector3D vp = new MWVector3D(p.X, p.Y, p.Z);
            //    double x = Vectors3D.ScalarProduct(vp, v);
            //    double y = Vectors3D.ScalarProduct(vp, w);
            //    return new MWPoint2D(x, y);
            //}).ToList();

            //double x0 = newPts.Min(p => p.X);
            //double y0 = newPts.Min(p => p.Y);
            //double x1 = newPts.Max(p => p.X);
            //double y1 = newPts.Max(p => p.Y);

            double l = Math.Max(t.Xmax.X - t.Xmin.X, t.Xmax.Y - t.Xmin.Y); // Math.Max(x1 - x0, y1 - y0);

            // from normalized 2D to non-normalized 2D
            List<MWPoint2D> dPoints = lv.Select(p => new MWPoint2D(t.Xmin.X + l * p.X, t.Xmin.Y + l * p.Y)).ToList();

            if (dPoints.Any(p => double.IsNaN(p.X)))
                Console.WriteLine("NaN");

            //Matrix<double> A = Matrix<double>.Build.DenseOfRowArrays(new[] { v.X, v.Y, v.Z }, new[] { w.X, w.Y, w.Z }, new[] { n.X, n.Y, n.Z });
            Matrix<double> Ainv = t.P.Inverse();
            p0 = t.X0; // s.Points[0];
            List<Point3> points3d = dPoints.Select(p =>
            {
                Vector<double> v2D = Vector<double>.Build.DenseOfArray(new[] { p.X, p.Y, 0.0 });
                Vector<double> v3D = Ainv.Multiply(v2D);
                return new Point3(p0.X + v3D[0], p0.Y + v3D[1], p0.Z + v3D[2]);
            }).ToList();

            return points3d;
        }

        public static bool ContainsVertex(List<Vertex> lv, Vertex p)
        {
            double tol = 1e-4;
            return lv.Any(v => Math.Abs(v.X - p.X) < tol && Math.Abs(v.Y - p.Y) < tol);
        }
        
        public FEModel()
        {
            
        }

        public static bool CheckLicense()
        {
            var license_path = Karamba.Licenses.License.licensePath();
            var license = Karamba.Licenses.License.getLicense();
            var has_expired = Karamba.Licenses.License.has_expired();
            var license_type = Karamba.Licenses.License.licenseType();

            if (license_type != feb.License.LicenseType.lic_pro)
            {
                MessageBox.Show("License not found !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
            //try
            //{
            //    var license_path = Karamba.Licenses.License.licensePath();
            //    var license = Karamba.Licenses.License.getLicense();
            //    var has_expired = Karamba.Licenses.License.has_expired();
            //    var license_type = Karamba.Licenses.License.licenseType();

            //    var k3d = new Toolkit();
            //    var logger = new MessageLogger();

            //    int nBeams = 50;
            //    int nFaces = 1500;
            //    double lengthBeams = 10.0;
            //    double xIncBeam = lengthBeams / nBeams;
            //    double xIncMesh = lengthBeams / nFaces;
            //    double limit_dist = xIncBeam / 100.0;

            //    // create beams
            //    var lines = new List<Line3>();
            //    var nodeI = new Point3(0, 0, 0);
            //    for (int beamInd = 0; beamInd < nBeams; ++beamInd)
            //    {
            //        var nodeK = new Point3(nodeI.X + xIncBeam, 0, 0);
            //        lines.Add(new Line3(nodeI, nodeK));
            //        nodeI = nodeK;
            //    }

            //    var builderElements = k3d.Part.LineToBeam(lines, new List<string>(), new List<CroSec>(), logger, out List<Point3> outPoints);

            //    // create a MeshLoad
            //    var mesh = new Mesh3((nFaces + 1) * 2, nFaces);
            //    mesh.AddVertex(new Point3(0, -0.5, 0));
            //    mesh.AddVertex(new Point3(0, 0.5, 0));
            //    for (var faceInd = 0; faceInd < nFaces; ++faceInd)
            //    {
            //        mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, -0.5, 0));
            //        mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, 0.5, 0));
            //        var nV = mesh.Vertices.Count;
            //        mesh.AddFace(nV - 4, nV - 3, nV - 1, nV - 2);
            //    }
            //    UnitsConversionFactory ucf = UnitsConversionFactories.Conv();
            //    UnitConversion m = ucf.m();
            //    var baseMesh = m.toBaseMesh(mesh);

            //    // create a mesh load
            //    var load = k3d.Load.MeshLoad(new List<Vector3>() { new Vector3(0, 0, -1) }, baseMesh);

            //    // create a support
            //    var support = k3d.Support.Support(new Point3(0, 0, 0), k3d.Support.SupportFixedConditions);

            //    // assemble the model
            //    var model = k3d.Model.AssembleModel(builderElements, new List<Support>() { support }, new List<Load>() { load },
            //        out var info, out var mass, out var cog, out var message, out var runtimeWarning);

            //    // calculate the model
            //    var model2 = k3d.Algorithms.AnalyzeThI(model, out var outMaxDisp, out var outG, out var outComp, out var warning);
            //    //Model model2;
            //    //ThIAnalyze.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning, out model2);
            //    //Assert.AreEqual(outMaxDisp[0], 2.8232103119228276, 1E-5);
            //    Console.WriteLine("result = {0}", outMaxDisp[0]);
            //    Console.WriteLine("expected result = {0}", 2.8232103119228276);

            //    //Karamba.Results.BeamDisplacements.solve(model2, 0, );
            //    Karamba.Results.NodalDisp.solve(model2, 0, out var trans, out var rot);

            //    var nodes = model2.nodes;
            //    for (int i = 0; i < nodes.Count; i++)
            //        Console.WriteLine("displacement node {0}: {1}", i, nodes[i].disp);
            //    return true;
            //}
            //catch(Exception e)
            //{
            //    MessageBox.Show("License not found");
            //    return false;
            //}

        }

        public class ShellMesh
        {
            public Shell Shell { get; set; }
            public Mesh3 Mesh { get; set; }

            public ShellMesh(Shell s, Mesh3 m)
            {
                Shell = s;
                Mesh = m;
            }
        }
    }
}
