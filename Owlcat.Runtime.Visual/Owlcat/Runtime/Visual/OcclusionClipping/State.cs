using System;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.Experimental.Geometry;
using Unity.Burst;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[BurstCompile]
internal struct State : IDisposable
{
	public NativeHashMap<int, TriggerData> Triggers;

	public NativeHashMap<int, VolumeData> Volumes;

	public NativeHashMap<int, RendererData> Renderers;

	public NativeBvh<TriggerNode> TriggerBvh;

	public NativeBvh<VolumeNode> VolumeBvh;

	public NativeBvh<RendererNode> RendererBvh;

	public NativeParallelMultiHashMap<int, int> TriggerVolumeLinks;

	public NativeParallelMultiHashMap<int, int> VolumeTriggerLinks;

	public NativeParallelMultiHashMap<int, int> VolumeRendererLinks;

	public NativeParallelMultiHashMap<int, int> RendererVolumeLinks;

	public NativeHashSet<int> ActiveProbeIds;

	public NativeHashSet<int> ActiveTriggerIds;

	public NativeHashSet<int> DirtyLinkVolumeIds;

	public NativeHashSet<int> DirtyLinkRendererIds;

	public NativeHashSet<int> DirtyOpacityRendererIds;

	public NativeHashSet<int> NextDirtyOpacityRendererIds;

	public State(Allocator allocator)
	{
		Triggers = new NativeHashMap<int, TriggerData>(128, allocator);
		Volumes = new NativeHashMap<int, VolumeData>(128, allocator);
		Renderers = new NativeHashMap<int, RendererData>(128, allocator);
		TriggerBvh = new NativeBvh<TriggerNode>(128u, allocator);
		VolumeBvh = new NativeBvh<VolumeNode>(128u, allocator);
		RendererBvh = new NativeBvh<RendererNode>(128u, allocator);
		TriggerVolumeLinks = new NativeParallelMultiHashMap<int, int>(128, allocator);
		VolumeTriggerLinks = new NativeParallelMultiHashMap<int, int>(128, allocator);
		VolumeRendererLinks = new NativeParallelMultiHashMap<int, int>(128, allocator);
		RendererVolumeLinks = new NativeParallelMultiHashMap<int, int>(128, allocator);
		ActiveProbeIds = new NativeHashSet<int>(128, allocator);
		ActiveTriggerIds = new NativeHashSet<int>(128, allocator);
		DirtyLinkVolumeIds = new NativeHashSet<int>(128, allocator);
		DirtyLinkRendererIds = new NativeHashSet<int>(128, allocator);
		DirtyOpacityRendererIds = new NativeHashSet<int>(128, allocator);
		NextDirtyOpacityRendererIds = new NativeHashSet<int>(128, allocator);
	}

	public void Dispose()
	{
		Triggers.Dispose();
		Volumes.Dispose();
		Renderers.Dispose();
		TriggerBvh.Dispose();
		VolumeBvh.Dispose();
		RendererBvh.Dispose();
		TriggerVolumeLinks.Dispose();
		VolumeTriggerLinks.Dispose();
		VolumeRendererLinks.Dispose();
		RendererVolumeLinks.Dispose();
		ActiveProbeIds.Dispose();
		ActiveTriggerIds.Dispose();
		DirtyLinkVolumeIds.Dispose();
		DirtyLinkRendererIds.Dispose();
		DirtyOpacityRendererIds.Dispose();
		NextDirtyOpacityRendererIds.Dispose();
	}

	public void CreateTrigger(int id, Obb box)
	{
		TriggerNode triggerNode = default(TriggerNode);
		triggerNode.TriggerId = id;
		triggerNode.Box = box;
		TriggerNode data = triggerNode;
		TriggerData triggerData = default(TriggerData);
		triggerData.NodeId = TriggerBvh.Insert(data, box.Bounds);
		TriggerData item = triggerData;
		Triggers.Add(id, item);
	}

	public void DestroyTrigger(int id)
	{
		if (!Triggers.TryGetValue(id, out var item))
		{
			return;
		}
		if (ActiveTriggerIds.Remove(id))
		{
			foreach (int item2 in TriggerVolumeLinks.GetValuesForKey(id))
			{
				DecrementVolumeClipCounter(item2);
			}
		}
		Triggers.Remove(id);
		TriggerBvh.Delete(item.NodeId);
	}

