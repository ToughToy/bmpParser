using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace bmpParser
{
    class FiberPoint
    {
        private int FiberCode;
        private int FiberR;
        private int WarningR;
        private List<int> BordersF;
        private List<int> BordersW;
        public FiberPoint(int fiberR, int warningR, List<int> bordersF, List<int> bordersW, int fiberCode)
        {
            this.FiberR = fiberR;
            this.WarningR = warningR;
            this.BordersF = bordersF;
            this.BordersW = bordersW;
            this.FiberCode = fiberCode;
        }

        public int GetOffsetX(int oldX, int oldY, int x, int y, ref Bitmap map)
        {
            int resX;
            if (CheckCell(x, y, FiberR, ref map, BordersF))
            {
                int offset = GetHorizontalOffset(x, y, WarningR, ref map, BordersW);
                if (offset == 0)
                    resX = x;
                else
                {
                    Point p = CheckLine(new Point(x, y), new Point(x + offset, y), ref map);
                    resX = p.X;
                }
            }
            else
            {
                int tempX = FindGoodX(x, y, FiberR, ref map, BordersF);
                if (tempX != 0)
                {
                    int offset = GetHorizontalOffset(tempX, y, WarningR, ref map, BordersW);
                    if (offset == 0)
                        resX = tempX;
                    else
                    {
                        Point p = CheckLine(new Point(tempX, y), new Point(tempX + offset, y), ref map);
                        resX = p.X;
                    }
                }
                else
                    resX = 0;
            }

            return resX;
            //if (resX != 0)
            //{
            //    if (oldX == 0 && oldY == 0) return resX;
            //    Point p = CheckLine(new Point(resX, y), new Point(oldX, oldY), ref map);
            //    if (!p.Equals(new Point(oldX, oldY)))
            //        return 0;
            //    else
            //        return resX;
            //}
            //else
            //    return 0;
        }

        // for fibers
        public Point CheckLine(Point p1, Point p2, ref Bitmap map)
        {
            List<Point> pointList = LineTo(p1, p2);
            foreach (Point p in pointList)
            {
                //if (p.equals(p1) continue; FiberR + 1 TODO:
                if ((p.X == p1.X) && (p.Y == p1.Y)) continue;
                if (!CheckPerimetr(p.X, p.Y, FiberR, ref map, BordersF))
                //if (!CheckCell(p.X, p.Y, FiberR + 1, ref map, BordersF))
                //if (!CheckCell(p.X, p.Y, FiberR, ref map, BordersF))
                {
                    int index = pointList.FindIndex(pL => (pL.X == p.X) && (pL.Y == p.Y));
                    //if (index - 1 < 0)
                    //    return new Point(0,0);
                    return pointList[index - 1];
                }
            }

            return p2;
        }

        private bool CheckCell(int x, int y, int r, ref Bitmap map, List<int> borders)
        {
            for (int i = y - r; i <= y + r; i++)
            {
                if (i < 0 || i > map.Height - 1) continue;
                for (int j = x - r; j <= x + r; j++)
                {
                    if (j < 0 || j > map.Width - 1) continue;
                    int px = map.GetPixel(j, i).ToArgb() & 0x00FFFFFF;
                    if (borders.Contains(px)) return false;
                }
            }

            return true;
        }

        private bool CheckPerimetr(int x, int y, int r, ref Bitmap map, List<int> borders)
        {
            int perimetrLen = (2 * r + 1) * (2 * r + 1) - 1;
            int i = 0;
            string dir = "right";
            int x0 = x - r;
            int y0 = y - r;
            int currentX = x0;
            int currentY = y0;
            while (i++ <= perimetrLen)
            {
                if ((currentX >= 0 && currentX <= map.Width - 1) &&
                    (currentY >= 0 && currentY <= map.Height - 1))
                {
                    int px = map.GetPixel(currentX, currentY).ToArgb() & 0x00FFFFFF;
                    if (borders.Contains(px)) return false;
                }

                switch (dir)
                {
                    case "right":
                        currentX++;
                        if (currentX > x0 + 2 * r)
                        {
                            dir = "down";
                            currentX--;
                        }
                        break;

                    case "down":
                        currentY++;
                        if (currentY > y0 + 2 * r)
                        {
                            dir = "left";
                            currentY--;
                        }
                        break;

                    case "left":
                        currentX--;
                        if (currentX < x0)
                        {
                            dir = "up";
                            currentX++;
                        }
                        break;

                    case "up":
                        currentY--;
                        if (currentY < y0)
                        {
                            //dir = "right";
                            currentY++;
                        }
                        break;
                }
            }

            return true;
        }

        private int Resultoffset(int x, int r, int xL, int xR, int dirL, int dirR)
        {
            if (dirL == dirR && dirL == 0) return 0;
            //dir == 1 -- right; dir == -1 -- left
            if (dirL == dirR && dirL == 1)
                return xR - (x - r);
            else if (dirL == dirR && dirL == -1)
                return ((x + r) - xL) * (-1);
            else
            {
                if (dirL == 1)  // dirR = -1
                {
                    int halfRange = (xR - xL) / 2;
                    if (xR - x > x - xL)
                        return halfRange;
                    else
                        return halfRange * (-1);
                }
                else            // dirR = 1
                {
                    if (x - xL > xR - x) //?
                    {
                        //return xR - (x - r);
                        int halfRange = (xR - xL) / 2;
                        if (xR - x > x - xL)
                            return halfRange;
                        else
                            return halfRange * (-1);
                    }
                    else
                    {
                        return ((x + r) - xL) * (-1);
                        //int halfRange = (xR - xL) / 2; bad
                        //if (xR - x > x - xL)
                        //    return halfRange;
                        //else
                        //    return halfRange * (-1);
                    }
                }
            }
        }
        // for WarningR
        private int GetHorizontalOffset(int x, int y, int r, ref Bitmap map, List<int> borders)
        {
            int xL = x + r, xR = x - r;
            int dirL = 0, dirR = 0;
            for (int str = y - r; str <= y + r; str++)
            {
                int tempXL, tempXR;
                if (str <= 0 || str > map.Height - 1) continue;

                // идем слева-направо
                for (int col = x - r; col <= x + r - 1; col++)
                {
                    if (col <= 0 || col > map.Width - 2) continue;
                    //if ((col + 1) < 0 || (col + 1) > map.Width - 1) continue;
                    int px1 = map.GetPixel(col, str).ToArgb() & 0x00FFFFFF;
                    int px2 = map.GetPixel(col + 1, str).ToArgb() & 0x00FFFFFF;
                    if (px1 != px2)
                    {
                        bool isBorder1 = borders.Contains(px1);
                        bool isBorder2 = borders.Contains(px2);
                        if (isBorder1 != isBorder2)
                        {
                            tempXL = isBorder1 ? col + 1 : col;
                            if (tempXL < xL)
                            {
                                xL = tempXL;
                                dirL = isBorder1 ? (1) : (-1);
                            }
                        }
                    }
                }

                // идем справа-налево
                for (int col = x + r; col >= x - r + 1; col--)
                {
                    if (col <= 0 || col > map.Width - 2) continue;
                    //if ((col - 1) < 0 || (col - 1) > map.Width - 2) continue;
                    int px1 = map.GetPixel(col, str).ToArgb() & 0x00FFFFFF;
                    int px2 = map.GetPixel(col - 1, str).ToArgb() & 0x00FFFFFF;
                    if (px1 != px2)
                    {
                        bool isBorder1 = borders.Contains(px1);
                        bool isBorder2 = borders.Contains(px2);
                        if (isBorder1 != isBorder2)
                        {
                            tempXR = isBorder1 ? col - 1 : col;
                            if (tempXR > xR)
                            {
                                xR = tempXR;
                                dirR = isBorder1 ? (-1) : (1);
                            }
                        }
                    }
                }
            }

            return Resultoffset(x, r, xL, xR, dirL, dirR);
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

        // for FiberR
        private int FindGoodX(int x, int y, int r, ref Bitmap map, List<int> borders)
        {
            int xL = x;
            int xR = x;
            int xLF = 0, xRF = 0;
            bool lFlag = true, rFlag = true;
            while (true)
            {
                if (xL > 0)
                {
                    if (CheckCell(xL, y, r, ref map, borders))
                    {
                        break;
                    }
                    if (((map.GetPixel(xL, y).ToArgb() & 0x00FFFFFF) == FiberCode) && lFlag)
                    {
                        xLF = xL;
                        lFlag = false;
                    }
                    xL--;
                }

                if (xR < map.Width - 1)
                {
                    if (CheckCell(xR, y, r, ref map, borders))
                    {
                        break;
                    }
                    if (((map.GetPixel(xR, y).ToArgb() & 0x00FFFFFF) == FiberCode) && rFlag)
                    {
                        xRF = xR;
                        rFlag = false;
                    }
                    xR++;
                }

                if (xL <= 0 && xR >= map.Width - 1) return 0;
            }

            if ((xLF - xL > 0) && xL > 0)
            {
                if (x - xLF < xR - x)
                    return 0;
            }
            if ((xR - xRF > 0) && xRF > 0)
            {
                if (xRF - x < x - xL)
                    return 0;
            }


            if ((x - xL > xR - x) && (xR != map.Width - 1))
                return xR;
            else if ((x - xL <= xR - x) && (xL > 0))
                return xL;
            else
                return 0;
        }
            private int FindGoodX1(int x, int y, int r, ref Bitmap map, List<int> borders)
        {
            int xL = x;
            int xR = x;
            int xFL = 0;
            int xFR = 0;
            // to right
            while (xR < map.Width)
            {
                int px = map.GetPixel(xR, y).ToArgb() & 0x00FFFFFF;
                if (px == (int)LayerPoint.IsFiber) // crap?
                {
                    xFR = xR;
                    break;
                }
                if (CheckCell(xR, y, r, ref map, borders)) break;
                xR++;
            }
            // to left
            while (xL > 0)
            {
                int px = map.GetPixel(xL, y).ToArgb() & 0x00FFFFFF;
                if (px == (int)LayerPoint.IsFiber) // crap?
                {
                    xFL = xL;
                    break;
                }
                if (CheckCell(xL, y, r, ref map, borders)) break;
                xL--;
            }

            if (xFR > 0 && xFL > 0) // crap?
                return 0;
            else if (Math.Abs(x - xFL) < Math.Abs(xR - x) && xFL != 0)
                return 0;
            else if (Math.Abs(xFR - x) < Math.Abs(x - xL) && xFR != 0)
                return 0;

            if (xL > 0 && xR < map.Width)
            {
                int dxR = xR - x;
                int dxL = x - xL;
                return (dxR < dxL) ? xR : xL;
            }
            else if (xL > 0)
                return xL;
            else if (xR < map.Width)
                return xR;

            return 0;
        }
    }
}
