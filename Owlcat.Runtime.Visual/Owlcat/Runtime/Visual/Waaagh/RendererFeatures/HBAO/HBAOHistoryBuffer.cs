using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.HBAO;

public class HBAOHistoryBuffer
{
	public Camera camera { get; set; }

	public BufferedRTHandleSystem historyRTSystem { get; set; }

	public int frameCount { get; set; }

	public int lastRenderedFrame { get; set; }
}
