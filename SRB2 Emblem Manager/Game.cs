using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRB2_Emblem_Manager
{
    public enum EmblemType
    {
        // Emblem information (emblem type)
        GLOBAL = 0, // Emblem with a position in space
        SKIN = 1, // Skin specific emblem with a position in space, var == skin
        MAP = 2, // Beat the map
        SCORE = 3, // Get the score
        TIME = 4, // Get the time
        RINGS = 5, // Get the rings
        NGRADE = 6, // Get the grade
        NTIME = 7 // Get the time (NiGHTS mode)
    }
    public enum GlobalEmblemVar
    {
        // Global emblem flags
        NIGHTSPULL = 1, // sun off the nights track - loop it
        NIGHTSITEM = 2 // moon on the nights track - find it
    }
    public enum MapEmblemVar
    { 
        // Map emblem flags
        ALLEMERALDS = 1,
        ULTIMATE = 2,
        PERFECT = 4
    }

    public struct Emblem
    {
        public byte type;      // Emblem type
        public short tag;       // Tag of emblem mapthing
        public short level;     // Level on which this emblem can be found.
        public char sprite;    // emblem sprite to use, 0 - 25
        public ushort color;    // skincolor to use
        public int var;       // If needed, specifies information on the target amount to achieve (or target skin)
        public string hint;  // Hint for emblem hints menu
        public byte collected; // Do you have this emblem? //checked individually within Memory.GetEmblemCount()
    }

    public struct ExtraEmblem
    {
        public string name;          // Name of the goal (used in the "emblem awarded" cecho)
        public string description;   // Description of goal (used in statistics)
        public byte conditionset;     // Condition set that awards this emblem.
        public byte showconditionset; // Condition set that shows this emblem.
        public char sprite;           // emblem sprite to use, 0 - 25
        public ushort color;           // skincolor to use
        public byte collected;        // Do you have this emblem? //checked individually within Memory.GetEmblemCount()
    }
}
