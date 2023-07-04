using System;
using System.Collections.Generic;
using System.Drawing;

namespace bmpParser
{
    enum LayerPoint : int //~A R G B
    {
        IsEmpty = 0x00FFFFFF,
        IsTempEmpty = 0x000FAAF0,
        IsNormal = 0x0000FF00,
        IsModel = 0x00000000,
        IsCountour = 0x007F7F7F,
        IsInside = 0x00FF80FF,
        IsFiber = 0x0000FFFF
    }

    public class Layer
    {
        public Layer(string path)
        {
            try
            {

                Contours = new List<Contour>();
                LayerMap = new Bitmap(path);
                ScanRange = 9;
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("Some shit happens");
            }

        }
        public int ScanRange;
        public Bitmap LayerMap;
        public List<Contour> Contours;
        private int FiberR;
        private int WarningR;
        private int FiberStep;
        private int LayStep;

        public void SetFibeOptions(int fiberR, int warningR, int fiberStep, int layStep)
        {
            if (!(fiberR > 0 && warningR > 0 && fiberStep > 0 && layStep > 0)) return;
            FiberR      = fiberR;
            WarningR    = warningR;
            FiberStep   = fiberStep;
            LayStep     = layStep;
        }

        public bool IsPointInContour(int x, int y)
        {
            foreach (Contour contour in Contours)
            {
                bool isContains = contour.Contains(new Point(x, y));
                if (isContains) return true;
            }
            return false;
        }

        private Point GetPointToLay(Point p)
        {
            for (int x = p.X; x < LayerMap.Width; x++)
            {
                for (int y = p.Y; y < LayerMap.Height; y++)
                {
                    int border = LayerMap.GetPixel(x, y).ToArgb() & 0x00FFFFFF;
                    if (border == (int)LayerPoint.IsModel)
                    {
                        Point point = new Point(x, y);
                        return point;
                    }
                }
            }

            return new Point(0, 0);
        }

        private void DrawFiber(List<Point> pointList)
        {
            if (pointList.Count < 2) return;
            for (int i = 0; i < pointList.Count - 1; i++)
            {
                if (!pointList[i].IsEmpty && !pointList[i + 1].IsEmpty)
                {
                    Color color = Color.FromArgb((int)LayerPoint.IsFiber | (0xFF << 24));
                    List<Point> section = LineTo(pointList[i], pointList[i + 1]);
                    DrawLine(section, color);
                }
            }
        }

        public void LayFibers()
        {
            List<int> borderF = new List<int>()
            {
                (int)LayerPoint.IsCountour,
                (int)LayerPoint.IsModel,
                (int)LayerPoint.IsEmpty,
                (int)LayerPoint.IsTempEmpty,
                (int)LayerPoint.IsFiber
            };

            List<int> borderW = new List<int>()
            {
                //(int)LayerPoint.IsCountour,
                //(int)LayerPoint.IsModel,
                (int)LayerPoint.IsNormal,
                (int)LayerPoint.IsEmpty
            };

            FiberPoint fiberPoint = new FiberPoint(FiberR, WarningR, borderF, borderW, (int)LayerPoint.IsFiber);

            setTypeContours();

            foreach (Contour c in Contours)
            {
                if (c.isInternal) continue;
                c.GetBoundingRect(out Point p1, out Point p2);
                int midX = (p2.X - p1.X) / 2;
                int xL = p1.X + LayStep, xR = p2.X - LayStep;
                while (true)
                {
                    List<Point> pointListL = LayOneLine(borderF, borderW, p1.Y + FiberStep, p2.Y, xL);
                    List<Point> pointListR = LayOneLine(borderF, borderW, p1.Y + FiberStep, p2.Y, xR);

                    DrawFiber(pointListL);
                    DrawFiber(pointListR);

                    //test
                    //foreach (Point p in pointList)
                    //    LayerMap.SetPixel(p.X, p.Y, Color.FromArgb((int)LayerPoint.IsFiber | (0xFF << 24)));
                    //string path = @"C:\Users\Mikhail\Desktop\Магистратура\ДИССЕРТАЦИЯ\Алгоритмы\bmpParser\temp\";
                    //string fileName = $"t_{xL}_{xR}.bmp";
                    //LayerMap.Save(path + fileName);

                    xL += LayStep;
                    xR -= LayStep;
                    if (xL > midX || xR < midX) return;
                }
            }
        }

