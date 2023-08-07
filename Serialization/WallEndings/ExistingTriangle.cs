using S_ExTowerCreator_Acad.Serialization.GeometricExtensions;

namespace S_ExTowerCreator_Acad.Serialization.WallEndings
{
    class ExistingTriangle
    {
        private ExtPoint2D p1;
        private ExtPoint2D p2;
        private ExtPoint2D p3;



        public ExtPoint2D P1 { get => p1; set => p1 = value; }
        public ExtPoint2D P2 { get => p2; set => p2 = value; }
        public ExtPoint2D P3 { get => p3; set => p3 = value; }

    }
}
