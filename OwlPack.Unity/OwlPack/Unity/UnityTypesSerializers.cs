using UnityEngine;

namespace OwlPack.Unity;

public static class UnityTypesSerializers
{
	private static bool s_Initialized;

	[RuntimeInitializeOnLoadMethod]
	public static void Register()
	{
		if (!s_Initialized)
		{
			Vector2Serializer.Register();
			Vector3Serializer.Register();
			Vector4Serializer.Register();
			QuaternionSerializer.Register();
			s_Initialized = true;
		}
	}
}
