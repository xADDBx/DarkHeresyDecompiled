using System;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.FogOfWar;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;

internal sealed class FogOfWarRendererFeature : IRendererFeature, IDisposable
{
	private sealed class SetupPassData
	{
		public FogOfWarArea Area;

		public Color Color;

		public Vector2 MaskSize;

		public Texture2D DefaultFogOfWarMask;
	}

	private sealed class PostProcessPassData
	{
		public Material Material;

		public int ShaderPass;

		public TextureHandle CameraColor;

		public FogOfWarSettings Settings;

		public FogOfWarArea Area;
	}

	private sealed class ShadowmapPassData
	{
		public TextureHandle FowHistoryCopyRT;

		public TextureHandle FowBlur0RT;

		public TextureHandle FowBlur1RT;

		public Matrix4x4 Proj;

		public Matrix4x4 View;

		public FogOfWarArea Area;

		public FogOfWarSettings Settings;

		public Material FowMaterial;

		public Material BlurMaterial;

		public int FowClearPass;

		public int FowDrawShadowsPass;

		public int FowDrawCharacterQuadPass;

		public int FowDrawCharacterQuadMaskPass;

		public int FowFinalBlendPass;

		public int FowFinalBlendAndMaskPass;

		public int FowHistoryCopyPass;

		public Mesh QuadMesh;
	}

	private readonly FogOfWarRendererFeatureAsset m_Asset;

	private readonly Material m_FowMaterial;

	private readonly Material m_ScreenSpaceMat;

	private readonly Material m_BlurMaterial;

	private readonly Material m_BlitMat;

	private readonly Texture2D m_DefaultFogOfWarMask;

	private readonly Mesh m_QuadMesh;

	private readonly int m_FowClearPass;

	private readonly int m_FowDrawShadowsPass;

	private readonly int m_FowDrawCharacterQuadPass;

	private readonly int m_FowDrawCharacterQuadMaskPass;

	private readonly int m_FowFinalBlendPass;

	private readonly int m_FowFinalBlendAndMaskPass;

	private readonly int m_FowHistoryCopyPass;

	private readonly int m_ScreenSpaceMatShaderPass;

	private int m_LastFrameId = -1;

	private FogOfWarArea m_ActiveFogOfWarArea;

	private bool m_ShouldApplyFogOfWar;

	private bool m_ShouldDrawShadowMap;

	public FogOfWarRendererFeature(FogOfWarRendererFeatureAsset asset)
	{
		m_Asset = asset;
		FogOfWarFeatureResources renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<FogOfWarFeatureResources>();
		m_FowMaterial = CoreUtils.CreateEngineMaterial(renderPipelineSettings.FogOfWarShader);
		m_ScreenSpaceMat = CoreUtils.CreateEngineMaterial(renderPipelineSettings.ScreenSpaceFogOfWarShader);
		m_BlurMaterial = CoreUtils.CreateEngineMaterial(renderPipelineSettings.BlurShader);
		m_BlitMat = CoreUtils.CreateEngineMaterial(renderPipelineSettings.BlitShader);
		m_DefaultFogOfWarMask = CreateDefaultFogOfWarMask();
		m_QuadMesh = CreateQuadMesh();
		m_FowClearPass = m_FowMaterial.FindPass("FOW CLEAR");
		m_FowDrawShadowsPass = m_FowMaterial.FindPass("FOW DRAW SHADOWS");
		m_FowDrawCharacterQuadPass = m_FowMaterial.FindPass("DRAW CHARACTER QUAD");
		m_FowDrawCharacterQuadMaskPass = m_FowMaterial.FindPass("DRAW CHARACTER QUAD MASK");
		m_FowFinalBlendPass = m_FowMaterial.FindPass("FINAL BLEND");
		m_FowFinalBlendAndMaskPass = m_FowMaterial.FindPass("FINAL BLEND AND STATIC MASK");
		m_FowHistoryCopyPass = m_FowMaterial.FindPass("HISTORY COPY");
		m_ScreenSpaceMatShaderPass = m_ScreenSpaceMat.FindPass("DRAW FOW SCREEN SPACE");
	}

