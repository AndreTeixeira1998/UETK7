using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UETK7.Types;

namespace UETK7.Data
{
    public static class ArrayExtensions
    {
        static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                || candidate == null
                || array.Length == 0
                || candidate.Length == 0
                || candidate.Length > array.Length;
        }

        static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }

        static readonly int[] Empty = new int[0];

        public static int[] Locate(this byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return Empty;

            var list = new List<int>();

            for (int i = 0; i < self.Length; i++)
            {
                if (!IsMatch(self, i, candidate))
                    continue;

                list.Add(i);
            }

            return list.Count == 0 ? Empty : list.ToArray();
        }

        public static IEnumerable<uint> PatternAt(byte[] source, byte[] pattern)
        {
            return PatternAt(source, pattern, 0x0);
        }

        public static IEnumerable<uint> PatternAt(byte[] source, byte[] pattern, uint offset)
        {
            for (uint i = offset; i < source.Length; i++)
            {
                if (source.Skip((int)i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    yield return i;
                }
            }

            yield return 0;
        }

        public static unsafe int IndexOfPattern(byte[] src, byte[] pattern)
        {
            fixed (byte* srcPtr = &src[0])
            fixed (byte* patternPtr = &pattern[0])
            {
                for (int x = 0; x < src.Length; x++)
                {
                    byte currentValue = *(srcPtr + x);

                    if (currentValue != *patternPtr) continue;

                    bool match = false;

                    for (int y = 0; y < pattern.Length; y++)
                    {
                        byte tempValue = *(srcPtr + x + y);
                        if (tempValue != *(patternPtr + y))
                        {
                            match = false;
                            break;
                        }

                        match = true;
                    }

                    if (match)
                    {
                        return x;
                    }

                }
            }

            TKContext.LogInner("WARNING", "No pattern found!", ConsoleColor.Yellow);

            return -1;
        }

        public static uint GetMagicNumber(byte[] data)
        {
            if (data.Length < 4)
            {
                return 0;
            }

            return BitConverter.ToUInt32(data, 0);
        }

        public static byte[] GetBytesFromByteArray(byte[] bytes, int start, int length)
        {

            byte[] newBytes = new byte[length];

            //Array.Copy(bytes, start, newBytes, 0, length);

            Buffer.BlockCopy(bytes, start, newBytes, 0, length);

            return newBytes;
        }

        public static byte[] GetByteRange(byte[] bytes, int offset1, int offset2)
        {
            if (bytes.Length < offset1 || bytes.Length < offset2)
                return null;

            int length = offset2 - offset1;

            byte[] newBytes = new byte[length];

            Array.Copy(bytes, offset1, newBytes, 0, length);

            return newBytes;
        }

        public static DataPair<byte[], int> ReadUntilTerminationAndGetEndOffset(byte[] bytes, int start)
        {
            int current = start;
            int count = 0;
            while (bytes[current] != 0x00)
            {
                current++;
                count++;
            }

            return new DataPair<byte[], int>(GetBytesFromByteArray(bytes, start, count), current + 1);
        }

        public static string ReadNullTerminatedString(byte[] bytes, int start)
        {
            int current = start;
            int count = 0;
            while (bytes[current] != 0x00)
            {
                current++;
                count++;
            }

            return System.Text.Encoding.ASCII.GetString(GetBytesFromByteArray(bytes, start, count));
        }

        public static string GetAOBString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(string.Format("{0} ", bytes[i].ToString("X2")));
            }

            return sb.ToString();
        }
    }
}
