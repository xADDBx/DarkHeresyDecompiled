using System;

namespace Owlcat.Runtime.Visual.VirtualTexture.Diagnostics;

public interface IVTAssetInfoResolver
{
	string ResolveTextureGuid(Guid guid);

	string ResolveMaterialInstance(int instanceId);
}
