using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.Util;
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Collections.Generic;

namespace TelegaApp.Models
{
    class RecognitionModel
    {
        private VideoCapture? cap;

        Dictionary<string, int[,]> collor;

        public RecognitionModel()
        {
            int[,] red = { { 0, 150, 130 }, { 10,255,255} };
            int[,] green = { { 45, 170, 150 }, { 65, 255, 255 } };
            int[,] blue = { { 90, 130, 100 }, { 130, 255, 255 } };
            int[,] purple = { { 145, 150, 100 }, { 160, 255, 255 } };
            int[,] yellow = { { 26, 150, 100 }, { 36, 255, 255 } };

            collor = new Dictionary<string, int[,]>()
            {
                {"Red", red },
                {"Green" , green },
                {"Blue", blue },
                {"Purple", purple },
                {"Yellow", yellow }
            };

            if (cap != null)
            {
                cap.Start();
            }
            else
            {
                cap = new VideoCapture(0);
                cap.ImageGrabbed += Cap_ImageGrabbed;
                cap.Start();
            }
        }

        private void Cap_ImageGrabbed(object? sender, EventArgs e)
        {
            if (cap != null)
            {
                Mat mat = new Mat();
                cap.Retrieve(mat);
            }
        }

        public Avalonia.Media.Imaging.Bitmap GetImage(List<string> checkBoxesChecked)
        {
            if (cap != null)
            {
                Mat mat = new Mat();
                cap.Retrieve(mat);
                var oldImage = cap.QueryFrame().ToImage<Bgr, byte>();

                for (int i = 0; i < checkBoxesChecked.Count; i++)
                {
                    VectorOfVectorOfPoint use_contours = getContours(oldImage.Convert<Hsv, byte>(), checkBoxesChecked[i]);

                    Point[] gravity = new Point[use_contours.Size];
                    gravity = GetPoints(use_contours);

                    foreach (Point cent in gravity)
                    {
                        CvInvoke.Circle(oldImage, cent, 2, new MCvScalar(0, 0, 255), 2);
                    }
                }
                var imageBmp = oldImage.ToBitmap();

                Avalonia.Media.Imaging.Bitmap avalonBmp;

                using (MemoryStream memory = new MemoryStream())
                {
                    imageBmp.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    avalonBmp = new Avalonia.Media.Imaging.Bitmap(memory);
                }
                return avalonBmp;
            }
            else
            {
                throw new Exception("Something going wrong");
            }
        }

        public Avalonia.Media.Imaging.Bitmap GetMask(List<string> checkBoxesChecked)
        {
            if (cap != null)
            {
                Mat multiMask = Mat.Zeros(cap.QueryFrame().Rows, cap.QueryFrame().Cols, Emgu.CV.CvEnum.DepthType.Cv8U, cap.QueryFrame().NumberOfChannels);
                var old = cap.QueryFrame().ToImage<Bgr, byte>();
                Image<Hsv, byte> rgbImage = old.Convert<Hsv, byte>();

                Image<Gray, byte> oldmask = null;
                for (int i = 0; i < checkBoxesChecked.Count; i++)
                {
                    var hsv = collor[checkBoxesChecked[i]];

                    var image = rgbImage.InRange(new Hsv(hsv[0, 0], hsv[0, 1], hsv[0, 2]), new Hsv(hsv[1, 0], hsv[1, 1], hsv[1, 2]));

                    var mask = image.Dilate(3);
                    if (i > 0 && oldmask != null)
                        CvInvoke.BitwiseOr(mask, oldmask, multiMask);
                    oldmask = mask;
                }
                
                var imageBmp = multiMask.ToBitmap();

                Avalonia.Media.Imaging.Bitmap avalonBmp;

                using (MemoryStream memory = new MemoryStream())
                {
                    imageBmp.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    avalonBmp = new Avalonia.Media.Imaging.Bitmap(memory);
                }
                return avalonBmp;
            }
            else
            {
                throw new Exception("Something going wrong");
            }
        }

        private VectorOfVectorOfPoint getContours(Image<Hsv,byte> rgbImage, string color)
        {
            var hsv = collor[color];
            var image = rgbImage.InRange(new Hsv(hsv[0,0], hsv[0,1], hsv[0,2]), new Hsv(hsv[1,0], hsv[1,1], hsv[1,2]));

            var mask = image.Dilate(3);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierachy = new Mat();

            CvInvoke.FindContours(mask, contours, hierachy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            VectorOfVectorOfPoint use_contours = new VectorOfVectorOfPoint();

            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint contour = contours[i];

                double area = CvInvoke.ContourArea(contour);
                if (area > 0)
                {
                    use_contours.Push(contour);
                }
            }
            return use_contours;
        }

        private Point[] GetPoints(VectorOfVectorOfPoint use_contours)
        {
            int ksize = use_contours.Size;
            double[] m00 = new double[ksize];
            double[] m01 = new double[ksize];
            double[] m10 = new double[ksize];
            Point[] gravity = new Point[ksize];
            Moments[] moments = new Moments[ksize];

            for (int i = 0; i < ksize; i++)
            {
                VectorOfPoint contour = use_contours[i];
                moments[i] = CvInvoke.Moments(contour, false);

                m00[i] = moments[i].M00;
                m01[i] = moments[i].M01;
                m10[i] = moments[i].M10;
                int x = Convert.ToInt32(m10[i] / m00[i]);
                int y = Convert.ToInt32(m01[i] / m00[i]);
                gravity[i] = new Point(x, y);
            }

            return gravity;
        }
    }
}
