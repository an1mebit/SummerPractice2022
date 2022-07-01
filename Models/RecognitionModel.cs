using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.Util;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;

namespace TelegaApp.Models
{
    class RecognitionModel
    {
        private VideoCapture? cap;

        public RecognitionModel()
        {
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

        public Avalonia.Media.Imaging.Bitmap GetImage()
        {
            if (cap != null)
            {
                Mat mat = new Mat();
                cap.Retrieve(mat);
                var oldImage = cap.QueryFrame().ToImage<Bgr, byte>();

                VectorOfVectorOfPoint use_contours = getContours(cap.QueryFrame().ToImage<Hsv, byte>());
                //CvInvoke.DrawContours(oldImage, use_contours, -1, new MCvScalar(255, 0, 0));

                Point[] gravity = new Point[use_contours.Size];
                gravity = GetPoints(use_contours);

                foreach (Point cent in gravity)
                {
                    CvInvoke.Circle(oldImage, cent, 2, new MCvScalar(0, 0, 255), 2);
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

        public Avalonia.Media.Imaging.Bitmap GetMask()
        {
            if (cap != null)
            {
                Image<Hsv, byte> rgbImage = cap.QueryFrame().ToImage<Hsv,byte>();

                var image = rgbImage.InRange(new Hsv(95, 80, 2), new Hsv(126, 255, 255));
                image = image.Convert<Gray, byte>();
                var mask = image.ThresholdBinary(new Gray(150), new Gray(255));
                mask = mask.Canny(75, 200);


                var imageBmp = mask.ToBitmap();

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

        private VectorOfVectorOfPoint getContours(Image<Hsv,byte> rgbImage)
        {
            var image = rgbImage.InRange(new Hsv(95, 80, 2), new Hsv(126, 255, 255));
            image = image.Convert<Gray, byte>();
            var mask = image.ThresholdBinary(new Gray(150), new Gray(255));
            //CvInvoke.BilateralFilter(mask, mask, 5, 175, 175);
            mask = mask.Canny(75, 200);

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
                // Рассчитываем момент текущего контура
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