        private List<Point> LayOneLine(List<int> borderF, List<int> borderW, int yStart, int yEnd, int x)
        {
            int y = yStart;
            int oldY = 0, oldX = 0;
            List<Point> pointList = new List<Point>();
            FiberPoint fiberPoint = new FiberPoint(FiberR, WarningR, borderF, borderW, (int)LayerPoint.IsFiber);
            while (y <= yEnd)
            {
                int newX = fiberPoint.GetOffsetX(oldX, oldY, x, y, ref LayerMap);
                if (newX != 0)
                {
                    if (oldX == 0 && oldY == 0)
                    {
                        pointList.Add(new Point(newX, y));
                    }
                    else
                    {
                        Point p = fiberPoint.CheckLine(new Point(newX, y), new Point(oldX, oldY), ref LayerMap);
                        if (!p.Equals(new Point(oldX, oldY)))
                        {
                            pointList.Add(new Point(0, 0));
                            pointList.Add(new Point(newX, y));
                        }
                        else
                            pointList.Add(new Point(newX, y));
                    }
                    oldX = newX;
                    oldY = y;

                }
                else
                {
                    pointList.Add(new Point(0, 0));
                    oldX = 0;
                    oldY = 0;
                }
                y += FiberStep;
            }

            return pointList;
        }

        public void setTypeContours()
        {
            foreach (Contour c in Contours)
                c.isInternal = false;
            for (int i = 0; i < Contours.Count; i++)
            {
                for (int j = 0; j < Contours.Count; j++)
                {
                    if (Contours[i].TakeOver(Contours[j]))
                    {
                        Contours[j].isInternal = !Contours[j].isInternal;
                    }
                }
            }
        }

        public void LayFibers1(int fiberR, int fiberStep, int offsetR, int step)
        {
            List<int> borderF = new List<int>()
            {
                (int)LayerPoint.IsCountour,
                (int)LayerPoint.IsModel,
                (int)LayerPoint.IsEmpty,
                (int)LayerPoint.IsTempEmpty,
                (int)LayerPoint.IsFiber
            };

            List<int> borderW = new List<int>()
            {
                //(int)LayerPoint.IsCountour,
                //(int)LayerPoint.IsModel,
                (int)LayerPoint.IsNormal,
                (int)LayerPoint.IsEmpty
            };

            FiberPoint fiberPoint = new FiberPoint(fiberR, offsetR, borderF, borderW, (int)LayerPoint.IsFiber);
            // todo: чиста для моего случая, в остальных надо по каждому внешнему контуру пройтись
            Point strartOutContour = GetPointToLay(new Point(0, 0));
            int oldX = 0, oldY = 0;

            if (!strartOutContour.IsEmpty)
            {
                int x0 = strartOutContour.X + step;
                int y0 = strartOutContour.Y + fiberStep;
                int x = x0;
                int y = y0;
                while (x < LayerMap.Width - 1)
                {
                    List<Point> pointList = new List<Point>();
                    while (y < LayerMap.Height)
                    {
                        int newX = fiberPoint.GetOffsetX(oldX, oldY, x, y, ref LayerMap);
                        if (newX != 0)
                        {
                            if (oldX == 0 && oldY == 0)
                            {
                                pointList.Add(new Point(newX, y));
                            }
                            else
                            {
                                Point p = fiberPoint.CheckLine(new Point(newX, y), new Point(oldX, oldY), ref LayerMap);
                                if (!p.Equals(new Point(oldX, oldY)))
                                {
                                    pointList.Add(new Point(0, 0));
                                    pointList.Add(new Point(newX, y));
                                }
                                else
                                    pointList.Add(new Point(newX, y));
                            }
                            oldX = newX;
                            oldY = y;

                        }
                        else
                        {
                            pointList.Add(new Point(0, 0));
                            oldX = 0;
                            oldY = 0;
                        }
                        y += fiberStep;
                    }
                    x += step;
                    y = y0;
                    DrawFiber(pointList);

                    //test
                    //foreach (Point p in pointList)
                    //    LayerMap.SetPixel(p.X, p.Y, Color.FromArgb((int)LayerPoint.IsFiber | (0xFF << 24)));
                    string path = @"C:\Users\Mikhail\Desktop\Магистратура\ДИССЕРТАЦИЯ\Алгоритмы\bmpParser\temp\";
                    string fileName = $"t1_{x}.bmp";
                    LayerMap.Save(path + fileName);
                    //test
                }

            }
        }

