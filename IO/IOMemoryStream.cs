using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UETK7.UnrealEngine;
using UETK7.UnrealEngine.Runtime.Core;

namespace UETK7.IO
{
    public class IOMemoryStream
    {
        public bool is_little_endian = true;

        public Stream ms;

        public long Length => ms.Length;

        public long position
        {
            get
            {
                return ms.Position;
            }
            set
            {
                ms.Position = value;
            }
        }

        public IOMemoryStream(Stream ms, bool is_little_endian)
        {
            this.ms = ms;
            this.is_little_endian = is_little_endian;
        }

        //API deserialization
        public ushort ReadUShort()
        {
            return BitConverter.ToUInt16(PrivateReadBytes(2), 0);
        }

        public bool TryReadNameTableEntry(UassetFile f, out string name)
        {
            int index = ReadInt();
            name = null;
            if (index < 0 || index > f.name_table.Length)
                return false;
            name = f.name_table[index];
            return true;
        }

        public void DebugNameTableEntry(UassetFile f)
        {
            Console.WriteLine(DebugNameTableEntryRet(f));
        }

        public string DebugNameTableEntryRet(UassetFile f)
        {
            long pos = position;
            int value = ReadInt();
            position = pos;
            try
            {
                return (ReadNameTableEntry(f) + $" ({value})");
            }
            catch
            {
                return (value.ToString());
            }
        }

        public short ReadShort()
        {
            return BitConverter.ToInt16(PrivateReadBytes(2), 0);
        }

        public uint ReadUInt()
        {
            return BitConverter.ToUInt32(PrivateReadBytes(4), 0);
        }

        public int ReadInt()
        {
            return BitConverter.ToInt32(PrivateReadBytes(4), 0);
        }

        public ulong ReadULong()
        {
            byte[] buf = PrivateReadBytes(8);
            return BitConverter.ToUInt64(buf, 0);
        }

        public long ReadLong()
        {
            return BitConverter.ToInt64(PrivateReadBytes(4), 0);
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(PrivateReadBytes(4), 0);
        }

        public double ReadDouble()
        {
            return BitConverter.ToDouble(PrivateReadBytes(8), 0);
        }

        public bool ReadIntBool()
        {
            int data = ReadInt();
            //This is really bad, Wildcard....
            if (data != 0 && data != 1)
                throw new Exception("Expected boolean, got " + data);
            return data == 1;
        }

        public bool ReadByteBool()
        {
            return ReadByte() != 0x00;
        }

