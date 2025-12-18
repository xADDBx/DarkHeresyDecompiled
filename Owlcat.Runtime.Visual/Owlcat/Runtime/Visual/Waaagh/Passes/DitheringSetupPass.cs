using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DitheringSetupPass : ScriptableRenderPass<DitheringSetupPassData>
{
	private readonly bool m_AllowJitter;

	private readonly RenderRuntimeTextures m_SharedTextureResources;

	public override string Name => "DitheringSetupPass";

	public DitheringSetupPass(RenderPassEvent evt, bool allowJitter)
		: base(evt)
	{
		m_AllowJitter = allowJitter;
		m_SharedTextureResources = GraphicsSettings.GetRenderPipelineSettings<RenderRuntimeTextures>();
	}

	protected override void Setup(RenderGraphBuilder builder, DitheringSetupPassData data, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		DitheringSettings ditheringSettings = WaaaghPipeline.Asset.DitheringSettings;
		Matrix4x4 jitterMatrix = waaaghCameraData.GetJitterMatrix();
		Vector2 vector = ditheringSettings.JitterScale * new Vector2(jitterMatrix.m03 * (float)waaaghCameraData.scaledWidth, jitterMatrix.m13 * (float)waaaghCameraData.scaledHeight);
		vector += new Vector2(0.5f, 0.5f);
		int frameIndex = ((m_AllowJitter && ditheringSettings.AnimateBlueNoiseWithTemporalAA && waaaghCameraData.IsTemporalAAEnabled()) ? (Time.frameCount + waaaghCameraData.taaSettings.jitterFrameCountOffset) : 0);
		Texture2D blackTexture = Texture2D.blackTexture;
		data.GlobalDitheringTexture = ResolveDitheringTextureOrDefault(ditheringSettings.TextureType, frameIndex, blackTexture);
		data.GlobalDitheringParams = new Vector4(data.GlobalDitheringTexture.width + (int)vector.x, data.GlobalDitheringTexture.height + (int)vector.y, data.GlobalDitheringTexture.width - 1, data.GlobalDitheringTexture.height - 1);
	}

	protected override void Render(DitheringSetupPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalVector(ShaderPropertyId._GlobalDitheringParams, data.GlobalDitheringParams);
		context.cmd.SetGlobalTexture(ShaderPropertyId._GlobalDitheringTexture, data.GlobalDitheringTexture);
	}

	private Texture2D ResolveDitheringTextureOrDefault(DitheringTextureType textureType, int frameIndex, Texture2D defaultTexture)
	{
		switch (textureType)
		{
		case DitheringTextureType.BayerMatrix:
			return m_SharedTextureResources.bayerMatrixTex;
		case DitheringTextureType.BlueNoise:
		{
			Texture2D[] blueNoise64Textures = m_SharedTextureResources.BlueNoise64Textures;
			return blueNoise64Textures[frameIndex % blueNoise64Textures.Length];
		}
		default:
			UnityEngine.Debug.LogWarning($"This Dithering Texture Type is not supported: {textureType}");
			return defaultTexture;
		}
	}
}
