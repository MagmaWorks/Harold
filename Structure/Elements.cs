using MWGeometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structure
{
    public class Column
    {
        public MWPoint2D RawCenter { get; set; }
        public MWPoint2D Center { get; set; }
        public List<MWPoint2D> Points { get => SectionPoints.Select(p => new MWPoint2D(Center.X + p.X, Center.Y + p.Y)).ToList(); }
        public List<MWPoint2D> SectionPoints { get; set; }
    }

    public class ColumnDims
    {
        public double D1 { get; set; }
        public double D2 { get; set; }
        public double Ratio { get => D1 > D2 ? D1 / D2 : D2 / D1; }

        public ColumnDims(double d1, double d2)
        {
            D1 = d1;
            D2 = d2;
        }
    }

    public class Wall
    {
        public List<MWPoint2D> RawPoints { get; set; }
        public List<MWPoint2D> Points { get; set; }
        public double Thickness { get; set; }
    }

    public class Slab
    {
        public List<MWPoint2D> RawPoints { get; set; }
        public List<MWPoint2D> Points { get; set; }
        public double Thickness { get; set; }
    }

    public class Opening
    {
        public List<MWPoint2D> RawPoints { get; set; }
        public List<MWPoint2D> Points { get; set; }
    }
}
