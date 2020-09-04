using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MWGeometry;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using Structure;

namespace Harold
{
    public class ExportDXF
    {
        public static void CreateDXF(ref StructuralModel structure)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = @"DXF files |*.DXF";
            saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            string filePathDxf = saveDialog.FileName;
            var dxfDrawing = generateDXF(structure);
            dxfDrawing.Save(filePathDxf);
        }

        public static DxfDocument generateDXF(StructuralModel structure)
        {
            var dxf = new netDxf.DxfDocument();

            var ColumnLayer = new netDxf.Tables.Layer("Column")
            {
                Color = netDxf.AciColor.Red,
                Lineweight = netDxf.Lineweight.W50
            };
            var SlabLayer = new netDxf.Tables.Layer("Slab")
            {
                Color = netDxf.AciColor.Yellow,
                Lineweight = netDxf.Lineweight.W50
            };
            var OpeningLayer = new netDxf.Tables.Layer("Opening")
            {
                Color = netDxf.AciColor.Blue,
                Lineweight = netDxf.Lineweight.W50
            };
            var WallLayer = new netDxf.Tables.Layer("Wall")
            {
                Color = netDxf.AciColor.Green,
                Lineweight = netDxf.Lineweight.W50
            };

            dxf.Layers.Add(ColumnLayer);
            dxf.Layers.Add(SlabLayer);
            dxf.Layers.Add(OpeningLayer);
            dxf.Layers.Add(WallLayer);

            foreach (var item in structure.Columns)
            {
                dxf.AddEntity(getPolylineFromPoints(item.Points, ColumnLayer));
            }
            foreach (var item in structure.Slabs)
            {
                dxf.AddEntity(getPolylineFromPoints(item.Points, SlabLayer));
            }
            foreach (var item in structure.Walls)
            {
                dxf.AddEntity(getPolylineFromPoints(item.Points, WallLayer, false));
            }
            foreach (var item in structure.Openings)
            {
                dxf.AddEntity(getPolylineFromPoints(item.Points, OpeningLayer));
            }

            return dxf;
        }

        // convert list of points to a polyline
        private static IEnumerable<EntityObject> getPolylineFromPoints(List<MWPoint2D> points, Layer layer, bool close = true)
        {
            int n = close ? points.Count : points.Count - 1;
            for(int i = 0; i < n; i++)
            {
                int k = i == points.Count - 1 ? 0 : i + 1;
                var dxfLine = new netDxf.Entities.Line(
                    new netDxf.Vector2(points[i].X * 1e3, points[i].Y * 1e3),
                    new netDxf.Vector2(points[k].X * 1e3, points[k].Y * 1e3))
                { Layer = layer };
                yield return dxfLine;
            }
        }
    }
}
