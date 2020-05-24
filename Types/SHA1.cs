using System;
using System.Text;

namespace UETK7.Types
{
    public class SHA1 : CustomValueType<SHA1, byte[]>
    {
        public SHA1(byte[] value) : base(value) { }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator SHA1(byte[] value)
        {
            if (value.Length > 20 || value.Length < 20)
                throw new ArgumentOutOfRangeException($"SHA1 requires 20 bytes in length, got {value.Length} instead.");

            return new SHA1(value);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="custom"></param>
        public static implicit operator byte[](SHA1 custom)
        {
            return custom.value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                sb.Append($"{value[i].ToString("X2")}");
            }

            return sb.ToString();
        }
    }
}
