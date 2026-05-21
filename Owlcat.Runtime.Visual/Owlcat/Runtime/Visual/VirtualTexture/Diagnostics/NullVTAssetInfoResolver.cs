using System;

namespace Owlcat.Runtime.Visual.VirtualTexture.Diagnostics;

public sealed class NullVTAssetInfoResolver : IVTAssetInfoResolver
{
	public static readonly NullVTAssetInfoResolver Instance = new NullVTAssetInfoResolver();

	public string ResolveTextureGuid(Guid guid)
	{
		if (!(guid == Guid.Empty))
		{
			return guid.ToString("N");
		}
		return "<empty>";
	}

	public string ResolveMaterialInstance(int instanceId)
	{
		return null;
	}
}
