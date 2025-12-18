using UnityEngine;

namespace Owlcat.Runtime.Visual.Terrain;

public static class OwlcatTerrainTransition
{
	public static bool Active { get; set; }

	public static Vector3 ShapeCenter { get; set; }

	public static float ShapeRadius { get; set; }

	public static float ShapeBlendWidth { get; set; }

	public static Vector4 MakeTransitionClipShape()
	{
		return MakeTransitionClipShape(ShapeCenter, ShapeRadius, ShapeBlendWidth);
	}

	public static Vector4 MakeTransitionClipShape(Vector3 origin, float radius, float blendWidth)
	{
		float num = radius * radius;
		float num2 = Mathf.Clamp(radius - blendWidth, 0f, radius - 0.001f);
		num2 *= num2;
		float num3 = -1f / (num - num2);
		float w = (0f - num3) * num;
		return new Vector4(origin.x, origin.z, num3, w);
	}
}
