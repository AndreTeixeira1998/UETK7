using UETK7.Types;

namespace UETK7.UnrealEngine.Types.Package
{
    public struct PackageFileDataHeader //Always 53 bytes
    {
        public ulong pad;
        public ulong size1;
        public ulong size2;
        public uint pad2;
        public SHA1 sha1; //20 Bytes
        public byte[] pad3; //5 Bytes
    }
}
