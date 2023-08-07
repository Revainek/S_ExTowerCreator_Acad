using Autodesk.AutoCAD.Geometry;
using S_ExTowerCreator_Acad.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace S_ExTowerCreator_Acad.Serialization
{
    [Serializable, XmlInclude(typeof(ExtPoint3D))]
    public class PFacadePolyInterpreter : IPFacadeInterpreter
    {
        public string FacadeType()
        { return Type; }
        public List<List<ExtPoint3D>> FacadePoints { get => facadePoints; set => facadePoints = value; }

        private List<List<ExtPoint3D>> facadePoints;

        public List<List<ExtPoint3D>> OpeningTroublePoints { get => openingTroublePoints; set => openingTroublePoints = value; }

        private List<List<ExtPoint3D>> openingTroublePoints;
        public List<List<ExtPoint3D>> NoAnchoringZonePoints { get => noAnchoringZonePoints; set => noAnchoringZonePoints = value; }

        private List<List<ExtPoint3D>> noAnchoringZonePoints;
        public List<List<ExtPoint3D>> FloorPoints { get => floorPoints; set => floorPoints = value; }

        private List<List<ExtPoint3D>> floorPoints;
        [XmlIgnore]
        private string Type { get; set; }

        public PFacadePolyInterpreter()
        {
            Type = "SinglePolylineFacade";
        }

        public void AddFacadePointsFromAcadPoints3d(List<List<Point3d>> facPoints)
        {
            List<List<ExtPoint3D>> newList = new List<List<ExtPoint3D>>();
            foreach (var facadeList in facPoints)
            {
                List<ExtPoint3D> subList = new List<ExtPoint3D>();
                foreach (Point3d p in facadeList)
                {
                    subList.Add(new ExtPoint3D(p));
                }
                newList.Add(subList);
            }
            this.FacadePoints = newList;
        }
        public void AddOpeningTroublePointsFromAcadPoints3d(List<List<Point3d>> openingPoints)
        {
            List<List<ExtPoint3D>> newList = new List<List<ExtPoint3D>>();
            foreach (var facadeList in openingPoints)
            {
                List<ExtPoint3D> subList = new List<ExtPoint3D>();
                foreach (Point3d p in facadeList)
                {
                    subList.Add(new ExtPoint3D(p));
                }
                newList.Add(subList);
            }
            this.OpeningTroublePoints = newList;
        }
        public void AddNoAnchorTroublePointsFromAcadPoints3d(List<List<Point3d>> NoAnchorPoints)
        {
            List<List<ExtPoint3D>> newList = new List<List<ExtPoint3D>>();
            foreach (var facadeList in NoAnchorPoints)
            {
                List<ExtPoint3D> subList = new List<ExtPoint3D>();
                foreach (Point3d p in facadeList)
                {
                    subList.Add(new ExtPoint3D(p));
                }
                newList.Add(subList);
            }
            this.NoAnchoringZonePoints = newList;
        }
        public void AddFloorPointsFromAcadPoints3d(List<List<Point3d>> _floorPoints)
        {
            List<List<ExtPoint3D>> newList = new List<List<ExtPoint3D>>();
            foreach (var facadeList in _floorPoints)
            {
                List<ExtPoint3D> subList = new List<ExtPoint3D>();
                foreach (Point3d p in facadeList)
                {
                    subList.Add(new ExtPoint3D(p));
                }
                newList.Add(subList);
            }
            this.FloorPoints = newList;
        }

        public static void SaveXML(string name, PFacadePolyInterpreter WallList)
        {
            try
            {
                using (Stream streamWrite = File.Create(name))
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(PFacadePolyInterpreter));
                    formatter.Serialize(streamWrite, WallList);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static PFacadePolyInterpreter LoadXMLFromPath(string FullFilePath)
        {

            PFacadePolyInterpreter SettingConfig = new PFacadePolyInterpreter();
            if (File.Exists(FullFilePath))
            {
                XmlSerializer x = new XmlSerializer(typeof(PFacadePolyInterpreter));
                FileStream z = new FileStream(FullFilePath, FileMode.Open, FileAccess.Read);
                object w = x.Deserialize(z);
                SettingConfig = w as PFacadePolyInterpreter;
                z.Dispose();
            }

            return SettingConfig;
        }
    }
}
