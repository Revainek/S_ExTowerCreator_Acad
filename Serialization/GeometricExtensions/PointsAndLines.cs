using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace S_ExTowerCreator_Acad.Serialization.GeometricExtensions
{
    [Serializable]
    public class ExtPoint2D
    {
        private double x;
        private double y;

        public ExtPoint2D()
        {

        }
        public ExtPoint2D(double _X, double _Y)
        {
            X = _X;
            Y = _Y;
        }
        public ExtPoint2D(Point3d p)
        {
            //X = Math.Round(p.X, 5);
            //Y = Math.Round(p.Y, 5);
            X = p.X;
            Y = p.Y;
        }
        public ExtPoint2D(Point2d p)
        {
            //X = Math.Round(p.X, 5);
            //Y = Math.Round(p.Y, 5);
            X = p.X;
            Y = p.Y;
        }
        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
        public static double GetDistanceBetweenPoints(ExtPoint2D p1, ExtPoint2D p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }
    [Serializable, XmlInclude(typeof(ExtPoint2D))]
    public class PLine2D
    {
        private ExtPoint2D p1;
        private ExtPoint2D p2;

        public PLine2D()
        {

        }
        public PLine2D(ExtPoint2D _p1, ExtPoint2D _p2)
        {
            p1 = _p1;
            p2 = _p2;
        }
        public PLine2D(Point3d Start, Point3d End)
        {
            //p1 = new ExtPoint2D(Math.Round(Start.X, 6), Math.Round(Start.Y, 6));
            //p2 = new ExtPoint2D(Math.Round(End.X, 6), Math.Round(End.Y, 6));
            p1 = new ExtPoint2D(Start.X, Start.Y);
            p2 = new ExtPoint2D(End.X, End.Y);

        }
        public PLine2D(Point2d Start, Point2d End)
        {
            //p1 = new ExtPoint2D(Math.Round(Start.X, 6), Math.Round(Start.Y, 6));
            //p2 = new ExtPoint2D(Math.Round(End.X, 6), Math.Round(End.Y, 6));
            p1 = new ExtPoint2D(Start.X, Start.Y);
            p2 = new ExtPoint2D(End.X, End.Y);
        }
        public ExtPoint2D P1 { get => p1; set => p1 = value; }
        public ExtPoint2D P2 { get => p2; set => p2 = value; }
        public ExtPoint2D FindMiddlePoint()
        {
            return new ExtPoint2D((p2.X + p1.X) / 2, (p2.Y + p1.Y) / 2);
        }

        public static void SaveXML(string name, List<PLine2D> WallList)
        {
            try
            {
                using (Stream streamWrite = File.Create(name))
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(List<PLine2D>));
                    formatter.Serialize(streamWrite, WallList);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static List<PLine2D> LoadXMLFromPath(string FullFilePath)
        {

            List<PLine2D> SettingConfig = new List<PLine2D>();
            if (File.Exists(FullFilePath))
            {
                XmlSerializer x = new XmlSerializer(typeof(List<PLine2D>));
                FileStream z = new FileStream(FullFilePath, FileMode.Open, FileAccess.Read);
                object w = x.Deserialize(z);
                SettingConfig = w as List<PLine2D>;
                z.Dispose();
            }

            return SettingConfig;
        }
    }
    internal class PLineEquation
    {
        public double a { get; set; }
        public double b { get; set; }
        public double c { get; set; }
        public PLineEquation()
        {

        }
        public PLineEquation(double A, double B, double C)
        {
            a = A;
            b = B;
            c = C;
        }
        public bool isPointOnLine(ExtPoint2D point)
        {
            bool returnvalue = false;
            if (c==1)
            {
                if (Math.Round(point.Y, 4) == Math.Round((a * point.X + b), 4))
                {
                    returnvalue = true;
                }
            }
            else
            {
                if (Math.Round(point.X, 4) == Math.Round(b, 4))
                {
                    returnvalue = true;
                }
            }
           

            return returnvalue;
        }
        public static PLineEquation GetEquationFrom2Points(ExtPoint2D p1, ExtPoint2D p2)
        {
            PLineEquation newEquation = new PLineEquation();
            if ((Math.Round(p1.Y, 4) == 0.00 && Math.Round(p2.Y, 4) == 0.00) || Math.Round(p1.Y - p2.Y, 4) == 0.00)
            {
                newEquation.a = 0;
                newEquation.b = p1.Y;
                newEquation.c = 1;
            }
            else if ((Math.Round(p1.X, 4) == 0.00 && Math.Round(p2.X, 4) == 0.00) || Math.Round(p1.X - p2.X, 4) == 0.00)
            {
                newEquation.a = 1;
                newEquation.b = p1.X;
                newEquation.c = 0;
            }
            else
            {
                newEquation.a = ((p1.Y - p2.Y) / (p1.X - p2.X));
                newEquation.b = p1.Y - newEquation.a * p1.X;
                newEquation.c = 1;
            }


            return newEquation;
        }
        public static PLineEquation GetEquationFromLine(PLine2D line)
        {
            PLineEquation newEquation = GetEquationFrom2Points(line.P1, line.P2);
            return newEquation;
        }
        public static PLineEquation GetEquationOfTriangleHeight(ExtPoint2D topPoint, ExtPoint2D p1Base, ExtPoint2D p2Base)
        {
            return PLineEquation.GetEquationFromPointAndPerpendicularLine(PLineEquation.GetEquationFrom2Points(p1Base, p2Base), topPoint);
        }
        public bool AreLinesParallel(PLineEquation l1, PLineEquation l2)
        {
            return false;
        }
        public static PLineEquation GetEquationFromPointAndPerpendicularLine(PLineEquation perpendicular, ExtPoint2D p)
        {
            PLineEquation newEquation = new PLineEquation();
            if (perpendicular.a == 0.0)
            {
                newEquation.a = 0;
                newEquation.b = p.X;
                newEquation.c = 0;
            }
            else if (perpendicular.c == 0)
            {
                newEquation.a = 0;
                newEquation.b = p.Y;
                newEquation.c = 1;
            }
            else
            {
                newEquation.a = (-1) / perpendicular.a;
                newEquation.b = p.Y - newEquation.a * p.X;
                newEquation.c = 1;
            }


            return newEquation;
        }
        public bool AreLinesPerpendicular(PLineEquation l1, PLineEquation l2)
        {
            return (Math.Round(l1.a, 5) == (-1) * Math.Round(l2.a, 4));
        }
    }
}
