using System;
using Owlcat.Runtime.Visual.ShaderGlobals;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ShaderGlobals;

internal sealed class ShaderGlobalsRendererFeature : IRendererFeature, IDisposable
{
	private sealed class SetupPassData
	{
		public ShaderGlobalsState State;
	}

	private readonly ShaderGlobalsRendererFeatureAsset m_Asset;

	public ShaderGlobalsRendererFeature(ShaderGlobalsRendererFeatureAsset asset)
	{
		m_Asset = asset;
	}

	public void Dispose()
	{
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeRendering, OnBeforeRendering);
	}

	private void OnBeforeRendering(in RecordContext context)
	{
		ShaderGlobalsCommon.EnsureConfig();
		SetupPassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<SetupPassData>("Setup Shader Globals", out passData2, WaaaghProfileId.SetupShaderGlobals.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\ShaderGlobals\\ShaderGlobalsRendererFeature.cs", 44);
		passData2.State = ShaderGlobalsState.Instance;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(SetupPassData passData, UnsafeGraphContext context)
		{
			passData.State.UploadToGPU(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd));
		});
	}
}
