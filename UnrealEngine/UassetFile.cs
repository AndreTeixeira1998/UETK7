using System;
using System.IO;
using System.Linq;
using System.Text;
using UETK7.IO;
using UETK7.UnrealEngine.Runtime.Core;

using static UETK7.Data.ArrayExtensions;

namespace UETK7.UnrealEngine
{
    public class UassetFile
    {
        private const uint UNREAL_ASSET_MAGIC = 0x9E2A83C1;

        private const int UNREAL_LICENSE_VERSION_TK7_CONSOLE = -7;
        private const int UNREAL_LICENSE_VERSION_TK7FR_2016 = -5;
        private const int UNREAL_LICENSE_VERSION_TK7_0_2015 = -3;

        private const int UNREAL_ASSET_IMPORT_TABLE_ITEM_SIZE = 0x1C;

        private const int UASSET_MAGIC_OFFSET = 0x0;
        private const int UASSET_VERSION_OFFSET = 0x4;
        /// <summary>
        /// 0x8-0x18
        /// </summary>
        private const int UASSET_UNK_DATA_OFFSET = 0x8;
        private const int UASSET_DATA_ENTRY_OFFSET = 0x18;
        private const int UASSET_PACKAGE_GROUP_OFFSET = 0x20;
        private const int UASSET_NAMES_COUNT_OFFSET = 0x29;
        private const int UASSET_NAMES_TABLE_OFFSET = 0x2D;
        private const int UASSET_EXPORT_COUNT_OFFSET = 0x39;
        private const int UASSET_EXPORT_TABLE_OFFSET = 0x3D;
        private const int UASSET_IMPORT_COUNT_OFFSET = 0x41;
        private const int UASSET_IMPORT_TABLE_OFFSET = 0x45;
        private const int UASSET_UNK_OFFSET_2 = 0x51;
        private const int UASSET_NAMES_COUNT_2_OFFSET = 0x71;
        private const int UASSET_UNK_OFFSET_3 = 0xA1;
        private const int UASSET_END_OF_FILE_OFFSET = 0xA5;
        private const int UASSET_DATA_ENTRY_OFFSET_2 = 0xB9;

        /// <summary>
        /// The unreal engine version that this uasset was built on/for.
        /// </summary>
        public uint Version { get; private set; }

        /// <summary>
        /// The uasset package group.
        /// </summary>
        public string PackageGroup;

        /// <summary>
        /// Package's flags.
        /// </summary>
        public uint PackageFlags;

        /// <summary>
        /// The uasset's file name without the extension.
        /// </summary>
        public string Filename;

        /// <summary>
        /// The full path of the uasset file.
        /// </summary>
        public string Fullpath;

        /// <summary>
        /// The file data of the uasset file.
        /// </summary>
        public byte[] FileData { get; private set; }

        #region Offsets
        public int UnkOffset { get; private set; }
        public int UnkOffset2 { get; private set; }
        public int UnkOffset3 { get; private set; }
        public int UnkOffset4 { get; private set; }

        /// <summary>
        /// The offset for the export table.
        /// </summary>
        public int ExportsOffset { get; private set; }
        /// <summary>
        /// The offset for the import table.
        /// </summary>
        public int ImportsOffset { get; private set; }
        /// <summary>
        /// The offset for the names table.
        /// </summary>
        public int NamesOffset { get; private set; }

        /// <summary>
        /// The offset for the end of file - 0x4.
        /// </summary>
        public int EndOfFileOffset { get; private set; }
        #endregion

        #region Variables
        /// <summary>
        /// The count of items in the names table.
        /// </summary>
        public int NamesCount { get; private set; }

        /// <summary>
        /// The count of itmes in the names table as definited in the offset <seealso cref="UASSET_NAMES_COUNT_2_OFFSET"/>.
        /// </summary>
        public int NamesCount2 { get; private set; }

        /// <summary>
        /// The count of items in the exports table.
        /// </summary>
        public int ExportsCount { get; private set; }

        /// <summary>
        /// The count of itmes in the imports table.
        /// </summary>
        public int ImportsCount { get; private set; }
        #endregion

        public string[] name_table { get; private set; }

        /// <summary>
        /// The unreal engine 4 version this uasset file probably came from.
        /// </summary>
        //public UE4Version UE4Version { get; private set; }

        public FNameEntry[] NamesTable { get; private set; }
        public FObjectImport[] ImportsTable { get; private set; }
        public FObjectExport[] ExportsTable { get; private set; }

        private IOMemoryStream stream;

        public UassetFile(string path) : this(File.ReadAllBytes(path), path) { }

