using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VertexPaint;

public sealed class VertexColorDataAsset : ScriptableObject
{
	public byte[] RawColors = Array.Empty<byte>();
}
