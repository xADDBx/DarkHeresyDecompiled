using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class Dithering
{
	private class PassData
	{
		public Texture2D GlobalDitheringTexture;

		public Vector4 GlobalDitheringParams;
	}

	public static void SetupDitheringPass(in RecordContext context, bool allowJitter)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("SetupDitheringPass", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\Dithering.cs", 22);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		WaaaghCameraData cameraData = context.CameraData;
		DitheringSettings ditheringSettings = WaaaghPipeline.Asset.DitheringSettings;
		Matrix4x4 jitterMatrix = cameraData.GetJitterMatrix();
		Vector2 vector = ditheringSettings.JitterScale * new Vector2(jitterMatrix.m03 * (float)cameraData.scaledWidth, jitterMatrix.m13 * (float)cameraData.scaledHeight);
		vector += new Vector2(0.5f, 0.5f);
		int frameIndex = ((allowJitter && ditheringSettings.AnimateBlueNoiseWithTemporalAA && cameraData.IsTemporalAAEnabled()) ? (Time.frameCount + cameraData.taaSettings.jitterFrameCountOffset) : 0);
		Texture2D blackTexture = Texture2D.blackTexture;
		passData.GlobalDitheringTexture = ResolveDitheringTextureOrDefault(context.Textures, ditheringSettings.TextureType, frameIndex, blackTexture);
		passData.GlobalDitheringParams = new Vector4(passData.GlobalDitheringTexture.width + (int)vector.x, passData.GlobalDitheringTexture.height + (int)vector.y, passData.GlobalDitheringTexture.width - 1, passData.GlobalDitheringTexture.height - 1);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			context.cmd.SetGlobalVector(ShaderPropertyId._GlobalDitheringParams, data.GlobalDitheringParams);
			context.cmd.SetGlobalTexture(ShaderPropertyId._GlobalDitheringTexture, data.GlobalDitheringTexture);
		});
	}

	private static Texture2D ResolveDitheringTextureOrDefault(RenderRuntimeTextures textureResources, DitheringTextureType textureType, int frameIndex, Texture2D defaultTexture)
	{
		switch (textureType)
		{
		case DitheringTextureType.BayerMatrix:
			return textureResources.bayerMatrixTex;
		case DitheringTextureType.BlueNoise:
		{
			Texture2D[] blueNoise64Textures = textureResources.BlueNoise64Textures;
			return blueNoise64Textures[frameIndex % blueNoise64Textures.Length];
		}
		default:
			Debug.LogWarning($"This Dithering Texture Type is not supported: {textureType}");
			return defaultTexture;
		}
	}
}
