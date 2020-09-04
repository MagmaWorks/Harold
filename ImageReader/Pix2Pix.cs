using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Reflection;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Ocl;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MWGeometry;
using BorderType = Emgu.CV.CvEnum.BorderType;
using Tesseract = Emgu.CV.OCR.Tesseract;

namespace ImageReader
{
    public enum PythonOrigin { local, server }
    public enum ProcessAction { Unblur, FilterColumns, FilterWalls, FilterOpenings, FilterSlabs }
    
    
    public class Pix2Pix
    {
        public int PicSize = 256;
        //private const string client_id = "0704d7a2-438e-4a10-a2f0-623e4ce1d63b";
        //private const string client_key = "7C97-41txiodLT0Pk5v4_3L6s9BJ_0~~gP";
        //private const string tenant_id = "8891bfdf-6f43-4081-bc7e-eed3d4b40b89";
        //private const string subscription_id = "c73c780b-b748-4960-9f12-ce55e4876e9f";
        //DataFactoryManagementClient inner_client;
        //string resourceGroupName = "pythonTasks";
        //string dataFactoryName = "GregoireCorreADFTutorialDataFactory";
        //string pipelineName = "testPipeline";
        public List<List<MWPoint2D>> Columns = new List<List<MWPoint2D>>();
        public List<List<MWPoint2D>> Walls = new List<List<MWPoint2D>>();
        public List<List<MWPoint2D>> Slabs = new List<List<MWPoint2D>>();
        public List<List<MWPoint2D>> Openings = new List<List<MWPoint2D>>();
        public double Scaling;
        string pythonPath { get => GetPythonPath(); }
        PythonOrigin pyOrigin = PythonOrigin.server;

        public Pix2Pix()
        {
            //create_module_graph();
        }

