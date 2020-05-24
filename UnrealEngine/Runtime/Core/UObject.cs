using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UETK7.IO;

namespace UETK7.UnrealEngine.Runtime.Core
{
    public struct FName
    {
        public int index1, index2, index3, index4; // size of 10
    }
    public struct FNameEntry
    {
        public int Size;
        public int StringLength;
        public string Name;
        public uint unkRef;

        public override string ToString()
        {
            return string.Format("Size: {0:X2} String length: {1} Name: {2} unkRef: {3:X8}", Size, StringLength, Name, unkRef);
        }
    }

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
            g.uassetFile = f;

            return g;
        }

        public byte[] GetSerializedData(IOMemoryStream ms)
        {
            ms.position = dataLocation;
            return ms.ReadBytes(dataLength);
        }

        public void WriteDebugString()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"DataPosition: {dataLocation}, ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"DataLength: {dataLength}, ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Type: {type}, ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"EntryLocation: {entryLocation}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n");
        }
    }

    public class UObject
    {
        public byte[] Data;
    }
}
