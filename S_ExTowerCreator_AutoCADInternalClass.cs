using Autodesk.AutoCAD.Runtime;

using Autodesk.AutoCAD.ApplicationServices;
//using AecApp = Autodesk.Aec.Arch.ApplicationServices;

using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using System.IO;
using S_ExTowerCreator_Acad.Serialization;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Face = Autodesk.AutoCAD.DatabaseServices.Face;
using S_ExTowerCreator_Acad.Serialization.WallEndings;
using S_ExTowerCreator_Acad.Serialization.GeometricExtensions;
using S_ExTowerCreator_Acad.Models;

namespace S_ExTowerCreator_Acad
{
    [ProgId("S_ExTowerCreator_Acad")]
    public class S_ExTowerCreator_AutoCADInternalClass : IExtensionApplication
    {

        [CommandMethod("InsertPObject")]
        public void InsertPart()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            _ = doc.Database;
            var ed = doc.Editor;


            var promptResult = ed.GetString("Article?");
            if (promptResult.Status != PromptStatus.OK)
                return;
            string ArtNr = promptResult.StringResult;

            var promptResult1 = ed.GetString("Orientation?");
            if (promptResult1.Status != PromptStatus.OK)
                return;
            string Orientation = promptResult1.StringResult;
            var promptResult2 = ed.GetPoint("InsertionPoint?");
            if (promptResult1.Status != PromptStatus.OK)
                return;
            Point3d InsertionPoint = promptResult2.Value;


            Object[] Commandtest1 = new Object[] { "PINSERTBAUTEIL", "000332", "", "0,0,0", "" };
            try
            {
                ed.Command(Commandtest1);
            }
            catch (Exception)
            { }
            ed.WriteMessage("Test Command is running correctly.");
        }
        [CommandMethod("S_ETC_ManualPurge", CommandFlags.Session)]
        public async void ManualPurge()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;

            var ed = doc.Editor;
            var options = new PromptStringOptions("\nEnter the file name: ");
            options.AllowSpaces = true;
            var result = ed.GetString(options);
            string dwgFile = result.StringResult;

            Application.DocumentManager.MdiActiveDocument = Application.DocumentManager.Open(dwgFile, false);
            var newDoc = Application.DocumentManager.MdiActiveDocument;
            //var newEd = newDoc.Editor;
            using (DocumentLock docLock = newDoc.LockDocument())
            {
                await Application.DocumentManager.ExecuteInCommandContextAsync(
                async (obj) =>
                {
                    Editor newEd = Application.DocumentManager.MdiActiveDocument.Editor;
                    await newEd.CommandAsync("S_ETC_CopyData") ;
                    //await ed.CommandAsync(new object[] { "._ZOOM", "Extents" });
                },
                null);
                //newEd.Command(new object[] { "S_ETC_CopyData" });
                //newDoc.SendStringToExecute("S_ETC_CopyData ", true, false, false);
            }
            //Application.DocumentManager.ExecuteInApplicationContext(
            //    async (obj) =>
            //    {
            //        Editor newEd = Application.DocumentManager.MdiActiveDocument.Editor;
            //       await newEd.CommandAsync(new object[] { "S_ETC_NewDrawing" });
            //    },null);
            var _assembly = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            string templatefile = _assembly + @"\Resources\acad.dwt";
            if (templatefile.Contains("file:\\")==true)
            { templatefile = templatefile.Replace("file:\\", ""); }

            Database database = newDoc.Database;
            string fileName = database.Filename;
            string newDir = Path.GetDirectoryName(fileName);
            string newSaveFile = newDir + "\\reworked\\"+ Path.GetFileName(database.Filename);
            newDoc.CloseAndDiscard();

