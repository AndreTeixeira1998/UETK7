using UETK7.Types;

namespace UETK7.UnrealEngine.Types.Package
{
    public struct PackageFooter
    {
        public uint magic; //0x5A6F12E1
        public uint version; // 3
        public ulong indexOffset;
        public ulong indexLength;
        public SHA1 indexHash; //20(d) bytes
    }
}
