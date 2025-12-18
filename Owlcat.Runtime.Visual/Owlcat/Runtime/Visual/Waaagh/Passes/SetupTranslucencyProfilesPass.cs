using Owlcat.Runtime.Visual.Lighting;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class SetupTranslucencyProfilesPass : ScriptableRenderPass<SetupTranslucencyProfilesPassData>
{
	private static class ShaderIDs
	{
		public static readonly int ConstantBuffer = Shader.PropertyToID("TranslucencyProfilesConstantBuffer");
	}

	public override string Name => "SetupTranslucencyProfilesPass";

	public SetupTranslucencyProfilesPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected unsafe override void Setup(RenderGraphBuilder builder, SetupTranslucencyProfilesPassData data, ContextContainer frameData)
	{
		WaaaghTranslucencyProfileSet instance = WaaaghTranslucencyProfileSet.Instance;
		if (instance == null)
		{
			data.ConstantBuffer._TranslucencyProfileCount.x = 0f;
		}
		else
		{
			int num = math.min(instance.Profiles.Count, 16);
			data.ConstantBuffer._TranslucencyProfileCount.x = num;
			fixed (TranslucencyProfilesConstantBuffer* ptr = &data.ConstantBuffer)
			{
				Vector4* ptr2 = (Vector4*)ptr->_TranslucencyProfileColors;
				Vector4* ptr3 = (Vector4*)ptr->_TranslucencyProfileSelfShadowingWorkaroundData;
				for (int i = 0; i < num; i++)
				{
					WaaaghTranslucencyProfileSet.TranslucencyProfile translucencyProfile = instance.Profiles[i];
					ptr2[i] = translucencyProfile.Color;
					ref WaaaghTranslucencyProfileSet.SelfShadowingWorkaroundData selfShadowingWorkaround = ref translucencyProfile.SelfShadowingWorkaround;
					ptr3[i] = new Vector4(selfShadowingWorkaround.DepthBias, selfShadowingWorkaround.DepthBiasPointSpot, selfShadowingWorkaround.NormalBias, selfShadowingWorkaround.NormalBiasPointSpot);
				}
			}
		}
		builder.AllowPassCulling(value: false);
	}

	protected override void Render(SetupTranslucencyProfilesPassData data, RenderGraphContext context)
	{
		ConstantBuffer.PushGlobal(context.cmd, in data.ConstantBuffer, ShaderIDs.ConstantBuffer);
	}
}