	public void Dispose()
	{
		CoreUtils.Destroy(m_FowMaterial);
		CoreUtils.Destroy(m_ScreenSpaceMat);
		CoreUtils.Destroy(m_BlurMaterial);
		CoreUtils.Destroy(m_BlitMat);
		CoreUtils.Destroy(m_DefaultFogOfWarMask);
		CoreUtils.Destroy(m_QuadMesh);
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddSetupDelegate(OnSetup);
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeRendering, OnBeforeRendering);
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeDrawTransparent2, OnBeforeDrawTransparent);
		registry.AddRecordDelegate(RecordExtensionPoint.AfterRendering, OnAfterRendering);
	}

	private void OnSetup(in SetupContext context)
	{
		m_ActiveFogOfWarArea = ((FogOfWarArea.Active != null && FogOfWarArea.Active.isActiveAndEnabled) ? FogOfWarArea.Active : null);
		CameraType cameraType = context.CameraData.cameraType;
		m_ShouldApplyFogOfWar = cameraType == CameraType.Game || cameraType == CameraType.SceneView;
		m_ShouldDrawShadowMap = m_LastFrameId != context.RenderingData.TimeData.FrameId;
		m_LastFrameId = context.RenderingData.TimeData.FrameId;
		if (!m_ShouldApplyFogOfWar || !(m_ActiveFogOfWarArea != null) || m_ActiveFogOfWarArea.FogOfWarMapRT == null || !m_ShouldDrawShadowMap)
		{
			return;
		}
		foreach (FogOfWarRevealer item in FogOfWarRevealer.All)
		{
			if (item != null)
			{
				item.HeightMinMax = m_Asset.Settings.CalculateHeightMinMax(item.Position.y);
				item.RebuildShadowMesh();
			}
		}
	}

	private void OnBeforeRendering(in RecordContext context)
	{
		if (m_ShouldApplyFogOfWar)
		{
			if (m_ActiveFogOfWarArea != null && m_ActiveFogOfWarArea.FogOfWarMapRT != null && m_ShouldDrawShadowMap)
			{
				DrawShadowMap(in context);
			}
			SetupFogOfWar(in context);
		}
	}

	private void OnBeforeDrawTransparent(in RecordContext context)
	{
		if (m_ShouldApplyFogOfWar && m_ActiveFogOfWarArea != null && m_ActiveFogOfWarArea.FogOfWarMapRT != null)
		{
			ApplyPostProcess(in context);
		}
	}

	private void OnAfterRendering(in RecordContext context)
	{
		if (m_ShouldApplyFogOfWar)
		{
			CleanupFogOfWar(in context);
		}
	}

	private void DrawShadowMap(in RecordContext context)
	{
		ShadowmapPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<ShadowmapPassData>("FogOfWar DrawShadowMap", out passData2, WaaaghProfileId.FogOfWarDrawShadowMap.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\FogOfWar\\FogOfWarRendererFeature.cs", 180);
		if (!m_ActiveFogOfWarArea.RevealOnStart)
		{
			TextureDesc desc = new TextureDesc(m_ActiveFogOfWarArea.FogOfWarMapRT.rt.width, m_ActiveFogOfWarArea.FogOfWarMapRT.rt.height);
			desc.name = "_FOWHistoryCopy";
			desc.colorFormat = GraphicsFormat.R8_UNorm;
			desc.filterMode = FilterMode.Bilinear;
			desc.wrapMode = TextureWrapMode.Clamp;
			passData2.FowHistoryCopyRT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		}
		passData2.Proj = m_ActiveFogOfWarArea.CalculateProjMatrix();
		passData2.View = m_ActiveFogOfWarArea.CalculateViewMatrix();
		passData2.Area = m_ActiveFogOfWarArea;
		passData2.Settings = m_Asset.Settings;
		int width = m_ActiveFogOfWarArea.FogOfWarMapRT.rt.width >> passData2.Settings.BlurDownsample;
		int height = m_ActiveFogOfWarArea.FogOfWarMapRT.rt.height >> passData2.Settings.BlurDownsample;
		TextureDesc desc2 = new TextureDesc(width, height);
		desc2.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		desc2.filterMode = FilterMode.Bilinear;
		desc2.wrapMode = TextureWrapMode.Clamp;
		desc2.name = "FowBlur0RT";
		passData2.FowBlur0RT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc2);
		desc2.name = "FowBlur1RT";
		passData2.FowBlur1RT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc2);
		passData2.FowMaterial = m_FowMaterial;
		passData2.BlurMaterial = m_BlurMaterial;
		passData2.FowClearPass = m_FowClearPass;
		passData2.FowDrawShadowsPass = m_FowDrawShadowsPass;
		passData2.FowDrawCharacterQuadPass = m_FowDrawCharacterQuadPass;
		passData2.FowDrawCharacterQuadMaskPass = m_FowDrawCharacterQuadMaskPass;
		passData2.FowFinalBlendPass = m_FowFinalBlendPass;
		passData2.FowFinalBlendAndMaskPass = m_FowFinalBlendAndMaskPass;
		passData2.FowHistoryCopyPass = m_FowHistoryCopyPass;
		passData2.QuadMesh = m_QuadMesh;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(ShadowmapPassData passData, UnsafeGraphContext context)
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			if (!passData.Area.RevealOnStart)
			{
				context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarShadowMap, passData.Area.FogOfWarMapRT);
				nativeCommandBuffer.Blit(null, passData.FowHistoryCopyRT, passData.FowMaterial, passData.FowHistoryCopyPass);
			}
			context.cmd.SetRenderTarget(passData.Area.FogOfWarMapRT);
			context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._ColorMask, 15f);
			context.cmd.SetGlobalColor(FogOfWarConstantBuffer._ClearColor, new Color(0f, passData.Area.RevealOnStart ? 1 : 0, 0f, 1f));
			nativeCommandBuffer.Blit(null, passData.Area.FogOfWarMapRT, passData.FowMaterial, passData.FowClearPass);
			context.cmd.SetGlobalMatrix(FogOfWarConstantBuffer.VIEW_PROJ, passData.Proj * passData.View);
			bool flag = true;
			foreach (FogOfWarRevealer item in FogOfWarRevealer.All)
			{
				context.cmd.EnableScissorRect(CalculateScissorRect(item, passData.Area));
				if (!flag)
				{
					context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._ColorMask, 1f);
					context.cmd.SetGlobalColor(FogOfWarConstantBuffer._ClearColor, new Color(0f, 0f, 0f, 1f));
					nativeCommandBuffer.Blit(null, passData.Area.FogOfWarMapRT, passData.FowMaterial, passData.FowClearPass);
				}
				context.cmd.SetGlobalVectorArray(FogOfWarConstantBuffer._Vertices, FogOfWarRevealer.DefaultVertices);
				context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._Radius, item.Radius);
				Vector3 size = passData.Area.Bounds.size;
				context.cmd.SetGlobalVector("_FogOfWarAreaSize", new Vector4(size.x, size.z));
				context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarRadius, item.Range);
				context.cmd.SetGlobalVector(FogOfWarConstantBuffer._LightPosition, new Vector2(item.Position.x, item.Position.z));
				context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._Falloff, passData.Settings.ShadowFalloff);
				context.cmd.DrawMesh(item.ShadowMesh, Matrix4x4.identity, passData.FowMaterial, 0, passData.FowDrawShadowsPass);
				if (item.MaskTexture == null)
				{
					Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(item.Range, 1f, item.Range), pos: item.Position, q: Quaternion.identity);
					context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarRadius, item.Range);
					context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarBorderWidth, passData.Settings.BorderWidth);
					context.cmd.DrawMesh(passData.QuadMesh, matrix, passData.FowMaterial, 0, passData.FowDrawCharacterQuadPass);
				}
				else
				{
					Matrix4x4 matrix2 = Matrix4x4.TRS(s: new Vector3(item.Scale.x, 1f, item.Scale.y), pos: item.Position, q: Quaternion.Euler(new Vector3(0f, item.Rotation, 0f)));
					context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarCustomRevealerMask, item.MaskTexture);
					context.cmd.DrawMesh(passData.QuadMesh, matrix2, passData.FowMaterial, 0, passData.FowDrawCharacterQuadMaskPass);
				}
				context.cmd.DisableScissorRect();
				flag = false;
			}
			if (passData.Settings.IsBlurEnabled && passData.Area.IsBlurEnabled)
			{
				float num = 1f / (1f * (float)(1 << passData.Settings.BlurDownsample));
				context.cmd.SetGlobalVector(FogOfWarConstantBuffer._Parameter, new Vector4(passData.Settings.BlurSize * num, (0f - passData.Settings.BlurSize) * num, 0f, 0f));
				nativeCommandBuffer.Blit(passData.Area.FogOfWarMapRT, passData.FowBlur0RT, passData.BlurMaterial, 0);
				int num2 = ((passData.Settings.BlurType != 0) ? 2 : 0);
				for (int i = 0; i < passData.Settings.BlurIterations; i++)
				{
					float num3 = (float)i * 1f;
					context.cmd.SetGlobalVector(FogOfWarConstantBuffer._Parameter, new Vector4(passData.Settings.BlurSize * num + num3, (0f - passData.Settings.BlurSize) * num - num3, 0f, 0f));
					nativeCommandBuffer.Blit(passData.FowBlur0RT, passData.FowBlur1RT, passData.BlurMaterial, 1 + num2);
					nativeCommandBuffer.Blit(passData.FowBlur1RT, passData.FowBlur0RT, passData.BlurMaterial, 2 + num2);
				}
				nativeCommandBuffer.Blit(passData.FowBlur0RT, passData.Area.FogOfWarMapRT);
			}
			context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._BorderEnabled, passData.Area.BorderSettings.Enabled ? 1 : 0);
			if (passData.Area.BorderSettings.Enabled)
			{
				context.cmd.SetGlobalVector(FogOfWarConstantBuffer._WorldSize, passData.Area.Bounds.size.To2D());
				context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._BorderWidth, passData.Area.BorderSettings.Width);
				context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._BorderOffset, passData.Area.BorderSettings.Offset);
			}
			if (!passData.Area.RevealOnStart)
			{
				context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarHistoryCopyMap, passData.FowHistoryCopyRT);
			}
			context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarRevealOnStart, passData.Area.RevealOnStart ? 1 : 0);
			if (passData.Area.StaticMask == null)
			{
				nativeCommandBuffer.Blit(null, passData.Area.FogOfWarMapRT, passData.FowMaterial, passData.FowFinalBlendPass);
			}
			else
			{
				context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._StaticMask, passData.Area.StaticMask);
				nativeCommandBuffer.Blit(null, passData.Area.FogOfWarMapRT, passData.FowMaterial, passData.FowFinalBlendAndMaskPass);
			}
		});
	}

	private void SetupFogOfWar(in RecordContext context)
	{
		SetupPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<SetupPassData>("FogOfWar Setup", out passData2, WaaaghProfileId.FogOfWarSetup.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\FogOfWar\\FogOfWarRendererFeature.cs", 355);
		if (m_ActiveFogOfWarArea != null)
		{
			passData2.Area = m_ActiveFogOfWarArea;
			passData2.Color = m_Asset.Settings.Color;
			passData2.MaskSize.x = m_ActiveFogOfWarArea.FogOfWarMapRT.rt.width;
			passData2.MaskSize.y = m_ActiveFogOfWarArea.FogOfWarMapRT.rt.height;
			passData2.DefaultFogOfWarMask = null;
		}
		else
		{
			passData2.Area = null;
			passData2.Color = default(Color);
			passData2.MaskSize = default(Vector2);
			passData2.DefaultFogOfWarMask = m_DefaultFogOfWarMask;
		}
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(SetupPassData passData, UnsafeGraphContext context)
		{
			if (passData.Area != null)
			{
				context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarMask, passData.Area.FogOfWarMapRT);
				context.cmd.SetGlobalVector(FogOfWarConstantBuffer._FogOfWarMask_ST, passData.Area.CalculateMaskST());
				context.cmd.SetGlobalColor(FogOfWarConstantBuffer._FogOfWarColor, passData.Color);
				context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarGlobalFlag, (!passData.Area.ApplyShaderManually) ? 1 : 0);
				context.cmd.SetGlobalVector(FogOfWarConstantBuffer._FogOfWarMaskSize, passData.MaskSize);
			}
			else
			{
				context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarGlobalFlag, 0f);
				context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarMask, passData.DefaultFogOfWarMask);
			}
		});
	}

	private static void CleanupFogOfWar(in RecordContext context)
	{
		object passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<object>("FogOfWar Cleanup", out passData, WaaaghProfileId.FogOfWarCleanup.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\FogOfWar\\FogOfWarRendererFeature.cs", 396);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(object _, UnsafeGraphContext context)
		{
			context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarGlobalFlag, 0f);
		});
	}

	private void ApplyPostProcess(in RecordContext context)
	{
		PostProcessPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PostProcessPassData>("FogOfWar PostProcess", out passData2, WaaaghProfileId.FogOfWarPostProcess.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\FogOfWar\\FogOfWarRendererFeature.cs", 409);
		passData2.Material = m_ScreenSpaceMat;
		passData2.ShaderPass = m_ScreenSpaceMatShaderPass;
		passData2.CameraColor = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData2.CameraColor, AccessFlags.Write);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PostProcessPassData passData, UnsafeGraphContext context)
		{
			CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).Blit(BuiltinRenderTextureType.None, passData.CameraColor, passData.Material, passData.ShaderPass);
		});
	}

	private static Rect CalculateScissorRect(FogOfWarRevealer revealer, FogOfWarArea area)
	{
		float2 @float = revealer.Range;
		if (revealer.MaskTexture != null)
		{
			@float.x = revealer.Scale.x;
			@float.y = revealer.Scale.y;
		}
		Bounds worldBounds = area.GetWorldBounds();
		float4 float2 = new float4(worldBounds.min.x, worldBounds.min.z, worldBounds.max.x, worldBounds.max.z);
		float4 float3 = math.saturate((new float4(revealer.Position.x - @float.x, revealer.Position.z - @float.y, revealer.Position.x + @float.x, revealer.Position.z + @float.y) - float2.xyxy) / (float2.zw - float2.xy).xyxy) * new float2(area.FogOfWarMapRT.rt.width, area.FogOfWarMapRT.rt.height).xyxy;
		return new Rect((Vector2)float3.xy, (Vector2)(float3.zw - float3.xy));
	}

	private static Texture2D CreateDefaultFogOfWarMask()
	{
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		texture2D.name = "FogOfWarDefaultMask";
		texture2D.hideFlags = HideFlags.DontSave;
		texture2D.SetPixel(0, 0, new Color(1f, 1f, 0f, 0f));
		texture2D.Apply();
		return texture2D;
	}

	private static Mesh CreateQuadMesh()
	{
		Mesh mesh = new Mesh
		{
			name = "FogOfWarQuad",
			hideFlags = HideFlags.DontSave
		};
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(1f, 0f, 1f),
			new Vector3(1f, 0f, -1f),
			new Vector3(-1f, 0f, 1f),
			new Vector3(-1f, 0f, -1f)
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(1f, 1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(0f, 0f)
		};
		int[] triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		return mesh;
	}
}
