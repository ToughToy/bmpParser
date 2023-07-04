using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;

namespace bmpParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"..\..\";
            string fileName = "forMaster.bmp";
            Console.WriteLine("Finding forMaster.bmp in project dir...");
            Layer layer = new Layer(path + fileName);
            Console.WriteLine("OK...");
            Console.WriteLine("Write Parameters:");
            int[] parameters = ReadParameters();
            testFibers(layer, path, parameters[0], parameters[1], parameters[2], parameters[3]);


            Console.ReadKey();
        }

        static int[] ReadParameters()
        {
            string paramString;
            int[] paramsTo = new int[4];
            Console.Write("Fiber Radius:\t");
            paramString = Console.ReadLine();
            paramsTo[0] = Convert.ToInt32(paramString);

            Console.Write("Object Radius:\t");
            paramString = Console.ReadLine();
            paramsTo[1] = Convert.ToInt32(paramString);

            Console.Write("Fiber step:\t");
            paramString = Console.ReadLine();
            paramsTo[2] = Convert.ToInt32(paramString);

            Console.Write("Step:\t");
            paramString = Console.ReadLine();
            paramsTo[3] = Convert.ToInt32(paramString);

            return paramsTo;
        }

        static int testFiberPoint(string path)
        {
            string fileName = "t1.bmp";
            Layer layer = new Layer(path + fileName);
            layer.FindContours();

            List<int> borderF = new List<int>()
            {
                (int)LayerPoint.IsCountour,
                (int)LayerPoint.IsModel,
                (int)LayerPoint.IsEmpty,
                (int)LayerPoint.IsFiber
            };

            List<int> borderW = new List<int>()
            {
                (int)LayerPoint.IsCountour,
                (int)LayerPoint.IsModel,
                (int)LayerPoint.IsEmpty
            };

            FiberPoint fiberPoint = new FiberPoint(1, 5, borderF, borderW, (int)LayerPoint.IsFiber);
            int resX = fiberPoint.GetOffsetX(0, 0, 17, 68, ref layer.LayerMap);

            return resX;
        }


        static void testFibers(Layer layer, string path, int rf, int rO, int stepF, int step)
        {
            layer.FindContours();
            foreach (Contour contour in layer.Contours)
            {
                layer.CloseContour(contour);
            }
            layer.FillContours();
            layer.SetFibeOptions(rf, rO, stepF, step);
            //layer.TestLayFibers(x, rf, stepF, rO, step);
            layer.LayFibers();

            string fileName = "t5.bmp";
            layer.LayerMap.Save(path + fileName);
        }

        static void testFillContours(Layer layer, string path)
        {
            layer.FindContours();
            foreach (Contour contour in layer.Contours)
            {
                layer.CloseContour(contour);
            }
            layer.FillContours();
            string fileName = "t4.bmp";
            layer.LayerMap.Save(path + fileName);
        }

        static void testCloseContour(Layer layer, string path)
        {
            layer.FindContours();
            foreach (Contour contour in layer.Contours)
            {
                layer.CloseContour(contour);
            }
            string fileName = "t3.bmp";
            layer.LayerMap.Save(path + fileName);
        }

        static void testFindLines(Layer layer, string path)
        {
            layer.FindContours();
            layer.FindLines((float)Math.PI / 2);

            Bitmap bitmap = new Bitmap(300, 300);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                    bitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255));
            }

            foreach (Contour contour in layer.Contours)
            {
                int r = 100;
                int g = 0;
                int b = 20;
                foreach (Line line in contour.Lines)
                {
                    
                    Color color = Color.FromArgb(r, g, b);
                    for (int i = line.BeginPosition; i <= line.EndPosition; i++)
                    {
                        bitmap.SetPixel(contour.Points[i].X, contour.Points[i].Y, color);
                    }
                    r = (r + 44) % 255; g = (g + 5) % 255; b = (b + 15) % 255;
                }
            }

            string fileName = "t2.bmp";
            bitmap.Save(path + fileName);

        }

        static void testFindContour(Layer layer, string path)
        {
            
            layer.FindContours();

            Bitmap bitmap = new Bitmap(300, 500);

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                    bitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255));
            }
            int greenPart = 100;
            foreach (Contour contour in layer.Contours)
            {
                Color color = Color.FromArgb(0, greenPart, 0);
                foreach (Point p in contour.Points)
                {
                    bitmap.SetPixel(p.X, p.Y, color);
                }
                greenPart += 100;
            }
            string fileName = "t1.bmp";
            bitmap.Save(path + fileName);
        }
    }
}
