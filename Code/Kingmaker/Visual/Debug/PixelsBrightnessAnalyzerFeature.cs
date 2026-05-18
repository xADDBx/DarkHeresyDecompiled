using System;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures;
using UnityEngine;

namespace Kingmaker.Visual.Debug;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Debug/Pixels Brightness Analyzer", fileName = "PixelsBrightnessAnalyzerFeature")]
public class PixelsBrightnessAnalyzerFeature : RendererFeatureAsset
{
	private sealed class RendererFeature : IRendererFeature, IDisposable
	{
		private readonly PixelsBrightnessAnalyzerPassResources m_Resources;

		public RendererFeature(ComputeShader shader)
		{
			m_Resources = new PixelsBrightnessAnalyzerPassResources(shader);
		}

		public void Dispose()
		{
			m_Resources.Dispose();
		}

		public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
		{
			registry.AddRecordDelegate(RecordExtensionPoint.BeforeDrawPostProcess, OnBeforeDrawPostProcess);
			registry.AddRecordDelegate(RecordExtensionPoint.AfterDrawPostProcess, OnAfterDrawPostProcess);
		}

		private void OnBeforeDrawPostProcess(in RecordContext context)
		{
			if (ShouldRun() && PixelsBrightnessAnalyzerSettings.BeforePostProcessing)
			{
				PixelsBrightnessAnalyzerPass.Record(in context, m_Resources, beforePostProcessing: true);
			}
		}

		private void OnAfterDrawPostProcess(in RecordContext context)
		{
			if (ShouldRun() && !PixelsBrightnessAnalyzerSettings.BeforePostProcessing)
			{
				PixelsBrightnessAnalyzerPass.Record(in context, m_Resources, beforePostProcessing: false);
			}
		}

		private static bool ShouldRun()
		{
			if (UnityEngine.Debug.isDebugBuild)
			{
				return PixelsBrightnessAnalyzerSettings.Enabled;
			}
			return false;
		}
	}

	[SerializeField]
	private ComputeShader m_AnalyzerShader;

	public override IRendererFeature CreateRendererFeature()
	{
		if (m_AnalyzerShader == null)
		{
			return null;
		}
		return new RendererFeature(m_AnalyzerShader);
	}
}
