using System;

namespace UETK7.Types
{
    /// <summary>
    /// Data Pair class
    /// </summary>
    /// <typeparam name="X"></typeparam>
    /// <typeparam name="Y"></typeparam>
    [Serializable]
    public class DataPair<X, Y>
    {
        public X Key { get; set; }
        public Y Value { get; set; }

        public DataPair(X x, Y y)
        {
            Key = x;
            Value = y;
        }

        public override string ToString()
        {
            return string.Format("Key: {0} Value: {1}", Key, Value);
        }
    }
}
