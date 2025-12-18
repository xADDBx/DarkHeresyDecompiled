using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Experimental.Geometry;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

internal sealed class ClippingSystemImplementation : IDisposable
{
	private readonly Dictionary<int, IClippingProbe> m_Probes = new Dictionary<int, IClippingProbe>();

	private readonly Dictionary<int, IClippingRenderer> m_Renderers = new Dictionary<int, IClippingRenderer>();

	private readonly OcclusionClippingSettings m_Settings;

	private State m_State;

	private ActionBuffer m_ActionBuffer;

	private bool m_VolumesTouched;

	private bool m_RenderersTouched;

	private float m_NextUpdateTriggersTime;

	private bool m_JobScheduled;

	private JobHandle m_JobHandle;

	public ClippingSystemImplementation(OcclusionClippingSettings settings)
	{
		m_Settings = settings;
		m_State = new State(Allocator.Persistent);
		m_ActionBuffer = new ActionBuffer(Allocator.TempJob);
	}

	public void Dispose()
	{
		if (m_JobScheduled)
		{
			m_JobHandle.Complete();
		}
		m_State.Dispose();
		m_ActionBuffer.Dispose();
	}

	public void SetDump(ClippingSystemDump dump)
	{
	}

	public void CreateProbe(int id, IClippingProbe probe)
	{
		m_Probes.Add(id, probe);
	}

	public void DestroyProbe(int id)
	{
		m_Probes.Remove(id);
	}

	public void CreateTrigger(int id, Obb box)
	{
		m_ActionBuffer.CreateTrigger(id, box);
	}

	public void DestroyTrigger(int id)
	{
		m_ActionBuffer.DestroyTrigger(id);
	}

	public void CreateVolume(int id, Obb box)
	{
		m_ActionBuffer.CreateVolume(id, box);
		m_VolumesTouched = true;
	}

	public void DestroyVolume(int id)
	{
		m_ActionBuffer.DestroyVolume(id);
	}

	public void CreateRenderer(int id, Obb box, IClippingRenderer renderer)
	{
		m_ActionBuffer.CreateRenderer(id, box);
		m_Renderers.Add(id, renderer);
		m_RenderersTouched = true;
	}

	public void DestroyRenderer(int id)
	{
		m_ActionBuffer.DestroyRenderer(id);
		m_Renderers.Remove(id);
	}

	public void CreateTriggerVolumeLink(int triggerId, int volumeId)
	{
		m_ActionBuffer.CreateTriggerVolumeLink(triggerId, volumeId);
	}

	public void DestroyTriggerVolumeLink(int triggerId, int volumeId)
	{
		m_ActionBuffer.DestroyTriggerVolumeLink(triggerId, volumeId);
	}

	public void Update()
	{
		CompleteJobs();
		ScheduleJobs();
	}

	private void CompleteJobs()
	{
		if (m_JobScheduled)
		{
			m_JobScheduled = false;
			m_JobHandle.Complete();
			PropagateOpacityToRenderers();
			ref NativeHashSet<int> dirtyOpacityRendererIds = ref m_State.DirtyOpacityRendererIds;
			ref NativeHashSet<int> nextDirtyOpacityRendererIds = ref m_State.NextDirtyOpacityRendererIds;
			NativeHashSet<int> nextDirtyOpacityRendererIds2 = m_State.NextDirtyOpacityRendererIds;
			NativeHashSet<int> dirtyOpacityRendererIds2 = m_State.DirtyOpacityRendererIds;
			dirtyOpacityRendererIds = nextDirtyOpacityRendererIds2;
			nextDirtyOpacityRendererIds = dirtyOpacityRendererIds2;
			m_State.NextDirtyOpacityRendererIds.Clear();
			m_State.DirtyLinkVolumeIds.Clear();
			m_State.DirtyLinkRendererIds.Clear();
		}
	}

	private void ScheduleJobs()
	{
		float opacityDelta = ((m_Settings.RendererFadeAnimationDuration > 0f) ? (Time.deltaTime / m_Settings.RendererFadeAnimationDuration) : 1f);
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		JobHandle dependsOn = default(JobHandle);
		if (m_ActionBuffer.IsCreated)
		{
			dependsOn = new ReplayActionsJob
			{
				State = m_State,
				ActionBuffer = m_ActionBuffer
			}.Schedule(dependsOn);
			dependsOn = m_ActionBuffer.Dispose(dependsOn);
		}
		if (realtimeSinceStartupAsDouble >= (double)m_NextUpdateTriggersTime)
		{
			m_NextUpdateTriggersTime += m_Settings.TriggerUpdateInterval;
			NativeArray<ProbeData> probes = new NativeArray<ProbeData>(m_Probes.Count, Allocator.TempJob);
			int num = 0;
			foreach (KeyValuePair<int, IClippingProbe> probe in m_Probes)
			{
				probes[num++] = new ProbeData
				{
					ProbeId = probe.Key,
					Sphere = probe.Value.BoundingSphere
				};
			}
			UpdateTriggersJob jobData = default(UpdateTriggersJob);
			jobData.State = m_State;
			jobData.Probes = probes;
			jobData.ActiveProbeExtension = m_Settings.ActiveProbeExpansion;
			dependsOn = jobData.Schedule(dependsOn);
			dependsOn = probes.Dispose(dependsOn);
		}
		if (m_VolumesTouched)
		{
			UpdateVolumeLinksJob jobData2 = default(UpdateVolumeLinksJob);
			jobData2.State = m_State;
			dependsOn = jobData2.Schedule(dependsOn);
		}
		if (m_RenderersTouched)
		{
			UpdateRendererLinksJob jobData3 = default(UpdateRendererLinksJob);
			jobData3.State = m_State;
			dependsOn = jobData3.Schedule(dependsOn);
		}
		UpdateRendererOpacityJob jobData4 = default(UpdateRendererOpacityJob);
		jobData4.State = m_State;
		jobData4.OpacityDelta = opacityDelta;
		dependsOn = jobData4.Schedule(dependsOn);
		m_JobScheduled = true;
		m_JobHandle = dependsOn;
		m_VolumesTouched = false;
		m_RenderersTouched = false;
		m_ActionBuffer = new ActionBuffer(Allocator.TempJob);
	}

	private void PropagateOpacityToRenderers()
	{
		foreach (int dirtyOpacityRendererId in m_State.DirtyOpacityRendererIds)
		{
			if (m_Renderers.TryGetValue(dirtyOpacityRendererId, out var value) && m_State.Renderers.TryGetValue(dirtyOpacityRendererId, out var item))
			{
				value.SetOpacity(item.Opacity);
			}
		}
	}

	public void UpdateDebugDump()
	{
	}
}
