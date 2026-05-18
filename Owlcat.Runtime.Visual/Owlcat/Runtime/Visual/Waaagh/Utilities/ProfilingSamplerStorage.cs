using System;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Utilities;

internal static class ProfilingSamplerStorage<TProfileId> where TProfileId : struct, Enum
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ProfilingSampler Get(string name, TProfileId? profileId)
	{
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ProfilingSampler Get(TProfileId profileId)
	{
		return null;
	}
}
