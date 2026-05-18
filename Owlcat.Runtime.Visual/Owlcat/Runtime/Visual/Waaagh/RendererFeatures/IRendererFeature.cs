using System;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures;

public interface IRendererFeature : IDisposable
{
	void RegisterExtensions(RendererFeatureExtensionRegistry registry);
}