        private bool CheckIsInside(int x, int y)
        {
            int tempX = x - 1;
            int pointType = LayerMap.GetPixel(tempX, y).ToArgb() & 0x00FFFFFF;
            while (pointType == (int)LayerPoint.IsCountour ||
                   pointType == (int)LayerPoint.IsModel ||
                   pointType == (int)LayerPoint.IsNormal)
            {
                tempX--;
                pointType = LayerMap.GetPixel(tempX, y).ToArgb() & 0x00FFFFFF;
            }
            if (pointType == (int)LayerPoint.IsTempEmpty)
                return true;
            return false;
        }

        public void FillContours()
        {
            Color emptyTempColor = Color.FromArgb((int)LayerPoint.IsTempEmpty | (0xFF << 24));
            Color emptyColor = Color.FromArgb((int)LayerPoint.IsEmpty | (0xFF << 24));
            Color insideColor = Color.FromArgb((int)LayerPoint.IsInside | (0xFF << 24));
            Color CrapColor = Color.FromArgb((int)LayerPoint.IsNormal | (0xFF << 24));
            
            FloodFill(LayerMap, new Point(0, 0), CrapColor);
            FloodFill(LayerMap, new Point(0, 0), emptyTempColor);

            for (int y = 0; y < LayerMap.Height; y++)
            {
                for (int x = 0; x < LayerMap.Width; x++)
                {
                    if (LayerMap.GetPixel(x, y) != emptyColor) continue;
                    bool isInsideModel = CheckIsInside(x, y);
                    if (isInsideModel)
                        FloodFill(LayerMap, new Point(x, y), insideColor);
                    else
                        while (LayerMap.GetPixel(x, y) == emptyColor)
                            x++;
                }
            }
            //           FloodFill(LayerMap, new Point(0, 0), emptyColor);
        }

        private void FloodFill(Bitmap bitmap, Point startPoint, Color fillColor)
        {
            Color oldColor = bitmap.GetPixel(startPoint.X, startPoint.Y);
            if (oldColor == fillColor) return;

            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(startPoint);

            while (queue.Count > 0)
            {
                Point point = queue.Dequeue();
                int x = point.X;
                int y = point.Y;

                if (x < 0 || x >= bitmap.Width || y < 0 || y >= bitmap.Height) continue;
                if (bitmap.GetPixel(x, y) != oldColor) continue;

                int leftX = x;
                int rightX = x;

                while (leftX > 0 && bitmap.GetPixel(leftX - 1, y) == oldColor)
                {
                    leftX--;
                }

                while (rightX < bitmap.Width - 1 && bitmap.GetPixel(rightX + 1, y) == oldColor)
                {
                    rightX++;
                }

                for (int i = leftX; i <= rightX; i++)
                {
                    bitmap.SetPixel(i, y, fillColor);

                    if (y > 0 && bitmap.GetPixel(i, y - 1) == oldColor)
                    {
                        queue.Enqueue(new Point(i, y - 1));
                    }

                    if (y < bitmap.Height - 1 && bitmap.GetPixel(i, y + 1) == oldColor)
                    {
                        queue.Enqueue(new Point(i, y + 1));
                    }
                }
            }
        }


        public void CloseContour(Contour contour)
        {
            int size = contour.Points.Count;
            Color color = Color.FromArgb((int)LayerPoint.IsCountour | (0xFF << 24));
            for (int i = 0; i < size - 1; i++)
            {
                List<Point> pointlist = LineTo(contour.Points[i], contour.Points[i + 1]);
                pointlist.RemoveAt(0);
                pointlist.RemoveAt(pointlist.Count - 1);
                DrawLine(pointlist, color);
            }
        }

        private void DrawLine(List<Point> points, Color color)
        {
            foreach (Point point in points)
                LayerMap.SetPixel(point.X, point.Y, color);

        }

        private List<Point> LineTo(Point p1, Point p2)
        {
            // Bresenham's line algorithm
            List<Point> pointList = new List<Point>();
            int dx = Math.Abs(p2.X - p1.X);
            int dy = Math.Abs(p2.Y - p1.Y);
            int sx = (p1.X < p2.X) ? 1 : -1;
            int sy = (p1.Y < p2.Y) ? 1 : -1;

            int error = dx - dy;
            int x = p1.X;
            int y = p1.Y;

            while (true)
            {
                pointList.Add(new Point(x, y));
                if (x == p2.X && y == p2.Y) break;

                int doubleError = 2 * error;

                if (doubleError > -dy)
                {
                    error -= dy;
                    x += sx;
                }

                if (doubleError < dx)
                {
                    error += dx;
                    y += sy;
                }
            }

            return pointList;
        }

