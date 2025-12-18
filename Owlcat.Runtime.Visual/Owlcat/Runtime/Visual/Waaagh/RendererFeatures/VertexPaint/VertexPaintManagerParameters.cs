using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VertexPaint;

[Serializable]
public class VertexPaintManagerParameters
{
	[Range(1f, 209715200f)]
	public int InitialCapacity = 32768;

	[Range(1f, 209715200f)]
	public int MaxCapacity = 52428800;

	[Range(1f, 131072f)]
	public int MaxAllocs = 32768;

	[Header("Debug")]
	public bool Logging;
}
