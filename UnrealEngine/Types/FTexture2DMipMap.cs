using System;
using System.Collections.Generic;

namespace UETK7.UnrealEngine
{
    /// <summary>
    /// A class represneting a 2D texture mip-map.
    /// </summary>
    public struct FTexture2DMipMap
    {
        /// <summary>
        /// The uncompressed size of this mip.
        /// </summary>
        public int UncompressedSize;

        /// <summary>
        /// The texture offset inside the uasset file.
        /// </summary>
        public int TextureUassetOffset;

        /// <summary>
        /// ???
        /// </summary>
        public int UnknownValue;

        /// <summary>
        /// The DDS texture.
        /// </summary>
        public byte[] Texture;

        /// <summary>
        /// Width of the mip-map.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the mip-map.
        /// </summary>
        public int Height;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        public FTexture2DMipMap(byte[] textureData, int width, int height, int uncompressedSize, int unknownValue, int textureOffset)
        {
            UncompressedSize = uncompressedSize;
            TextureUassetOffset = textureOffset;
            UnknownValue = unknownValue;
            Texture = textureData;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Returns this struct as an array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(UncompressedSize));
            bytes.AddRange(BitConverter.GetBytes(UncompressedSize));

            bytes.AddRange(BitConverter.GetBytes(TextureUassetOffset));
            bytes.AddRange(BitConverter.GetBytes(UnknownValue));

            bytes.AddRange(Texture);

            bytes.AddRange(BitConverter.GetBytes(Width));
            bytes.AddRange(BitConverter.GetBytes(Height));

            return bytes.ToArray();
        }

        public override string ToString()
        {
            return $"MipMap info: {Width}x{Height} Compressed Size: 0x{UncompressedSize:X8}";
        }
    }
}