        public void ConvertImage(string filename, string outputPath, string textOutputPath, PythonOrigin pyOrig)
        {
            Console.WriteLine("Entering ConvertImage");
            string inputPath = filename; //@"C:\workspace\Harold\Harold\Resources\pix2pix\samples\1.png";
            pyOrigin = pyOrig;

            // image processing
            Console.WriteLine("before cv2 ImDecode");
            Console.WriteLine("input file : {0}", filename);
            Image im = Image.FromFile(filename);
            Console.WriteLine("im size : {0}",im.Size);
            byte[] arr;
            using (MemoryStream ms = new MemoryStream())
            {
                im.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                arr = ms.ToArray();
            }
            Image<Bgr, byte>  img = new Image<Bgr, byte>(filename);

            // Text extraction
            string text;
            string boxedText;
            //(text, boxedText) = ExtractText(img);
            //Console.WriteLine(text);

            // Element extraction
            double height = img.Height;
            double width = img.Width;
            //int minDim = (int)Math.Min(width, height);
            //int colStart = Math.Max((int)(width / 2 - height / 2), 0);
            //int colEnd = Math.Min((int)(width / 2 + height / 2), (int)width);

            //int rowStart = Math.Max((int)(height / 2 - width / 2), 0);
            //int rowEnd = Math.Min((int)(height / 2 + width / 2), (int)height);

            ////img = img.ColRange(colStart, colEnd);
            //img.ROI = new Rectangle(colStart, rowStart, colEnd - colStart, rowEnd - rowStart);
            //img = img.Copy();

            img = img.Resize(PicSize, PicSize, Inter.Linear);
            inputPath = inputPath.Replace(".jpg", ".png");
            img.Save(inputPath);
            Console.WriteLine("Before request");
            try
            {
                TensorflowSharp.SendRequest(inputPath, outputPath);
                Console.WriteLine("Request OK");
                var imgOut = new Image<Bgr, byte>(outputPath);
                imgOut = imgOut.Resize((int)Math.Max(PicSize, PicSize * width / height), (int)Math.Max(PicSize, PicSize * height / width), Inter.Linear);
                //imgOut.Save("C:/WebcamSnapshots/resizeTest.png");
                PostProcess(imgOut.ToBitmap(), outputPath);
                Console.WriteLine("Post Process OK");
                //Scaling = Math.Min(1, FindScaling(text, boxedText, height, width));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //private (string, string) ExtractText(Image<Bgr, byte> image)
        //{
        //    string tessPath = Environment.CurrentDirectory + @"\tesseract\tessdata";
        //    if (!File.Exists(tessPath))
        //        tessPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Magma Works\Harold\tesseract\tessdata";
        //    Console.WriteLine(tessPath);
        //    Tesseract ocr = new Tesseract(tessPath, "eng", Emgu.CV.OCR.OcrEngineMode.Default);
        //    ocr.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopkrstuvwxyz");

        //    ocr.SetImage(image);
        //    ocr.Recognize();

        //    return (ocr.GetUTF8Text(), ocr.GetBoxText());
        //}

        private void PostProcess(Bitmap bmp, string path)
        {
            // Filter bitmaps to extract elements contours
            Bitmap bmpUnblurred = ImageProcess(bmp, ProcessAction.Unblur);
            bmpUnblurred.Save(path.Replace(".png","_unblurred.png"));
            Bitmap bmpColumns = ImageProcess(bmpUnblurred, ProcessAction.FilterColumns);
            Bitmap bmpWalls = ImageProcess(bmpUnblurred, ProcessAction.FilterWalls);
            Bitmap bmpSlabs = ImageProcess(bmpUnblurred, ProcessAction.FilterSlabs);
            Bitmap bmpOpenings = ImageProcess(bmpUnblurred, ProcessAction.FilterOpenings);

            // Use filtered bitmaps to identify structural elements
            Columns = ExtractContour(ref bmpColumns, r: 0.001, minarea: 10, savePath: path.Replace(".png", "_columns.png"));
            Slabs = ExtractContour(ref bmpSlabs, savePath: path.Replace(".png", "_slabs.png"));
            Openings = ExtractContour(ref bmpOpenings, r: 0.05, minarea: 10, savePath: path.Replace(".png", "_openings.png"));
            Walls = WallProcess(ref bmpWalls, savePath: path.Replace(".png", "_walls.png"));
        }

        private double FindScaling(string text, string boxedText, double height, double width)
        {
            List<LocatedNumber> nums = LocateNumbers(text, boxedText);

            int redWidth = (int)Math.Max(PicSize, PicSize * width / height);
            int redHeight = (int)Math.Max(PicSize, PicSize * height / width);

            nums.ForEach(x => x.Location = new MWPoint2D(x.Location.X * redWidth/width, (1 - x.Location.Y/height) * redHeight));

            for (int i = 0; i < Slabs.Count; i++)
            {
                for(int j = 0; j < Slabs[i].Count - 1; j++)
                {
                    for(int k = 0; k < nums.Count; k++)
                    {
                        double dist = Points.DistancePointToLine(Slabs[i][j], Slabs[i][j + 1], nums[k].Location);
                        if (dist < PicSize / 25)
                        {
                            return nums[k].Number / Points.Distance(Slabs[i][j], Slabs[i][j + 1]);
                        }
                    }
                }
            }

            return 1;
        }

        private List<LocatedNumber> LocateNumbers(string text, string boxedText)
        {
            List<LocatedNumber> nums = new List<LocatedNumber>();

            text = text.Replace("\r", ",").Replace("\n", ",");
            char[] sep = new char[] { ',' };
            string[] strNums = text.Split(sep,StringSplitOptions.RemoveEmptyEntries);

            string[] boxes = boxedText.Replace("0\r\n",",").Split(sep,StringSplitOptions.RemoveEmptyEntries);
            List<LocatedChar> chars = new List<LocatedChar>();
            foreach(var str in boxes)
            {
                LocatedChar lc = new LocatedChar() { character = str[0] };
                string str0 = str.Remove(0, 2);
                str0 = str0.Replace(" ", ",");
                string[] locs = str0.Split(',');
                lc.Location = new MWPoint2D(Convert.ToInt32(locs[0]), Convert.ToInt32(locs[1]));
                chars.Add(lc);
            }

            foreach(var str in strNums)
            {
                string strNum = "";
                for(int i = 0; i < str.Length; i++)
                {
                    var c = str[i];
                    int n = 0;
                    bool isInt = Int32.TryParse(c.ToString(), out n);
                    if (isInt)
                        strNum += n;
                    else
                    {
                        if (strNum != "")
                        {
                            int num = Convert.ToInt32(strNum);
                            LocatedNumber ln = new LocatedNumber() { Number = num };
                            char ch = strNum[0];
                            ln.Location = chars.First(x => x.character == ch).Location;
                            nums.Add(ln);
                            strNum = "";
                        }
                    }
                    if(i == str.Length-1 && strNum != "")
                    {
                        int num = Convert.ToInt32(strNum);
                        LocatedNumber ln = new LocatedNumber() { Number = num };
                        char ch = strNum[0];
                        ln.Location = chars.First(x => x.character == ch).Location;
                        nums.Add(ln);
                        strNum = "";
                    }
                }
            }

            return nums;
        }

        private List<List<MWPoint2D>> ExtractContour(ref Bitmap bmp, double r = 0.01, double minarea = 1000, string savePath = "")
        {
            Image<Bgr, byte> img = bmp.ToImage<Bgr, byte>();
            Image<Gray, byte> threshold  = Preprocess(bmp);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(threshold, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            double maxEdges = 60;
            double maxarea = 0.9 * img.Width * img.Height;
            int contCount = contours.Size;
            List<List<MWPoint2D>> Contours = new List<List<MWPoint2D>>();
            for (int i = 0; i < contCount; i++)
            {
                double area = CvInvoke.ContourArea(contours[i]);
                if(area > minarea && area < maxarea)
                {
                    var cont = contours[i];
                    VectorOfPoint approx = new VectorOfPoint();
                    CvInvoke.ApproxPolyDP(cont, approx, r * CvInvoke.ArcLength(cont, true), true);
                    if(approx.Size < maxEdges)
                    {
                        CvInvoke.DrawContours(img, new VectorOfVectorOfPoint(new VectorOfPoint[] { approx }), 0, new MCvScalar(0, 0, 0));
                        List<MWPoint2D> pts = new List<MWPoint2D>();
                        for (int j = 0; j < approx.Size; j++)
                            pts.Add(new MWPoint2D(approx[j].X, approx[j].Y));
                        Contours.Add(pts);
                    }
                }
                
            }
            bmp = img.ToBitmap<Bgr, byte>();
            if(savePath != "") bmp.Save(savePath);
            return Contours;
        }

        private List<List<MWPoint2D>> WallProcess(ref Bitmap bmp, string savePath = "")
        {
            Mat mat0 = new Mat();
            Image<Bgr, byte> img = bmp.ToImage<Bgr, byte>();
            //img.Save("C:/WebcamSnapshots/not_preprocessed.png");
            Image<Gray, byte> threshold = Preprocess(bmp);
            //threshold.Save("C:/WebcamSnapshots/preprocessed.png");

            //var se1 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3),new Point(-1,-1));
            //var se2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 2),new Point(-1,-1));
            //var mask = new Mat();
            //CvInvoke.MorphologyEx(threshold, mask, MorphOp.Close, se1, new Point(-1, -1), 1, BorderType.Reflect101, new MCvScalar(0, 0, 0));
            //CvInvoke.MorphologyEx(mask, mask, MorphOp.Open, se2, new Point(-1, -1), 1, BorderType.Reflect101, new MCvScalar(0, 0, 0));

            //CvInvoke.BitwiseNot(threshold, threshold);
            //CvInvoke.BitwiseNot(threshold, threshold, mask);

            Matrix<byte> kernel1 = new Matrix<byte>(new byte[5, 5] { { 0, 0, 1, 1, 1 }, { 0, 0, 1, 1, 1 }, { 0, 0, 1, 1, 1 }, { 0, 0, 1, 1, 1 }, { 0, 0, 1, 1, 1 } });
            Matrix<byte> kernel2 = new Matrix<byte>(new byte[5, 5] { { 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 0 }, { 0, 1, 1, 1, 0 }, { 0, 1, 1, 1, 0 }, { 0, 0, 0, 0, 0 } });
            Matrix<byte> kernel3 = new Matrix<byte>(new byte[5, 5] { { 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1 }, { 0, 0, 1, 1, 1 }, { 0, 0, 1, 1, 1 }, { 0, 0, 1, 1, 1 } });
            Matrix<int> kernel4 = new Matrix<int>(new int[3,3] { { -1, -1, -1 }, { -1, 1, -1 } , { -1, -1, -1 } });
            //StructuringElementEx element2 = new StructuringElementEx(new int[3, 3] { { 0, 1, 0 }, { 1, 0, 1 }, { 0, 1, 0 } }, 1, 1);
            //Image<Gray, byte> tmp = threshold.MorphologyEx(MorphOp.Open, kernel1, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //CvInvoke.BitwiseAnd(threshold, threshold, threshold, tmp);

            //Image<Gray, byte> tmp2 = threshold.MorphologyEx(MorphOp.Open, kernel2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //CvInvoke.BitwiseAnd(threshold, threshold, threshold, tmp2);

            CvInvoke.BitwiseNot(threshold, threshold);
            Image<Gray, byte> tmp3 = threshold.MorphologyEx(MorphOp.Open, kernel4, new Point(-1, -1), 5, BorderType.Default, new MCvScalar());
            CvInvoke.BitwiseAnd(threshold, threshold, threshold, tmp3);

            //CvInvoke.BitwiseNot(threshold, threshold);
            //CvInvoke.BitwiseNot(threshold, threshold);
            //CvInvoke.BitwiseNot(threshold, threshold);

            Mat matOrig = threshold.Mat.Clone();
            Mat newMat = new Mat(matOrig.Size, DepthType.Cv8U, 1);
            CvInvoke.CvtColor(matOrig, newMat, ColorConversion.Gray2Bgr);

            Mat filteredMat = new Mat();
            CvInvoke.BilateralFilter(matOrig, filteredMat, 9, 100, 100);
            CvInvoke.BitwiseNot(filteredMat, filteredMat);
            threshold = filteredMat.ToImage<Gray, byte>();

            //threshold.Save("C:/WebcamSnapshots/filtered.png");

            Image<Gray, byte> skel = Skeletonize(threshold);
            // Gaussian blur
            var mat = new Mat();
            CvInvoke.GaussianBlur(skel, mat, new Size(3, 3), 0);
            Image<Gray, byte> blurred = mat.ToImage<Gray, byte>();

            // canny
            CvInvoke.Canny(blurred, mat, 50, 150);
            Image<Gray, byte> edges = mat.ToImage<Gray, byte>();

            double rho = 0.1; // distance resolution in pixels of the Hough grid
            double theta = 10 * Math.PI / 180; // angular resolution in radians of the Hough grid
            int thresh = 5; // minimum number of votes (intersections in Hough grid cell)
            double minLineLength = 1; // minimum number of pixels making up a line
            double maxLineGap = 10; // maximum gap in pixels between connectable line segments
            //double line_image = np.copy(img0) * 0; // creating a blank to draw lines on
            LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, rho, theta, thresh, minLineLength, maxLineGap);
            Image<Bgr, byte> linesImg = img.Copy();
            foreach (var l in lines)
                CvInvoke.Line(linesImg, l.P1, l.P2, new MCvScalar(0, 0, 0), 1);

            Bitmap linesBmp = linesImg.ToBitmap();
            //linesBmp.Save("C:/WebcamSnapshots/houghLines.png");

            if (lines != null && lines.Length > 0)
            {
                int n = lines.Length;
                int[][] lineGroups = new int[n][];
                List<int> oneArr = new List<int>() { 1 };
                List<int> zeroArr = new List<int>() { 0 };
                for (int k = 1; k < n; k++)
                {
                    oneArr.Add(0);
                    zeroArr.Add(0);
                }
                lineGroups[0] = oneArr.ToArray();
                for(int i = 1; i < n; i++)
                {
                    lineGroups[i] = zeroArr.ToArray();
                    lineGroups = CompareLines(lines[i], i, lines, lineGroups); // groups lines together
                }

                int ii = lineGroups.Where(l => l[0] != 0).Count();
                List<LineSegment2D> walls = new List<LineSegment2D>();
                for(int i = 0; i < ii; i++)
                {
                    List<double> ptsX = new List<double>();
                    List<double> ptsY = new List<double>();
                    int jj = lineGroups[i].ToList().IndexOf(0);
                    List<PointF> pts = new List<PointF>();
                    for (int j = 0; j < jj; j++)
                    {
                        LineSegment2D line = lines[lineGroups[i][j] - 1];
                        pts.Add(new PointF(line.P1.X, line.P1.Y));
                        pts.Add(new PointF(line.P2.X, line.P2.Y));
                    }
                    RotatedRect rect = CvInvoke.MinAreaRect(pts.ToArray());
                    PointF[] boxPts = CvInvoke.BoxPoints(rect);
                    //for(int k = 0; k < 4; k++)
                    //{
                    //    int x1 = (int)boxPts[k].X;
                    //    int y1 = (int)boxPts[k].Y;
                    //    int x2 = (int)boxPts[(k+1)%4].X;
                    //    int y2 = (int)boxPts[(k+1)%4].Y;
                    //    CvInvoke.Line(img, new Point(x1, y1), new Point(x2, y2), new MCvScalar(0, 0, 0), 1, LineType.EightConnected);
                    //}
                    walls.Add(GetCenterline(boxPts));
                }
                bmp = img.ToBitmap<Bgr, byte>();
                Image<Bgr, byte> wallsImg = img.Copy();
                foreach (var w in walls)
                    CvInvoke.Line(wallsImg, w.P1, w.P2, new MCvScalar(0, 0, 0));
                //wallsImg.Save("C:/WebcamSnapshots/walls.png");
                if (savePath != "") wallsImg.Save(savePath);
                return walls.Select(l => new List<MWPoint2D> { new MWPoint2D(l.P1.X, l.P1.Y), new MWPoint2D(l.P2.X, l.P2.Y) }).ToList();
            }
            return null;
        }

        private LineSegment2D GetCenterline(PointF[] box)
        {
            double d1 = Points.Distance(new MWPoint2D(box[0].X, box[0].Y), new MWPoint2D(box[1].X, box[1].Y));
            double d2 = Points.Distance(new MWPoint2D(box[0].X, box[0].Y), new MWPoint2D(box[3].X, box[3].Y));

            Point p1;
            Point p2;
            if (d1 > d2)
            {
                p1 = new Point((int)(0.5 * (box[0].X + box[3].X)), (int)(0.5 * (box[0].Y + box[3].Y)));
                p2 = new Point((int)(0.5 * (box[1].X + box[2].X)), (int)(0.5 * (box[1].Y + box[2].Y)));
            }
            else
            {
                p1 = new Point((int)(0.5 * (box[0].X + box[1].X)), (int)(0.5 * (box[0].Y + box[1].Y)));
                p2 = new Point((int)(0.5 * (box[3].X + box[2].X)), (int)(0.5 * (box[3].Y + box[2].Y)));
            }
            return new LineSegment2D(p1, p2);
        }

        private int[][] CompareLines(LineSegment2D line, int index, LineSegment2D[] lines, int[][] lineGroups)
        {
            int n = lines.Length;
            for(int i = 0; i < n; i++)
            {
                if(lineGroups[i][0] != 0)
                {
                    int idx = lineGroups[i].ToList().IndexOf(0);
                    for(int j = 0; j < idx; j++)
                    {
                        if (SimilarLines(line,lines[lineGroups[i][j] - 1]))
                        {
                            lineGroups[i][idx] = index + 1;
                            return lineGroups;
                        }
                    }
                }
                else
                {
                    lineGroups[i][0] = index + 1;
                    return lineGroups;
                }
            }
            return null;
        }

        private bool SimilarLines(LineSegment2D l1, LineSegment2D l2)
        {
            MWPoint2D p1_1 = new MWPoint2D(l1.P1.X, l1.P1.Y);
            MWPoint2D p1_2 = new MWPoint2D(l1.P2.X, l1.P2.Y);
            MWPoint2D p2_1 = new MWPoint2D(l2.P1.X, l2.P1.Y);
            MWPoint2D p2_2 = new MWPoint2D(l2.P2.X, l2.P2.Y);
            double d0 = Points.DistancePointToLine(p1_1, p1_2, p2_1);
            double d1 = Points.DistancePointToLine(p1_1, p1_2, p2_2);
            double d2 = Points.DistancePointToLine(p2_1, p2_2, p1_1);
            double d3 = Points.DistancePointToLine(p2_1, p2_2, p1_2);

            int closeEps = 5;
            bool closeLines = d0 < closeEps && d1 < closeEps && d2 < closeEps && d3 < closeEps;

            int closeEps2 = 3;
            bool b1 = Points.Distance(p2_1, p1_1) + Points.Distance(p2_1, p1_2) - Points.Distance(p1_1, p1_2) < closeEps2;
            bool b2 = Points.Distance(p2_2, p1_1) + Points.Distance(p2_2, p1_2) - Points.Distance(p1_1, p1_2) < closeEps2;
            bool b3 = Points.Distance(p1_1, p2_1) + Points.Distance(p1_1, p2_2) - Points.Distance(p2_1, p2_2) < closeEps2;
            bool b4 = Points.Distance(p1_2, p2_1) + Points.Distance(p1_2, p2_1) - Points.Distance(p2_1, p2_2) < closeEps2;

            bool closeLines2 = b1 || b2 || b3 || b4;

            return closeLines && closeLines2;
        }

        public static Image<Gray, byte> Skeletonize(Image<Gray, byte> image)
        {
            //Mat mat = new Mat();
            //CvInvoke.CvtColor(image, mat, ColorConversion.Bgr2Gray);
            Image<Gray, byte> img2 = (new Image<Gray, byte>(image.Width, image.Height, new Gray(255))).Sub(image);
            Image<Gray, byte> eroded = new Image<Gray, byte>(img2.Size);
            Image<Gray, byte> temp = new Image<Gray, byte>(img2.Size);
            Image<Gray, byte> skel = new Image<Gray, byte>(img2.Size);
            skel.SetValue(0);
            CvInvoke.Threshold(img2, img2, 127, 256, 0);
            var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(3, 3), new Point(-1, -1));
            bool done = false;

            while (!done)
            {
                CvInvoke.Erode(img2, eroded, element, new Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
                CvInvoke.Dilate(eroded, temp, element, new Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
                CvInvoke.Subtract(img2, temp, temp);
                CvInvoke.BitwiseOr(skel, temp, skel);
                eroded.CopyTo(img2);
                if (CvInvoke.CountNonZero(img2) == 0) done = true;
            }
            //mat = new Mat();
            //CvInvoke.CvtColor(skel, mat, ColorConversion.Gray2Bgr);
            return skel; // mat.ToImage<Gray,byte>();
        }

        private Image<Gray, byte> Preprocess(Bitmap bmp)
        {
            ImageConverter converter = new ImageConverter();
            byte[] bytes = (byte[])converter.ConvertTo(bmp, typeof(byte[]));

            Image<Bgr, byte> img = bmp.ToImage<Bgr, byte>();
            Image<Gray, byte> grayImage = img.Convert<Gray, byte>();
            Gray thresh = new Gray(250);
            Gray max = new Gray(255);
            Image<Gray, Byte> threshold = grayImage.ThresholdBinary(thresh, max);

            Mat mat = new Mat();
            CvInvoke.BitwiseNot(threshold, mat);
            threshold = mat.ToImage<Gray, byte>();

            //Matrix<byte> kernel1 = new Matrix<byte>(new byte[5, 5] { { 0, 0, 1, 1, 1 }, { 0, 0, 1, 1, 1 }, { 0, 0, 1, 1, 1 }, { 0, 0, 1, 1, 1 }, { 0, 0, 1, 1, 1 } });
            Matrix<byte> kernel1 = new Matrix<byte>(new byte[3, 3] { { 0, 1, 1 }, { 0, 1, 1}, { 0, 1, 1} });
            Matrix<byte> kernel2 = new Matrix<byte>(new byte[5, 5] { { 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 0 }, { 0, 1, 1, 1, 0 }, { 0, 1, 1, 1, 0 }, { 0, 0, 0, 0, 0 } });
            Matrix<byte> kernel3 = new Matrix<byte>(new byte[5, 5] { { 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 1 }, { 1, 0, 0, 0, 1 }, { 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1 } });
            //StructuringElementEx element2 = new StructuringElementEx(new int[3, 3] { { 0, 1, 0 }, { 1, 0, 1 }, { 0, 1, 0 } }, 1, 1);
            Image<Gray, byte> tmp = threshold.MorphologyEx(MorphOp.Open, kernel2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            Mat mat1 = new Mat();
            CvInvoke.BitwiseAnd(threshold, threshold, mat1, tmp);

            threshold = mat1.ToImage<Gray,byte> ();
            Image<Gray, byte> tmp2 = threshold.MorphologyEx(MorphOp.Dilate, kernel1, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            Mat mat2 = new Mat();
            CvInvoke.BitwiseAnd(mat1, mat1, mat2, tmp2);

            threshold = mat2.ToImage<Gray, byte>();
            CvInvoke.BitwiseNot(threshold, threshold);
            Image<Gray, byte> tmp3 = threshold.MorphologyEx(MorphOp.Open, kernel3, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            Mat mat3 = new Mat();
            CvInvoke.BitwiseAnd(threshold, threshold, mat3, tmp3);

            return mat3.ToImage<Gray,byte>();
        }

        private Bitmap ImageProcess(Bitmap bmp0, ProcessAction pa)
        {
            Bitmap bmp = (Bitmap)bmp0.Clone();
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Process action
            MethodInfo theMethod = this.GetType().GetMethod(pa.ToString());
            rgbValues = (byte[])theMethod.Invoke(this, new object[] { rgbValues, bmpData.Stride });
                
            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            return bmp;
            // Draw the modified image.
            //e.Graphics.DrawImage(bmp, 0, 150);
        }

        public byte[] Unblur(byte[] rgbValues, int stride)
        {
            for (int i = 0; i < rgbValues.Length; i++)
            {
                if (rgbValues[i] < 128)
                    rgbValues[i] = 0;
                else
                    rgbValues[i] = 255;
            }
            return rgbValues;
        }

        public byte[] FilterColumns(byte[] rgbValues, int stride)
        {
            //byte[] column_color = { 255, 0, 0 };
            byte[] column_color = { 0, 0, 255 };
            int counter = 0;
            int k = 1;
            for(int i = 0; i < rgbValues.Length; i += 3)
            {
                if (k * stride - 3 < i && i <= k * stride)
                {
                    i = k * stride;
                    if (i >= rgbValues.Length) break;
                    k++;
                }
                if (rgbValues[i] != column_color[0] || rgbValues[i + 1] != column_color[1] || rgbValues[i + 2] != column_color[2])
                {
                    rgbValues[i] = 255;
                    rgbValues[i+1] = 255;
                    rgbValues[i+2] = 255;
                }
                if (rgbValues[i] == column_color[0] && rgbValues[i + 1] == column_color[1] && rgbValues[i + 2] == column_color[2])
                    counter++;
            }
            return rgbValues;
        }

        public byte[] FilterWalls(byte[] rgbValues, int stride)
        {
            //byte[] wall_color = { 0, 255, 0 };
            byte[] wall_color = { 0, 255, 0 };
            int k = 1;
            for (int i = 0; i < rgbValues.Length; i += 3)
            {
                if (k * stride - 3 < i && i <= k * stride)
                {
                    i = k * stride;
                    if (i >= rgbValues.Length) break;
                    k++;
                }
                if (rgbValues[i] != wall_color[0] || rgbValues[i + 1] != wall_color[1] || rgbValues[i + 2] != wall_color[2])
                {
                    rgbValues[i] = 255;
                    rgbValues[i + 1] = 255;
                    rgbValues[i + 2] = 255;
                }
            }
            return rgbValues;
        }

        public byte[] FilterSlabs(byte[] rgbValues, int stride)
        {
            //byte[] text_color = { 0, 255, 255 };
            byte[] text_color = { 255, 255, 0 };
            //byte[] slab_color = { 255, 255, 0 };
            byte[] slab_color = { 0, 255, 255 };
            int k = 1;
            for (int i = 0; i < rgbValues.Length; i += 3)
            {
                if (k * stride - 3 < i && i <= k * stride)
                {
                    i = k * stride;
                    if (i >= rgbValues.Length) break;
                    k++;
                }
                if (rgbValues[i] == text_color[0] && rgbValues[i + 1] == text_color[1] && rgbValues[i + 2] == text_color[2])
                {
                    rgbValues[i] = 255;
                    rgbValues[i + 1] = 255;
                    rgbValues[i + 2] = 255;
                }
                else if(rgbValues[i] != 255 || rgbValues[i + 1] != 255 || rgbValues[i + 2] != 255)
                {
                    rgbValues[i] = slab_color[0];
                    rgbValues[i + 1] = slab_color[1];
                    rgbValues[i + 2] = slab_color[2];
                }
            }
            return rgbValues;
        }

        public byte[] FilterOpenings(byte[] rgbValues, int stride)
        {
            //byte[] opening_color = { 0, 0, 255 };
            byte[] opening_color = { 255, 0, 0 };
            int k = 1;
            for (int i = 0; i < rgbValues.Length; i += 3)
            {
                if (k * stride - 3 < i && i <= k * stride)
                {
                    i = k * stride;
                    if (i >= rgbValues.Length) break;
                    k++;
                }
                if (rgbValues[i] != opening_color[0] || rgbValues[i + 1] != opening_color[1] || rgbValues[i + 2] != opening_color[2])
                {
                    rgbValues[i] = 255;
                    rgbValues[i + 1] = 255;
                    rgbValues[i + 2] = 255;
                }
            }
            return rgbValues;
        }

        private void run_cmd(string cmd, string args)
        {
            //ProcessStartInfo start = new ProcessStartInfo(@"C:\Users\Grégoire Corre\AppData\Local\Programs\Python\Python37\python.exe");
            Console.WriteLine("Python path : {0}", pythonPath);
            ProcessStartInfo start = new ProcessStartInfo(pythonPath);
            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            //start.RedirectStandardInput = true;
            start.CreateNoWindow = true;

            var errors = "";
            var results = "";

            using (Process process = Process.Start(start))
            {
                errors = process.StandardError.ReadToEnd();
                results = process.StandardOutput.ReadToEnd();
            }

            Console.WriteLine("ERRORS:");
            Console.WriteLine(errors);
            Console.WriteLine("RESULTS:");
            Console.WriteLine(results);


        }

        //private void create_adf_client()
        //{
        //    var authenticationContext = new AuthenticationContext($"https://login.windows.net/{tenant_id}");
        //    var credential = new ClientCredential(clientId: client_id, clientSecret: client_key);
        //    var result = authenticationContext.AcquireTokenAsync(resource: "https://management.core.windows.net/", clientCredential: credential);
        //    if (result == null)
        //    {
        //        throw new InvalidOperationException("Failed to obtain the JWT token");
        //    }
        //    var token = result.Result.AccessToken;
        //    var _credentials = new TokenCloudCredentials(subscription_id, token);
        //    inner_client = new DataFactoryManagementClient(_credentials);
        //}

        //private void StartPipeline(string resourceGroup, string dataFactory, string pipelineName, DateTime slice)
        //{
        //    var pipeline = inner_client.Pipelines.Get(resourceGroup, dataFactory, pipelineName);

        //    pipeline.Pipeline.Properties.Start = DateTime.Parse($"{slice.Date:yyyy-MM-dd}T00:00:00Z");
        //    pipeline.Pipeline.Properties.End = DateTime.Parse($"{slice.Date:yyyy-MM-dd}T23:59:59Z");
        //    pipeline.Pipeline.Properties.IsPaused = false;

        //    inner_client.Pipelines.CreateOrUpdate(resourceGroup, dataFactory, new PipelineCreateOrUpdateParameters()
        //    {
        //        Pipeline = pipeline.Pipeline
        //    });
        //}

        private string GetPythonPath()
        {
            if(pyOrigin == PythonOrigin.local)
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                string pathVariable = environmentVariables["Path"] as string;
                if (pathVariable != null)
                {
                    string[] allPaths = pathVariable.Split(';');
                    foreach (var path in allPaths)
                    {
                        string pythonPathFromEnv = path + "\\python.exe";
                        if (File.Exists(pythonPathFromEnv))
                            return pythonPathFromEnv;
                    }
                }
                MessageBox.Show("Python path not found");
                return null;
            }
            else if(pyOrigin == PythonOrigin.server)
            {
                return "W:\\WW General\\WW Software Resources\\Magma Works\\Python37\\python.exe";
            }
            return null;
        }
    }

    public class LocatedNumber
    {
        public int Number;
        public MWPoint2D Location;
    }

    public class LocatedChar
    {
        public char character;
        public MWPoint2D Location;
    }
}
