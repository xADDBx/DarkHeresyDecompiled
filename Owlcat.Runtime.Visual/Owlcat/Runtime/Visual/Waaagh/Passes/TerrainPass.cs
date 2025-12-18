using Owlcat.Runtime.Visual.Terrain;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

internal sealed class TerrainPass : ScriptableRenderPass<TerrainPass.PassData>
{
	public sealed class PassData : PassDataBase
	{
		public bool TransitionActive;

		public RendererList RendererList;

		public TextureHandle CameraDepthBuffer;

		public TextureHandle CameraAlbedoRT;

		public TextureHandle CameraSpecularRT;

		public TextureHandle CameraNormalsRT;

		public TextureHandle CameraEmissionRT;

		public TextureHandle CameraBakedGIRT;

		public TextureHandle CameraShadowmaskRT;

		public TextureHandle CameraTranslucencyRT;
	}

	public override string Name => "Terrain Pass";

	private protected override WaaaghProfileId? ProfileId => WaaaghProfileId.TerrainPass;

	public TerrainPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	public override void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		base.ConfigureRendererLists(context, frameData);
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		DependsOn(in waaaghRendererListData.TerrainGBuffer.List);
	}

	protected override void Setup(RenderGraphBuilder builder, PassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		data.RendererList = waaaghRendererListData.TerrainGBuffer.List;
		data.TransitionActive = OwlcatTerrainTransition.Active;
		TextureHandle input = waaaghResourceData.CameraDepthBuffer;
		data.CameraDepthBuffer = builder.UseDepthBuffer(in input, DepthAccess.ReadWrite);
		data.CameraAlbedoRT = builder.UseColorBuffer(in waaaghResourceData.CameraAlbedoRT, 0);
		data.CameraSpecularRT = builder.UseColorBuffer(in waaaghResourceData.CameraSpecularRT, 1);
		data.CameraNormalsRT = builder.UseColorBuffer(in waaaghResourceData.CameraNormalsRT, 2);
		input = waaaghResourceData.CameraColorBuffer;
		data.CameraEmissionRT = builder.UseColorBuffer(in input, 3);
		data.CameraTranslucencyRT = builder.UseColorBuffer(in waaaghResourceData.CameraTranslucencyRT, 4);
		data.CameraBakedGIRT = builder.UseColorBuffer(in waaaghResourceData.CameraBakedGIRT, 5);
		data.CameraShadowmaskRT = builder.UseColorBuffer(in waaaghResourceData.CameraShadowmaskRT, 6);
	}

	protected override void Render(PassData data, RenderGraphContext context)
	{
		if (data.TransitionActive)
		{
			context.cmd.EnableShaderKeyword("_TERRAIN_TRANSITION");
			context.cmd.SetGlobalVector(OwlcatTerrainShader.TerrainTransitionShape, OwlcatTerrainTransition.MakeTransitionClipShape());
		}
		else
		{
			context.cmd.DisableShaderKeyword("_TERRAIN_TRANSITION");
		}
		context.cmd.DrawRendererList(data.RendererList);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraAlbedoRT, data.CameraAlbedoRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraSpecularRT, data.CameraSpecularRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsTexture, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraTranslucencyRT, data.CameraTranslucencyRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraBakedGIRT, data.CameraBakedGIRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraShadowmaskRT, data.CameraShadowmaskRT);
	}
}
