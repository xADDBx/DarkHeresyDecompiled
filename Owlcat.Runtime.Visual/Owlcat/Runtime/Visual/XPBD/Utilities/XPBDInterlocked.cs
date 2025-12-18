using System.Threading;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Utilities;

public static class XPBDInterlocked
{
	public static float AddFloat(ref int target, float value)
	{
		int num = target;
		int num2;
		do
		{
			num2 = num;
			num = math.asint(value + math.asfloat(num2));
			num = Interlocked.CompareExchange(ref target, num, num2);
		}
		while (num2 != num);
		return math.asfloat(num2);
	}

	public static float3 AddFloat3(ref int3 target, float3 value)
	{
		float x = AddFloat(ref target.x, value.x);
		float y = AddFloat(ref target.y, value.y);
		float z = AddFloat(ref target.z, value.z);
		return new float3(x, y, z);
	}
}
