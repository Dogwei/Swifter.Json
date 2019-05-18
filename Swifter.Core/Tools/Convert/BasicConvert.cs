using System;

using static System.Convert;

namespace Swifter.Tools
{
    internal sealed class BasicConvert :

        IXConvert<int, bool>, IXConvert<string, bool>, IXConvert<ulong, bool>, IXConvert<long, bool>, IXConvert<uint, bool>, IXConvert<decimal, bool>, IXConvert<ushort, bool>, IXConvert<short, bool>, IXConvert<byte, bool>, IXConvert<char, bool>, IXConvert<sbyte, bool>, IXConvert<bool, bool>, IXConvert<DateTime, bool>, IXConvert<float, bool>, IXConvert<double, bool>,
        IXConvert<uint, byte>, IXConvert<int, byte>, IXConvert<ushort, byte>, IXConvert<short, byte>, IXConvert<sbyte, byte>, IXConvert<char, byte>, IXConvert<byte, byte>, IXConvert<bool, byte>, IXConvert<DateTime, byte>, IXConvert<long, byte>, IXConvert<ulong, byte>, IXConvert<float, byte>, IXConvert<double, byte>, IXConvert<decimal, byte>, IXConvert<string, byte>, IXConvert<object, byte>,
        IXConvert<ulong, char>, IXConvert<object, char>, IXConvert<bool, char>, IXConvert<char, char>, IXConvert<sbyte, char>, IXConvert<byte, char>, IXConvert<short, char>, IXConvert<ushort, char>, IXConvert<int, char>, IXConvert<uint, char>, IXConvert<double, char>, IXConvert<string, char>, IXConvert<float, char>, IXConvert<decimal, char>, IXConvert<DateTime, char>, IXConvert<long, char>,
        IXConvert<sbyte, DateTime>, IXConvert<decimal, DateTime>, IXConvert<byte, DateTime>, IXConvert<uint, DateTime>, IXConvert<int, DateTime>, IXConvert<bool, DateTime>, IXConvert<ushort, DateTime>, IXConvert<short, DateTime>, IXConvert<double, DateTime>, IXConvert<long, DateTime>, IXConvert<char, DateTime>, IXConvert<float, DateTime>, IXConvert<string, DateTime>, IXConvert<ulong, DateTime>, IXConvert<DateTime, DateTime>, IXConvert<object, DateTime>,
        IXConvert<string, decimal>, IXConvert<double, decimal>, IXConvert<float, decimal>, IXConvert<ulong, decimal>, IXConvert<long, decimal>, IXConvert<uint, decimal>, IXConvert<int, decimal>, IXConvert<short, decimal>, IXConvert<decimal, decimal>, IXConvert<byte, decimal>, IXConvert<sbyte, decimal>, IXConvert<object, decimal>, IXConvert<ushort, decimal>, IXConvert<char, decimal>, IXConvert<bool, decimal>, IXConvert<DateTime, decimal>,
        IXConvert<sbyte, double>, IXConvert<object, double>, IXConvert<DateTime, double>, IXConvert<char, double>, IXConvert<ushort, double>, IXConvert<int, double>, IXConvert<short, double>, IXConvert<uint, double>, IXConvert<ulong, double>, IXConvert<float, double>, IXConvert<double, double>, IXConvert<decimal, double>, IXConvert<string, double>, IXConvert<bool, double>, IXConvert<long, double>, IXConvert<byte, double>,
        IXConvert<string, Guid>, IXConvert<object, Guid>,
        IXConvert<short, short>, IXConvert<long, short>, IXConvert<object, short>, IXConvert<ulong, short>, IXConvert<float, short>, IXConvert<double, short>, IXConvert<decimal, short>, IXConvert<string, short>, IXConvert<DateTime, short>, IXConvert<uint, short>, IXConvert<int, short>, IXConvert<ushort, short>, IXConvert<byte, short>, IXConvert<sbyte, short>, IXConvert<char, short>, IXConvert<bool, short>,
        IXConvert<DateTime, int>, IXConvert<int, int>, IXConvert<decimal, int>, IXConvert<string, int>, IXConvert<object, int>, IXConvert<bool, int>, IXConvert<char, int>, IXConvert<byte, int>, IXConvert<short, int>, IXConvert<ushort, int>, IXConvert<uint, int>, IXConvert<long, int>, IXConvert<ulong, int>, IXConvert<float, int>, IXConvert<double, int>, IXConvert<sbyte, int>,
        IXConvert<long, long>, IXConvert<float, long>, IXConvert<double, long>, IXConvert<decimal, long>, IXConvert<DateTime, long>, IXConvert<string, long>, IXConvert<ulong, long>, IXConvert<uint, long>, IXConvert<bool, long>, IXConvert<ushort, long>, IXConvert<short, long>, IXConvert<byte, long>, IXConvert<sbyte, long>, IXConvert<char, long>, IXConvert<object, long>, IXConvert<int, long>,
        IXConvert<sbyte, sbyte>, IXConvert<char, sbyte>, IXConvert<byte, sbyte>, IXConvert<object, sbyte>, IXConvert<bool, sbyte>, IXConvert<DateTime, sbyte>, IXConvert<string, sbyte>, IXConvert<decimal, sbyte>, IXConvert<double, sbyte>, IXConvert<float, sbyte>, IXConvert<ulong, sbyte>, IXConvert<long, sbyte>, IXConvert<uint, sbyte>, IXConvert<short, sbyte>, IXConvert<int, sbyte>, IXConvert<ushort, sbyte>,
        IXConvert<byte, float>, IXConvert<object, float>, IXConvert<sbyte, float>, IXConvert<DateTime, float>, IXConvert<string, float>, IXConvert<decimal, float>, IXConvert<bool, float>, IXConvert<float, float>, IXConvert<ulong, float>, IXConvert<long, float>, IXConvert<uint, float>, IXConvert<int, float>, IXConvert<ushort, float>, IXConvert<short, float>, IXConvert<double, float>, IXConvert<char, float>,
        IXConvert<string, string>, IXConvert<object, string>, IXConvert<decimal, string>, IXConvert<DateTime, string>, IXConvert<byte[], string>, IXConvert<bool, string>, IXConvert<short, string>, IXConvert<char, string>, IXConvert<double, string>, IXConvert<float, string>, IXConvert<ulong, string>, IXConvert<long, string>, IXConvert<uint, string>, IXConvert<int, string>, IXConvert<ushort, string>, IXConvert<byte, string>, IXConvert<sbyte, string>, IXConvert<Guid, string>,
        IXConvert<object, ushort>, IXConvert<DateTime, ushort>, IXConvert<string, ushort>, IXConvert<decimal, ushort>, IXConvert<double, ushort>, IXConvert<sbyte, ushort>, IXConvert<ulong, ushort>, IXConvert<long, ushort>, IXConvert<uint, ushort>, IXConvert<ushort, ushort>, IXConvert<int, ushort>, IXConvert<bool, ushort>, IXConvert<char, ushort>, IXConvert<byte, ushort>, IXConvert<float, ushort>, IXConvert<short, ushort>,
        IXConvert<DateTime, uint>, IXConvert<string, uint>, IXConvert<decimal, uint>, IXConvert<float, uint>, IXConvert<double, uint>, IXConvert<long, uint>, IXConvert<uint, uint>, IXConvert<int, uint>, IXConvert<ushort, uint>, IXConvert<short, uint>, IXConvert<object, uint>, IXConvert<byte, uint>, IXConvert<char, uint>, IXConvert<bool, uint>, IXConvert<ulong, uint>, IXConvert<sbyte, uint>,
        IXConvert<byte, ulong>, IXConvert<char, ulong>, IXConvert<bool, ulong>, IXConvert<object, ulong>, IXConvert<sbyte, ulong>, IXConvert<short, ulong>, IXConvert<int, ulong>, IXConvert<uint, ulong>, IXConvert<long, ulong>, IXConvert<ulong, ulong>, IXConvert<float, ulong>, IXConvert<double, ulong>, IXConvert<decimal, ulong>, IXConvert<string, ulong>, IXConvert<DateTime, ulong>, IXConvert<ushort, ulong>
    {

        public static readonly BasicConvert Instance = new BasicConvert();

        private BasicConvert() { }


        bool IXConvert<int, bool>.Convert(int value) => ToBoolean(value);
        bool IXConvert<string, bool>.Convert(string value) => ToBoolean(value);
        bool IXConvert<ulong, bool>.Convert(ulong value) => ToBoolean(value);
        bool IXConvert<long, bool>.Convert(long value) => ToBoolean(value);
        bool IXConvert<uint, bool>.Convert(uint value) => ToBoolean(value);
        bool IXConvert<decimal, bool>.Convert(decimal value) => ToBoolean(value);
        bool IXConvert<ushort, bool>.Convert(ushort value) => ToBoolean(value);
        bool IXConvert<short, bool>.Convert(short value) => ToBoolean(value);
        bool IXConvert<byte, bool>.Convert(byte value) => ToBoolean(value);
        bool IXConvert<char, bool>.Convert(char value) => ToBoolean(value);
        bool IXConvert<sbyte, bool>.Convert(sbyte value) => ToBoolean(value);
        bool IXConvert<bool, bool>.Convert(bool value) => ToBoolean(value);
        bool IXConvert<DateTime, bool>.Convert(DateTime value) => ToBoolean(value);
        bool IXConvert<float, bool>.Convert(float value) => ToBoolean(value);
        bool IXConvert<double, bool>.Convert(double value) => ToBoolean(value);

        byte IXConvert<uint, byte>.Convert(uint value) => ToByte(value);
        byte IXConvert<int, byte>.Convert(int value) => ToByte(value);
        byte IXConvert<ushort, byte>.Convert(ushort value) => ToByte(value);
        byte IXConvert<short, byte>.Convert(short value) => ToByte(value);
        byte IXConvert<sbyte, byte>.Convert(sbyte value) => ToByte(value);
        byte IXConvert<char, byte>.Convert(char value) => ToByte(value);
        byte IXConvert<byte, byte>.Convert(byte value) => ToByte(value);
        byte IXConvert<bool, byte>.Convert(bool value) => ToByte(value);
        byte IXConvert<DateTime, byte>.Convert(DateTime value) => ToByte(value);
        byte IXConvert<long, byte>.Convert(long value) => ToByte(value);
        byte IXConvert<ulong, byte>.Convert(ulong value) => ToByte(value);
        byte IXConvert<float, byte>.Convert(float value) => ToByte(value);
        byte IXConvert<double, byte>.Convert(double value) => ToByte(value);
        byte IXConvert<decimal, byte>.Convert(decimal value) => ToByte(value);
        byte IXConvert<string, byte>.Convert(string value) => ToByte(value);
        byte IXConvert<object, byte>.Convert(object value) => ToByte(value);

        char IXConvert<ulong, char>.Convert(ulong value) => ToChar(value);
        char IXConvert<object, char>.Convert(object value) => ToChar(value);
        char IXConvert<bool, char>.Convert(bool value) => ToChar(value);
        char IXConvert<char, char>.Convert(char value) => ToChar(value);
        char IXConvert<sbyte, char>.Convert(sbyte value) => ToChar(value);
        char IXConvert<byte, char>.Convert(byte value) => ToChar(value);
        char IXConvert<short, char>.Convert(short value) => ToChar(value);
        char IXConvert<ushort, char>.Convert(ushort value) => ToChar(value);
        char IXConvert<int, char>.Convert(int value) => ToChar(value);
        char IXConvert<uint, char>.Convert(uint value) => ToChar(value);
        char IXConvert<double, char>.Convert(double value) => ToChar(value);
        char IXConvert<string, char>.Convert(string value) => ToChar(value);
        char IXConvert<float, char>.Convert(float value) => ToChar(value);
        char IXConvert<decimal, char>.Convert(decimal value) => ToChar(value);
        char IXConvert<DateTime, char>.Convert(DateTime value) => ToChar(value);
        char IXConvert<long, char>.Convert(long value) => ToChar(value);

        DateTime IXConvert<sbyte, DateTime>.Convert(sbyte value) => ToDateTime(value);
        DateTime IXConvert<decimal, DateTime>.Convert(decimal value) => ToDateTime(value);
        DateTime IXConvert<byte, DateTime>.Convert(byte value) => ToDateTime(value);
        DateTime IXConvert<uint, DateTime>.Convert(uint value) => ToDateTime(value);
        DateTime IXConvert<int, DateTime>.Convert(int value) => ToDateTime(value);
        DateTime IXConvert<bool, DateTime>.Convert(bool value) => ToDateTime(value);
        DateTime IXConvert<ushort, DateTime>.Convert(ushort value) => ToDateTime(value);
        DateTime IXConvert<short, DateTime>.Convert(short value) => ToDateTime(value);
        DateTime IXConvert<double, DateTime>.Convert(double value) => ToDateTime(value);
        DateTime IXConvert<long, DateTime>.Convert(long value) => ToDateTime(value);
        DateTime IXConvert<char, DateTime>.Convert(char value) => ToDateTime(value);
        DateTime IXConvert<float, DateTime>.Convert(float value) => ToDateTime(value);
        DateTime IXConvert<string, DateTime>.Convert(string value) => ToDateTime(value);
        DateTime IXConvert<ulong, DateTime>.Convert(ulong value) => ToDateTime(value);
        DateTime IXConvert<DateTime, DateTime>.Convert(DateTime value) => ToDateTime(value);
        DateTime IXConvert<object, DateTime>.Convert(object value) => ToDateTime(value);

        decimal IXConvert<string, decimal>.Convert(string value) => ToDecimal(value);
        decimal IXConvert<double, decimal>.Convert(double value) => ToDecimal(value);
        decimal IXConvert<float, decimal>.Convert(float value) => ToDecimal(value);
        decimal IXConvert<ulong, decimal>.Convert(ulong value) => ToDecimal(value);
        decimal IXConvert<long, decimal>.Convert(long value) => ToDecimal(value);
        decimal IXConvert<uint, decimal>.Convert(uint value) => ToDecimal(value);
        decimal IXConvert<int, decimal>.Convert(int value) => ToDecimal(value);
        decimal IXConvert<short, decimal>.Convert(short value) => ToDecimal(value);
        decimal IXConvert<decimal, decimal>.Convert(decimal value) => ToDecimal(value);
        decimal IXConvert<byte, decimal>.Convert(byte value) => ToDecimal(value);
        decimal IXConvert<sbyte, decimal>.Convert(sbyte value) => ToDecimal(value);
        decimal IXConvert<object, decimal>.Convert(object value) => ToDecimal(value);
        decimal IXConvert<ushort, decimal>.Convert(ushort value) => ToDecimal(value);
        decimal IXConvert<char, decimal>.Convert(char value) => ToDecimal(value);
        decimal IXConvert<bool, decimal>.Convert(bool value) => ToDecimal(value);
        decimal IXConvert<DateTime, decimal>.Convert(DateTime value) => ToDecimal(value);

        double IXConvert<sbyte, double>.Convert(sbyte value) => ToDouble(value);
        double IXConvert<object, double>.Convert(object value) => ToDouble(value);
        double IXConvert<DateTime, double>.Convert(DateTime value) => ToDouble(value);
        double IXConvert<char, double>.Convert(char value) => ToDouble(value);
        double IXConvert<ushort, double>.Convert(ushort value) => ToDouble(value);
        double IXConvert<int, double>.Convert(int value) => ToDouble(value);
        double IXConvert<short, double>.Convert(short value) => ToDouble(value);
        double IXConvert<uint, double>.Convert(uint value) => ToDouble(value);
        double IXConvert<ulong, double>.Convert(ulong value) => ToDouble(value);
        double IXConvert<float, double>.Convert(float value) => ToDouble(value);
        double IXConvert<double, double>.Convert(double value) => ToDouble(value);
        double IXConvert<decimal, double>.Convert(decimal value) => ToDouble(value);
        double IXConvert<string, double>.Convert(string value) => ToDouble(value);
        double IXConvert<bool, double>.Convert(bool value) => ToDouble(value);
        double IXConvert<long, double>.Convert(long value) => ToDouble(value);
        double IXConvert<byte, double>.Convert(byte value) => ToDouble(value);

        Guid IXConvert<string, Guid>.Convert(string value) => ConvertAdd.ToGuid(value);
        Guid IXConvert<object, Guid>.Convert(object value) => ConvertAdd.ToGuid(value);

        short IXConvert<short, short>.Convert(short value) => ToInt16(value);
        short IXConvert<long, short>.Convert(long value) => ToInt16(value);
        short IXConvert<object, short>.Convert(object value) => ToInt16(value);
        short IXConvert<ulong, short>.Convert(ulong value) => ToInt16(value);
        short IXConvert<float, short>.Convert(float value) => ToInt16(value);
        short IXConvert<double, short>.Convert(double value) => ToInt16(value);
        short IXConvert<decimal, short>.Convert(decimal value) => ToInt16(value);
        short IXConvert<string, short>.Convert(string value) => ToInt16(value);
        short IXConvert<DateTime, short>.Convert(DateTime value) => ToInt16(value);
        short IXConvert<uint, short>.Convert(uint value) => ToInt16(value);
        short IXConvert<int, short>.Convert(int value) => ToInt16(value);
        short IXConvert<ushort, short>.Convert(ushort value) => ToInt16(value);
        short IXConvert<byte, short>.Convert(byte value) => ToInt16(value);
        short IXConvert<sbyte, short>.Convert(sbyte value) => ToInt16(value);
        short IXConvert<char, short>.Convert(char value) => ToInt16(value);
        short IXConvert<bool, short>.Convert(bool value) => ToInt16(value);

        int IXConvert<DateTime, int>.Convert(DateTime value) => ToInt32(value);
        int IXConvert<int, int>.Convert(int value) => ToInt32(value);
        int IXConvert<decimal, int>.Convert(decimal value) => ToInt32(value);
        int IXConvert<string, int>.Convert(string value) => ToInt32(value);
        int IXConvert<object, int>.Convert(object value) => ToInt32(value);
        int IXConvert<bool, int>.Convert(bool value) => ToInt32(value);
        int IXConvert<char, int>.Convert(char value) => ToInt32(value);
        int IXConvert<byte, int>.Convert(byte value) => ToInt32(value);
        int IXConvert<short, int>.Convert(short value) => ToInt32(value);
        int IXConvert<ushort, int>.Convert(ushort value) => ToInt32(value);
        int IXConvert<uint, int>.Convert(uint value) => ToInt32(value);
        int IXConvert<long, int>.Convert(long value) => ToInt32(value);
        int IXConvert<ulong, int>.Convert(ulong value) => ToInt32(value);
        int IXConvert<float, int>.Convert(float value) => ToInt32(value);
        int IXConvert<double, int>.Convert(double value) => ToInt32(value);
        int IXConvert<sbyte, int>.Convert(sbyte value) => ToInt32(value);

        long IXConvert<long, long>.Convert(long value) => ToInt64(value);
        long IXConvert<float, long>.Convert(float value) => ToInt64(value);
        long IXConvert<double, long>.Convert(double value) => ToInt64(value);
        long IXConvert<decimal, long>.Convert(decimal value) => ToInt64(value);
        long IXConvert<DateTime, long>.Convert(DateTime value) => ToInt64(value);
        long IXConvert<string, long>.Convert(string value) => ToInt64(value);
        long IXConvert<ulong, long>.Convert(ulong value) => ToInt64(value);
        long IXConvert<uint, long>.Convert(uint value) => ToInt64(value);
        long IXConvert<bool, long>.Convert(bool value) => ToInt64(value);
        long IXConvert<ushort, long>.Convert(ushort value) => ToInt64(value);
        long IXConvert<short, long>.Convert(short value) => ToInt64(value);
        long IXConvert<byte, long>.Convert(byte value) => ToInt64(value);
        long IXConvert<sbyte, long>.Convert(sbyte value) => ToInt64(value);
        long IXConvert<char, long>.Convert(char value) => ToInt64(value);
        long IXConvert<object, long>.Convert(object value) => ToInt64(value);
        long IXConvert<int, long>.Convert(int value) => ToInt64(value);

        sbyte IXConvert<sbyte, sbyte>.Convert(sbyte value) => ToSByte(value);
        sbyte IXConvert<char, sbyte>.Convert(char value) => ToSByte(value);
        sbyte IXConvert<byte, sbyte>.Convert(byte value) => ToSByte(value);
        sbyte IXConvert<object, sbyte>.Convert(object value) => ToSByte(value);
        sbyte IXConvert<bool, sbyte>.Convert(bool value) => ToSByte(value);
        sbyte IXConvert<DateTime, sbyte>.Convert(DateTime value) => ToSByte(value);
        sbyte IXConvert<string, sbyte>.Convert(string value) => ToSByte(value);
        sbyte IXConvert<decimal, sbyte>.Convert(decimal value) => ToSByte(value);
        sbyte IXConvert<double, sbyte>.Convert(double value) => ToSByte(value);
        sbyte IXConvert<float, sbyte>.Convert(float value) => ToSByte(value);
        sbyte IXConvert<ulong, sbyte>.Convert(ulong value) => ToSByte(value);
        sbyte IXConvert<long, sbyte>.Convert(long value) => ToSByte(value);
        sbyte IXConvert<uint, sbyte>.Convert(uint value) => ToSByte(value);
        sbyte IXConvert<short, sbyte>.Convert(short value) => ToSByte(value);
        sbyte IXConvert<int, sbyte>.Convert(int value) => ToSByte(value);
        sbyte IXConvert<ushort, sbyte>.Convert(ushort value) => ToSByte(value);

        float IXConvert<byte, float>.Convert(byte value) => ToSingle(value);
        float IXConvert<object, float>.Convert(object value) => ToSingle(value);
        float IXConvert<sbyte, float>.Convert(sbyte value) => ToSingle(value);
        float IXConvert<DateTime, float>.Convert(DateTime value) => ToSingle(value);
        float IXConvert<string, float>.Convert(string value) => ToSingle(value);
        float IXConvert<decimal, float>.Convert(decimal value) => ToSingle(value);
        float IXConvert<bool, float>.Convert(bool value) => ToSingle(value);
        float IXConvert<float, float>.Convert(float value) => ToSingle(value);
        float IXConvert<ulong, float>.Convert(ulong value) => ToSingle(value);
        float IXConvert<long, float>.Convert(long value) => ToSingle(value);
        float IXConvert<uint, float>.Convert(uint value) => ToSingle(value);
        float IXConvert<int, float>.Convert(int value) => ToSingle(value);
        float IXConvert<ushort, float>.Convert(ushort value) => ToSingle(value);
        float IXConvert<short, float>.Convert(short value) => ToSingle(value);
        float IXConvert<double, float>.Convert(double value) => ToSingle(value);
        float IXConvert<char, float>.Convert(char value) => ToSingle(value);

        string IXConvert<string, string>.Convert(string value) => Convert.ToString(value);
        string IXConvert<object, string>.Convert(object value) => Convert.ToString(value);
        string IXConvert<decimal, string>.Convert(decimal value) => Convert.ToString(value);
        string IXConvert<DateTime, string>.Convert(DateTime value) => Convert.ToString(value);
        string IXConvert<byte[], string>.Convert(byte[] value) => ToBase64String(value);
        string IXConvert<bool, string>.Convert(bool value) => Convert.ToString(value);
        string IXConvert<short, string>.Convert(short value) => Convert.ToString(value);
        string IXConvert<char, string>.Convert(char value) => Convert.ToString(value);
        string IXConvert<double, string>.Convert(double value) => Convert.ToString(value);
        string IXConvert<float, string>.Convert(float value) => Convert.ToString(value);
        string IXConvert<ulong, string>.Convert(ulong value) => Convert.ToString(value);
        string IXConvert<long, string>.Convert(long value) => Convert.ToString(value);
        string IXConvert<uint, string>.Convert(uint value) => Convert.ToString(value);
        string IXConvert<int, string>.Convert(int value) => Convert.ToString(value);
        string IXConvert<ushort, string>.Convert(ushort value) => Convert.ToString(value);
        string IXConvert<byte, string>.Convert(byte value) => Convert.ToString(value);
        string IXConvert<sbyte, string>.Convert(sbyte value) => Convert.ToString(value);
        string IXConvert<Guid, string>.Convert(Guid value) => ConvertAdd.ToString(value);

        ushort IXConvert<object, ushort>.Convert(object value) => ToUInt16(value);
        ushort IXConvert<DateTime, ushort>.Convert(DateTime value) => ToUInt16(value);
        ushort IXConvert<string, ushort>.Convert(string value) => ToUInt16(value);
        ushort IXConvert<decimal, ushort>.Convert(decimal value) => ToUInt16(value);
        ushort IXConvert<double, ushort>.Convert(double value) => ToUInt16(value);
        ushort IXConvert<sbyte, ushort>.Convert(sbyte value) => ToUInt16(value);
        ushort IXConvert<ulong, ushort>.Convert(ulong value) => ToUInt16(value);
        ushort IXConvert<long, ushort>.Convert(long value) => ToUInt16(value);
        ushort IXConvert<uint, ushort>.Convert(uint value) => ToUInt16(value);
        ushort IXConvert<ushort, ushort>.Convert(ushort value) => ToUInt16(value);
        ushort IXConvert<int, ushort>.Convert(int value) => ToUInt16(value);
        ushort IXConvert<bool, ushort>.Convert(bool value) => ToUInt16(value);
        ushort IXConvert<char, ushort>.Convert(char value) => ToUInt16(value);
        ushort IXConvert<byte, ushort>.Convert(byte value) => ToUInt16(value);
        ushort IXConvert<float, ushort>.Convert(float value) => ToUInt16(value);
        ushort IXConvert<short, ushort>.Convert(short value) => ToUInt16(value);

        uint IXConvert<DateTime, uint>.Convert(DateTime value) => ToUInt32(value);
        uint IXConvert<string, uint>.Convert(string value) => ToUInt32(value);
        uint IXConvert<decimal, uint>.Convert(decimal value) => ToUInt32(value);
        uint IXConvert<float, uint>.Convert(float value) => ToUInt32(value);
        uint IXConvert<double, uint>.Convert(double value) => ToUInt32(value);
        uint IXConvert<long, uint>.Convert(long value) => ToUInt32(value);
        uint IXConvert<uint, uint>.Convert(uint value) => ToUInt32(value);
        uint IXConvert<int, uint>.Convert(int value) => ToUInt32(value);
        uint IXConvert<ushort, uint>.Convert(ushort value) => ToUInt32(value);
        uint IXConvert<short, uint>.Convert(short value) => ToUInt32(value);
        uint IXConvert<object, uint>.Convert(object value) => ToUInt32(value);
        uint IXConvert<byte, uint>.Convert(byte value) => ToUInt32(value);
        uint IXConvert<char, uint>.Convert(char value) => ToUInt32(value);
        uint IXConvert<bool, uint>.Convert(bool value) => ToUInt32(value);
        uint IXConvert<ulong, uint>.Convert(ulong value) => ToUInt32(value);
        uint IXConvert<sbyte, uint>.Convert(sbyte value) => ToUInt32(value);

        ulong IXConvert<byte, ulong>.Convert(byte value) => ToUInt64(value);
        ulong IXConvert<char, ulong>.Convert(char value) => ToUInt64(value);
        ulong IXConvert<bool, ulong>.Convert(bool value) => ToUInt64(value);
        ulong IXConvert<object, ulong>.Convert(object value) => ToUInt64(value);
        ulong IXConvert<sbyte, ulong>.Convert(sbyte value) => ToUInt64(value);
        ulong IXConvert<short, ulong>.Convert(short value) => ToUInt64(value);
        ulong IXConvert<int, ulong>.Convert(int value) => ToUInt64(value);
        ulong IXConvert<uint, ulong>.Convert(uint value) => ToUInt64(value);
        ulong IXConvert<long, ulong>.Convert(long value) => ToUInt64(value);
        ulong IXConvert<ulong, ulong>.Convert(ulong value) => ToUInt64(value);
        ulong IXConvert<float, ulong>.Convert(float value) => ToUInt64(value);
        ulong IXConvert<double, ulong>.Convert(double value) => ToUInt64(value);
        ulong IXConvert<decimal, ulong>.Convert(decimal value) => ToUInt64(value);
        ulong IXConvert<string, ulong>.Convert(string value) => ToUInt64(value);
        ulong IXConvert<DateTime, ulong>.Convert(DateTime value) => ToUInt64(value);
        ulong IXConvert<ushort, ulong>.Convert(ushort value) => ToUInt64(value);

    }
}