	public void CreateVolume(int id, Obb box)
	{
		VolumeNode volumeNode = default(VolumeNode);
		volumeNode.VolumeId = id;
		volumeNode.Box = box;
		VolumeNode data = volumeNode;
		VolumeData volumeData = default(VolumeData);
		volumeData.NodeId = VolumeBvh.Insert(data, box.Bounds);
		volumeData.Box = box;
		volumeData.ClipCounter = 0;
		VolumeData item = volumeData;
		foreach (int item2 in VolumeTriggerLinks.GetValuesForKey(id))
		{
			if (ActiveTriggerIds.Contains(item2))
			{
				item.ClipCounter++;
			}
		}
		Volumes.Add(id, item);
		DirtyLinkVolumeIds.Add(id);
	}

	public void DestroyVolume(int id)
	{
		if (!Volumes.TryGetValue(id, out var item))
		{
			return;
		}
		if (item.ClipCounter > 0)
		{
			foreach (int item2 in VolumeRendererLinks.GetValuesForKey(id))
			{
				DecrementRendererClipCounter(item2);
			}
		}
		Volumes.Remove(id);
		VolumeBvh.Delete(item.NodeId);
		DirtyLinkVolumeIds.Remove(id);
	}

	public void CreateRenderer(int id, Obb box)
	{
		RendererNode rendererNode = default(RendererNode);
		rendererNode.RendererId = id;
		rendererNode.Box = box;
		RendererNode data = rendererNode;
		RendererData rendererData = default(RendererData);
		rendererData.NodeId = RendererBvh.Insert(data, box.Bounds);
		rendererData.Box = box;
		rendererData.ClipCounter = 0;
		rendererData.Opacity = 1f;
		RendererData item = rendererData;
		foreach (int item3 in RendererVolumeLinks.GetValuesForKey(id))
		{
			if (Volumes.TryGetValue(item3, out var item2) && item2.ClipCounter > 0)
			{
				item.ClipCounter++;
			}
		}
		item.Opacity = ((item.ClipCounter <= 0) ? 1 : 0);
		Renderers.Add(id, item);
		DirtyLinkRendererIds.Add(id);
	}

	public void DestroyRenderer(int id)
	{
		if (Renderers.TryGetValue(id, out var item))
		{
			Renderers.Remove(id);
			RendererBvh.Delete(item.NodeId);
			DirtyLinkRendererIds.Remove(id);
			DirtyOpacityRendererIds.Remove(id);
		}
	}

	public void CreateTriggerVolumeLink(int triggerId, int volumeId)
	{
		TriggerVolumeLinks.Add(triggerId, volumeId);
		VolumeTriggerLinks.Add(volumeId, triggerId);
	}

	public void DestroyTriggerVolumeLink(int triggerId, int volumeId)
	{
		TriggerVolumeLinks.Remove(triggerId, volumeId);
		VolumeTriggerLinks.Remove(volumeId, triggerId);
	}

	public void CreateVolumeRendererLink(int volumeId, int rendererId)
	{
		VolumeRendererLinks.Add(volumeId, rendererId);
		RendererVolumeLinks.Add(rendererId, volumeId);
	}

	public void DestroyVolumeRendererLink(int volumeId, int rendererId)
	{
		VolumeRendererLinks.Remove(volumeId, rendererId);
		RendererVolumeLinks.Remove(rendererId, volumeId);
	}

	public void HandleProbeOverlaps(NativeHashSet<int> overlapProbeIds, NativeHashSet<int> overlapTriggerIds)
	{
		ActiveProbeIds.Clear();
		ActiveProbeIds.UnionWith(overlapProbeIds);
		NativeList<int> nativeList = new NativeList<int>(ActiveTriggerIds.Count, Allocator.Temp);
		foreach (int activeTriggerId in ActiveTriggerIds)
		{
			int value = activeTriggerId;
			if (!overlapTriggerIds.Contains(value))
			{
				nativeList.Add(in value);
			}
		}
		foreach (int item in nativeList)
		{
			ActiveTriggerIds.Remove(item);
			foreach (int item2 in TriggerVolumeLinks.GetValuesForKey(item))
			{
				DecrementVolumeClipCounter(item2);
			}
		}
		foreach (int item3 in overlapTriggerIds)
		{
			if (ActiveTriggerIds.Contains(item3))
			{
				continue;
			}
			ActiveTriggerIds.Add(item3);
			foreach (int item4 in TriggerVolumeLinks.GetValuesForKey(item3))
			{
				IncrementVolumeClipCounter(item4);
			}
		}
	}

