

using System;

namespace Swifter.Tools
{
	partial class BasicConvert :
		IXConverter<float, bool>,
		IXConverter<DateTime, bool>,
		IXConverter<decimal, bool>,
		IXConverter<double, bool>,
		IXConverter<ulong, bool>,
		IXConverter<long, bool>,
		IXConverter<uint, bool>,
		IXConverter<int, bool>,
		IXConverter<ushort, bool>,
		IXConverter<string, bool>,
		IXConverter<byte, bool>,
		IXConverter<char, bool>,
		IXConverter<sbyte, bool>,
		IXConverter<bool, bool>,
		IXConverter<object, bool>,
		IXConverter<short, bool>,
		IXConverter<char, byte>,
		IXConverter<sbyte, byte>,
		IXConverter<short, byte>,
		IXConverter<ushort, byte>,
		IXConverter<int, byte>,
		IXConverter<uint, byte>,
		IXConverter<long, byte>,
		IXConverter<double, byte>,
		IXConverter<float, byte>,
		IXConverter<DateTime, byte>,
		IXConverter<string, byte>,
		IXConverter<decimal, byte>,
		IXConverter<object, byte>,
		IXConverter<byte, byte>,
		IXConverter<ulong, byte>,
		IXConverter<bool, byte>,
		IXConverter<string, byte[]>,
		IXConverter<decimal, char>,
		IXConverter<DateTime, char>,
		IXConverter<string, char>,
		IXConverter<object, char>,
		IXConverter<bool, char>,
		IXConverter<char, char>,
		IXConverter<sbyte, char>,
		IXConverter<byte, char>,
		IXConverter<short, char>,
		IXConverter<ushort, char>,
		IXConverter<int, char>,
		IXConverter<uint, char>,
		IXConverter<long, char>,
		IXConverter<ulong, char>,
		IXConverter<double, char>,
		IXConverter<float, char>,
		IXConverter<double, DateTime>,
		IXConverter<float, DateTime>,
		IXConverter<DateTime, DateTime>,
		IXConverter<decimal, DateTime>,
		IXConverter<object, DateTime>,
		IXConverter<string, DateTime>,
		IXConverter<sbyte, DateTime>,
		IXConverter<byte, DateTime>,
		IXConverter<short, DateTime>,
		IXConverter<ushort, DateTime>,
		IXConverter<int, DateTime>,
		IXConverter<uint, DateTime>,
		IXConverter<long, DateTime>,
		IXConverter<ulong, DateTime>,
		IXConverter<bool, DateTime>,
		IXConverter<char, DateTime>,
		IXConverter<double, decimal>,
		IXConverter<string, decimal>,
		IXConverter<float, decimal>,
		IXConverter<bool, decimal>,
		IXConverter<DateTime, decimal>,
		IXConverter<decimal, decimal>,
		IXConverter<ulong, decimal>,
		IXConverter<object, decimal>,
		IXConverter<uint, decimal>,
		IXConverter<int, decimal>,
		IXConverter<ushort, decimal>,
		IXConverter<short, decimal>,
		IXConverter<char, decimal>,
		IXConverter<byte, decimal>,
		IXConverter<sbyte, decimal>,
		IXConverter<long, decimal>,
		IXConverter<decimal, double>,
		IXConverter<double, double>,
		IXConverter<float, double>,
		IXConverter<ulong, double>,
		IXConverter<long, double>,
		IXConverter<uint, double>,
		IXConverter<short, double>,
		IXConverter<ushort, double>,
		IXConverter<char, double>,
		IXConverter<byte, double>,
		IXConverter<sbyte, double>,
		IXConverter<object, double>,
		IXConverter<DateTime, double>,
		IXConverter<int, double>,
		IXConverter<string, double>,
		IXConverter<bool, double>,
		IXConverter<string, Guid>,
		IXConverter<object, Guid>,
		IXConverter<bool, short>,
		IXConverter<sbyte, short>,
		IXConverter<byte, short>,
		IXConverter<ushort, short>,
		IXConverter<int, short>,
		IXConverter<uint, short>,
		IXConverter<short, short>,
		IXConverter<DateTime, short>,
		IXConverter<char, short>,
		IXConverter<string, short>,
		IXConverter<decimal, short>,
		IXConverter<double, short>,
		IXConverter<float, short>,
		IXConverter<ulong, short>,
		IXConverter<long, short>,
		IXConverter<object, short>,
		IXConverter<DateTime, int>,
		IXConverter<string, int>,
		IXConverter<object, int>,
		IXConverter<bool, int>,
		IXConverter<char, int>,
		IXConverter<sbyte, int>,
		IXConverter<byte, int>,
		IXConverter<short, int>,
		IXConverter<ushort, int>,
		IXConverter<uint, int>,
		IXConverter<int, int>,
		IXConverter<long, int>,
		IXConverter<ulong, int>,
		IXConverter<float, int>,
		IXConverter<double, int>,
		IXConverter<decimal, int>,
		IXConverter<long, long>,
		IXConverter<float, long>,
		IXConverter<double, long>,
		IXConverter<decimal, long>,
		IXConverter<DateTime, long>,
		IXConverter<ulong, long>,
		IXConverter<string, long>,
		IXConverter<uint, long>,
		IXConverter<ushort, long>,
		IXConverter<short, long>,
		IXConverter<byte, long>,
		IXConverter<sbyte, long>,
		IXConverter<char, long>,
		IXConverter<bool, long>,
		IXConverter<object, long>,
		IXConverter<int, long>,
		IXConverter<string, sbyte>,
		IXConverter<ulong, sbyte>,
		IXConverter<float, sbyte>,
		IXConverter<double, sbyte>,
		IXConverter<decimal, sbyte>,
		IXConverter<DateTime, sbyte>,
		IXConverter<object, sbyte>,
		IXConverter<bool, sbyte>,
		IXConverter<sbyte, sbyte>,
		IXConverter<char, sbyte>,
		IXConverter<byte, sbyte>,
		IXConverter<short, sbyte>,
		IXConverter<ushort, sbyte>,
		IXConverter<int, sbyte>,
		IXConverter<uint, sbyte>,
		IXConverter<long, sbyte>,
		IXConverter<string, float>,
		IXConverter<decimal, float>,
		IXConverter<double, float>,
		IXConverter<float, float>,
		IXConverter<ulong, float>,
		IXConverter<long, float>,
		IXConverter<uint, float>,
		IXConverter<bool, float>,
		IXConverter<ushort, float>,
		IXConverter<short, float>,
		IXConverter<char, float>,
		IXConverter<byte, float>,
		IXConverter<sbyte, float>,
		IXConverter<object, float>,
		IXConverter<int, float>,
		IXConverter<DateTime, float>,
		IXConverter<string, string>,
		IXConverter<sbyte, string>,
		IXConverter<DateTime, string>,
		IXConverter<decimal, string>,
		IXConverter<double, string>,
		IXConverter<float, string>,
		IXConverter<ulong, string>,
		IXConverter<long, string>,
		IXConverter<uint, string>,
		IXConverter<int, string>,
		IXConverter<ushort, string>,
		IXConverter<short, string>,
		IXConverter<byte, string>,
		IXConverter<object, string>,
		IXConverter<char, string>,
		IXConverter<bool, string>,
		IXConverter<Guid, string>,
		IXConverter<byte[], string>,
		IXConverter<object, TypeCode>,
		IXConverter<DateTime, ushort>,
		IXConverter<string, ushort>,
		IXConverter<decimal, ushort>,
		IXConverter<double, ushort>,
		IXConverter<float, ushort>,
		IXConverter<ulong, ushort>,
		IXConverter<uint, ushort>,
		IXConverter<long, ushort>,
		IXConverter<int, ushort>,
		IXConverter<short, ushort>,
		IXConverter<byte, ushort>,
		IXConverter<sbyte, ushort>,
		IXConverter<char, ushort>,
		IXConverter<bool, ushort>,
		IXConverter<object, ushort>,
		IXConverter<ushort, ushort>,
		IXConverter<byte, uint>,
		IXConverter<object, uint>,
		IXConverter<bool, uint>,
		IXConverter<char, uint>,
		IXConverter<sbyte, uint>,
		IXConverter<short, uint>,
		IXConverter<ushort, uint>,
		IXConverter<int, uint>,
		IXConverter<uint, uint>,
		IXConverter<long, uint>,
		IXConverter<ulong, uint>,
		IXConverter<float, uint>,
		IXConverter<double, uint>,
		IXConverter<decimal, uint>,
		IXConverter<string, uint>,
		IXConverter<DateTime, uint>,
		IXConverter<decimal, ulong>,
		IXConverter<double, ulong>,
		IXConverter<float, ulong>,
		IXConverter<ulong, ulong>,
		IXConverter<long, ulong>,
		IXConverter<uint, ulong>,
		IXConverter<int, ulong>,
		IXConverter<sbyte, ulong>,
		IXConverter<short, ulong>,
		IXConverter<byte, ulong>,
		IXConverter<DateTime, ulong>,
		IXConverter<char, ulong>,
		IXConverter<bool, ulong>,
		IXConverter<object, ulong>,
		IXConverter<ushort, ulong>,
		IXConverter<string, ulong>
	{
        bool IXConverter<float, bool>.Convert(float value)
			=> Convert.ToBoolean(value);

		bool IXConverter<DateTime, bool>.Convert(DateTime value)
			=> Convert.ToBoolean(value);

		bool IXConverter<decimal, bool>.Convert(decimal value)
			=> Convert.ToBoolean(value);

		bool IXConverter<double, bool>.Convert(double value)
			=> Convert.ToBoolean(value);

		bool IXConverter<ulong, bool>.Convert(ulong value)
			=> Convert.ToBoolean(value);

		bool IXConverter<long, bool>.Convert(long value)
			=> Convert.ToBoolean(value);

		bool IXConverter<uint, bool>.Convert(uint value)
			=> Convert.ToBoolean(value);

		bool IXConverter<int, bool>.Convert(int value)
			=> Convert.ToBoolean(value);

		bool IXConverter<ushort, bool>.Convert(ushort value)
			=> Convert.ToBoolean(value);

		bool IXConverter<string, bool>.Convert(string value)
			=> Convert.ToBoolean(value);

		bool IXConverter<byte, bool>.Convert(byte value)
			=> Convert.ToBoolean(value);

		bool IXConverter<char, bool>.Convert(char value)
			=> Convert.ToBoolean(value);

		bool IXConverter<sbyte, bool>.Convert(sbyte value)
			=> Convert.ToBoolean(value);

		bool IXConverter<bool, bool>.Convert(bool value)
			=> Convert.ToBoolean(value);

		bool IXConverter<object, bool>.Convert(object value)
			=> Convert.IsDBNull(value);

		bool IXConverter<short, bool>.Convert(short value)
			=> Convert.ToBoolean(value);

		byte IXConverter<char, byte>.Convert(char value)
			=> Convert.ToByte(value);

		byte IXConverter<sbyte, byte>.Convert(sbyte value)
			=> Convert.ToByte(value);

		byte IXConverter<short, byte>.Convert(short value)
			=> Convert.ToByte(value);

		byte IXConverter<ushort, byte>.Convert(ushort value)
			=> Convert.ToByte(value);

		byte IXConverter<int, byte>.Convert(int value)
			=> Convert.ToByte(value);

		byte IXConverter<uint, byte>.Convert(uint value)
			=> Convert.ToByte(value);

		byte IXConverter<long, byte>.Convert(long value)
			=> Convert.ToByte(value);

		byte IXConverter<double, byte>.Convert(double value)
			=> Convert.ToByte(value);

		byte IXConverter<float, byte>.Convert(float value)
			=> Convert.ToByte(value);

		byte IXConverter<DateTime, byte>.Convert(DateTime value)
			=> Convert.ToByte(value);

		byte IXConverter<string, byte>.Convert(string value)
			=> Convert.ToByte(value);

		byte IXConverter<decimal, byte>.Convert(decimal value)
			=> Convert.ToByte(value);

		byte IXConverter<object, byte>.Convert(object value)
			=> Convert.ToByte(value);

		byte IXConverter<byte, byte>.Convert(byte value)
			=> Convert.ToByte(value);

		byte IXConverter<ulong, byte>.Convert(ulong value)
			=> Convert.ToByte(value);

		byte IXConverter<bool, byte>.Convert(bool value)
			=> Convert.ToByte(value);

		byte[] IXConverter<string, byte[]>.Convert(string value)
			=> Convert.FromBase64String(value);

		char IXConverter<decimal, char>.Convert(decimal value)
			=> Convert.ToChar(value);

		char IXConverter<DateTime, char>.Convert(DateTime value)
			=> Convert.ToChar(value);

		char IXConverter<string, char>.Convert(string value)
			=> Convert.ToChar(value);

		char IXConverter<object, char>.Convert(object value)
			=> Convert.ToChar(value);

		char IXConverter<bool, char>.Convert(bool value)
			=> Convert.ToChar(value);

		char IXConverter<char, char>.Convert(char value)
			=> Convert.ToChar(value);

		char IXConverter<sbyte, char>.Convert(sbyte value)
			=> Convert.ToChar(value);

		char IXConverter<byte, char>.Convert(byte value)
			=> Convert.ToChar(value);

		char IXConverter<short, char>.Convert(short value)
			=> Convert.ToChar(value);

		char IXConverter<ushort, char>.Convert(ushort value)
			=> Convert.ToChar(value);

		char IXConverter<int, char>.Convert(int value)
			=> Convert.ToChar(value);

		char IXConverter<uint, char>.Convert(uint value)
			=> Convert.ToChar(value);

		char IXConverter<long, char>.Convert(long value)
			=> Convert.ToChar(value);

		char IXConverter<ulong, char>.Convert(ulong value)
			=> Convert.ToChar(value);

		char IXConverter<double, char>.Convert(double value)
			=> Convert.ToChar(value);

		char IXConverter<float, char>.Convert(float value)
			=> Convert.ToChar(value);

		DateTime IXConverter<double, DateTime>.Convert(double value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<float, DateTime>.Convert(float value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<DateTime, DateTime>.Convert(DateTime value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<decimal, DateTime>.Convert(decimal value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<object, DateTime>.Convert(object value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<string, DateTime>.Convert(string value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<sbyte, DateTime>.Convert(sbyte value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<byte, DateTime>.Convert(byte value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<short, DateTime>.Convert(short value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<ushort, DateTime>.Convert(ushort value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<int, DateTime>.Convert(int value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<uint, DateTime>.Convert(uint value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<long, DateTime>.Convert(long value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<ulong, DateTime>.Convert(ulong value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<bool, DateTime>.Convert(bool value)
			=> Convert.ToDateTime(value);

		DateTime IXConverter<char, DateTime>.Convert(char value)
			=> Convert.ToDateTime(value);

		decimal IXConverter<double, decimal>.Convert(double value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<string, decimal>.Convert(string value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<float, decimal>.Convert(float value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<bool, decimal>.Convert(bool value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<DateTime, decimal>.Convert(DateTime value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<decimal, decimal>.Convert(decimal value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<ulong, decimal>.Convert(ulong value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<object, decimal>.Convert(object value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<uint, decimal>.Convert(uint value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<int, decimal>.Convert(int value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<ushort, decimal>.Convert(ushort value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<short, decimal>.Convert(short value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<char, decimal>.Convert(char value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<byte, decimal>.Convert(byte value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<sbyte, decimal>.Convert(sbyte value)
			=> Convert.ToDecimal(value);

		decimal IXConverter<long, decimal>.Convert(long value)
			=> Convert.ToDecimal(value);

		double IXConverter<decimal, double>.Convert(decimal value)
			=> Convert.ToDouble(value);

		double IXConverter<double, double>.Convert(double value)
			=> Convert.ToDouble(value);

		double IXConverter<float, double>.Convert(float value)
			=> Convert.ToDouble(value);

		double IXConverter<ulong, double>.Convert(ulong value)
			=> Convert.ToDouble(value);

		double IXConverter<long, double>.Convert(long value)
			=> Convert.ToDouble(value);

		double IXConverter<uint, double>.Convert(uint value)
			=> Convert.ToDouble(value);

		double IXConverter<short, double>.Convert(short value)
			=> Convert.ToDouble(value);

		double IXConverter<ushort, double>.Convert(ushort value)
			=> Convert.ToDouble(value);

		double IXConverter<char, double>.Convert(char value)
			=> Convert.ToDouble(value);

		double IXConverter<byte, double>.Convert(byte value)
			=> Convert.ToDouble(value);

		double IXConverter<sbyte, double>.Convert(sbyte value)
			=> Convert.ToDouble(value);

		double IXConverter<object, double>.Convert(object value)
			=> Convert.ToDouble(value);

		double IXConverter<DateTime, double>.Convert(DateTime value)
			=> Convert.ToDouble(value);

		double IXConverter<int, double>.Convert(int value)
			=> Convert.ToDouble(value);

		double IXConverter<string, double>.Convert(string value)
			=> Convert.ToDouble(value);

		double IXConverter<bool, double>.Convert(bool value)
			=> Convert.ToDouble(value);

		Guid IXConverter<string, Guid>.Convert(string value)
			=> ConvertAdd.ToGuid(value);

		Guid IXConverter<object, Guid>.Convert(object value)
			=> ConvertAdd.ToGuid(value);

		short IXConverter<bool, short>.Convert(bool value)
			=> Convert.ToInt16(value);

		short IXConverter<sbyte, short>.Convert(sbyte value)
			=> Convert.ToInt16(value);

		short IXConverter<byte, short>.Convert(byte value)
			=> Convert.ToInt16(value);

		short IXConverter<ushort, short>.Convert(ushort value)
			=> Convert.ToInt16(value);

		short IXConverter<int, short>.Convert(int value)
			=> Convert.ToInt16(value);

		short IXConverter<uint, short>.Convert(uint value)
			=> Convert.ToInt16(value);

		short IXConverter<short, short>.Convert(short value)
			=> Convert.ToInt16(value);

		short IXConverter<DateTime, short>.Convert(DateTime value)
			=> Convert.ToInt16(value);

		short IXConverter<char, short>.Convert(char value)
			=> Convert.ToInt16(value);

		short IXConverter<string, short>.Convert(string value)
			=> Convert.ToInt16(value);

		short IXConverter<decimal, short>.Convert(decimal value)
			=> Convert.ToInt16(value);

		short IXConverter<double, short>.Convert(double value)
			=> Convert.ToInt16(value);

		short IXConverter<float, short>.Convert(float value)
			=> Convert.ToInt16(value);

		short IXConverter<ulong, short>.Convert(ulong value)
			=> Convert.ToInt16(value);

		short IXConverter<long, short>.Convert(long value)
			=> Convert.ToInt16(value);

		short IXConverter<object, short>.Convert(object value)
			=> Convert.ToInt16(value);

		int IXConverter<DateTime, int>.Convert(DateTime value)
			=> Convert.ToInt32(value);

		int IXConverter<string, int>.Convert(string value)
			=> Convert.ToInt32(value);

		int IXConverter<object, int>.Convert(object value)
			=> Convert.ToInt32(value);

		int IXConverter<bool, int>.Convert(bool value)
			=> Convert.ToInt32(value);

		int IXConverter<char, int>.Convert(char value)
			=> Convert.ToInt32(value);

		int IXConverter<sbyte, int>.Convert(sbyte value)
			=> Convert.ToInt32(value);

		int IXConverter<byte, int>.Convert(byte value)
			=> Convert.ToInt32(value);

		int IXConverter<short, int>.Convert(short value)
			=> Convert.ToInt32(value);

		int IXConverter<ushort, int>.Convert(ushort value)
			=> Convert.ToInt32(value);

		int IXConverter<uint, int>.Convert(uint value)
			=> Convert.ToInt32(value);

		int IXConverter<int, int>.Convert(int value)
			=> Convert.ToInt32(value);

		int IXConverter<long, int>.Convert(long value)
			=> Convert.ToInt32(value);

		int IXConverter<ulong, int>.Convert(ulong value)
			=> Convert.ToInt32(value);

		int IXConverter<float, int>.Convert(float value)
			=> Convert.ToInt32(value);

		int IXConverter<double, int>.Convert(double value)
			=> Convert.ToInt32(value);

		int IXConverter<decimal, int>.Convert(decimal value)
			=> Convert.ToInt32(value);

		long IXConverter<long, long>.Convert(long value)
			=> Convert.ToInt64(value);

		long IXConverter<float, long>.Convert(float value)
			=> Convert.ToInt64(value);

		long IXConverter<double, long>.Convert(double value)
			=> Convert.ToInt64(value);

		long IXConverter<decimal, long>.Convert(decimal value)
			=> Convert.ToInt64(value);

		long IXConverter<DateTime, long>.Convert(DateTime value)
			=> Convert.ToInt64(value);

		long IXConverter<ulong, long>.Convert(ulong value)
			=> Convert.ToInt64(value);

		long IXConverter<string, long>.Convert(string value)
			=> Convert.ToInt64(value);

		long IXConverter<uint, long>.Convert(uint value)
			=> Convert.ToInt64(value);

		long IXConverter<ushort, long>.Convert(ushort value)
			=> Convert.ToInt64(value);

		long IXConverter<short, long>.Convert(short value)
			=> Convert.ToInt64(value);

		long IXConverter<byte, long>.Convert(byte value)
			=> Convert.ToInt64(value);

		long IXConverter<sbyte, long>.Convert(sbyte value)
			=> Convert.ToInt64(value);

		long IXConverter<char, long>.Convert(char value)
			=> Convert.ToInt64(value);

		long IXConverter<bool, long>.Convert(bool value)
			=> Convert.ToInt64(value);

		long IXConverter<object, long>.Convert(object value)
			=> Convert.ToInt64(value);

		long IXConverter<int, long>.Convert(int value)
			=> Convert.ToInt64(value);

		sbyte IXConverter<string, sbyte>.Convert(string value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<ulong, sbyte>.Convert(ulong value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<float, sbyte>.Convert(float value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<double, sbyte>.Convert(double value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<decimal, sbyte>.Convert(decimal value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<DateTime, sbyte>.Convert(DateTime value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<object, sbyte>.Convert(object value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<bool, sbyte>.Convert(bool value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<sbyte, sbyte>.Convert(sbyte value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<char, sbyte>.Convert(char value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<byte, sbyte>.Convert(byte value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<short, sbyte>.Convert(short value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<ushort, sbyte>.Convert(ushort value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<int, sbyte>.Convert(int value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<uint, sbyte>.Convert(uint value)
			=> Convert.ToSByte(value);

		sbyte IXConverter<long, sbyte>.Convert(long value)
			=> Convert.ToSByte(value);

		float IXConverter<string, float>.Convert(string value)
			=> Convert.ToSingle(value);

		float IXConverter<decimal, float>.Convert(decimal value)
			=> Convert.ToSingle(value);

		float IXConverter<double, float>.Convert(double value)
			=> Convert.ToSingle(value);

		float IXConverter<float, float>.Convert(float value)
			=> Convert.ToSingle(value);

		float IXConverter<ulong, float>.Convert(ulong value)
			=> Convert.ToSingle(value);

		float IXConverter<long, float>.Convert(long value)
			=> Convert.ToSingle(value);

		float IXConverter<uint, float>.Convert(uint value)
			=> Convert.ToSingle(value);

		float IXConverter<bool, float>.Convert(bool value)
			=> Convert.ToSingle(value);

		float IXConverter<ushort, float>.Convert(ushort value)
			=> Convert.ToSingle(value);

		float IXConverter<short, float>.Convert(short value)
			=> Convert.ToSingle(value);

		float IXConverter<char, float>.Convert(char value)
			=> Convert.ToSingle(value);

		float IXConverter<byte, float>.Convert(byte value)
			=> Convert.ToSingle(value);

		float IXConverter<sbyte, float>.Convert(sbyte value)
			=> Convert.ToSingle(value);

		float IXConverter<object, float>.Convert(object value)
			=> Convert.ToSingle(value);

		float IXConverter<int, float>.Convert(int value)
			=> Convert.ToSingle(value);

		float IXConverter<DateTime, float>.Convert(DateTime value)
			=> Convert.ToSingle(value);

		string IXConverter<string, string>.Convert(string value)
			=> Convert.ToString(value);

		string IXConverter<sbyte, string>.Convert(sbyte value)
			=> Convert.ToString(value);

		string IXConverter<DateTime, string>.Convert(DateTime value)
			=> Convert.ToString(value);

		string IXConverter<decimal, string>.Convert(decimal value)
			=> Convert.ToString(value);

		string IXConverter<double, string>.Convert(double value)
			=> Convert.ToString(value);

		string IXConverter<float, string>.Convert(float value)
			=> Convert.ToString(value);

		string IXConverter<ulong, string>.Convert(ulong value)
			=> Convert.ToString(value);

		string IXConverter<long, string>.Convert(long value)
			=> Convert.ToString(value);

		string IXConverter<uint, string>.Convert(uint value)
			=> Convert.ToString(value);

		string IXConverter<int, string>.Convert(int value)
			=> Convert.ToString(value);

		string IXConverter<ushort, string>.Convert(ushort value)
			=> Convert.ToString(value);

		string IXConverter<short, string>.Convert(short value)
			=> Convert.ToString(value);

		string IXConverter<byte, string>.Convert(byte value)
			=> Convert.ToString(value);

		string IXConverter<object, string>.Convert(object value)
			=> Convert.ToString(value);

		string IXConverter<char, string>.Convert(char value)
			=> Convert.ToString(value);

		string IXConverter<bool, string>.Convert(bool value)
			=> Convert.ToString(value);

		string IXConverter<Guid, string>.Convert(Guid value)
			=> ConvertAdd.ToString(value);

		string IXConverter<byte[], string>.Convert(byte[] value)
			=> Convert.ToBase64String(value);

		TypeCode IXConverter<object, TypeCode>.Convert(object value)
			=> Convert.GetTypeCode(value);

		ushort IXConverter<DateTime, ushort>.Convert(DateTime value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<string, ushort>.Convert(string value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<decimal, ushort>.Convert(decimal value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<double, ushort>.Convert(double value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<float, ushort>.Convert(float value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<ulong, ushort>.Convert(ulong value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<uint, ushort>.Convert(uint value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<long, ushort>.Convert(long value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<int, ushort>.Convert(int value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<short, ushort>.Convert(short value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<byte, ushort>.Convert(byte value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<sbyte, ushort>.Convert(sbyte value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<char, ushort>.Convert(char value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<bool, ushort>.Convert(bool value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<object, ushort>.Convert(object value)
			=> Convert.ToUInt16(value);

		ushort IXConverter<ushort, ushort>.Convert(ushort value)
			=> Convert.ToUInt16(value);

		uint IXConverter<byte, uint>.Convert(byte value)
			=> Convert.ToUInt32(value);

		uint IXConverter<object, uint>.Convert(object value)
			=> Convert.ToUInt32(value);

		uint IXConverter<bool, uint>.Convert(bool value)
			=> Convert.ToUInt32(value);

		uint IXConverter<char, uint>.Convert(char value)
			=> Convert.ToUInt32(value);

		uint IXConverter<sbyte, uint>.Convert(sbyte value)
			=> Convert.ToUInt32(value);

		uint IXConverter<short, uint>.Convert(short value)
			=> Convert.ToUInt32(value);

		uint IXConverter<ushort, uint>.Convert(ushort value)
			=> Convert.ToUInt32(value);

		uint IXConverter<int, uint>.Convert(int value)
			=> Convert.ToUInt32(value);

		uint IXConverter<uint, uint>.Convert(uint value)
			=> Convert.ToUInt32(value);

		uint IXConverter<long, uint>.Convert(long value)
			=> Convert.ToUInt32(value);

		uint IXConverter<ulong, uint>.Convert(ulong value)
			=> Convert.ToUInt32(value);

		uint IXConverter<float, uint>.Convert(float value)
			=> Convert.ToUInt32(value);

		uint IXConverter<double, uint>.Convert(double value)
			=> Convert.ToUInt32(value);

		uint IXConverter<decimal, uint>.Convert(decimal value)
			=> Convert.ToUInt32(value);

		uint IXConverter<string, uint>.Convert(string value)
			=> Convert.ToUInt32(value);

		uint IXConverter<DateTime, uint>.Convert(DateTime value)
			=> Convert.ToUInt32(value);

		ulong IXConverter<decimal, ulong>.Convert(decimal value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<double, ulong>.Convert(double value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<float, ulong>.Convert(float value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<ulong, ulong>.Convert(ulong value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<long, ulong>.Convert(long value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<uint, ulong>.Convert(uint value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<int, ulong>.Convert(int value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<sbyte, ulong>.Convert(sbyte value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<short, ulong>.Convert(short value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<byte, ulong>.Convert(byte value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<DateTime, ulong>.Convert(DateTime value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<char, ulong>.Convert(char value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<bool, ulong>.Convert(bool value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<object, ulong>.Convert(object value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<ushort, ulong>.Convert(ushort value)
			=> Convert.ToUInt64(value);

		ulong IXConverter<string, ulong>.Convert(string value)
			=> Convert.ToUInt64(value);

	}
}