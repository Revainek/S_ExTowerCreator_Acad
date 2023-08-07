using Autodesk.AutoCAD.Geometry;
using S_ExTowerCreator_Acad.Models;
using S_ExTowerCreator_Acad.Serialization.GeometricExtensions;
using S_ExTowerCreator_Acad.Serialization.WallEndings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace S_ExTowerCreator_Acad.Serialization
{
    [Serializable, XmlInclude(typeof(ExtPoint2D))]
    public class PWallInterpreter
    {
        public List<ExtPoint2D> WallGeometry { get => wallGeometry; set => wallGeometry = value; }
        public List<ObjectTriple<int, ExtPoint2D, ExtPoint2D>> WallEnds { get => wallEnds; set => wallEnds = value; }
       
        private List<ExtPoint2D> wallGeometry;
        private List<ObjectTriple<int, ExtPoint2D, ExtPoint2D>> wallEnds;

        public PWallInterpreter()
        {

        }

        public static void SaveXML(string name, PWallInterpreter WallList)
        {
            try
            {
                using (Stream streamWrite = File.Create(name))
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(PWallInterpreter));
                    formatter.Serialize(streamWrite, WallList);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static PWallInterpreter LoadXMLFromPath(string FullFilePath)
        {

            PWallInterpreter SettingConfig = new PWallInterpreter();
            if (File.Exists(FullFilePath))
            {
                XmlSerializer x = new XmlSerializer(typeof(PWallInterpreter));
                FileStream z = new FileStream(FullFilePath, FileMode.Open, FileAccess.Read);
                object w = x.Deserialize(z);
                SettingConfig = w as PWallInterpreter;
                z.Dispose();
            }

            return SettingConfig;
        }

        internal void GetGeometryAnalysed(List<PLine2D> outputLines, List<ExistingWallBox> existingWalls, List<ExistingTriangle> existingTriangles)
        {
            WallGeometry = new List<ExtPoint2D>();
            WallEnds = new List<ObjectTriple<int, ExtPoint2D, ExtPoint2D>>();
            foreach (PLine2D line in outputLines)
            {
                WallGeometry.Add(new ExtPoint2D(line.P1.X, line.P1.Y));
            }
            foreach  (ExistingTriangle triangle in existingTriangles)
            {
                ObjectTriple<int, ExtPoint2D, ExtPoint2D> end = GetWallEndFromTriangle(outputLines, triangle);
                if (end.Third!=null && end.Second!=null)
                WallEnds.Add(end);
            }
            foreach  (ExistingWallBox box in existingWalls)
            {
                ObjectTriple<int, ExtPoint2D, ExtPoint2D> end = GetWallFromWallBox(outputLines, box);
                if (end.Third != null && end.Second != null)
                    WallEnds.Add(end);
            }
            double minPointX = WallGeometry.Select(x => x.X).Min();
            double minPointY = WallGeometry.Select(x => x.Y).Min();
           for (int i = 0; i<WallGeometry.Count; i++)
            {
                WallGeometry[i] = new ExtPoint2D(WallGeometry[i].X - minPointX, WallGeometry[i].Y - minPointY);
            }
            for (int i = 0; i < WallEnds.Count; i++)
            {
                WallEnds[i].Second = new ExtPoint2D(WallEnds[i].Second.X - minPointX, WallEnds[i].Second.Y - minPointY);
                WallEnds[i].Third = new ExtPoint2D(WallEnds[i].Third.X - minPointX, WallEnds[i].Third.Y - minPointY);
            }
        }

        private ObjectTriple<int, ExtPoint2D, ExtPoint2D> GetWallFromWallBox(List<PLine2D> outputLines, ExistingWallBox box)
        {
            List<ObjectTriple<int, ExtPoint2D, ExtPoint2D>> PossiblewallEnd = new List<ObjectTriple<int, ExtPoint2D, ExtPoint2D>>();
            List<double> PossibleWallEndingDistances = new List<double>();

            ///LineEquations for triangle sides
            PLineEquation line1 = PLineEquation.GetEquationFrom2Points(box.P1, box.P2);
            PLineEquation line2 = PLineEquation.GetEquationFrom2Points(box.P2, box.P3);
            PLineEquation line3 = PLineEquation.GetEquationFrom2Points(box.P3, box.P4);
            PLineEquation line4 = PLineEquation.GetEquationFrom2Points(box.P4, box.P1);

            foreach (PLine2D line in outputLines)
            {
                PLineEquation wallEq = PLineEquation.GetEquationFromLine(line);
                if (Math.Round(line1.a,4)==Math.Round(wallEq.a,4) && Math.Round(line1.b, 4) == Math.Round(wallEq.b, 4) && Math.Round(line1.c, 4) == Math.Round(wallEq.c, 4))
                {
                    PossiblewallEnd.Add(new ObjectTriple<int, ExtPoint2D, ExtPoint2D>(3, line.P1, line.P2));
                }
                else if (Math.Round(line2.a, 4) == Math.Round(wallEq.a, 4) && Math.Round(line2.b, 4) == Math.Round(wallEq.b, 4) && Math.Round(line2.c, 4) == Math.Round(wallEq.c, 4))
                {
                    PossiblewallEnd.Add(new ObjectTriple<int, ExtPoint2D, ExtPoint2D>(3, line.P1, line.P2));
                }
                else if (Math.Round(line3.a, 4) == Math.Round(wallEq.a, 4) && Math.Round(line3.b, 4) == Math.Round(wallEq.b, 4) && Math.Round(line3.c, 4) == Math.Round(wallEq.c, 4))
                {
                    PossiblewallEnd.Add(new ObjectTriple<int, ExtPoint2D, ExtPoint2D>(3, line.P1, line.P2));
                }
                else if (Math.Round(line4.a, 4) == Math.Round(wallEq.a, 4) && Math.Round(line4.b, 4) == Math.Round(wallEq.b, 4) && Math.Round(line4.c, 4) == Math.Round(wallEq.c, 4))
                {
                    PossiblewallEnd.Add(new ObjectTriple<int, ExtPoint2D, ExtPoint2D>(3, line.P1, line.P2));
                }
            }
            ObjectTriple<int, ExtPoint2D, ExtPoint2D> finalending = new ObjectTriple<int, ExtPoint2D, ExtPoint2D>();
            for (int i=0;i<PossiblewallEnd.Count;i++)
            {
                ObjectTriple<int, ExtPoint2D, ExtPoint2D> possibleW = PossiblewallEnd[i];
                if (IsWallEndingWithinBoundariesOfBox(possibleW,box)==true)
                {
                    finalending = possibleW;
                    break;
                }
            }

            return finalending;
        }

        private bool IsWallEndingWithinBoundariesOfBox(ObjectTriple<int, ExtPoint2D, ExtPoint2D> possibleW, ExistingWallBox box)
        {
            bool p1 = false;
            bool p2 = false;
           if (IsPointWithinLineSegment(possibleW.Second,box.P1,box.P2))
            { p1 = true; }
           else if (IsPointWithinLineSegment(possibleW.Second, box.P2, box.P3))
            { p1 = true; }
            else if (IsPointWithinLineSegment(possibleW.Second, box.P3, box.P4))
            { p1 = true; }
            else if (IsPointWithinLineSegment(possibleW.Second, box.P4, box.P1))
            { p1 = true; }

            if (IsPointWithinLineSegment(possibleW.Third, box.P1, box.P2))
            { p2 = true; }
            else if (IsPointWithinLineSegment(possibleW.Third, box.P2, box.P3))
            { p2 = true; }
            else if (IsPointWithinLineSegment(possibleW.Third, box.P3, box.P4))
            { p2 = true; }
            else if (IsPointWithinLineSegment(possibleW.Third, box.P4, box.P1))
            { p2 = true; }

            if (p1 && p2)
            { return true; }
            else { return false; }
        }
        private bool IsPointWithinLineSegment(ExtPoint2D point, ExtPoint2D boundingPoint1, ExtPoint2D boundingPoint2)
        {

           double L1 =  ExtPoint2D.GetDistanceBetweenPoints(point, boundingPoint1);
            double L2 = ExtPoint2D.GetDistanceBetweenPoints(point, boundingPoint2);
            double L3 = ExtPoint2D.GetDistanceBetweenPoints(boundingPoint2, boundingPoint1);

            if (Math.Round(L1+L2,4)==Math.Round(L3,4))
            {
                return true;
            }
            //var crossProduct = (point.Y - boundingPoint1.Y) * (boundingPoint2.X - boundingPoint1.X) - (point.X - boundingPoint1.X) * (boundingPoint2.Y - boundingPoint1.Y);
            //var dotProduct = (point.X - boundingPoint1.X) * (boundingPoint2.X - boundingPoint1.X) + (point.Y - boundingPoint1.Y) * (boundingPoint2.Y - boundingPoint1.Y);
            //var squaredLengthba = (boundingPoint2.X - boundingPoint1.X) * (boundingPoint2.X - boundingPoint1.X) + (boundingPoint2.Y - boundingPoint1.Y) * (boundingPoint2.Y - boundingPoint1.Y);

            return false;
        }
        private ObjectTriple<int, ExtPoint2D, ExtPoint2D> GetWallEndFromTriangle(List<PLine2D> outputLines, ExistingTriangle triangle)
        {
            List<ObjectTriple<int, ExtPoint2D, ExtPoint2D>> PossiblewallEnd = new List<ObjectTriple<int, ExtPoint2D, ExtPoint2D>>();
            List<double> PossibleWallEndingDistances = new List<double>();
            try
            {
                ///LineEquations for triangle sides
                PLineEquation line1 = PLineEquation.GetEquationFrom2Points(triangle.P1, triangle.P2);
                PLineEquation line2 = PLineEquation.GetEquationFrom2Points(triangle.P2, triangle.P3);
                PLineEquation line3 = PLineEquation.GetEquationFrom2Points(triangle.P3, triangle.P1);
                /// LineEquations for TriangleHeights
                PLineEquation H1 = PLineEquation.GetEquationFromPointAndPerpendicularLine(line1, triangle.P3);
                PLineEquation H2= PLineEquation.GetEquationFromPointAndPerpendicularLine(line2, triangle.P1);
                PLineEquation H3 = PLineEquation.GetEquationFromPointAndPerpendicularLine(line3, triangle.P2);

             
                foreach (PLine2D line in outputLines)
                {
                    var EdgeCenter = line.FindMiddlePoint();
                    double value1, value2, value3;
                    if (H1.isPointOnLine(EdgeCenter)==true)
                    {
                      value1 =   ExtPoint2D.GetDistanceBetweenPoints(triangle.P3, EdgeCenter);
                    }
                    else
                    {
                        value1 = 999.0;
                    }
                    if (H2.isPointOnLine(EdgeCenter) == true)
                    {
                        value2 = ExtPoint2D.GetDistanceBetweenPoints(triangle.P1, EdgeCenter);
                    }
                    else
                    {
                        value2 = 999.0;
                    }
                    if (H3.isPointOnLine(EdgeCenter) == true)
                    {
                        value3 = ExtPoint2D.GetDistanceBetweenPoints(triangle.P2, EdgeCenter);
                    }
                    else
                    {
                        value3  = 999.0;
                    }

                    IsThereAResult(value1, value2, value3, out int Index);
                    if (Index!=0)
                    {
                       switch (Index)
                        {
                            case 1:
                                PossibleWallEndingDistances.Add(value1);
                                break;
                            case 2:
                                PossibleWallEndingDistances.Add(value2);
                                break;
                            case 3:
                                PossibleWallEndingDistances.Add(value3);
                                break;
                        }
                        PossiblewallEnd.Add(new ObjectTriple<int, ExtPoint2D, ExtPoint2D>(4, line.P1, line.P2));

                    }
                }
            }
            catch (Exception)
            { }

            if (PossibleWallEndingDistances.Count>0)
            {
                return PossiblewallEnd[PossibleWallEndingDistances.FindIndex(x => x == PossibleWallEndingDistances.Min())];

            }
            else
            {
                return new ObjectTriple<int, ExtPoint2D, ExtPoint2D>();
            }
        }
        private void IsThereAResult(double v1,double v2, double v3, out int Index)
        {
            Index = 0;
            if (v1!=0.0 && v1<v2 && v1<v3)
            {
                Index = 1;
            }
            else if (v2 != 0.0 && v2 < v1 && v2 < v3)
            {
                Index = 2;
            }
            else if (v3 != 0.0 && v3 < v2 && v3 < v1)
            {
                Index = 3;
            }
        }

     
    }
}