            try
            {
                using (var _newDatabase = new Database(false, true))
                {
                    _newDatabase.ReadDwgFile(templatefile, FileOpenMode.OpenForReadAndAllShare, true, null);


                    if (Directory.Exists(newDir + "\\reworked\\") == false)
                    {
                        Directory.CreateDirectory(newDir + "\\reworked\\");
                    }
                    _newDatabase.SaveAs(newSaveFile, DwgVersion.Current);
                }
            }
            catch (Exception)
            {

            }
            Application.DocumentManager.MdiActiveDocument = Application.DocumentManager.Open(newSaveFile, false);
            //newDoc.SendStringToExecute("S_ETC_NewDrawing ", true, false, false);
            var nextDoc = Application.DocumentManager.MdiActiveDocument;

                await Application.DocumentManager.ExecuteInCommandContextAsync(
              async (obj) =>
              {
                  Editor newEd = Application.DocumentManager.MdiActiveDocument.Editor;
                  await newEd.CommandAsync(new object[] { "S_ETC_Paste" });
                  await newEd.CommandAsync(new object[] { "S_ETC_Orientate" });
              },
              null);

            nextDoc.CloseAndDiscard();

        }
        [CommandMethod("S_ETC_OpenDrawing", CommandFlags.Session)]
        public void OpenDrawing()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;
            var options = new PromptStringOptions("\nEnter the file name: ");
            options.AllowSpaces = true;
            var result = ed.GetString(options);
            string dwgFile = result.StringResult;

            Application.DocumentManager.MdiActiveDocument = Application.DocumentManager.Open(dwgFile, false);
        }
        [CommandMethod("S_ETC_CopyData")]
        public async void CopyDrawing()
        {
            var _assembly = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            _ = _assembly + @"\Resources\acad.dwt";
            Document document = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = document.LockDocument())
            {
                Database database = document.Database;
                string fileName = database.Filename;
                Editor ed = document.Editor;
                PromptSelectionResult res = ed.SelectAll();

                if (res.Status == PromptStatus.OK)
                {
                  await ed.CommandAsync(new object[] { "_COPYBASE", "0,0,0", "ALL", "" });

                }
            }

        }
        [CommandMethod("P_ImportSingleFacadePolyline")]
        public void P_ImportFacadePolyline()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            

            Database db = doc.Database;
            TypedValue[] filterlist = new TypedValue[0];
            SelectionFilter filter = new SelectionFilter(filterlist);
            PromptSelectionResult selRes = ed.SelectAll(filter);

            if (selRes.Status != PromptStatus.OK)
                return;
            Autodesk.AutoCAD.DatabaseServices.ObjectId[] ids = selRes.Value.GetObjectIds();
            List< List < Point3d >> WhitePointsCollection = new List<List<Point3d>>();
            List<List<Point3d>> RedPointsCollection = new List<List<Point3d>>();
            List<List<Point3d>> BluePointsCollection = new List<List<Point3d>>();
            List<List<Point3d>> GreenPointsCollection = new List<List<Point3d>>();
           
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                int i = 0;
                foreach (Autodesk.AutoCAD.DatabaseServices.ObjectId id in ids)
                {
                    var ent = tr.GetObject(id, OpenMode.ForRead);
                    if (ent is Polyline)
                    {
                        List<Point3d> WhitePoints = new List<Point3d>();
                        List<Point3d> BluePoints = new List<Point3d>();
                        List<Point3d> GreenPoints = new List<Point3d>();
                        List<Point3d> RedPoints = new List<Point3d>();
                        var pl = ent as Polyline;
                        for (int z = 0; z < pl.NumberOfVertices; z++)
                        {
                            var plPoint = pl.GetPoint3dAt(z);
                            if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 5))
                            {
                                BluePoints.Add(plPoint);
                            }
                            else if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 256) || pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 7))
                            {
                                WhitePoints.Add(plPoint);
                            }
                            else if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 3))
                            {
                                GreenPoints.Add(plPoint);
                            }
                            else if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 1))
                            {
                                GreenPoints.Add(plPoint);
                            }
                        }
                        if (BluePoints.Count>0)
                        { BluePointsCollection.Add(BluePoints); }
                        if (WhitePoints.Count > 0)
                        { WhitePointsCollection.Add(WhitePoints); }
                        if (GreenPoints.Count > 0)
                        { GreenPointsCollection.Add(GreenPoints); }
                        if (RedPoints.Count > 0)
                        { RedPointsCollection.Add(RedPoints); }
                    }
                    else if (ent is Autodesk.AutoCAD.DatabaseServices.Surface)
                    {
                        ed.WriteMessage("Surface has been found on drawing. Omitting.");
                    }
                    else if (ent is Solid3d)
                    {
                        ed.WriteMessage("Solid has been found on drawing. Omitting.");
                    }
                    else if (ent is Polyline3d)
                    {
                        ed.WriteMessage("Polyline3D has been found on drawing. Omitting.");
                    }
                }

            }
            foreach (var coll in WhitePointsCollection)
            {

            }
            
            PFacadePolyInterpreter output = new PFacadePolyInterpreter();
            output.AddFacadePointsFromAcadPoints3d(WhitePointsCollection);
            output.AddFloorPointsFromAcadPoints3d(GreenPointsCollection);
            output.AddNoAnchorTroublePointsFromAcadPoints3d(RedPointsCollection);
            output.AddOpeningTroublePointsFromAcadPoints3d(BluePointsCollection);

            string somepath = @"C:\P_GuidanceApp\ImportedGeometries\FacadeScaffolding\" + Path.GetFileNameWithoutExtension(db.Filename) + "_"+"SinglePolyFacade.xml";
            PFacadePolyInterpreter.SaveXML(somepath, output);
            }
        [CommandMethod("P_Import3DSolidFacadePolyline")]
        public void P_ImportSolidFacadePolyline()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
          

            Database db = doc.Database;
            TypedValue[] filterlist = new TypedValue[0];
            SelectionFilter filter = new SelectionFilter(filterlist);
            PromptSelectionResult selRes = ed.SelectAll(filter);

            if (selRes.Status != PromptStatus.OK)
                return;
            Autodesk.AutoCAD.DatabaseServices.ObjectId[] ids = selRes.Value.GetObjectIds();
            List<List<Point3d>> WhitePointsCollection = new List<List<Point3d>>();
            List<List<Point3d>> RedPointsCollection = new List<List<Point3d>>();
            List<List<Point3d>> BluePointsCollection = new List<List<Point3d>>();
            List<List<Point3d>> GreenPointsCollection = new List<List<Point3d>>();
            List<List<Point3d>> SurfPointsCollection = new List<List<Point3d>>();
            List<List<Point3d>> SolidPointsCollection = new List<List<Point3d>>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {



                int i = 0;
                foreach (Autodesk.AutoCAD.DatabaseServices.ObjectId id in ids)
                {
                    var ent = tr.GetObject(id, OpenMode.ForRead);
                    if (ent is Polyline)
                    {
                        List<Point3d> WhitePoints = new List<Point3d>();
                        List<Point3d> BluePoints = new List<Point3d>();
                        List<Point3d> GreenPoints = new List<Point3d>();
                        List<Point3d> RedPoints = new List<Point3d>();
                        var pl = ent as Polyline;
                        for (int z = 0; z < pl.NumberOfVertices; z++)
                        {
                            var plPoint = pl.GetPoint3dAt(z);
                            if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 5))
                            {
                                BluePoints.Add(plPoint);
                            }
                            else if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 256) || pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 7))
                            {
                                WhitePoints.Add(plPoint);
                            }
                            else if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 3))
                            {
                                GreenPoints.Add(plPoint);
                            }
                            else if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 1))
                            {
                                RedPoints.Add(plPoint);
                            }
                        }
                        if (BluePoints.Count > 0)
                        { BluePointsCollection.Add(BluePoints); }
                        if (WhitePoints.Count > 0)
                        { WhitePointsCollection.Add(WhitePoints); }
                        if (GreenPoints.Count > 0)
                        { GreenPointsCollection.Add(GreenPoints); }
                        if (RedPoints.Count > 0)
                        { RedPointsCollection.Add(RedPoints); }
                    }
                    else if (ent is Autodesk.AutoCAD.DatabaseServices.Surface)
                    {
                        List<Point3d> SurfPoints = new List<Point3d>();
                        var surf = ent as Autodesk.AutoCAD.DatabaseServices.Surface;
                        using (var brep = new Brep(surf))
                        {
                            foreach (var vertex in brep.Vertices)
                            {
                                var p = vertex.Point;
                                SurfPoints.Add(p);
                            }
                        }
                        SurfPointsCollection.Add(SurfPoints);
                    }
                    else if (ent is Solid3d)
                    {
                        List<Point3d> SolidPoints = new List<Point3d>();
                        var solid = ent as Solid3d;
                        Brep brp = new Brep(solid);
                        using (brp)
                        {
                            int complexCount = 0;
                            // Get all the Complexes 
                            foreach (Complex cmp in brp.Complexes)
                            {

                                // Get all the shells within a complex. 
                                int shellCount = 0;
                                foreach (Shell shl in cmp.Shells)
                                {
                                    // Get all the faces in a shell. 
                                    int faceCount = 0;
                                    foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face fce in shl.Faces)
                                    {
                                        // Get all the boundary loops within a face
                                        try
                                        {
                                            int loopCount = 0;
                                            foreach (BoundaryLoop lp in fce.Loops)
                                            {
                                                SolidPoints = new List<Point3d>();
                                                foreach (var x in lp.Vertices)
                                                {
                                                    SolidPoints.Add(x.Point);
                                                }
                                                SolidPointsCollection.Add(SolidPoints);
                                            }
                                        }
                                        catch
                                        {
                                            ed.WriteMessage(
                                              "\n        Problem getting loops/edges:" +
                                              " object is probably unbounded " +
                                              "(e.g. a sphere or a torus)."
                                            );
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (ent is Polyline3d)
                    {
                        List<Point3d> Poly3DGreenPoints = new List<Point3d>();
                        List<Point3d> Poly3DBluePoints = new List<Point3d>();
                        List<Point3d> Poly3DRedPoints = new List<Point3d>();
                        var pl = ent as Autodesk.AutoCAD.DatabaseServices.Polyline3d;
                        // Use foreach to get each contained vertex
                        foreach (ObjectId vId in pl)
                        {
                            PolylineVertex3d v3d = (PolylineVertex3d)tr.GetObject(vId, OpenMode.ForRead);

                            if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 3))
                            {
                                Poly3DGreenPoints.Add(v3d.Position);
                            }
                            if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 5))
                            {
                                Poly3DBluePoints.Add(v3d.Position);
                            }
                            else if (pl.Color == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 1))
                            {
                                Poly3DRedPoints.Add(v3d.Position);
                            }
                        }
                        if (Poly3DBluePoints.Count > 0)
                        { BluePointsCollection.Add(Poly3DBluePoints); }
                        if (Poly3DRedPoints.Count > 0)
                        { WhitePointsCollection.Add(Poly3DRedPoints); }
                        if (Poly3DGreenPoints.Count > 0)
                        { GreenPointsCollection.Add(Poly3DGreenPoints); }
                    }
                }
            }
            foreach (var coll in WhitePointsCollection)
            {

            }

            PFacadePolySolidInterpreter output = new PFacadePolySolidInterpreter();
            output.AddFacadePointsFromAcadPoints3d(SolidPointsCollection);
            output.AddFloorPointsFromAcadPoints3d(GreenPointsCollection);
            output.AddNoAnchorTroublePointsFromAcadPoints3d(RedPointsCollection);
            output.AddOpeningTroublePointsFromAcadPoints3d(BluePointsCollection);

            string somepath = @"C:\P_App\ImportedGeometries\" + Path.GetFileNameWithoutExtension(db.Filename) + "_" + "Solid3DFacade.xml";
            PFacadePolySolidInterpreter.SaveXML(somepath, output);
        }
        [CommandMethod("P_ImportGeometry")]
        public void ExtImportGeometry()
        {
            List<AcadWall> OutputWalls = new List<AcadWall>();


            List<List<PLine2D>> OutputLinesGroup = new List<List<PLine2D>>();
            List < List <ExistingWallBox>> ExistingWallsGroup = new List<List<ExistingWallBox>>();
            List < List <ExistingTriangle>> ExistingTrianglesGroup = new List<List<ExistingTriangle>>();


            List<PLine2D> OutputLines = new List<PLine2D>();
            List<ExistingWallBox> ExistingWalls = new List<ExistingWallBox>();
            List<ExistingTriangle> ExistingTriangles = new List<ExistingTriangle>();
            var doc = Application.DocumentManager.MdiActiveDocument;

            var ed = doc.Editor;
            var options = new PromptStringOptions("\nEnter output Path: ");
            options.AllowSpaces = true;
            var result = ed.GetString(options);
            string OutputFolder = result.StringResult;


            Database db = doc.Database;
            TypedValue[] filterlist = new TypedValue[0];
            SelectionFilter filter = new SelectionFilter(filterlist);
            PromptSelectionResult selRes = ed.SelectAll(filter);

            if (selRes.Status != PromptStatus.OK)
                return;

            Autodesk.AutoCAD.DatabaseServices.ObjectId[] ids = selRes.Value.GetObjectIds();

            List<DBObjectCollection> FirstExplosionGroup = new List<DBObjectCollection>();
            List<DBObjectCollection> SecondExplosionGroup = new List<DBObjectCollection>();
            DBObjectCollection FirstExplosion = new DBObjectCollection();
            DBObjectCollection SecondExplosion = new DBObjectCollection();
            List<Point3d> PWallLocation = new List<Point3d>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                int i = 0;
                foreach (Autodesk.AutoCAD.DatabaseServices.ObjectId id in ids)
                {
                    var ent = tr.GetObject(id, OpenMode.ForRead);
                    try

                    {  if ((ent is Autodesk.Aec.DatabaseServices.Geo))
                        {
                            if ((ent as Autodesk.Aec.DatabaseServices.Geo).DisplayName == "P_WALL")
                            {
                                PWallLocation.Add((ent as Autodesk.Aec.DatabaseServices.Geo).Location);
                                (ent as Autodesk.AutoCAD.DatabaseServices.Entity).Explode(FirstExplosion);

                                FirstExplosionGroup.Add(FirstExplosion);
                                FirstExplosion = new DBObjectCollection();
                            }
                        }
                    }
                    catch( Exception)
                    {

                    }
                    #region oldTest
                    //if (ent is AecDB.Wall)
                    //   {

                    //      ( ent as Entity).Explode(FirstExplosion);
                    //       //i++;

                    //       //Point3d startPoint = (ent as AecDB.Wall).StartPoint;
                    //       //Point3d endPoint = (ent as AecDB.Wall).EndPoint;
                    //       //double Rotation = (ent as AecDB.Wall).Rotation;
                    //       //double Length = (ent as AecDB.Wall).Length;
                    //       //double Width = (ent as AecDB.Wall).Width;

                    //       //AcadWall wall = new AcadWall(startPoint, endPoint, Rotation, Length, Width);
                    //       //OutputWalls.Add(wall);
                    //       ////var Info = new PromptStringOptions("\n Wall " + i + " Start: " +  startPoint.X + " , " + startPoint.Y + " End: " + endPoint.X + " , " + endPoint.Y + " Length: " + Length + " Width: " + Width + " Rotation: " + Rotation);
                    //       ////ed.DoPrompt(Info);
                    //   }
                    //else
                    //   {

                    //   }

                    #endregion
                }
                for (int chuj = 0; chuj < FirstExplosionGroup.Count; chuj++)
                {
                    DBObjectCollection explosionGroup = FirstExplosionGroup[chuj];
                    foreach (Autodesk.AutoCAD.DatabaseServices.DBObject SubEntity in explosionGroup)
                    {
                        if (SubEntity is Autodesk.AutoCAD.DatabaseServices.BlockReference)
                        {
                            (SubEntity as Autodesk.AutoCAD.DatabaseServices.BlockReference).Position = PWallLocation[chuj];
                            (SubEntity as Autodesk.AutoCAD.DatabaseServices.BlockReference).Rotation = 0;
                            (SubEntity as Autodesk.AutoCAD.DatabaseServices.BlockReference).Explode(SecondExplosion);

                            SecondExplosionGroup.Add(SecondExplosion);
                            SecondExplosion = new DBObjectCollection();
                        }
                        else
                        {

                        }
                    }
                }

                foreach (DBObjectCollection explosionGroup in SecondExplosionGroup)
                {
                    foreach (Autodesk.AutoCAD.DatabaseServices.DBObject SubSubEntity in explosionGroup)
                    {

                        if (SubSubEntity is Hatch)
                        {
                            var Colour = (SubSubEntity as Hatch).Color;
                            if (Colour == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 30))
                            {
                                var HatchLoop = (SubSubEntity as Hatch).GetLoopAt(0);
                                BulgeVertexCollection points = HatchLoop.Polyline;

                                for (int vx = 0; vx < points.Count - 1; vx++)
                                {
                                    var p1 = points[vx].Vertex;
                                    var p2 = points[vx + 1].Vertex;
                                    PLine2D line = new PLine2D(p1, p2);
                                    OutputLines.Add(line);
                                }
                            }
                            else if (Colour == Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByColor, 63))
                            {
                                var HatchLoop = (SubSubEntity as Hatch).GetLoopAt(0);
                                if (HatchLoop.IsPolyline)
                                {
                                    BulgeVertexCollection points = HatchLoop.Polyline;
                                    if (points.Count == 4)
                                    {
                                        ExistingWallBox box = new ExistingWallBox
                                        {
                                            P1 = new ExtPoint2D(points[0].Vertex),
                                            P2 = new ExtPoint2D(points[1].Vertex),
                                            P3 = new ExtPoint2D(points[2].Vertex),
                                            P4 = new ExtPoint2D(points[3].Vertex)
                                        };

                                        ExistingWalls.Add(box);

                                    }
                                }
                                else
                                {
                                    Curve2dCollection points = HatchLoop.Curves;
                                    if (points.Count == 4)
                                    {
                                        ExistingWallBox box = new ExistingWallBox
                                        {
                                            P1 = new ExtPoint2D(points[0].StartPoint),
                                            P2 = new ExtPoint2D(points[1].StartPoint),
                                            P3 = new ExtPoint2D(points[2].StartPoint),
                                            P4 = new ExtPoint2D(points[3].StartPoint)
                                        };

                                        ExistingWalls.Add(box);

                                    }
                                }

                            }

                        }

                        else
                        {
                            if (SubSubEntity is Line)
                            {
                                continue;
                            }
                            else if (SubSubEntity is Face)
                            {
                                var Colour = (SubSubEntity as Face).Color;
                                if (Colour != Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 3))
                                {

                                    Face WallEndingObject = SubSubEntity as Face;

                                    if (WallEndingObject.Bounds.Value.MinPoint.Z < 0.001 && WallEndingObject.Bounds.Value.MaxPoint.Z < 0.001)
                                    {
                                        bool isTriangle = false;
                                        try
                                        {
                                            WallEndingObject.GetVertexAt(3);
                                            if (Math.Round(WallEndingObject.GetVertexAt(3).X, 4) == Math.Round(WallEndingObject.GetVertexAt(2).X, 4) && Math.Round(WallEndingObject.GetVertexAt(3).Y, 4) == Math.Round(WallEndingObject.GetVertexAt(2).Y, 4))
                                            {
                                                isTriangle = true;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            isTriangle = true;
                                        }
                                        finally
                                        {
                                            if (isTriangle)
                                            {
                                                ExistingTriangle triangle = new ExistingTriangle();
                                                triangle.P1 = new ExtPoint2D(WallEndingObject.GetVertexAt(0));
                                                triangle.P2 = new ExtPoint2D(WallEndingObject.GetVertexAt(1));
                                                triangle.P3 = new ExtPoint2D(WallEndingObject.GetVertexAt(2));
                                                ExistingTriangles.Add(triangle);
                                                //Point3d p1 =  WallEndingObject.GetVertexAt(0);
                                                //Point3d p2 = WallEndingObject.GetVertexAt(1);   
                                                //Point3d p3 = WallEndingObject.GetVertexAt(2);
                                            }
                                        }

                                    }
                                }
                            }

                        }
                    }
                    OutputLinesGroup.Add(OutputLines);
                    ExistingWallsGroup.Add(ExistingWalls);
                    ExistingTrianglesGroup.Add(ExistingTriangles);
                    OutputLines = new List<PLine2D>();
                    ExistingWalls = new List<ExistingWallBox>();
                    ExistingTriangles = new List<ExistingTriangle>();
                }

                tr.Commit();
            }
            string FileName = db.Filename;
            if (string.IsNullOrEmpty(db.Filename))
            {
                FileName = "UnnamedGeometry";
            }
            PWallInterpreter WI;
            for (int i = 0;i<OutputLinesGroup.Count;i++)
            {
                WI = new PWallInterpreter();
                WI.GetGeometryAnalysed(OutputLinesGroup[i], ExistingWallsGroup[i], ExistingTrianglesGroup[i]);
                if (Directory.Exists(OutputFolder)==false)
                {
                    Directory.CreateDirectory(OutputFolder);
                }
                PWallInterpreter.SaveXML(OutputFolder + Path.GetFileNameWithoutExtension(FileName)+"_" + i + ".xml", WI);
            }
            //PWallInterpreter WI = new PWallInterpreter();
            //WI.GetGeometryAnalysed(OutputLines, ExistingWalls, ExistingTriangles);
            //PWallInterpreter.SaveXML(OutputFolder, WI);
        }

        [CommandMethod("S_ETC_NewDrawing",CommandFlags.Session)]
        public void RewriteDrawingPart2()
        {   //test path
            string templatefile = @"C:\Temp_Work\etc\dwg\acad.dwt";
            Document document = Application.DocumentManager.MdiActiveDocument;

            Database database = document.Database;
            string fileName = database.Filename;
            string newDir = Path.GetDirectoryName(fileName);
            string newSaveFile = newDir + "\\reworked\\" + Path.GetFileName(database.Filename);
            document.CloseAndDiscard();

            try
            {
                using (var _newDatabase = new Database(false, true))
                {
                    _newDatabase.ReadDwgFile(templatefile, FileOpenMode.OpenForReadAndAllShare, true, null);


                    if (Directory.Exists(newDir + "\\reworked\\") == false)
                    {
                        Directory.CreateDirectory(newDir + "\\reworked\\");
                    }
                   
                    _newDatabase.SaveAs(newSaveFile, DwgVersion.Current);
                }
            }
            catch (Exception)
            {

            }
            Application.DocumentManager.MdiActiveDocument = Application.DocumentManager.Open(newSaveFile, false);
        }
        [CommandMethod("S_ETC_Paste")]
        public async void PasteDrawing()
        {
            Document newDocument = Application.DocumentManager.MdiActiveDocument;
            Database db = newDocument.Database;
            _ = newDocument.Editor;
            using (DocumentLock docLock = newDocument.LockDocument())
            {
              await newDocument.Editor.CommandAsync(new object[] { "._pasteclip", "0,0,0" });
              await newDocument.Editor.CommandAsync(new object[] { "._zoom", "Extents"});
                //newDocument.SendStringToExecute("._pasteclip 0,0,0 ", true, false, false);
                //newDocument.SendStringToExecute("._zoom Extents ", true, false, false);
            }
            db.UpdateExt(true);
        }
        [CommandMethod("S_ETC_Orientate")]
        public async void OrientateDrawing()
        {
            Document newDocument = Application.DocumentManager.MdiActiveDocument;
            Database db = newDocument.Database;
            Editor newEditor = newDocument.Editor;
            using (DocumentLock docLock = newDocument.LockDocument())
            {
                db.UpdateExt(true);
                Extents3d extents = new Extents3d();
                if (db.Extmin.X > db.Extmax.X && db.TileMode)
                {
                    Point3d tmp1 = new Point3d(db.Extmin.X, db.Extmin.Y, db.Extmin.Z);
                    Point3d tmp2 = new Point3d(db.Extmin.X, db.Extmin.Y, db.Extmin.Z);
                    extents = new Extents3d(tmp1, tmp2);
                }
                else
                {
                    extents = db.TileMode ?
                     new Extents3d(db.Extmin, db.Extmax) :
                     Convert.ToInt32(Application.GetSystemVariable("CVPORT")) == 1 ?
                         new Extents3d(db.Pextmin, db.Pextmax) :
                         new Extents3d(db.Extmin, db.Extmax);
                }
                Vector3d viewDirection = new Vector3d(-1.0, -1.0, 1.0);

                using (Transaction tr = db.TransactionManager.StartTransaction())
                using (ViewTableRecord view = newEditor.GetCurrentView())
                {
                    Matrix3d viewTransform =
                        Matrix3d.PlaneToWorld(viewDirection)
                        .PreMultiplyBy(Matrix3d.Displacement(view.Target - Point3d.Origin))
                        .PreMultiplyBy(Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target))
                        .Inverse();

                    extents.TransformBy(viewTransform);

                    view.ViewDirection = viewDirection;
                    view.Width = (extents.MaxPoint.X - extents.MinPoint.X) * 1.2;
                    view.Height = (extents.MaxPoint.Y - extents.MinPoint.Y) * 1.2;
                    view.CenterPoint = new Point2d(
                        (extents.MinPoint.X + extents.MaxPoint.X) / 2.0,
                        (extents.MinPoint.Y + extents.MaxPoint.Y) / 2.0);
                    newEditor.SetCurrentView(view);
                    tr.Commit();
                }
                await newDocument.Editor.CommandAsync(new object[] { "._QSAVE" });
                //newDocument.SendStringToExecute("_QSAVE ", true, false, false);
            }
        }
        

        #region Initialization

        void IExtensionApplication.Initialize()
        {

        }

        void IExtensionApplication.Terminate()
        {

        }
    }
        namespace AutoCadUtilsLibrary
    {
        public static class Extension
        {
            public static Matrix3d EyeToWorld(this AbstractViewTableRecord view)
            {
                if (view == null)
                    throw new ArgumentNullException("view");
                return
                    Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
                    Matrix3d.Displacement(view.Target.GetAsVector()) *
                    Matrix3d.PlaneToWorld(view.ViewDirection);
            }

            public static Matrix3d WorldToEye(this AbstractViewTableRecord view)
            {
                if (view == null)
                    throw new ArgumentNullException("view");
                return
                    Matrix3d.WorldToPlane(view.ViewDirection) *
                    Matrix3d.Displacement(view.Target.GetAsVector().Negate()) *
                    Matrix3d.Rotation(view.ViewTwist, view.ViewDirection, view.Target);
            }

            public static Matrix3d DCS2WCS(this Editor ed)
            {
                if (ed == null)
                    throw new ArgumentNullException("ed");
                using (ViewTableRecord view = ed.GetCurrentView())
                {
                    return view.EyeToWorld();
                }
            }
            public static Matrix3d WCS2DCS(this Editor ed)
            {
                if (ed == null)
                    throw new ArgumentNullException("ed");
                using (ViewTableRecord view = ed.GetCurrentView())
                {
                    return view.WorldToEye();
                }
            }
        }
    }
    #endregion
}

