using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace S_ExTowerCreator_Acad.Models
{
    [Serializable, XmlInclude(typeof(ExtPoint3D))]
    public class AcadWall
    {
        private ExtPoint3D startPoint;
        private ExtPoint3D endPoint;
        private double rotation;
        private double length;
        private double width;

        public AcadWall()
        {

        }
        public AcadWall(Point3d Start,Point3d End, double _rotation, double _length, double _width)
        {
            StartPoint = new ExtPoint3D(Math.Round(Start.X,6), Math.Round(Start.Y, 6),Math.Round(Start.Z,6));
            EndPoint = new ExtPoint3D(Math.Round(End.X, 6), Math.Round(End.Y, 6), Math.Round(End.Z, 6));
            Rotation = Math.Round(_rotation,6);
            Length = Math.Round(_length, 6);
            Width = Math.Round(_width, 6);
        }
        public ExtPoint3D StartPoint { get => startPoint; set => startPoint = value; }
        public ExtPoint3D EndPoint { get => endPoint; set => endPoint = value; }
        public double Rotation { get => rotation; set => rotation = value; }
        public double Length { get => length; set => length = value; }
        public double Width { get => width; set => width = value; }
        public static void SaveXML(string name, List<AcadWall> WallList)
        {
            try
            {
                using (Stream streamWrite = File.Create(name))
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(List<AcadWall>));
                    formatter.Serialize(streamWrite, WallList);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static List<AcadWall> LoadXMLFromPath(string FullFilePath)
        {

            List<AcadWall> SettingConfig = new List<AcadWall>();
            if (File.Exists(FullFilePath))
            {
                XmlSerializer x = new XmlSerializer(typeof(List<AcadWall>));
                FileStream z = new FileStream(FullFilePath, FileMode.Open, FileAccess.Read);
                object w = x.Deserialize(z);
                SettingConfig = w as List<AcadWall>;
                z.Dispose();
            }
           
            return SettingConfig;
        }
    }
    [Serializable]
   public class ExtPoint3D
    {
        private double x;
        private double y;
        private double z;
        public ExtPoint3D()
        {

        }
        public ExtPoint3D(double _X,double _Y, double _Z)
        {
            X = _X;
            Y = _Y;
            Z = _Z;
        }
        public ExtPoint3D(Point3d p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }
        public ExtPoint3D(Point2d p)
        {
            X = p.X;
            Y = p.Y;
            Z = 0.00;
        }
        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
        public double Z { get => z; set => z = value; }
    }
}
