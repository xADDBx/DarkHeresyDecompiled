using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

internal static class WaaaghProfileIdExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ProfilingSampler Sampler(this WaaaghProfileId id)
	{
		return ProfilingSamplerStorage<WaaaghProfileId>.Get(id);
	}
}
