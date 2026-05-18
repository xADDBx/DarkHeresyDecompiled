using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture;

public static class ShaderPropertyId
{
	public static readonly int _VTAtlas = Shader.PropertyToID("_VTAtlas");

	public static readonly int _VTIndirectTex = Shader.PropertyToID("_VTIndirectTex");

	public static readonly int _VTTextureStackDataBuffer = Shader.PropertyToID("_VTTextureStackDataBuffer");

	public static readonly int _VTFeedbackBuffer = Shader.PropertyToID("_VTFeedbackBuffer");

	public static readonly int _VTFeedbackBufferLength = Shader.PropertyToID("_VTFeedbackBufferLength");

	public static readonly int _VTFeedbackRT = Shader.PropertyToID("_VTFeedbackRT");

	public static readonly int _VirtualAtlasWidthInTiles = Shader.PropertyToID("_VirtualAtlasWidthInTiles");

	public static readonly int _RawData = Shader.PropertyToID("_RawData");

	public static readonly int _Result = Shader.PropertyToID("_Result");

	public static readonly int _RawDataOffset = Shader.PropertyToID("_RawDataOffset");

	public static readonly int _RawDataWidth = Shader.PropertyToID("_RawDataWidth");

	public static readonly int _LimitX = Shader.PropertyToID("_LimitX");

	public static readonly int _LimitY = Shader.PropertyToID("_LimitY");

	public static readonly int _MipWidth = Shader.PropertyToID("_MipWidth");

	public static readonly int _VTStackId = Shader.PropertyToID("_VTStackId");

	public static readonly int _VTStackIndices = Shader.PropertyToID("_VTStackIndices");
}
