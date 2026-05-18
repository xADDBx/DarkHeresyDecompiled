using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public interface IPipelineRenderer : IDisposable
{
	void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, WaaaghCameraData cameraData);

	void Setup(in RendererSetupContext context);

	void Record(in RendererRecordContext context);

	void Cleanup();

	bool SupportsPipelineFeature(PipelineFeature feature);
}
