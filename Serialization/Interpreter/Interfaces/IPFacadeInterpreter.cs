using S_ExTowerCreator_Acad.Models;
using System.Collections.Generic;

namespace S_ExTowerCreator_Acad.Serialization
{
    public interface IPFacadeInterpreter
    {
        string FacadeType();
        List<List<ExtPoint3D>> FacadePoints { get; set; }
        List<List<ExtPoint3D>> OpeningTroublePoints { get; set; }
        List<List<ExtPoint3D>> NoAnchoringZonePoints { get; set; }
        List<List<ExtPoint3D>> FloorPoints { get; set; }

    }
}
