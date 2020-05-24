using System;
using UETK7.IO;

namespace UETK7.UnrealEngine
{
    // size of 28
    public class FObjectImport
    {
        public long startPos;

        public string coreType; //Seems to be script or UObject so far
        public int unknown1;
        public string objectType; //Class sometimes, other when coreType is script
        public int unknown2;
        public int index; //Index
        public string name; //Name used by the game
        public int unknown4;

        public static FObjectImport ReadEntry(IOMemoryStream ms, UassetFile f)
        {
            //Read in
            FObjectImport g = new FObjectImport();
            g.startPos = ms.position;
            g.coreType = ms.ReadNameTableEntry(f);
            g.unknown1 = ms.ReadInt();
            g.objectType = ms.ReadNameTableEntry(f);
            g.unknown2 = ms.ReadInt();
            g.index = ms.ReadInt();
            g.name = ms.ReadNameTableEntry(f);
            g.unknown4 = ms.ReadInt();
            return g;
        }
    }
}
