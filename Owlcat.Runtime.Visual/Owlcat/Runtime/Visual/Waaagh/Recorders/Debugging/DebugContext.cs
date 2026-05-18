using System;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Shadows;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

public sealed class DebugContext : IDisposable
{
	private readonly DebugMaterialLibrary m_MaterialLibrary;

	private readonly DebugMipMapTexture m_MipMapTexture;

	private readonly WaaaghDebugData m_DebugData;

	private readonly NativeQuadTreeDebugger m_QuadTreeDebugger;

	public WaaaghDebugData DebugData => m_DebugData;

	public DebugMipMapTexture DebugMipMapTexture => m_MipMapTexture;

	public DebugMaterialLibrary MaterialLibrary => m_MaterialLibrary;

	internal NativeQuadTreeDebugger QuadTreeDebugger => m_QuadTreeDebugger;

	public DebugContext(WaaaghDebugData debugData)
	{
		m_DebugData = debugData;
		m_MipMapTexture = new DebugMipMapTexture();
		m_MaterialLibrary = new DebugMaterialLibrary(m_DebugData.Resources);
		m_QuadTreeDebugger = new NativeQuadTreeDebugger();
	}

	public void Dispose()
	{
		m_MipMapTexture.Dispose();
		m_MaterialLibrary.Dispose();
		m_QuadTreeDebugger.Dispose();
	}
}
