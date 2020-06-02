using System;
using UETK7.IO;

namespace UETK7.UnrealEngine
{
    // size of whatever
    public class FObjectExport
    {
        private UassetFile uassetFile;
        public long entryLocation; //Location of this entry.

        public int id;
        public int unknown1;
        public int unknown2;
        public int unknown3;
        public string type;
        public int unknown4;
        public int unknown5;
        public int dataLength;
        public int dataLocation; //Location of the data from the beginning of the file
        public int unknown6;
        public int unknown7;
        public int unknown8;
        public int unknown9;
        public int unknown10;
        public int unknown11;
        public int unknown12;
        public int unknown13;
        public int unknown14;
        public int unknown15;
        public int unknown16;
        public int unknown17;
        public int unknown18;
        public int unknown19;
        public int unknown20;
        public int unknown21;
        public int unknown22;
        public byte[] data;

        public int LengthOffset;
        public static FObjectExport ReadEntry(IOMemoryStream ms, UassetFile f)
        {
            //Read in
            FObjectExport g = new FObjectExport();

            g.entryLocation = ms.position;
            g.id = ms.ReadInt();
            g.unknown1 = ms.ReadInt();
            g.unknown2 = ms.ReadInt();
            g.unknown3 = ms.ReadInt();
            g.type = ms.ReadNameTableEntry(f);
            g.unknown4 = ms.ReadInt();
            g.unknown5 = ms.ReadInt();
            g.LengthOffset = (int)ms.position;
            g.dataLength = ms.ReadInt();
            g.dataLocation = ms.ReadInt();
            g.unknown6 = ms.ReadInt();
            g.unknown7 = ms.ReadInt();
            g.unknown8 = ms.ReadInt();
            g.unknown9 = ms.ReadInt();
            g.unknown10 = ms.ReadInt();
            g.unknown11 = ms.ReadInt();
            g.unknown12 = ms.ReadInt();
            g.unknown13 = ms.ReadInt();
            g.unknown14 = ms.ReadInt();
            g.unknown15 = ms.ReadInt();
            g.unknown16 = ms.ReadInt();
            g.unknown17 = ms.ReadInt();
            g.unknown18 = ms.ReadInt();
            g.unknown19 = ms.ReadInt();
            g.unknown20 = ms.ReadInt();
            g.unknown21 = ms.ReadInt();
            g.unknown22 = ms.ReadInt();

            long lastPos = ms.position;

            ms.position = g.dataLocation;
            g.data = ms.ReadBytes(g.dataLength);
            ms.position = lastPos;

            g.uassetFile = f;

            return g;
        }
    }
}
