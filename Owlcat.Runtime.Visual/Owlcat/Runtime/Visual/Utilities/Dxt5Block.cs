using System.Runtime.InteropServices;

namespace Owlcat.Runtime.Visual.Utilities;

[StructLayout(LayoutKind.Sequential, Size = 16)]
internal readonly struct Dxt5Block
{
	public const int kSizeInBytes = 16;

	public const int kSizeInTexels = 4;

	public static readonly Dxt5Block Normal = new Dxt5Block(byte.MaxValue, byte.MaxValue, 73, 146, 36, 73, 146, 36, 223, 131, 31, 124, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static readonly Dxt5Block Clear = new Dxt5Block(0, 0, 73, 146, 36, 73, 146, 36, 0, 0, 0, 0, 170, 170, 170, 170);

	public static readonly Dxt5Block Black = new Dxt5Block(byte.MaxValue, byte.MaxValue, 73, 146, 36, 73, 146, 36, 0, 0, 0, 0, 170, 170, 170, 170);

	public static readonly Dxt5Block White = new Dxt5Block(byte.MaxValue, byte.MaxValue, 73, 146, 36, 73, 146, 36, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, 170, 170, 170, 170);

	public static readonly Dxt5Block Gray = new Dxt5Block(byte.MaxValue, byte.MaxValue, 73, 146, 36, 73, 146, 36, 241, 139, 15, 124, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static readonly Dxt5Block Red = new Dxt5Block(byte.MaxValue, byte.MaxValue, 73, 146, 36, 73, 146, 36, 0, 248, 0, 248, 170, 170, 170, 170);

	public static readonly Dxt5Block Green = new Dxt5Block(byte.MaxValue, byte.MaxValue, 73, 146, 36, 73, 146, 36, 224, 7, 224, 7, 170, 170, 170, 170);

	public static readonly Dxt5Block Blue = new Dxt5Block(byte.MaxValue, byte.MaxValue, 73, 146, 36, 73, 146, 36, 31, 0, 31, 0, 170, 170, 170, 170);

	public static readonly Dxt5Block Yellow = new Dxt5Block(byte.MaxValue, byte.MaxValue, 73, 146, 36, 73, 146, 36, 224, byte.MaxValue, 224, byte.MaxValue, 170, 170, 170, 170);

	public static readonly Dxt5Block Magenta = new Dxt5Block(byte.MaxValue, byte.MaxValue, 73, 146, 36, 73, 146, 36, 31, 248, 31, 248, 170, 170, 170, 170);

	public static readonly Dxt5Block Cyan = new Dxt5Block(byte.MaxValue, byte.MaxValue, 73, 146, 36, 73, 146, 36, byte.MaxValue, 7, byte.MaxValue, 7, 170, 170, 170, 170);

	public readonly byte Value0;

	public readonly byte Value1;

	public readonly byte Value2;

	public readonly byte Value3;

	public readonly byte Value4;

	public readonly byte Value5;

	public readonly byte Value6;

	public readonly byte Value7;

	public readonly byte Value8;

	public readonly byte Value9;

	public readonly byte Value10;

	public readonly byte Value11;

	public readonly byte Value12;

	public readonly byte Value13;

	public readonly byte Value14;

	public readonly byte Value15;

	public Dxt5Block(byte value0, byte value1, byte value2, byte value3, byte value4, byte value5, byte value6, byte value7, byte value8, byte value9, byte value10, byte value11, byte value12, byte value13, byte value14, byte value15)
	{
		Value0 = value0;
		Value1 = value1;
		Value2 = value2;
		Value3 = value3;
		Value4 = value4;
		Value5 = value5;
		Value6 = value6;
		Value7 = value7;
		Value8 = value8;
		Value9 = value9;
		Value10 = value10;
		Value11 = value11;
		Value12 = value12;
		Value13 = value13;
		Value14 = value14;
		Value15 = value15;
	}
}
