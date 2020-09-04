using MWGeometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEModel
{
    public class FEResults
    {
        public List<MWPoint3D> PointsPos { get; set; }
        public List<MWVector3D> PointsDisps { get; set; }
        public double MaxDeflection { get; set; }
        public double MaxDrift { get; set; }
        public int NumberElems { get; set; }
        public List<int[]> T3 { get; set; }
        public List<int[]> L2 { get; set; }
    }
}
