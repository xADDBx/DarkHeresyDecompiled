using Owlcat.Runtime.Visual.Experimental.Geometry;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

internal static class ClippingSystem
{
	private static ClippingSystemImplementation s_Implementation;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Initialize()
	{
		OcclusionClippingSettings renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<OcclusionClippingSettings>();
		if (renderPipelineSettings != null && renderPipelineSettings.Enabled)
		{
			s_Implementation = new ClippingSystemImplementation(renderPipelineSettings);
			PlayerLoopUtility.RegisterUpdateDelegate(typeof(PostLateUpdate), typeof(ClippingSystem), OnPostLateUpdate);
		}
	}

	public static void CreateProbe(int id, IClippingProbe probe)
	{
		s_Implementation?.CreateProbe(id, probe);
	}

	public static void DestroyProbe(int id)
	{
		s_Implementation?.DestroyProbe(id);
	}

	public static void CreateTrigger(int id, Obb box)
	{
		s_Implementation?.CreateTrigger(id, box);
	}

	public static void DestroyTrigger(int id)
	{
		s_Implementation?.DestroyTrigger(id);
	}

	public static void CreateVolume(int id, Obb box)
	{
		s_Implementation?.CreateVolume(id, box);
	}

	public static void DestroyVolume(int id)
	{
		s_Implementation?.DestroyVolume(id);
	}

	public static void CreateRenderer(int id, Obb box, IClippingRenderer renderer)
	{
		s_Implementation?.CreateRenderer(id, box, renderer);
	}

	public static void DestroyRenderer(int id)
	{
		s_Implementation?.DestroyRenderer(id);
	}

	public static void CreateTriggerVolumeLink(int triggerId, int volumeId)
	{
		s_Implementation?.CreateTriggerVolumeLink(triggerId, volumeId);
	}

	public static void DestroyTriggerVolumeLink(int triggerId, int volumeId)
	{
		s_Implementation?.DestroyTriggerVolumeLink(triggerId, volumeId);
	}

	public static void SetDump(ClippingSystemDump dump)
	{
		s_Implementation?.SetDump(dump);
	}

	private static void OnPostLateUpdate()
	{
		s_Implementation?.Update();
	}
}