        public string ReadString()
        {
            List<byte> bytes = new List<byte>();
            byte b = ReadByte();
            bytes.Add(b);
            while ((b = ReadByte()) != 0x00) {
                bytes.Add(b);
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        public FNameEntry ReadNameEntry(int maxLen = 10485760, UE4Version ue4Version = UE4Version.UE4__4_14)
        {
            FNameEntry entry = new FNameEntry();
            entry.StringLength = this.ReadInt();

            if (entry.StringLength == 0)
                return entry;

            //Validate length
            if (entry.StringLength > maxLen)
                throw new Exception($"Failed to read null-terminated string; Length from file exceeded maximum length requested. Offset {ms.Position:X8}");

            //My friend's arg broke this reader. Turns out extended characters use TWO bytes. I think if the length is negative, it's two bytes per character
            if (entry.StringLength < 0)
            {
                //Read this many bytes * 2
                //byte[] buf = ReadBytes((-length * 2) - 1);
                byte[] buf = ReadBytes(entry.StringLength - 1);
                //Read null byte, but discard
                byte nullByte1 = ReadByte();
                if (nullByte1 != 0x00)
                    throw new Exception("Failed to read null-terminated string; 1st terminator in 2-bytes-per-character string was not null!");

                //Convert to string
                entry.Name = Encoding.ASCII.GetString(buf);
            }
            else
            {
                //Read this many bytes.
                byte[] buf = ReadBytes(entry.StringLength - 1);
                //Read null byte, but discard
                byte nullByte = ReadByte();
                if (nullByte != 0x00)
                    throw new Exception("Failed to read null-terminated string; Terminator was not null!");
                //Convert to string
                entry.Name = Encoding.ASCII.GetString(buf);
            }

            if(ue4Version == UE4Version.UE4__4_14)
                entry.unkRef = ReadUInt();

            return entry;
        }

        public string ReadUEStringEx(int maxLen = 10485760)
        {
            /*//Read length
            int length = this.ReadInt();
            if (length == 0)
                return "";

            //Validate length
            if (length > maxLen)
                throw new Exception("Failed to read null-terminated string; Length from file exceeded maximum length requested.");
            string str = "";
            if (length < 0)
            {
                //uint var1 = ReadUInt();
                str = ReadString();
            }

            return str;*/

            //Read length
            int length = this.ReadInt();
            if (length == 0)
                return "";

            //Validate length
            if (length > maxLen)
                throw new Exception("Failed to read null-terminated string; Length from file exceeded maximum length requested.");

            //My friend's arg broke this reader. Turns out extended characters use TWO bytes. I think if the length is negative, it's two bytes per character
            if (length < 0)
            {
                //Read this many bytes * 2
                //byte[] buf = ReadBytes((-length * 2) - 1);
                byte[] buf = ReadBytes(length - 1);
                //Read null byte, but discard
                byte nullByte1 = ReadByte();
                if (nullByte1 != 0x00)
                    throw new Exception("Failed to read null-terminated string; 1st terminator in 2-bytes-per-character string was not null!");

                //Convert to string
                return Encoding.ASCII.GetString(buf);
            }
            else
            {
                //Read this many bytes.
                byte[] buf = ReadBytes(length - 1);
                //Read null byte, but discard
                byte nullByte = ReadByte();
                if (nullByte != 0x00)
                    throw new Exception("Failed to read null-terminated string; Terminator was not null!");
                //Convert to string
                return Encoding.ASCII.GetString(buf);
            }
        }

        public string ReadUEString(int maxLen = 10485760)
        {
            //Read length
            int length = this.ReadInt();
            if (length == 0)
                return "";

            //Validate length
            if (length > maxLen)
                throw new Exception("Failed to read null-terminated string; Length from file exceeded maximum length requested.");

            //My friend's arg broke this reader. Turns out extended characters use TWO bytes. I think if the length is negative, it's two bytes per character
            if (length < 0)
            {
                //Read this many bytes * 2
                byte[] buf = ReadBytes((-length * 2) - 1);

                //Read null byte, but discard
                byte nullByte1 = ReadByte();
                if (nullByte1 != 0x00)
                    throw new Exception("Failed to read null-terminated string; 1st terminator in 2-bytes-per-character string was not null!");

                //Convert to string
                return Encoding.Unicode.GetString(buf);
            }
            else
            {
                //Read this many bytes.
                byte[] buf = ReadBytes(length - 1);
                //Read null byte, but discard
                byte nullByte = ReadByte();
                if (nullByte != 0x00)
                    throw new Exception("Failed to read null-terminated string; Terminator was not null!");
                //Convert to string
                return Encoding.UTF8.GetString(buf);
            }
        }

        public FNameEntry[] ReadFNameEntries(int length, UE4Version uE4Version = UE4Version.UE4__4_14)
        {
            //Create array
            FNameEntry[] data = new FNameEntry[length];

            //Read
            for (int i = 0; i < length; i++)
            {
                data[i] = ReadNameEntry(10485760, uE4Version);
            }
            //data[i] = ReadUEString();

            return data;
        }

        public string[] ReadStringArray(int length)
        {
            //Create array
            string[] data = new string[length];

            //Read
            for (int i = 0; i < length; i++)
            {
                data[i] = ReadUEStringEx();
                ReadInt();
            }
                //data[i] = ReadUEString();

            return data;
        }

        public string[] ReadStringArray()
        {
            //Read the length
            int len = ReadInt();

            return ReadStringArray(len);
        }

        public byte[] ReadBytes(int length)
        {
            byte[] buf = new byte[length];
            ms.Read(buf, 0, length);
            return buf;
        }

        public byte ReadByte()
        {
            return ReadBytes(1)[0];
        }

        public string ReadNameTableEntry(UassetFile f)
        {
            int index = ReadInt();
            if (index < 0)
                return "NOT FOUND";
            return f.NamesTable[index].Name;
        }

        public void Close()
        {
            ms.Close();
        }

        //Private deserialization API
        private byte[] PrivateReadBytes(int size)
        {
            //Read in from the buffer and respect the little endian setting.
            byte[] buf = new byte[size];
            //Read
            ms.Read(buf, 0, size);
            //Respect endians
            if (is_little_endian != BitConverter.IsLittleEndian)
                Array.Reverse(buf);
            return buf;
        }
    }
}
