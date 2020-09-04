using Emgu.CV;
using Emgu.CV.Structure;
using MWGeometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Structure
{
    public class StructuralModel
    {
        // Elements directly extracted from picture
        public List<List<MWPoint2D>> PicSlabs { get; set; } = new List<List<MWPoint2D>>();
        public List<List<MWPoint2D>> PicColumns { get; set; } = new List<List<MWPoint2D>>();
        public List<List<MWPoint2D>> PicWalls { get; set; } = new List<List<MWPoint2D>>();
        public List<List<MWPoint2D>> PicOpenings { get; set; } = new List<List<MWPoint2D>>();

        // Elements post-processed
        public List<Slab> Slabs { get; set; } = new List<Slab>();
        public List<Wall> Walls { get; set; } = new List<Wall>();
        public List<Column> Columns { get; set; } = new List<Column>();
        public List<Opening> Openings { get; set; } = new List<Opening>();

        public double StoryHeight { get; set; } = 3.15; // story height in m
        public double SlabThickness { get; set; } = 0.3; // slab thickness in m
        public int NumStories { get; set; } = 1;
        public double ScalingLength { get; set; } = 1;
        public double LongestEdge { get; set; } = 20;
        public double PlaneScaling { get; set; }
        public double WallThickness { get; set; } = 300; // thickness in mm
        public List<ColumnDims> ColDims { get; set; }
        public Load sLoad { get; set; }

        public StructuralModel()
        {
            ColDims = new List<ColumnDims>()
            {
                new ColumnDims(300, 300),
                new ColumnDims(1200, 250),
                new ColumnDims(1000, 250),
                new ColumnDims(900, 250),
                new ColumnDims(800, 250),
                new ColumnDims(600, 300)
            };
            sLoad = new Load();
        }

        public void SetScaling()
        {
            PlaneScaling = LongestEdge / GetPicLongestEdge();
        }

        public double GetPicLongestEdge()
        {
            List<List<MWPoint2D>> slabsClosed = PicSlabs.Select(s => s.Append(s[0]).ToList()).ToList();
            double max = 0;
            for (int i = 0; i < slabsClosed.Count; i++)
            {
                for (int j = 0; j < slabsClosed[i].Count - 1; j++)
                {
                    double dist = Points.Distance(slabsClosed[i][j], slabsClosed[i][j + 1]);
                    max = dist > max ? dist : max;
                }
            }
            return max;
        }
        public void ReInitialize()
        {
            if (PicWalls == null)
            {
                PicWalls = new List<List<MWPoint2D>>();
                Walls = new List<Wall>();
            }
            if (PicColumns == null)
            {
                PicColumns = new List<List<MWPoint2D>>();
                Columns = new List<Column>();
            }
            if (PicSlabs == null)
            {
                PicSlabs = new List<List<MWPoint2D>>();
                Slabs = new List<Slab>();
            }
            if (PicOpenings == null)
            {
                PicOpenings = new List<List<MWPoint2D>>();
                Openings = new List<Opening>();
            }
                
        }

        public void ProcessSlabs()
        {
            //for (int i = 0; i < Slabs.Count; i++)
            //    Slabs[i] = Slabs[i].Select(p => new MWPoint2D(p.X * pictureScale, p.Y * pictureScale)).ToList();
            Slabs = PicSlabs.Select(s => new Slab() { RawPoints = s.Select(p => new MWPoint2D(p.X, p.Y)).ToList() }).ToList();
        }

        public void ProcessOpenings()
        {
            for (int i = 0; i < Openings.Count; i++)
            {
                //InputArray pts = InputArray.Create(Openings[i].Select(p => new Point(p.X, p.Y)));
                //RotatedRect rect = Cv2.MinAreaRect(pts);

                RotatedRect rect = CvInvoke.MinAreaRect(PicOpenings[i].Select(p => new PointF((float)p.X, (float)p.Y)).ToArray());
                var corners = rect.GetVertices().Select(p => new MWPoint2D(p.X, p.Y)).ToList();
                PicOpenings[i] = corners;
            }
            Openings = PicOpenings.Select(op => new Opening() { RawPoints = op.Select(p => new MWPoint2D(p.X, p.Y)).ToList() }).ToList();
            RescaleOpenings();
        }

        public void ProcessColumns()
        {
            for (int i = 0; i < PicColumns?.Count; i++)
            {
                RotatedRect rect = CvInvoke.MinAreaRect(PicColumns[i].Select(p => new PointF((float)p.X, (float)p.Y)).ToArray());
                var corners = rect.GetVertices().Select(p => new MWPoint2D(p.X, p.Y)).ToList();
                double l1 = Points.Distance(corners[0], corners[1]);
                double l2 = Points.Distance(corners[1], corners[2]);
                List<MWPoint2D> newPoints = new List<MWPoint2D>();
                MWPoint2D center = new MWPoint2D(0.5 * (corners[0].X + corners[2].X) , 0.5 * (corners[0].Y + corners[2].Y));
                ColumnDims cd;
                MWVector2D v;
                if (l1 > l2)
                {
                    double ratio = l1 / l2;
                    cd = ColDims.Aggregate(ColDims[0], (closest, next) =>
                                    Math.Abs(ratio - next.Ratio) < Math.Abs(ratio - closest.Ratio) ? next : closest);
                    v = new MWVector2D(corners[1].X - corners[0].X, corners[1].Y - corners[0].Y);
                }
                else
                {
                    double ratio = l2 / l1;
                    cd = ColDims.Aggregate(ColDims[0], (closest, next) =>
                                    Math.Abs(ratio - next.Ratio) < Math.Abs(ratio - closest.Ratio) ? next : closest);
                    v = new MWVector2D(corners[2].X - corners[1].X, corners[2].Y - corners[1].Y);
                }
                double L = Math.Max(cd.D1, cd.D2) / 1e3;
                double W = Math.Min(cd.D1, cd.D2) / 1e3;

                v = v.Normalize();
                MWVector2D n = new MWVector2D(-v.Y, v.X);

                newPoints.Add(new MWPoint2D(+0.5 * L * v.X + 0.5 * W * n.X, +0.5 * L * v.Y + 0.5 * W * n.Y));
                newPoints.Add(new MWPoint2D(+0.5 * L * v.X - 0.5 * W * n.X, +0.5 * L * v.Y - 0.5 * W * n.Y));
                newPoints.Add(new MWPoint2D(-0.5 * L * v.X - 0.5 * W * n.X, -0.5 * L * v.Y - 0.5 * W * n.Y));
                newPoints.Add(new MWPoint2D(-0.5 * L * v.X + 0.5 * W * n.X, -0.5 * L * v.Y + 0.5 * W * n.Y));

                Columns.Add(new Column()
                {
                    RawCenter = center,
                    SectionPoints = newPoints
                });

            }
            RescaleColumns();
        }
        public void ProcessWalls()
        {
            for (int i = 0; i < PicWalls.Count; i++)
            {
                if (Points.Distance(PicWalls[i][0], PicWalls[i][1]) < 15)
                {
                    PicWalls.RemoveAt(i);
                    i--;
                }

            }
            for (int i = 0; i < PicWalls.Count; i++)
            {
                MWPoint2D pi1 = new MWPoint2D(PicWalls[i][0].X, PicWalls[i][0].Y);
                MWPoint2D pi2 = new MWPoint2D(PicWalls[i][1].X, PicWalls[i][1].Y);
                for (int j = 0; j < PicWalls.Count; j++)
                {
                    if (j != i)
                    {
                        MWPoint2D pj1 = new MWPoint2D(PicWalls[j][0].X, PicWalls[j][0].Y);
                        MWPoint2D pj2 = new MWPoint2D(PicWalls[j][1].X, PicWalls[j][1].Y);
                        if (pj1.X == pj2.X && pj1.Y == pj2.Y) break;
                        PointToLine(ref pi1, ref pi2, ref pj1, ref pj2);
                        PointToPoint(ref pi1, ref pi2, ref pj1, ref pj2);
                        PointToLine(ref pi1, ref pi2, ref pj1, ref pj2);
                        PointToPoint(ref pi1, ref pi2, ref pj1, ref pj2);
                    }
                }
                PicWalls[i][0] = new MWPoint2D(pi1.X, pi1.Y);
                PicWalls[i][1] = new MWPoint2D(pi2.X, pi2.Y);
            }

            Walls = PicWalls.Select(w => new Wall() { RawPoints = w }).ToList();
            RescaleWalls();
        }

        public void RescaleSlabs()
        {
           Slabs.ForEach(s => s.Points = s.RawPoints.Select(p => new MWPoint2D(p.X * PlaneScaling, p.Y * PlaneScaling)).ToList());
        }
        public void RescaleOpenings()
        {
            Openings.ForEach(op => op.Points = op.RawPoints.Select(p => new MWPoint2D(p.X * PlaneScaling, p.Y * PlaneScaling)).ToList());
        }
        public void RescaleColumns()
        {
            Columns.ForEach(c => c.Center = new MWPoint2D(c.RawCenter.X * PlaneScaling, c.RawCenter.Y * PlaneScaling));
        }
        public void RescaleWalls()
        {
            Walls.ForEach(w => w.Points = w.RawPoints.Select(p => new MWPoint2D(p.X * PlaneScaling, p.Y * PlaneScaling)).ToList());
        }

        public void RescaleAll()
        {
            SetScaling();
            RescaleSlabs();
            RescaleOpenings();
            RescaleColumns();
            RescaleWalls();
        }

        private void PointToLine(ref MWPoint2D pi1, ref MWPoint2D pi2, ref MWPoint2D pj1, ref MWPoint2D pj2, int tol = 10)
        {
            double d1 = Points.DistancePointToLine(pj1, pj2, pi1, infinite: false);
            if (d1 < tol) pi1 = Points.PointProjOnLine(pj1, pj2, pi1);

            double d2 = Points.DistancePointToLine(pj1, pj2, pi2, infinite: false);
            if (d2 < tol) pi2 = Points.PointProjOnLine(pj1, pj2, pi1);
        }

        private void PointToPoint(ref MWPoint2D pi1, ref MWPoint2D pi2, ref MWPoint2D pj1, ref MWPoint2D pj2, int tol = 10)
        {
            double d11 = Points.Distance(pi1, pj1);
            if (d11 < tol) pi1 = pj1;
            double d12 = Points.Distance(pi1, pj2);
            if (d12 < tol) pi1 = pj2;
            double d21 = Points.Distance(pi2, pj1);
            if (d21 < tol) pi2 = pj1;
            double d22 = Points.Distance(pi2, pj2);
            if (d22 < tol) pi2 = pj2;
        }

        private MWPoint2D GetBarycenter()
        {
            return MWGeometry.Points.GetBarycenter(PicSlabs.SelectMany(s => s.Select(p => new MWPoint2D(p.X, p.Y)).ToList()).ToList());
        }

        public void TransformPoints()
        {
            MWPoint2D bary = this.GetBarycenter();

            for (int i = 0; i < (PicSlabs?.Count ?? 0); i++)
                PicSlabs[i] = PicSlabs[i].Select(p => new MWPoint2D(-(p.X - bary.X), (p.Y - bary.Y))).ToList();
            foreach (var s in PicSlabs)
                s.Reverse();

            for (int i = 0; i < (PicWalls?.Count ?? 0); i++)
                PicWalls[i] = PicWalls[i].Select(p => new MWPoint2D(-(p.X - bary.X), (p.Y - bary.Y))).ToList();

            for (int i = 0; i < (PicColumns?.Count ?? 0); i++)
                PicColumns[i] = PicColumns[i].Select(p => new MWPoint2D(-(p.X - bary.X), (p.Y - bary.Y))).ToList();

            for (int i = 0; i < (PicOpenings?.Count ?? 0); i++)
                PicOpenings[i] = PicOpenings[i].Select(p => new MWPoint2D(-(p.X - bary.X), (p.Y - bary.Y))).ToList();
        }

        public List<MWPoint2D> ExpandWall(MWPoint2D p1, MWPoint2D p2)
        {
            MWVector2D v = new MWVector2D(p2.X - p1.X, p2.Y - p1.Y);
            v = v.Normalize();
            MWVector2D n = new MWVector2D(-v.Y, v.X);

            return new List<MWPoint2D>()
            {
                new MWPoint2D(p1.X + this.WallThickness/1e3/2 * (+ n.X - v.X), p1.Y + this.WallThickness/1e3/2 * (+ n.Y - v.Y)),
                new MWPoint2D(p1.X + this.WallThickness/1e3/2 * (- n.X - v.X), p1.Y + this.WallThickness/1e3/2 * (- n.Y - v.Y)),
                new MWPoint2D(p2.X + this.WallThickness/1e3/2 * (- n.X + v.X), p2.Y + this.WallThickness/1e3/2 * (- n.Y + v.Y)),
                new MWPoint2D(p2.X + this.WallThickness/1e3/2 * (+ n.X + v.X), p2.Y + this.WallThickness/1e3/2 * (+ n.Y + v.Y)),
            };
        }
    }

    public class Load
    {
        public double LL { get; set; } = 2.5;
        public double DL { get; }
        public double SDL { get; set; } = 1;
        public double CLAD { get; set; } = 0;
        public double SLS { get => LL + DL + SDL + CLAD; }
        public double ULS { get => 1.35 * (DL + SDL + CLAD) + 1.5 * LL; }

    }

    
}
