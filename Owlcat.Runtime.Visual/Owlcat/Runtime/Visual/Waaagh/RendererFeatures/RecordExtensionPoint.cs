namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures;

public enum RecordExtensionPoint
{
	BeforeRendering,
	BeforeDrawDepthPrePass,
	BeforeDrawDeferredDecals,
	AfterDrawDeferredDecals,
	BeforeDeferredLighting,
	AfterDeferredLighting,
	BeforeDrawTransparent1,
	BeforeDrawTransparent2,
	AfterDrawTransparent,
	BeforeDrawPostProcess,
	AfterDrawPostProcess,
	AfterRendering
}
