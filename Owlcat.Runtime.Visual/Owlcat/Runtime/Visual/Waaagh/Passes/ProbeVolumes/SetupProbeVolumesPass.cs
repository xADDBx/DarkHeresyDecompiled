using System;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.ProbeVolumes;

public class SetupProbeVolumesPass : ScriptableRenderPass
{
	private static class ShaderIDs
	{
		public static readonly int _EnableProbeVolumes = Shader.PropertyToID("_EnableProbeVolumes");
	}

	private class PassData
	{
		public Camera Camera;

		public CameraType CameraType;

		public ShEvalMode EvaluationMode;

		public bool IsTemporalAAEnabled;

		public ProbeVolumeSHBands ProbeVolumeSHBands;

		public ProbeVolumesOptions ProbeVolumesOptions;

		public bool SupportsLightLayers;

		public bool SupportsProbeVolumes;

		public bool EnablesProbeVolumes;
	}

	private readonly struct OptionsOverride : IDisposable
	{
		private static readonly FloatParameter s_TempFloatParameter = new FloatParameter(0f);

		private readonly float m_InitialIntensityMultiplier;

		private readonly ProbeVolumesOptions m_Reference;

		public OptionsOverride(ProbeVolumesOptions reference, WaaaghProbeVolumeOverrides overrides)
		{
			m_Reference = reference;
			m_InitialIntensityMultiplier = m_Reference.intensityMultiplier.value;
			s_TempFloatParameter.value = m_InitialIntensityMultiplier * overrides.IntensityMultiplier.value;
			reference.intensityMultiplier.SetValue(s_TempFloatParameter);
		}

		public void Dispose()
		{
			if (m_Reference != null)
			{
				m_Reference.intensityMultiplier.value = m_InitialIntensityMultiplier;
			}
		}
	}

	private static readonly BaseRenderFunc<PassData, RenderGraphContext> s_RenderFunc = ExecutePass;

	public override string Name => "SetupProbeVolumesPass";

	public SetupProbeVolumesPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = frameData.Get<WaaaghRenderingData>().RenderGraph.AddRenderPass<PassData>(Name, out passData, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ProbeVolumes\\SetupProbeVolumesPass.cs", 27);
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		passData.SupportsProbeVolumes = waaaghCameraData.SupportsProbeVolumes;
		passData.EnablesProbeVolumes = waaaghCameraData.EnablesProbeVolumes;
		passData.SupportsLightLayers = asset.SupportsLightLayers;
		passData.ProbeVolumeSHBands = asset.ProbeVolumesSettings.SHBands;
		passData.EvaluationMode = asset.ProbeVolumesSettings.EvaluationMode;
		passData.IsTemporalAAEnabled = waaaghCameraData.IsTemporalAAEnabled();
		passData.CameraType = waaaghCameraData.cameraType;
		passData.Camera = waaaghCameraData.camera;
		passData.ProbeVolumesOptions = ((!waaaghCameraData.camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component)) ? null : component.VolumeStack?.GetComponent<ProbeVolumesOptions>());
		renderGraphBuilder.SetRenderFunc(s_RenderFunc);
	}

	private static void ExecutePass(PassData passData, RenderGraphContext context)
	{
		bool supportsProbeVolumes = passData.SupportsProbeVolumes;
		ProbeReferenceVolume.instance.SetEnableStateFromSRP(supportsProbeVolumes);
		ProbeReferenceVolume.instance.SetVertexSamplingEnabled(passData.EvaluationMode == ShEvalMode.PerVertex);
		if (supportsProbeVolumes && ProbeReferenceVolume.instance.isInitialized)
		{
			ProbeReferenceVolume.instance.PerformPendingOperations();
			if (passData.CameraType != CameraType.Reflection && passData.CameraType != CameraType.Preview)
			{
				ProbeReferenceVolume.instance.UpdateCellStreaming(context.cmd, passData.Camera, passData.ProbeVolumesOptions);
			}
		}
		if (supportsProbeVolumes)
		{
			ProbeReferenceVolume.instance.BindAPVRuntimeResources(context.cmd, isProbeVolumeEnabled: true);
		}
		bool flag = passData.EnablesProbeVolumes && ProbeReferenceVolume.instance.DataHasBeenLoaded();
		context.cmd.SetKeyword(in ShaderGlobalKeywords.PROBE_VOLUMES_L1, flag && passData.ProbeVolumeSHBands == ProbeVolumeSHBands.SphericalHarmonicsL1);
		context.cmd.SetKeyword(in ShaderGlobalKeywords.PROBE_VOLUMES_L2, flag && passData.ProbeVolumeSHBands == ProbeVolumeSHBands.SphericalHarmonicsL2);
		context.cmd.SetKeyword(in ShaderGlobalKeywords.EVALUATE_SH_VERTEX, flag && passData.EvaluationMode == ShEvalMode.PerVertex);
		UpdateShaderVariables(passData, in context, VolumeManager.instance.stack);
	}

	private static void UpdateShaderVariables(PassData passData, in RenderGraphContext context, VolumeStack stack)
	{
		ProbeVolumesOptions component = stack.GetComponent<ProbeVolumesOptions>();
		bool flag;
		using (new OptionsOverride(component, stack.GetComponent<WaaaghProbeVolumeOverrides>()))
		{
			flag = ProbeReferenceVolume.instance.UpdateShaderVariablesProbeVolumes(context.cmd, component, passData.IsTemporalAAEnabled ? Time.frameCount : 0, passData.SupportsLightLayers);
		}
		context.cmd.SetGlobalInt(ShaderIDs._EnableProbeVolumes, flag ? 1 : 0);
	}
}
