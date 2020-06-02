using System;
using System.IO;
using System.Collections.Generic;
using UETK7.UnrealEngine.Types.Package;

namespace UETK7.UnrealEngine
{
    public class Package
    {
        private const uint FOOTER_MAGIC = 0x5A6F12E1;
        private const int END_FOOTER_POS = 0x2C;
        private const uint FHEADER_SIZE = 0x35;

        /// <summary>
        /// The full path to the package file (.pak)
        /// </summary>
        public string FullPath { get; private set; }

        /// <summary>
        /// The file name of the pak file, extension included.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// The file size of the pak file, in bytes.
        /// </summary>
        public long FileSize { get; private set; }

        /// <summary>
        /// The amount of files the package contains.
        /// </summary>
        public uint FileCount { get; private set; }

        /// <summary>
        /// The file data header struct of this package.
        /// </summary>
        public PackageFileDataHeader FileDataHeader { get; private set; }

        /// <summary>
        /// The package footer struct of this package.
        /// </summary>
        public PackageFooter Footer { get; private set; }

        /// <summary>
        /// The file entires for this package.
        /// </summary>
        public PackageFileEntry[] FileEntries { get; private set; }

        /// <summary>
        /// Returns a value that indicates whether the stream reader will ignore the magic number when reading the file.
        /// </summary>
        public bool IgnoreMagicNumber { get; private set; }

        public Package(string pakPath, bool ignoreMagicNumber = false)
        {
            if (!File.Exists(pakPath))
                throw new FileNotFoundException($"The specified pak file: {FullPath} doesn't exist!");

            FullPath = pakPath;
            FileName = Path.GetFileName(pakPath);
            FileSize = new FileInfo(pakPath).Length;

            IgnoreMagicNumber = ignoreMagicNumber;
        }

        public bool Read()
        {
            try
            {
                using (FileStream fs = new FileStream(FullPath, FileMode.Open, FileAccess.Read))
                {
                    using(BinaryReader br = new BinaryReader(fs))
                    {
                        // Go to 0x2C from the end of the file.
                        br.BaseStream.Seek(-END_FOOTER_POS, SeekOrigin.End);

                        var magic = br.ReadUInt32();

                        // Read the file's magic number and see if it's valid.
                        if (magic != FOOTER_MAGIC && !IgnoreMagicNumber)
                        {
                            // The + operator is for nice code readiblity so don't get upset.
                            TKContext.LogError($"The package: {FileName} contains an invalid magic number." +
                                " Set IgnoreMagicNumber to false if you want the stream reader to ignore it.");

                            return false;
                        }

                        PackageFooter footer = new PackageFooter();
                        footer.magic = magic;
                        footer.version = br.ReadUInt32();
                        footer.indexOffset = br.ReadUInt64();
                        footer.indexLength = br.ReadUInt64();
                        footer.indexHash = br.ReadBytes(20);

                        br.BaseStream.Seek((long)footer.indexOffset, SeekOrigin.Begin);
                        var skipbytes = br.ReadUInt32();
                        br.BaseStream.Seek(skipbytes, SeekOrigin.Current);
                        FileCount = br.ReadUInt32();

                        FileEntries = new PackageFileEntry[FileCount];

                        for(int i = 0; i < FileEntries.Length; i++)
                        {
                            PackageFileEntry packageFileEntry = new PackageFileEntry();

                            packageFileEntry.fileNameLength = br.ReadUInt32();
                            packageFileEntry.fileName = new string(br.ReadChars((int)(packageFileEntry.fileNameLength - 1)));

                            br.BaseStream.Seek(1, SeekOrigin.Current);

                            packageFileEntry.offset = br.ReadUInt64();
                            packageFileEntry.size1 = br.ReadUInt64();
                            packageFileEntry.size2 = br.ReadUInt64();
                            packageFileEntry.pad = br.ReadUInt32();
                            packageFileEntry.sha1 = br.ReadBytes(20);
                            packageFileEntry.pad2 = br.ReadBytes(5);

#if DEBUG
                            TKContext.Log("PackFileEntry", i.ToString(), $"File name: {packageFileEntry.fileName} Offset: 0x{packageFileEntry.offset:X8} " +
                                $"Size1: 0x{packageFileEntry.size1:X8} Size2: 0x{packageFileEntry.size2:X8} SHA1: {packageFileEntry.sha1.ToString()}", TKContext.LOG_TYPE_DEBUG, ConsoleColor.DarkCyan);
#endif

                            FileEntries[i] = packageFileEntry;
                        }

                        Footer = footer;
                    }
                }
                TKContext.LogInner("INFO", $"Loaded package {FileName}", ConsoleColor.Yellow);
                Array.Sort(FileEntries, (x, y) =>
                {
                    var lengthComp = Comparer<int>.Default.Compare((int)y.fileNameLength, (int)x.fileNameLength);
                    if (lengthComp == 0)
                        return string.Compare(y.fileName, x.fileName);
                    return lengthComp;
                });
                return true;
            }
            catch (Exception ex)
            {
                TKContext.LogException(ex.ToString());
                return false;
            }
        }
    }
}
