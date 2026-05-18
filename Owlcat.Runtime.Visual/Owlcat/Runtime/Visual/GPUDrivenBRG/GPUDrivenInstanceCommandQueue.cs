using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenInstanceCommandQueue
{
	private struct Command
	{
		public GPUDrivenBatchRendererGroup.RendererDesc RendererDesc;

		public GPUDrivenBatchRendererGroup.RendererParams RendererParams;

		public GPUDrivenRendererGroupPool.RendererUpdateFlags UpdateFlags;

		public bool UpdateMesh;

		public CommandType CommandType;
	}

	private enum CommandType
	{
		AddOrUpdate,
		Remove
	}

	private static class Profiling
	{
		public static ProfilingSampler FlushSampler = new ProfilingSampler("FlushSampler");
	}

	private static int s_InstanceIDCounter;

	private static readonly List<Command> PendingCommands = new List<Command>();

	private static GPUDrivenBatchRendererGroup s_BRG;

	public static GPUDrivenInstanceID GetUniqueCustomInstanceID()
	{
		if (s_InstanceIDCounter == -1)
		{
			Debug.LogWarning("Custom instance ID has reach its limit. Reusing old values.");
		}
		return GPUDrivenInstanceID.Custom(s_InstanceIDCounter++);
	}

	internal static void Init(GPUDrivenBatchRendererGroup batchRendererGroup)
	{
		s_BRG = batchRendererGroup;
	}

	internal static void Cleanup()
	{
		foreach (Command pendingCommand in PendingCommands)
		{
			Release(pendingCommand);
		}
		PendingCommands.Clear();
		s_BRG = null;
	}

	public static void ScheduleAddOrUpdate(in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, in GPUDrivenBatchRendererGroup.RendererParams rendererParams, GPUDrivenRendererGroupPool.RendererUpdateFlags rendererUpdateFlags, bool updateMesh = false)
	{
		VerifyInstanceID(in rendererDesc);
		Command command = default(Command);
		command.RendererDesc = rendererDesc;
		command.RendererParams = rendererParams;
		command.UpdateFlags = rendererUpdateFlags;
		command.UpdateMesh = updateMesh;
		command.CommandType = CommandType.AddOrUpdate;
		Command item = command;
		if (item.RendererParams.Materials != null)
		{
			List<Material> list = ListPool<Material>.Get();
			foreach (Material material in item.RendererParams.Materials)
			{
				list.Add(material);
			}
			item.RendererParams.Materials = list;
		}
		if (item.RendererParams.PerInstanceProperties.IsCreated)
		{
			NativeArray<GPUDrivenRenderer.PropertyData> perInstanceProperties = new NativeArray<GPUDrivenRenderer.PropertyData>(item.RendererParams.PerInstanceProperties, Allocator.TempJob);
			perInstanceProperties.CopyFrom(item.RendererParams.PerInstanceProperties);
			item.RendererParams.PerInstanceProperties = perInstanceProperties;
		}
		PendingCommands.Add(item);
	}

	public static void ScheduleRemove(GPUDrivenInstanceID instanceID)
	{
		GPUDrivenBatchRendererGroup.RendererDesc rendererDesc = GPUDrivenBatchRendererGroup.RendererDesc.Custom(instanceID);
		VerifyInstanceID(in rendererDesc);
		PendingCommands.Add(new Command
		{
			RendererDesc = rendererDesc,
			CommandType = CommandType.Remove
		});
	}

	private static void VerifyInstanceID(in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc)
	{
	}

	internal static void Flush()
	{
		if (!s_BRG.IsEnabledAndInitialized && PendingCommands.Count > 0)
		{
			using (new ProfilingScope(Profiling.FlushSampler))
			{
				Debug.LogWarning("Scheduled instance modification while GPU Driven Rendering is disabled.");
				foreach (Command pendingCommand in PendingCommands)
				{
					Release(pendingCommand);
				}
			}
		}
		else if (PendingCommands.Count > 0)
		{
			using (new ProfilingScope(Profiling.FlushSampler))
			{
				GPUDrivenBatchRendererGroup.ChangeContext changeContext = GPUDrivenRenderingUtils.CreateChangeContext();
				AllocatorManager.AllocatorHandle allocator = Allocator.TempJob;
				NativeList<EntityId> nativeList = new NativeList<EntityId>(PendingCommands.Count, allocator);
				NativeList<float4x4> nativeList2 = new NativeList<float4x4>(PendingCommands.Count, allocator);
				foreach (Command pendingCommand2 in PendingCommands)
				{
					Command current = pendingCommand2;
					switch (current.CommandType)
					{
					case CommandType.AddOrUpdate:
					{
						GPUDrivenBatchRendererGroup.RendererUpdateStatus rendererUpdateStatus = s_BRG.AddOrUpdateRenderer(in current.RendererDesc, in changeContext, current.UpdateFlags, in current.RendererParams);
						if (current.UpdateMesh)
						{
							s_BRG.UpdateRendererMesh(in current.RendererDesc, in changeContext, in current.RendererParams);
						}
						if ((rendererUpdateStatus == GPUDrivenBatchRendererGroup.RendererUpdateStatus.Added || rendererUpdateStatus == GPUDrivenBatchRendererGroup.RendererUpdateStatus.UpdatedRegistered || rendererUpdateStatus == GPUDrivenBatchRendererGroup.RendererUpdateStatus.DidNotUpdateRegistered) && (rendererUpdateStatus == GPUDrivenBatchRendererGroup.RendererUpdateStatus.Added || current.UpdateFlags != GPUDrivenRendererGroupPool.RendererUpdateFlags.CustomPerInstanceData))
						{
							EntityId value = current.RendererDesc.InstanceID.RawInstanceID;
							nativeList.Add(in value);
							nativeList2.Add(in current.RendererParams.LocalToWorldMatrix);
						}
						break;
					}
					case CommandType.Remove:
						s_BRG.RemoveRenderer(current.RendererDesc.InstanceID);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					Release(current);
				}
				if (nativeList.Length > 0)
				{
					GPUDrivenProcessor.NativeRenderersData nativeRenderersData = default(GPUDrivenProcessor.NativeRenderersData);
					nativeRenderersData.RendererIDs = nativeList.AsArray();
					nativeRenderersData.LocalToWorldMatrix = nativeList2.AsArray();
					nativeRenderersData.PrevLocalToWorldMatrix = nativeList2.AsArray();
					GPUDrivenProcessor.NativeRenderersData nativeRenderersData2 = nativeRenderersData;
					s_BRG.UpdateNativeData(nativeRenderersData2, in changeContext, GPUDrivenInstanceID.InstanceIDType.Custom);
				}
				nativeList.Dispose();
				nativeList2.Dispose();
			}
		}
		PendingCommands.Clear();
	}

	private static void Release(Command command)
	{
		if (command.RendererParams.PerInstanceProperties.IsCreated)
		{
			command.RendererParams.PerInstanceProperties.Dispose();
		}
		if (command.RendererParams.Materials != null)
		{
			ListPool<Material>.Release(command.RendererParams.Materials);
		}
	}
}
