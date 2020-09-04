using MathNet.Numerics.LinearAlgebra;
using MWGeometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEModel
{
    public class Geometry
    {
        public List<Shell> Shells { get; set; }
        public List<Beam> Beams { get; set; }

        public Geometry()
        {
            Shells = new List<Shell>();
            Beams = new List<Beam>();
        }
    }

    public enum ShellType { Wall, Slab }
    public class Shell
    {
        public List<MWPoint3D> Points { get; set; } 
        public double Thickness { get; set; } // in mm
        public List<List<MWPoint3D>> Holes { get; set; }
        public List<List<MWPoint3D>> IncludedSegments { get; set; } = new List<List<MWPoint3D>>();
        public List<MWPoint3D> IncludedVertices { get; set; } = new List<MWPoint3D>();
        public ShellType type { get; set; }

        public Shell(List<MWPoint3D> pts, double thickness, ShellType st, List<List<MWPoint3D>> holes = null)
        {
            Points = pts;
            Thickness = thickness;
            Holes = holes ?? new List<List<MWPoint3D>>();
            type = st;
        }

        public Shell Clone()
        {
            return new Shell(this.Points, this.Thickness, this.type, this.Holes)
            {
                IncludedSegments = this.IncludedSegments
            };
        }
    }

    public class Beam
    {
        public MWPoint3D Start { get; set; }
        public MWPoint3D End { get; set; }
        public List<MWPoint2D> Section { get; set; } // we assume the section coordinates are centered on the beam neutral fiber and already oriented

        public Beam(MWPoint3D start, MWPoint3D end, List<MWPoint2D> secPts)
        {
            Start = start;
            End = end;
            Section = secPts;
        }

        public double GetArea()
        {
            return Polygons.GetPolygonArea(Section);
        }

        //public doule Get
    }

    public class Loads
    {
        public double LL { get; set; } = 2.5;
        public double DL { get; }
        public double SDL { get; set; } = 1;
        public double CLAD { get; set; } = 0;
        public double SLS { get => LL + DL + SDL + CLAD; }
        public double ULS { get => 1.35 * (DL + SDL + CLAD) + 1.5 * LL; }
    }


    public class GeoTransform
    {
        public MWPoint3D X0;
        public Matrix<double> P;
        public MWPoint2D Xmin;
        public MWPoint2D Xmax;
    }
}
