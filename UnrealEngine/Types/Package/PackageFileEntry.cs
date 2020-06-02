using UETK7.Types;

namespace UETK7.UnrealEngine.Types.Package
{
    public struct PackageFileEntry
    {
        public uint fileNameLength;
        public string fileName;
        public ulong offset;
        public ulong size1;
        public ulong size2;
        public uint pad;
        public SHA1 sha1; //20 Bytes
        public byte[] pad2; //5 Bytes
    }
}
