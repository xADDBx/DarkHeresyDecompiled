using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal sealed class FinalTargetHandles : IDisposable
{
	public RTHandle ColorHandle;

	public RTHandle DepthHandle;

	public void Dispose()
	{
		ColorHandle?.Release();
		DepthHandle?.Release();
	}
}