        public List<int> FindLines(float limitAngle)
        {
            List<int> linesCount = new List<int>();

            foreach (Contour contour in Contours)
            {
                linesCount.Add(contour.FindLines(limitAngle));
            }

            return linesCount;
        }

        public bool FindContours()
        {
            List<Point> modelPoints = GetModelPoints();
            bool isGood = true;
            int numberContour = 0;
            if (modelPoints.Count == 0)
                return false;

            while (isGood)
            {
                if (modelPoints.Count == 0)
                    return true;
                Contours.Add(FindContour(ref modelPoints));
                if (Contours[numberContour].IsEmptyPointList())
                    return false;
                numberContour++;
            }
            return false;
        }

        private Contour FindContour(ref List<Point> modelPoints)
        {// TODO: доработать, а то могут быть ошибки
            bool isGood = true;
            Point startPoint = modelPoints[0];
            Point currentPoint = startPoint;
            Contour contour = new Contour();
            contour.AddPoint(currentPoint);
            modelPoints.RemoveAt(0);
            while (isGood)
            {
                List<Point> neighbours = FindNeighbours(modelPoints, currentPoint);
                currentPoint = FindNearSurfacePoint(neighbours, currentPoint);
                if (currentPoint.IsEmpty)
                {
                    contour.AddPoint(startPoint);
                    return contour;
                }
                else
                {
                    contour.AddPoint(currentPoint);
                    isGood = modelPoints.Remove(currentPoint);
                }
            }
            contour.ClearPoints();
            return contour;
        }

        private List<Point> GetModelPoints()
        {
            List<Point> modelPoints = new List<Point>();

            for (int x = 0; x < LayerMap.Width; x++)
            {
                for (int y = 0; y < LayerMap.Height; y++)
                {
                    int color = LayerMap.GetPixel(x, y).ToArgb() & 0x00FFFFFF;
                    if (color == (int)LayerPoint.IsModel)
                        modelPoints.Add(new Point(x, y));
                }
            }

            return modelPoints;
        }

        private List<Point> FindNeighbours(List<Point> modelPoints, Point currentPoint)
        {
            List<Point> neighbours = new List<Point>();

            foreach (Point point in modelPoints)
            {
                if (GetRange(currentPoint, point) <= ScanRange)
                    neighbours.Add(point);
            }

            return neighbours;
        }

        private Point FindNearSurfacePoint(List<Point> neighbours, Point currentPoint)
        {
            bool isFirstIteration = true;
            Point neighbourPoint = new Point();

            foreach (Point point in neighbours)
            {
                float angle = GetNormalAngle(point) - GetNormalAngle(currentPoint);
                bool isGoodSurface = angle < Math.PI;

                if (isGoodSurface)
                {
                    if (isFirstIteration)
                    {
                        neighbourPoint = point;
                        isFirstIteration = false;
                        continue;
                    }
                    if (GetRange(point, currentPoint) < GetRange(neighbourPoint, currentPoint))
                        neighbourPoint = point;
                }
            }
            return neighbourPoint;
        }

        private float GetNormalAngle(Point modelPoint)
        {
            List<Point> normalPoints = GetNormalsPoints(modelPoint);
            Point resaultVector = new Point(0, 0);

            foreach (Point point in normalPoints)
            {
                resaultVector.X = (resaultVector.X + point.X) % 2;
                resaultVector.Y = (resaultVector.Y + point.Y) % 2;
            }

            if (resaultVector.X == 0) return (float)Math.PI / 2;

            return Convert.ToSingle(Math.Atan2(resaultVector.Y, resaultVector.X));
        }

        private List<Point> GetNormalsPoints(Point modelPoint)
        {
            List<Point> normalPoints = new List<Point>();
            for (int x = modelPoint.X - 1; x <= modelPoint.X + 1; x++)
            {
                for (int y = modelPoint.Y - 1; y <= modelPoint.Y + 1; y++)
                {
                    int pixel = LayerMap.GetPixel(x, y).ToArgb();
                    if (pixel == (int)LayerPoint.IsNormal)
                        normalPoints.Add(new Point(x - modelPoint.X, y - modelPoint.Y));
                }
            }

            return normalPoints;
        }

        private float GetRange(Point point1, Point point2)
        {
            float range = Convert.ToSingle(Math.Pow(point2.X - point1.X, 2) +
                    Math.Pow(point2.Y - point1.Y, 2));

            return (float)Math.Sqrt(range);

        }

    }
}