        public UassetFile(byte[] data, string fullPath)
        {
            Fullpath = fullPath;
            Filename = Path.GetFileNameWithoutExtension(Fullpath);

            //Create a stream
            stream = new IOMemoryStream(new MemoryStream(data), true);

            stream.position = 0;

            var signature = stream.ReadUInt();

            // Check for the file's signature.
            if (signature != UNREAL_ASSET_MAGIC)
            {
                TKContext.LogError("The file provided is not a valid uasset file.");
                return;
            }

            Version = stream.ReadUInt();

            stream.ReadBytes(0x10);
            UnkOffset = stream.ReadInt();
            stream.ReadInt();

            PackageGroup = Encoding.ASCII.GetString(stream.ReadBytes(0x4));

            stream.ReadBytes(0x1);

            PackageFlags = stream.ReadUInt();
            NamesCount = stream.ReadInt();
            NamesOffset = stream.ReadInt();

            stream.ReadBytes(0x8);

            ExportsCount = stream.ReadInt();
            ExportsOffset = stream.ReadInt();

            ImportsCount = stream.ReadInt();
            ImportsOffset = stream.ReadInt();

            stream.ReadBytes(0x8);

            UnkOffset2 = stream.ReadInt();

            stream.ReadBytes(0x1C);

            NamesCount2 = stream.ReadInt();

            stream.ReadBytes(0x2C);

            UnkOffset3 = stream.ReadInt();
            EndOfFileOffset = stream.ReadInt();

            stream.ReadBytes(0x10);
            UnkOffset4 = stream.ReadInt();

            stream.position = NamesOffset;

            NamesTable = stream.ReadFNameEntries(NamesCount, UE4Version.UE4__4_14);


            for(int i = 0; i < NamesCount; i++)
            {
                TKContext.DebugLog("UassetFile", $"Name Table Entry {i}", NamesTable[i].Name, ConsoleColor.Yellow);
            }

            TKContext.LogInner("INFO", $"Uasset names count is {NamesCount}");

            //Starts directly after the name table. Assume we're already there
            ImportsTable = new FObjectImport[ImportsCount];
            for (int i = 0; i < ImportsCount; i++)
            {
                FObjectImport h = FObjectImport.ReadEntry(stream, this);
                ImportsTable[i] = h;
                DebugDump($"FObjectImport {i} @ {h.startPos}", ConsoleColor.Blue, "cType", h.coreType, "u1", h.unknown1.ToString(), "oType", h.objectType, "u2", h.unknown2.ToString(), "i", h.index.ToString(), "name", h.name, "u4", h.unknown4.ToString());
            }

            TKContext.LogInner("INFO", $"Uasset import table size is {ImportsCount}");

            //Starts directly after the referenced GameObject table. Assume we're already there
            ExportsTable = new FObjectExport[ExportsCount];
            for (int i = 0; i < ExportsCount; i++)
            {
                ExportsTable[i] = FObjectExport.ReadEntry(stream, this);
                DebugDump($"FObjectExport {i} @ {ExportsTable[i].entryLocation}", ConsoleColor.Magenta, "id", ExportsTable[i].id.ToString(), "u2", ExportsTable[i].unknown2.ToString(), 
                    "u3", ExportsTable[i].unknown3.ToString(), "type", ExportsTable[i].type, "u4", ExportsTable[i].unknown4.ToString(), "u5", ExportsTable[i].unknown5.ToString(),
                    "length", ExportsTable[i].dataLength.ToString(), "location", ExportsTable[i].dataLocation.ToString(), "u6", ExportsTable[i].unknown6.ToString(), "u7", 
                    ExportsTable[i].unknown7.ToString(), "u8", ExportsTable[i].unknown8.ToString(), "u9", ExportsTable[i].unknown9.ToString(), "u10", ExportsTable[i].unknown10.ToString(), "u11", ExportsTable[i].unknown11.ToString(),
                    "u12", ExportsTable[i].unknown12.ToString(), "u13", ExportsTable[i].unknown13.ToString(), "u14", ExportsTable[i].unknown14.ToString(), "u15", ExportsTable[i].unknown15.ToString(),
                    "u16", ExportsTable[i].unknown16.ToString(), "u17", ExportsTable[i].unknown17.ToString(), "u18", ExportsTable[i].unknown18.ToString(), "u19", ExportsTable[i].unknown19.ToString(),
                    "u20", ExportsTable[i].unknown20.ToString(), "u21", ExportsTable[i].unknown21.ToString(), "u22", ExportsTable[i].unknown22.ToString());
            }

            TKContext.LogInner("INFO", $"Uasset export table size is {ExportsCount}");

            using (MemoryStream ms = new MemoryStream())
            {
                stream.position = 0;
                stream.ms.CopyTo(ms);
                stream.Close();

                FileData = ms.ToArray();
            }
        }

        public void DebugDump(string name, ConsoleColor color, params string[] data)
        {
            if (!TKContext.DebugLogging)
                return;

            //Build
            string msg = $"";
            bool wasLastLabel = false;
            foreach (string s in data)
            {
                if (!wasLastLabel)
                    msg += s + "=";
                else
                    msg += s + ", ";
                wasLastLabel = !wasLastLabel;
            }

            TKContext.DebugLog("UassetFile", name, msg, color);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DATA START:");
            this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).ToList().ForEach(x => {
                sb.AppendLine(string.Format("Variable: {0} Value: {1:X2}", x.Name, x.GetValue(this)));
            });

            sb.AppendLine(PackageGroup);
            return sb.ToString();
        }
    }
}
