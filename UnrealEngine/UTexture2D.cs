/* 
 * A really fucked up way of reading Tekken 7 textures.
 * I should've went to law school and researched the legality of reverse-engineering in certain countries instead of focusing on computer sciene or anything computer related.
 * But I love computers and reverse-engineering.
*/

using System;
using System.Collections.Generic;
using System.Text;
using UETK7.Data;
using UETK7.UnrealEngine.Runtime.Core;

namespace UETK7.UnrealEngine
{
    public class UTexture2D
    {
        public static readonly byte[] PF_HEADER = { 0x00, 0x08, 0x00, 0x00, 0x00, 0x50, 0x46, 0x5F };
        public static readonly byte[] PF_BGRA8_HEADER = { 0x00, 0x0C, 0x00, 0x00, 0x00, 0x50, 0x46, 0x5F };

        public const long PF_SEPERATOR = 0x4800000001;
        public const int SEPERATOR_UNCOMPRESSED_SIZE_OFFSET = 0x8;
        public const int SEPERATOR_UNCOMPRESSED_SIZE_OFFSET_2 = 0xC;
        public const int SEPERATOR_TEXTURE_START_OFFSET_OFFSET = 0x10;
        public const int SEPERATOR_UNKNOWN_OFFSET = 0x14;
        public const int SEPERATOR_TEXTURE_DATA = 0x18;

        public const int PF_SEPERATOR_RELATIVE_DDS_DATA = 0x18;

        public static int UASSET_RELATIVE_PATTERN_PF = 0x05;

        public static int UASSET_RELATIVE_PF_HEIGHT = 0xC;
        public static int UASSET_RELATIVE_PF_WIDTH = 0x10;
        public static int UASSET_RELATIVE_PF_TEXTURE_DATA = 0x28;
        public static int UASSET_RELATIVE_PF_SIZE = 0x18;

        public UassetFile UassetFile;

        public int PFOffset { get; private set; }
        public int FirstSeperatorOffset { get; private set; }

        public EPixelFormat PixelFormat { get; private set; }

        public int UnknownValue { get; private set; }

        public int MipMapCount { get; private set; }

        public List<FTexture2DMipMap> Mipmaps { get; private set; }

        /// <summary>
        /// The texture's width.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The texture's height.
        /// </summary>
        public int Height { get; private set; }

        public byte[] Size { get; private set; }

        private UTexture2D() {
        }

        public static UTexture2D GetTextureFromUAsset(UassetFile uassetFile)
        {
            if (uassetFile.UE4Version != UE4Version.UE4__4_13_2_r0)
                throw new NotSupportedException($"Texture extraction from {uassetFile.UE4Version.ToString()} uasset files is not supported.");

            TKContext.LogInner("INFO", $"Attempting to get a texture from file {uassetFile.Filename}");

            try
            {
                FObjectExport export = uassetFile.ExportsTable[0];
                EPixelFormat ePixelFormat = EPixelFormat.PF_Unknown;

                int pfOffset = 0x0;

                // TODO: Simplify this mess.
                for (int i = 0; i < uassetFile.NamesTable.Length; i++)
                {
                    if (uassetFile.NamesTable[i].Name.ToUpper().StartsWith("PF_"))
                    {
                        if (!Enum.TryParse(uassetFile.NamesTable[i].Name, out ePixelFormat))
                        {
                            TKContext.LogError("Pixel Format is not supported.");
                            return null;
                        }
                    }
                }

                UTexture2D uTexture2D = new UTexture2D();
                //uTexture2D.USerializedData = uSerializedData;
                uTexture2D.UassetFile = uassetFile;
                TKContext.LogInner("INFO", $"Attempting to find {ePixelFormat.ToString()}");
                pfOffset = ArrayExtensions.IndexOfPattern(export.data, Encoding.ASCII.GetBytes(ePixelFormat.ToString()));
                TKContext.LogInner("INFO", $"Pixel Format offset is 0x{pfOffset:X8}");

                if (pfOffset == int.MaxValue)
                {
                    TKContext.LogError("No texture was found.");
                    return null;
                }

                uTexture2D.PFOffset = pfOffset;

                uTexture2D.PixelFormat = ePixelFormat;
                uTexture2D.Height = BitConverter.ToInt32(ArrayExtensions.GetBytesFromByteArray(export.data, pfOffset - UASSET_RELATIVE_PF_HEIGHT, 4), 0);
                uTexture2D.Width = BitConverter.ToInt32(ArrayExtensions.GetBytesFromByteArray(export.data, pfOffset - UASSET_RELATIVE_PF_WIDTH, 4), 0);
                uTexture2D.UnknownValue = BitConverter.ToInt32(ArrayExtensions.GetBytesFromByteArray(export.data, pfOffset + 0x8, 4), 0);
                
                TKContext.LogInner("INFO", $"Texture resolution is {uTexture2D.Width}x{uTexture2D.Height}");

                int[] offsets = ArrayExtensions.Locate(export.data, BitConverter.GetBytes(PF_SEPERATOR));

                uTexture2D.MipMapCount = BitConverter.ToInt32(ArrayExtensions.GetBytesFromByteArray(export.data, offsets[0] - 0x4, 4), 0);
                uTexture2D.Mipmaps = new List<FTexture2DMipMap>();

                TKContext.LogInner("INFO", $"Texture has {uTexture2D.MipMapCount} mipmaps");

                uTexture2D.FirstSeperatorOffset = offsets[0];

                for (int i = 0; i < uTexture2D.MipMapCount; i++)
                {
                    FTexture2DMipMap fTexture2DMipMap = new FTexture2DMipMap();

                    fTexture2DMipMap.UncompressedSize = BitConverter.ToInt32(ArrayExtensions.GetBytesFromByteArray(export.data, offsets[i] + SEPERATOR_UNCOMPRESSED_SIZE_OFFSET, 4), 0);
                    fTexture2DMipMap.TextureUassetOffset = BitConverter.ToInt32(ArrayExtensions.GetBytesFromByteArray(export.data, offsets[i] + SEPERATOR_TEXTURE_START_OFFSET_OFFSET, 4), 0);
                    fTexture2DMipMap.UnknownValue = BitConverter.ToInt32(ArrayExtensions.GetBytesFromByteArray(export.data, offsets[i] + SEPERATOR_UNKNOWN_OFFSET, 4), 0);
                    fTexture2DMipMap.Texture = ArrayExtensions.GetBytesFromByteArray(export.data, offsets[i] + SEPERATOR_TEXTURE_DATA, fTexture2DMipMap.UncompressedSize);
                    fTexture2DMipMap.Width = BitConverter.ToInt32(ArrayExtensions.GetBytesFromByteArray(export.data, offsets[i] + SEPERATOR_TEXTURE_DATA + fTexture2DMipMap.UncompressedSize, 4), 0);
                    fTexture2DMipMap.Height = BitConverter.ToInt32(ArrayExtensions.GetBytesFromByteArray(export.data, offsets[i] + SEPERATOR_TEXTURE_DATA + fTexture2DMipMap.UncompressedSize + 0x4, 4), 0);

                    uTexture2D.Mipmaps.Add(fTexture2DMipMap);

                    TKContext.LogInner("INFO", fTexture2DMipMap.ToString());
                }

                return uTexture2D;
            }
            catch(Exception ex)
            {
                TKContext.LogException(ex.ToString());
                return null;
            }
        }
    }
}
