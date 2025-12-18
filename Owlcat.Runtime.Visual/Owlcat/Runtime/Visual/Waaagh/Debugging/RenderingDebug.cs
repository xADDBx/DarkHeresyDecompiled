using System;
using Owlcat.ShaderLibrary.Visual.Debug;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
public class RenderingDebug
{
	public DebugMapOverlay DebugMapOverlay;

	public float MapSize = 0.5f;

	public DebugMaterialMode DebugMaterialMode;

	public DebugOverdrawMode OverdrawMode;

	public QuadOverdrawSettings QuadOverdrawSettings = new QuadOverdrawSettings();

	public bool DebugMipMap;
}
