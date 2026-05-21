using System;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class APV
{
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

	private static class ShaderIDs
	{
		public static readonly int _EnableProbeVolumes = Shader.PropertyToID("_EnableProbeVolumes");
	}

	public static void SetupProbeVolumesPass(in RecordContext context)
	{
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		WaaaghCameraData cameraData = context.CameraData;
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("SetupProbeVolumesPass", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\APV.cs", 62);
		passData.SupportsProbeVolumes = cameraData.SupportsProbeVolumes;
		passData.EnablesProbeVolumes = cameraData.EnablesProbeVolumes;
		passData.SupportsLightLayers = asset.SupportsLightLayers;
		passData.ProbeVolumeSHBands = asset.ProbeVolumesSettings.SHBands;
		passData.EvaluationMode = asset.ProbeVolumesSettings.EvaluationMode;
		passData.IsTemporalAAEnabled = cameraData.IsTemporalAAEnabled();
		passData.CameraType = cameraData.cameraType;
		passData.Camera = cameraData.camera;
		passData.ProbeVolumesOptions = ((!cameraData.camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component)) ? null : component.VolumeStack?.GetComponent<ProbeVolumesOptions>());
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc<PassData>(ExecuteSetupProbeVolumesPass);
	}

	private static void ExecuteSetupProbeVolumesPass(PassData data, UnsafeGraphContext context)
	{
		CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
		bool supportsProbeVolumes = data.SupportsProbeVolumes;
		ProbeReferenceVolume.instance.SetEnableStateFromSRP(supportsProbeVolumes);
		ProbeReferenceVolume.instance.SetVertexSamplingEnabled(data.EvaluationMode == ShEvalMode.PerVertex);
		if (supportsProbeVolumes && ProbeReferenceVolume.instance.isInitialized)
		{
			ProbeReferenceVolume.instance.PerformPendingOperations();
			if (data.CameraType != CameraType.Reflection && data.CameraType != CameraType.Preview)
			{
				ProbeReferenceVolume.instance.UpdateCellStreaming(cmd, data.Camera, data.ProbeVolumesOptions);
			}
		}
		if (supportsProbeVolumes)
		{
			ProbeReferenceVolume.instance.BindAPVRuntimeResources(cmd, isProbeVolumeEnabled: true);
		}
		bool flag = data.EnablesProbeVolumes && ProbeReferenceVolume.instance.DataHasBeenLoaded();
		context.cmd.SetKeyword(in ShaderGlobalKeywords.PROBE_VOLUMES_L1, flag && data.ProbeVolumeSHBands == ProbeVolumeSHBands.SphericalHarmonicsL1);
		context.cmd.SetKeyword(in ShaderGlobalKeywords.PROBE_VOLUMES_L2, flag && data.ProbeVolumeSHBands == ProbeVolumeSHBands.SphericalHarmonicsL2);
		context.cmd.SetKeyword(in ShaderGlobalKeywords.EVALUATE_SH_VERTEX, flag && data.EvaluationMode == ShEvalMode.PerVertex);
		UpdateShaderVariables(data, in cmd, VolumeManager.instance.stack);
	}

	private static void UpdateShaderVariables(PassData passData, in CommandBuffer cmd, VolumeStack stack)
	{
		ProbeVolumesOptions component = stack.GetComponent<ProbeVolumesOptions>();
		bool flag;
		using (new OptionsOverride(component, stack.GetComponent<WaaaghProbeVolumeOverrides>()))
		{
			flag = ProbeReferenceVolume.instance.UpdateShaderVariablesProbeVolumes(cmd, component, passData.IsTemporalAAEnabled ? Time.frameCount : 0, passData.SupportsLightLayers);
		}
		cmd.SetGlobalInt(ShaderIDs._EnableProbeVolumes, flag ? 1 : 0);
	}
}
