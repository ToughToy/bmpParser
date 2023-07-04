using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;


namespace bmpParser
{
    public struct Line
    {
        public int BeginPosition;
        public int EndPosition;
        public byte Priority;
    }

    public class Contour
    {
        public Contour()
        {
            Points = new List<Point>();
            Lines = new List<Line>();
        }
        public List<Point> Points;
        public List<Line> Lines;
        public bool isInternal;

        public bool TakeOver(Contour contour)
        {
            foreach(Point p in contour.Points)
            {
                if (!Contains(p))
                    return false;
            }

            return true;
        }

        public void GetBoundingRect(out Point p1, out Point p2)
        {
            p1 = new Point(0, 0);
            p2 = new Point(0, 0);
            int xMin = 0, xMax = 0, yMin = 0, yMax = 0;
            xMax = Points.Max(p => p.X);
            xMin = Points.Min(p => p.X);
            yMax = Points.Max(p => p.Y);
            xMin = Points.Min(p => p.Y);

            p1.X = xMin;
            p1.Y = yMin;
            p2.X = xMax;
            p2.Y = yMax;
        }

        public bool Contains(Point point)
        {
            int intersections = 0;
            int vertexCount = Points.Count;

            for (int i = 0; i < vertexCount; i++)
            {
                Point a = Points[i];
                Point b = Points[(i + 1) % vertexCount];

                if (IsIntersection(point, a, b))
                {
                    intersections++;
                }
            }

            return intersections % 2 != 0;
        }

        private bool IsIntersection(Point point, Point a, Point b)
        {
            if (a.Y > b.Y)
            {
                Point temp = a;
                a = b;
                b = temp;
            }

            if (point.Y == a.Y || point.Y == b.Y)
            {
                point.Y += 1;
            }

            if (point.Y < a.Y || point.Y > b.Y || point.X > Math.Max(a.X, b.X))
            {
                return false;
            }

            if (point.X < Math.Min(a.X, b.X))
            {
                return true;
            }

            long area = (long)(b.X - a.X) * (point.Y - a.Y) - (long)(b.Y - a.Y) * (point.X - a.X);
            return area > 0;
        }

        public int FindLines(float limitAngle)
        {
            List<Point> window = new List<Point>();
            int i = 0;
            Line line = new Line();
            foreach (Point p in Points)
            {
                if (window.Count < 2)
                {
                    window.Add(p);
                    if (window.Count == 1) line.BeginPosition = i;
                }
                else
                {
                    PointF currentPoint = window[window.Count - 1];
                    PointF predictPoint = GetPredict(window);
                    PointF vector1 = new PointF(), vector2 = new PointF();

                    vector1.X = p.X - currentPoint.X;
                    vector1.Y = p.Y - currentPoint.Y;

                    vector2.X = predictPoint.X - currentPoint.X;
                    vector2.Y = predictPoint.Y - currentPoint.Y;

                    float numenator = vector1.X * vector2.X + vector1.Y * vector2.Y;
                    float lenVector1 = (float)(Math.Pow(vector1.X, 2) + Math.Pow(vector1.Y, 2));
                    float lenVector2 = (float)(Math.Pow(vector2.X, 2) + Math.Pow(vector2.Y, 2));
                    float denumenator = (float)(Math.Sqrt(lenVector1) * Math.Sqrt(lenVector2));

                    float angle = Convert.ToSingle(Math.Acos(numenator / denumenator));
                    if (Math.Abs(angle) >= limitAngle)
                    {
                        line.EndPosition = i - 1;
                        Lines.Add(line);
                        window.Clear();
                        window.Add(p);
                        line.BeginPosition = i;
                    }
                    else
                    {
                        window.Add(p);
                        window.RemoveAt(0);
                    }
                }
                i++;
            }
            line.EndPosition = i - 1;
            Lines.Add(line);

            return Lines.Count;
        }

        private PointF GetPredict(List<Point> window)
        {
            PointF predictPoint = new PointF(0, 0);
            Point lastPoint = window[window.Count - 1];

            for (int i = 1; i < window.Count; i++)
            {
                predictPoint.X += window[i].X - window[i - 1].X;
                predictPoint.Y += window[i].Y - window[i - 1].Y;
            }
            predictPoint.X /= window.Count;
            predictPoint.Y /= window.Count;

            predictPoint.X += lastPoint.X;
            predictPoint.Y += lastPoint.Y;

            return predictPoint;
        }

        public void AddPoint(Point point)
        {
            Points.Add(point);
        }

        public void ClearPoints()
        {
            Points.Clear();
        }

        public bool IsEmptyPointList()
        {
            return Points.Count == 0;
        }
    }
}
