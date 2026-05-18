using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class ShaderConstants
{
	public static class LightModeTags
	{
		public static readonly ShaderTagId GBuffer;

		public static readonly ShaderTagId TerrainGBuffer;

		public static readonly ShaderTagId SRPDefaultUnlit;

		public static readonly ShaderTagId ForwardLit;

		public static readonly ShaderTagId DecalDeferred;

		public static readonly ShaderTagId DecalGUI;

		public static readonly ShaderTagId DepthOnly;

		public static readonly ShaderTagId MotionVectors;

		public static readonly ShaderTagId[] OpaqueAll;

		public static readonly ShaderTagId[] ForwardAll;

		static LightModeTags()
		{
			GBuffer = new ShaderTagId("GBuffer");
			TerrainGBuffer = new ShaderTagId("TerrainGBuffer");
			SRPDefaultUnlit = new ShaderTagId("SRPDefaultUnlit");
			ForwardLit = new ShaderTagId("ForwardLit");
			DecalDeferred = new ShaderTagId("DecalDeferred");
			DecalGUI = new ShaderTagId("DecalGUI");
			DepthOnly = new ShaderTagId("DepthOnly");
			MotionVectors = new ShaderTagId("MotionVectors");
			OpaqueAll = new ShaderTagId[2] { GBuffer, TerrainGBuffer };
			ForwardAll = new ShaderTagId[4] { SRPDefaultUnlit, ForwardLit, DecalDeferred, DecalGUI };
		}
	}

	public static class RenderQueueRanges
	{
		public static RenderQueueRange Opaque => WaaaghRenderQueue.Opaque;

		public static RenderQueueRange OpaqueAlphaTest => WaaaghRenderQueue.OpaqueAlphaTest;

		public static RenderQueueRange OpaqueDistortion => WaaaghRenderQueue.OpaqueDistortion;

		public static RenderQueueRange OpaqueAll => new RenderQueueRange(WaaaghRenderQueue.Opaque.lowerBound, WaaaghRenderQueue.OpaqueDistortion.upperBound);

		public static RenderQueueRange Transparent => WaaaghRenderQueue.Transparent;
	}
}
