using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleLib.ClientLib.Constants
{
    /// <summary>
    /// The localisation number for each regional MapleStory version.
    /// </summary>
    public enum MapleStoryLocalisation : int
    {
        MapleStoryKorea = 1,
        MapleStoryKoreaTespia = 2,
        Unknown3 = 3,
        Unknown4 = 4,
        MapleStoryTespia = 5,
        Unknown6 = 6,
        MapleStorySEA = 7,
        MapleStoryGlobal = 8,
        MapleStoryEurope = 9,

        Not_Known = 999,

        // TODO: other values
    }
}