	public void HandleVolumeOverlaps(int volumeInstanceId, NativeHashSet<int> overlapRendererInstanceIds)
	{
		NativeList<int> nativeList = new NativeList<int>(16, Allocator.Temp);
		foreach (int item in VolumeRendererLinks.GetValuesForKey(volumeInstanceId))
		{
			int value = item;
			if (!overlapRendererInstanceIds.Contains(value))
			{
				nativeList.Add(in value);
			}
		}
		foreach (int item2 in nativeList)
		{
			VolumeRendererLinks.Remove(volumeInstanceId, item2);
			RendererVolumeLinks.Remove(item2, volumeInstanceId);
		}
		foreach (int item3 in overlapRendererInstanceIds)
		{
			if (!VolumeRendererLinks.ContainsValue(volumeInstanceId, item3))
			{
				VolumeRendererLinks.Add(volumeInstanceId, item3);
				RendererVolumeLinks.Add(item3, volumeInstanceId);
			}
		}
	}

	public void HandleRendererOverlaps(int rendererInstanceId, NativeHashSet<int> overlapVolumeInstanceIds)
	{
		NativeList<int> nativeList = new NativeList<int>(16, Allocator.Temp);
		foreach (int item in RendererVolumeLinks.GetValuesForKey(rendererInstanceId))
		{
			int value = item;
			if (!overlapVolumeInstanceIds.Contains(value))
			{
				nativeList.Add(in value);
			}
		}
		foreach (int item2 in nativeList)
		{
			VolumeRendererLinks.Remove(item2, rendererInstanceId);
			RendererVolumeLinks.Remove(rendererInstanceId, item2);
		}
		foreach (int item3 in overlapVolumeInstanceIds)
		{
			if (!RendererVolumeLinks.ContainsValue(rendererInstanceId, item3))
			{
				VolumeRendererLinks.Add(item3, rendererInstanceId);
				RendererVolumeLinks.Add(rendererInstanceId, item3);
			}
		}
	}

	private void IncrementVolumeClipCounter(int volumeInstanceId)
	{
		if (!Volumes.TryGetValue(volumeInstanceId, out var item))
		{
			return;
		}
		item.ClipCounter++;
		Volumes[volumeInstanceId] = item;
		if (item.ClipCounter != 1)
		{
			return;
		}
		foreach (int item2 in VolumeRendererLinks.GetValuesForKey(volumeInstanceId))
		{
			IncrementRendererClipCounter(item2);
		}
	}

	private void DecrementVolumeClipCounter(int volumeInstanceId)
	{
		if (!Volumes.TryGetValue(volumeInstanceId, out var item))
		{
			return;
		}
		item.ClipCounter--;
		Volumes[volumeInstanceId] = item;
		if (item.ClipCounter != 0)
		{
			return;
		}
		foreach (int item2 in VolumeRendererLinks.GetValuesForKey(volumeInstanceId))
		{
			DecrementRendererClipCounter(item2);
		}
	}

	private void IncrementRendererClipCounter(int rendererInstanceId)
	{
		if (Renderers.TryGetValue(rendererInstanceId, out var item))
		{
			item.ClipCounter++;
			Renderers[rendererInstanceId] = item;
			if (item.ClipCounter == 1)
			{
				DirtyOpacityRendererIds.Add(rendererInstanceId);
			}
		}
	}

	private void DecrementRendererClipCounter(int rendererInstanceId)
	{
		if (Renderers.TryGetValue(rendererInstanceId, out var item))
		{
			item.ClipCounter--;
			Renderers[rendererInstanceId] = item;
			if (item.ClipCounter == 0)
			{
				DirtyOpacityRendererIds.Add(rendererInstanceId);
			}
		}
	}
}
