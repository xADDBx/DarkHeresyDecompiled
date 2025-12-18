using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Core.ObjectTracking;
using Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Main;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using Owlcat.ShaderLibrary.Visual.Debug;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenBRGInstanceCuller : IDisposable, IGPUDrivenMemoryProfilingSource
{
	private struct SplitCPUCullingInfo
	{
		public int CullingPlanesOffset;

		public int CullingPlanesCount;

		public int DrawCommandVisibilityMaskOffset;

		public int DrawCommandVisibilityMaskCount;
	}

	[BurstCompile]
	private struct PrepareCullingDataJob : IJob
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool _003CAreAllCullingSpheresValid_003Eg__HasValidCullingSphere_007C5_0_0000198A_0024PostfixBurstDelegate(in CullingSplit cullingSplit);

		internal static class _003CAreAllCullingSpheresValid_003Eg__HasValidCullingSphere_007C5_0_0000198A_0024BurstDirectCall
		{
			private static IntPtr Pointer;

			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr P_0)
			{
				if (Pointer == (IntPtr)0)
				{
					Pointer = BurstCompiler.CompileFunctionPointer<_003CAreAllCullingSpheresValid_003Eg__HasValidCullingSphere_007C5_0_0000198A_0024PostfixBurstDelegate>(HasValidCullingSphere).Value;
				}
				P_0 = Pointer;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				[BurstCompile]
				[MonoPInvokeCallback(typeof(_003CAreAllCullingSpheresValid_003Eg__HasValidCullingSphere_007C5_0_0000198A_0024PostfixBurstDelegate))]
				static bool HasValidCullingSphere(in CullingSplit cullingSplit)
				{
					return Invoke(in cullingSplit);
				}
			}

			private static IntPtr GetFunctionPointer()
			{
				nint result = 0;
				GetFunctionPointerDiscard(ref result);
				return result;
			}

			public unsafe static bool Invoke(in CullingSplit cullingSplit)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = GetFunctionPointer();
					if (functionPointer != (IntPtr)0)
					{
						return ((delegate* unmanaged[Cdecl]<ref CullingSplit, bool>)functionPointer)(ref cullingSplit);
					}
				}
				return _003CAreAllCullingSpheresValid_003Eg__HasValidCullingSphere_007C5_0_0024BurstManaged(in cullingSplit);
			}
		}

		[NativeDisableUnsafePtrRestriction]
		public BatchCullingContext BatchCullingContext;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<Plane> CameraReceiverPlanes;

		public ReceiverPlanes ReceiverPlanes;

		public bool CullingSpheresAreValid;

		public void Execute()
		{
			ReceiverPlanes.Init(in BatchCullingContext, CameraReceiverPlanes);
			CullingSpheresAreValid = AreAllCullingSpheresValid(in BatchCullingContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool AreAllCullingSpheresValid(in BatchCullingContext batchCullingContext)
		{
			for (int i = 0; i < batchCullingContext.cullingSplits.Length; i++)
			{
				if (!HasValidCullingSphere(in UnsafeCollectionExtensions.ElementAsRef(in batchCullingContext.cullingSplits, i)))
				{
					return false;
				}
			}
			return true;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[BurstCompile]
			[MonoPInvokeCallback(typeof(_003CAreAllCullingSpheresValid_003Eg__HasValidCullingSphere_007C5_0_0000198A_0024PostfixBurstDelegate))]
			static bool HasValidCullingSphere(in CullingSplit cullingSplit)
			{
				return _003CAreAllCullingSpheresValid_003Eg__HasValidCullingSphere_007C5_0_0000198A_0024BurstDirectCall.Invoke(in cullingSplit);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CompilerGenerated]
		[BurstCompile]
		public static bool _003CAreAllCullingSpheresValid_003Eg__HasValidCullingSphere_007C5_0_0024BurstManaged(in CullingSplit cullingSplit)
		{
			return cullingSplit.sphereRadius > 0f;
		}
	}

	private readonly struct DebugUtility : IDisposable
	{
		public struct DrawingOverrides
		{
			public BatchMaterialID OverrideBatchMaterialID;

			public BatchMaterialID OverrideBatchMaterialIDForward;

			public byte? OverrideBatchLayer;
		}

		[CanBeNull]
		private readonly WaaaghDebugData m_DebugData;

		private readonly GPUDrivenResourceRegistry m_ResourceRegistry;

		[CanBeNull]
		private readonly Material m_DebugOverdrawMaterial;

		private readonly GPUDrivenIndexAllocator.IndexAllocation m_DebugOverdrawMaterialIndex;

		[CanBeNull]
		private readonly Material m_DebugOverdrawMaterialForward;

		private readonly GPUDrivenIndexAllocator.IndexAllocation m_DebugOverdrawMaterialIndexForward;

		public DebugUtility(GPUDrivenResourceRegistry resourceRegistry, [CanBeNull] WaaaghDebugData debugData)
		{
			m_ResourceRegistry = resourceRegistry;
			m_DebugData = debugData;
			WaaaghDebugData debugData2 = m_DebugData;
			if ((object)debugData2 != null)
			{
				DebugResources resources = debugData2.Resources;
				if (resources != null)
				{
					Shader debugOverdrawPS = resources.DebugOverdrawPS;
					if ((object)debugOverdrawPS != null)
					{
						m_DebugOverdrawMaterial = CoreUtils.CreateEngineMaterial(debugOverdrawPS);
						m_DebugOverdrawMaterial.SetShaderPassEnabled("GBuffer", enabled: true);
						m_DebugOverdrawMaterial.SetShaderPassEnabled("ForwardLit", enabled: true);
						m_DebugOverdrawMaterialIndex = m_ResourceRegistry.GetOrRegisterMaterial(m_DebugOverdrawMaterial).EnsureAllocationValidity();
						m_DebugOverdrawMaterialForward = CoreUtils.CreateEngineMaterial(debugOverdrawPS);
						m_DebugOverdrawMaterialForward.SetShaderPassEnabled("GBuffer", enabled: false);
						m_DebugOverdrawMaterialForward.SetShaderPassEnabled("ForwardLit", enabled: true);
						m_DebugOverdrawMaterialIndexForward = m_ResourceRegistry.GetOrRegisterMaterial(m_DebugOverdrawMaterialForward).EnsureAllocationValidity();
						return;
					}
				}
			}
			m_DebugOverdrawMaterial = null;
			m_DebugOverdrawMaterialIndex = GPUDrivenIndexAllocator.IndexAllocation.Invalid;
			m_DebugOverdrawMaterialForward = null;
			m_DebugOverdrawMaterialIndexForward = GPUDrivenIndexAllocator.IndexAllocation.Invalid;
		}

		public DrawingOverrides? GetDrawingOverridesOrDefault(in GPUDrivenRendererGroupPool.ViewType viewType)
		{
			if (viewType == GPUDrivenRendererGroupPool.ViewType.Camera)
			{
				WaaaghDebugData debugData = m_DebugData;
				if ((object)debugData != null)
				{
					RenderingDebug renderingDebug = debugData.RenderingDebug;
					if (renderingDebug != null && renderingDebug.OverdrawMode == DebugOverdrawMode.QuadOverdraw && m_DebugOverdrawMaterial != null && !m_DebugOverdrawMaterialIndex.Equals(GPUDrivenIndexAllocator.IndexAllocation.Invalid) && m_DebugOverdrawMaterialForward != null && !m_DebugOverdrawMaterialIndexForward.Equals(GPUDrivenIndexAllocator.IndexAllocation.Invalid))
					{
						DrawingOverrides value = default(DrawingOverrides);
						value.OverrideBatchMaterialID = m_ResourceRegistry.ReadUnmanagedMaterialInfo(m_DebugOverdrawMaterialIndex).EffectiveBatchMaterialID;
						value.OverrideBatchMaterialIDForward = m_ResourceRegistry.ReadUnmanagedMaterialInfo(m_DebugOverdrawMaterialIndexForward).EffectiveBatchMaterialID;
						value.OverrideBatchLayer = 7;
						return value;
					}
				}
			}
			return null;
		}

		public void Dispose()
		{
			CoreUtils.Destroy(m_DebugOverdrawMaterial);
			CoreUtils.Destroy(m_DebugOverdrawMaterialForward);
		}
	}

	public struct ConstructDrawRangesAndCommandsContext
	{
		public int VisibleIndicesCount;

		public GPUDrivenCullingContext.LODInfo LODInfo;

		public bool CullingSpheresAreValid;
	}

	public readonly struct ViewObject
	{
		public readonly UnityEngine.Object View;

		[CanBeNull]
		public readonly Camera AsCamera;

		public readonly Vector3 Position;

		public ViewObject(UnityEngine.Object view)
		{
			View = view;
			AsCamera = view as Camera;
			Position = ((view is Component component) ? component.transform.position : Vector3.zero);
		}
	}

	private struct CPUInstanceVisibilityData
	{
		public NativeArray<uint> Mask;

		public JobHandle JobHandle;
	}

	private struct ViewBatch
	{
		public GPUDrivenRendererGroupPool.ViewType ViewType;

		public byte BatchLayer;
	}

	private struct SkipSubmittingDrawCommandsJob : IJob
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe BatchCullingOutputDrawCommands* DrawCommandsPtr;

		public unsafe void Execute()
		{
			DrawCommandsPtr->drawRangeCount = 0;
			DrawCommandsPtr->indirectDrawCommandCount = 0;
			DrawCommandsPtr->instanceSortingPositionFloatCount = 0;
		}
	}

	[BurstCompile]
	private struct CoarseInstanceCullingJob : IJobParallelForBatch
	{
		public const int kBatchSize = 32;

		public GPUDrivenCullingUtils.SplitInfo SplitInfo;

		public uint CullingLayerMask;

		public ulong SceneCullingMask;

		public GPUDrivenCullingContext.LODInfo LODInfo;

		public GPUDrivenDynamicFlags RequiredDynamicFlags;

		[ReadOnly]
		public NativeArray<int>.ReadOnly AllIndices;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<Plane>.ReadOnly CullingPlanes;

		[ReadOnly]
		public NativeArray<GPUDrivenVisibilityInfo>.ReadOnly VisibilityInfo;

		[ReadOnly]
		public NativeArray<GPUDrivenLODGroupData>.ReadOnly LODGroups;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<GPUDrivenLODViewCollection.ViewDependentLODGroupData>.ReadOnly ViewLODGroupData;

		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<uint> InstanceVisibilityMask;

		[NativeDisableUnsafePtrRestriction]
		public unsafe int* CullingStatsCounterPtr;

		public void Execute(int startIndex, int count)
		{
			uint num = 0u;
			for (int i = 0; i < count; i++)
			{
				int index = startIndex + i;
				int index2 = AllIndices[index];
				GPUDrivenVisibilityInfo visibilityInfo = VisibilityInfo[index2];
				if ((CullingLayerMask & visibilityInfo.GameObjectLayerMask) != 0)
				{
					bool flag = (visibilityInfo.VisibilityFlags & GPUDrivenVisibilityFlags.ForceVisibleForCPUCulling) != 0 || ((RequiredDynamicFlags == GPUDrivenDynamicFlags.None || (visibilityInfo.DynamicFlags & RequiredDynamicFlags) != 0) && GPUDrivenCullingUtils.LightSphereCulling(in SplitInfo, visibilityInfo.BoundingSphere));
					if (flag)
					{
						flag = GPUDrivenCullingUtils.FrustumCulling(ref CullingPlanes, visibilityInfo.BoundingSphere.xyz, visibilityInfo.AABBExtents.xyz);
					}
					if (flag && GPUDrivenLODVisibility.Compute(in visibilityInfo, in LODInfo, ref LODGroups, ViewLODGroupData) == 0f)
					{
						flag = false;
					}
					if (flag)
					{
						num |= (uint)(1 << i);
					}
				}
			}
			InstanceVisibilityMask[startIndex / 32] = num;
		}
	}

	[BurstCompile]
	private struct DrawCommandMaskingJob : IJobParallelForBatch
	{
		public struct SplitInfo
		{
			public GraphicsBufferHandle IndirectArgsBufferHandle;

			public GraphicsBufferHandle VisibleIndicesBufferHandle;

			public uint VisibleIndicesOffset;

			public int IndirectArgsOffset;
		}

		public const int kBatchSize = 64;

		[ReadOnly]
		public NativeArray<SplitCPUCullingInfo> SplitCPUCullingInfos;

		[ReadOnly]
		public NativeArray<int>.ReadOnly AllIndices;

		[ReadOnly]
		[DeallocateOnJobCompletion]
		public NativeArray<IntPtr> InstanceVisibilityMasks;

		[ReadOnly]
		public NativeArray<GPUDrivenRendererGroupPool.RendererGroupSlice>.ReadOnly RendererGroupSlices;

		public int PersistentIndexOffset;

		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<ulong> DrawCommandVisibilityMasks;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> ReadbackInstanceVisibilityMask;

		public int DrawCommandOffset;

		public int DrawCommandsPerSplit;

		[NativeDisableUnsafePtrRestriction]
		public unsafe BatchDrawCommandIndirect* IndirectDrawCommandsPtr;

		[ReadOnly]
		public NativeSlice<BatchDrawCommandIndirect> SingleSplitDrawCommands;

		[ReadOnly]
		[DeallocateOnJobCompletion]
		public NativeArray<SplitInfo> Splits;

		public unsafe void Execute(int startIndex, int count)
		{
			for (int i = 0; i < SplitCPUCullingInfos.Length; i++)
			{
				SplitCPUCullingInfo splitCPUCullingInfo = SplitCPUCullingInfos[i];
				Span<ulong> span = DrawCommandVisibilityMasks.AsSpan().Slice(splitCPUCullingInfo.DrawCommandVisibilityMaskOffset, splitCPUCullingInfo.DrawCommandVisibilityMaskCount);
				uint* ptr = (uint*)(void*)InstanceVisibilityMasks[i];
				ulong num = 0uL;
				for (int j = 0; j < count; j++)
				{
					int index = startIndex + j;
					GPUDrivenRendererGroupPool.RendererGroupSlice rendererGroupSlice = RendererGroupSlices[index];
					bool flag = false;
					for (int k = 0; k < rendererGroupSlice.IndexCount; k++)
					{
						int num2 = rendererGroupSlice.PersistentIndexOffset + k + PersistentIndexOffset;
						long num3 = ptr[num2 / 32];
						ulong num4 = (ulong)(1L << num2 % 32);
						int instanceIndex = AllIndices[num2];
						if (((ulong)num3 & num4) != 0L && IsVisibleInReadbackVisibilityMask(instanceIndex))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						num |= (ulong)(1L << j);
					}
				}
				span[startIndex / 64] = num;
				if (num == 0)
				{
					continue;
				}
				SplitInfo splitInfo = Splits[i];
				for (int l = 0; l < count; l++)
				{
					if ((num & (ulong)(1L << l)) != 0L)
					{
						int num5 = startIndex + l;
						BatchDrawCommandIndirect batchDrawCommandIndirect = SingleSplitDrawCommands[num5];
						int num6 = DrawCommandOffset + i * DrawCommandsPerSplit;
						BatchDrawCommandIndirect* num7 = IndirectDrawCommandsPtr + num6 + num5;
						*num7 = batchDrawCommandIndirect;
						num7->visibleOffset += splitInfo.VisibleIndicesOffset;
						num7->splitVisibilityMask = (ushort)(1 << i);
						num7->indirectArgsBufferOffset = (uint)((splitInfo.IndirectArgsOffset + num5) * 20);
						num7->indirectArgsBufferHandle = splitInfo.IndirectArgsBufferHandle;
						num7->visibleInstancesBufferHandle = splitInfo.VisibleIndicesBufferHandle;
					}
				}
			}
		}

		private bool IsVisibleInReadbackVisibilityMask(int instanceIndex)
		{
			if (!ReadbackInstanceVisibilityMask.IsCreated)
			{
				return true;
			}
			int num = instanceIndex / 32;
			int num2 = instanceIndex % 32;
			if (num >= ReadbackInstanceVisibilityMask.Length)
			{
				return true;
			}
			return (ReadbackInstanceVisibilityMask[num] & (1 << num2)) != 0;
		}
	}

	[BurstCompile]
	private struct FixupBatchDrawRangesJob : IJob
	{
		[ReadOnly]
		public NativeArray<BatchDrawRange> SingleSplitDrawRanges;

		[ReadOnly]
		[DeallocateOnJobCompletion]
		public NativeArray<SplitCPUCullingInfo> SplitCPUCullingInfos;

		[ReadOnly]
		[DeallocateOnJobCompletion]
		public NativeArray<ulong> DrawCommandVisibilityMasks;

		[ReadOnly]
		[DeallocateOnJobCompletion]
		public NativeArray<Plane> AllCullingPlanes;

		public uint BaseDrawCommandsOffset;

		public uint DrawCommandsPerSplit;

		public ulong SceneCullingMask;

		public DebugUtility.DrawingOverrides? DrawingOverrides;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 DestinationDrawCommandCounter;

		[NativeDisableUnsafePtrRestriction]
		public unsafe BatchDrawRange* DestinationDrawRangesPtr;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 DestinationDrawRangeCounter;

		public int SortingPositionsCount;

		[NativeDisableUnsafePtrRestriction]
		public unsafe float* DestinationSortingPositionsPtr;

		[NativeDisableUnsafePtrRestriction]
		public unsafe float* SourceSortingPositionsPtr;

		public unsafe void Execute()
		{
			NativeList<BatchDrawRange> nativeArray = new NativeList<BatchDrawRange>(SingleSplitDrawRanges.Length, Allocator.Temp);
			for (int i = 0; i < SplitCPUCullingInfos.Length; i++)
			{
				SplitCPUCullingInfo splitCPUCullingInfo = SplitCPUCullingInfos[i];
				ReadOnlySpan<ulong> readOnlySpan = DrawCommandVisibilityMasks.AsReadOnlySpan().Slice(splitCPUCullingInfo.DrawCommandVisibilityMaskOffset, splitCPUCullingInfo.DrawCommandVisibilityMaskCount);
				nativeArray.Clear();
				int num = 0;
				foreach (BatchDrawRange singleSplitDrawRange in SingleSplitDrawRanges)
				{
					int? num2 = null;
					uint drawCommandsBegin = singleSplitDrawRange.drawCommandsBegin;
					int num3 = (int)(singleSplitDrawRange.drawCommandsBegin + singleSplitDrawRange.drawCommandsCount);
					for (int j = (int)drawCommandsBegin; j < num3; j++)
					{
						ulong num4 = readOnlySpan[j / 64];
						ulong num5 = (ulong)(1L << j % 64);
						if ((num4 & num5) != 0L)
						{
							num++;
							if (num2 == j)
							{
								num2++;
								UnsafeCollectionExtensions.ElementAsRef(in nativeArray, nativeArray.Length - 1).drawCommandsCount++;
								continue;
							}
							num2 = j + 1;
							BatchDrawRange value = singleSplitDrawRange;
							value.drawCommandsBegin = (uint)j;
							value.drawCommandsCount = 1u;
							nativeArray.Add(in value);
						}
					}
				}
				DestinationDrawCommandCounter.Add(num);
				uint num6 = BaseDrawCommandsOffset + (uint)(i * (int)DrawCommandsPerSplit);
				for (int k = 0; k < nativeArray.Length; k++)
				{
					ref BatchDrawRange reference = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray, k);
					reference.drawCommandsBegin += num6;
					reference.filterSettings.sceneCullingMask = SceneCullingMask;
				}
				int num7 = DestinationDrawRangeCounter.Add(nativeArray.Length);
				UnsafeUtility.MemCpy(DestinationDrawRangesPtr + num7, nativeArray.GetUnsafePtr(), nativeArray.Length * UnsafeUtility.SizeOf<BatchDrawRange>());
			}
			if (SortingPositionsCount > 0)
			{
				UnsafeUtility.MemCpy(DestinationSortingPositionsPtr, SourceSortingPositionsPtr, SortingPositionsCount * 4);
			}
		}
	}

	private struct CullingOutputData
	{
		public int TotalDrawCommandCount;

		public unsafe BatchDrawCommandIndirect* IndirectDrawCommands;

		public int TotalDrawRangeCount;

		public unsafe BatchDrawRange* DrawRanges;

		public int TotalSortingPositionCount;

		public unsafe float* SortingPositions;
	}

	private struct ViewTypeCullingInfo
	{
		public ViewBatch ViewBatch;

		public int DrawCommandOffset;

		public int DrawCommandCount;

		public int DrawRangeOffset;

		public int DrawRangeCount;

		public int SortingPositionOffset;

		public int SortingPositionCount;
	}

	private struct CommandCache : IGPUDrivenMemoryProfilingSource
	{
		public NativeArray<BatchDrawCommandIndirect> SingleSplitCommands;

		public NativeList<BatchDrawRange> SingleSplitDrawRanges;

		public NativeArray<float3> SortingPositions;

		public bool Dirty;

		public JobHandle? JobHandle;

		public int SingleSplitDrawCommandCount;

		public CommandCache(int initialCapacity, Allocator allocator)
		{
			SingleSplitCommands = new NativeArray<BatchDrawCommandIndirect>(initialCapacity, allocator);
			SingleSplitDrawRanges = new NativeList<BatchDrawRange>(initialCapacity, allocator);
			SortingPositions = new NativeArray<float3>(initialCapacity, allocator);
			Dirty = true;
			SingleSplitDrawCommandCount = 0;
			JobHandle = null;
		}

		public void Reset()
		{
			SingleSplitDrawRanges.Clear();
			SingleSplitDrawCommandCount = 0;
		}

		public void EnsureCapacity(int capacity)
		{
			int length = SingleSplitCommands.Length;
			if (length < capacity)
			{
				int num = (int)math.ceil((float)capacity / (float)length) * length;
				ResizeAndFillNewItems(ref SingleSplitCommands, num);
				SingleSplitDrawRanges.SetCapacity(num);
				ResizeAndFillNewItems(ref SortingPositions, num);
			}
		}

		private unsafe static void ResizeAndFillNewItems<T>(ref NativeArray<T> array, int newCapacity) where T : unmanaged
		{
			int length = array.Length;
			ArrayExtensions.ResizeArray(ref array, newCapacity);
			byte* destination = (byte*)array.GetUnsafePtr() + (nint)length * (nint)sizeof(T);
			int num = (newCapacity - length) * UnsafeUtility.SizeOf<T>();
			UnsafeUtility.MemSet(destination, 0, num);
		}

		public void Dispose()
		{
			if (SingleSplitCommands.IsCreated)
			{
				SingleSplitCommands.Dispose();
				SingleSplitCommands = default(NativeArray<BatchDrawCommandIndirect>);
			}
			if (SingleSplitDrawRanges.IsCreated)
			{
				SingleSplitDrawRanges.Dispose();
				SingleSplitDrawRanges = default(NativeList<BatchDrawRange>);
			}
			if (SortingPositions.IsCreated)
			{
				SortingPositions.Dispose();
				SortingPositions = default(NativeArray<float3>);
			}
		}

		public void FillMemoryCounters(Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory.Counters.CounterCollection counters)
		{
			counters.CollectBufferSize(counters.CullingBatchingCPU, SingleSplitCommands);
			counters.CollectBufferSize(counters.CullingBatchingCPU, SingleSplitDrawRanges);
			counters.CollectBufferSize(counters.CullingBatchingCPU, SortingPositions);
		}

		public void Invalidate()
		{
			Dirty = true;
			JobHandle?.Complete();
			JobHandle = null;
		}
	}

	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private struct MarkSingleSplitDrawRangesForMergeJob : IJobParallelFor
	{
		public const int kBatchSize = 4;

		[WriteOnly]
		[NativeDisableUnsafePtrRestriction]
		public unsafe ulong* CanBeMergedMaskPtr;

		[ReadOnly]
		public NativeArray<GPUDrivenRendererGroupPool.RendererGroupSlice>.ReadOnly RendererGroupSlices;

		[ReadOnly]
		public NativeArray<GPUDrivenRendererGroupPool.RendererGroup>.ReadOnly RendererGroups;

		public unsafe void Execute(int maskIndex)
		{
			ulong num = 0uL;
			for (int i = 0; i < 64; i++)
			{
				int num2 = maskIndex * 64 + i;
				if (num2 >= RendererGroupSlices.Length)
				{
					break;
				}
				bool flag;
				if (num2 == 0)
				{
					flag = false;
				}
				else
				{
					GPUDrivenRendererGroupPool.RendererSettings groupRendererSettings = GetGroupRendererSettings(num2);
					GPUDrivenRendererGroupPool.RendererSettings groupRendererSettings2 = GetGroupRendererSettings(num2 - 1);
					flag = groupRendererSettings.Equals(groupRendererSettings2);
				}
				if (flag)
				{
					num |= (ulong)(1L << i);
				}
			}
			CanBeMergedMaskPtr[maskIndex] = num;
		}

		private GPUDrivenRendererGroupPool.RendererSettings GetGroupRendererSettings(int sliceIndex)
		{
			GPUDrivenIndexAllocator.IndexAllocation groupIndexAllocation = RendererGroupSlices[sliceIndex].GroupIndexAllocation;
			return RendererGroups[groupIndexAllocation.Index].Key.RendererSettings;
		}
	}

	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private struct CreateSingleSplitDrawRangesJob : IJob
	{
		[ReadOnly]
		[NativeDisableUnsafePtrRestriction]
		public unsafe ulong* CanBeMergedMaskPtr;

		public NativeList<BatchDrawRange> BatchDrawRanges;

		public int RendererGroupSlicesCount;

		public NativeList<int> DrawRangeGroupSliceIndices;

		public unsafe void Execute()
		{
			for (int i = 0; i < RendererGroupSlicesCount; i += 64)
			{
				int num = i / 64;
				ulong num2 = CanBeMergedMaskPtr[num];
				for (int j = 0; j < 64; j++)
				{
					int num3 = i + j;
					if (num3 >= RendererGroupSlicesCount)
					{
						break;
					}
					if (((ulong)(1L << j) & num2) != 0)
					{
						UnsafeCollectionExtensions.ElementAsRef(in BatchDrawRanges, BatchDrawRanges.Length - 1).drawCommandsCount++;
						continue;
					}
					uint drawCommandsBegin = 0u;
					if (BatchDrawRanges.Length > 0)
					{
						ref BatchDrawRange reference = ref UnsafeCollectionExtensions.ElementAsRef(in BatchDrawRanges, BatchDrawRanges.Length - 1);
						drawCommandsBegin = reference.drawCommandsBegin + reference.drawCommandsCount;
					}
					BatchDrawRange batchDrawRange = default(BatchDrawRange);
					batchDrawRange.drawCommandsBegin = drawCommandsBegin;
					batchDrawRange.drawCommandsCount = 1u;
					BatchDrawRange value = batchDrawRange;
					BatchDrawRanges.AddNoResize(value);
					DrawRangeGroupSliceIndices.AddNoResize(num3);
				}
			}
		}
	}

	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private struct FillSingleSplitBatchDrawRangesJob : IJobParallelFor
	{
		public const int kBatchSize = 16;

		[ReadOnly]
		public NativeArray<int> DrawRangeGroupSliceIndices;

		[ReadOnly]
		public NativeArray<GPUDrivenRendererGroupPool.RendererGroupSlice>.ReadOnly RendererGroupSlices;

		[ReadOnly]
		public NativeArray<GPUDrivenRendererGroupPool.RendererGroup>.ReadOnly RendererGroups;

		public NativeArray<BatchDrawRange> BatchDrawRanges;

		public byte BatchLayer;

		public bool OpaqueSorting;

		public void Execute(int index)
		{
			if (index < BatchDrawRanges.Length)
			{
				int index2 = DrawRangeGroupSliceIndices[index];
				GPUDrivenRendererGroupPool.RendererGroupSlice rendererGroupSlice = RendererGroupSlices[index2];
				int index3 = rendererGroupSlice.GroupIndexAllocation.Index;
				GPUDrivenRendererGroupPool.RendererSettings rendererSettings = RendererGroups[index3].Key.RendererSettings;
				ref BatchDrawRange reference = ref UnsafeCollectionExtensions.ElementAsRefUnchecked(in BatchDrawRanges, index);
				reference.drawCommandsType = BatchDrawCommandType.Indirect;
				reference.filterSettings = new BatchFilterSettings
				{
					renderingLayerMask = rendererSettings.RenderingLayerMask,
					layer = rendererSettings.Layer,
					batchLayer = BatchLayer,
					motionMode = rendererSettings.MotionVectorGenerationMode,
					shadowCastingMode = rendererSettings.ShadowCastingMode,
					receiveShadows = ((rendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.ReceiveShadows) != 0),
					staticShadowCaster = ((rendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.StaticShadowCater) != 0),
					allDepthSorted = (OpaqueSorting || rendererGroupSlice.IsTransparent)
				};
			}
		}
	}

	private static class Profiling
	{
		public static readonly ProfilingSampler BuildSingleSplitDraws = new ProfilingSampler("BuildSingleSplitDraws");

		public static readonly ProfilingSampler EnsureCommandCacheAndFillViewTypes = new ProfilingSampler("EnsureCommandCacheAndFillViewTypes");

		public static readonly ProfilingSampler ScheduleFillDrawCommands = new ProfilingSampler("ScheduleFillDrawCommands");

		public static readonly ProfilingSampler AllocateCullingContexts = new ProfilingSampler("AllocateCullingContexts");

		public static readonly ProfilingSampler ConstructDrawRangesAndCommands = new ProfilingSampler("ConstructDrawRangesAndCommands");

		public static readonly ProfilingSampler CoarseCPUFrustumCulling = new ProfilingSampler("CoarseCPUFrustumCulling");

		public static readonly ProfilingSampler GetCullingSphereAndLightMatrix = new ProfilingSampler("GetCullingSphereAndLightMatrix");

		public static readonly ProfilingSampler CombineJobs = new ProfilingSampler("CombineJobs");

		public static readonly ProfilingSampler PostRender = new ProfilingSampler("GPUDrivenBRGInstanceCuller PostRender");
	}

	[BurstCompile]
	private struct FreeUnsafePtrJob : IJob
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe void* Ptr;

		public Allocator Allocator;

		public unsafe void Execute()
		{
			UnsafeUtility.Free(Ptr, Allocator);
		}
	}

	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private struct BuildSingleSplitDrawCommandsJob : IJobParallelFor
	{
		public const int kBatchSize = 4;

		public int IndicesOffset;

		public GPUDrivenRendererGroupPool.ViewType ViewType;

		public int FixedLODIndex;

		public bool OpaqueSorting;

		public DebugUtility.DrawingOverrides? DrawingOverrides;

		[ReadOnly]
		public NativeArray<GPUDrivenRendererGroupPool.RendererGroupSlice>.ReadOnly RendererGroupSlices;

		[ReadOnly]
		public NativeArray<GPUDrivenRendererGroupPool.RendererGroup>.ReadOnly RendererGroups;

		[ReadOnly]
		public NativeArray<GPUDrivenResourceRegistry.UnmanagedMeshInfo>.ReadOnly MeshInfos;

		[ReadOnly]
		public NativeArray<GPUDrivenResourceRegistry.UnmanagedMaterialInfo>.ReadOnly MaterialInfos;

		[ReadOnly]
		public NativeArray<GPUDrivenVisibilityInfo>.ReadOnly VisibilityInfo;

		[ReadOnly]
		public NativeArray<int>.ReadOnly AllViewIndices;

		[ReadOnly]
		public NativeHashMap<GPUDrivenLightmapping.SceneMaterialKey, BatchMaterialID>.ReadOnly SceneLightmappedMaterials;

		[WriteOnly]
		public NativeArray<BatchDrawCommandIndirect> ResultingCommands;

		[WriteOnly]
		public NativeArray<float3> SortingPositions;

		public void Execute(int index)
		{
			GPUDrivenRendererGroupPool.RendererGroupSlice rendererGroupSlice = RendererGroupSlices[index];
			GPUDrivenRendererGroupPool.RendererGroup rendererGroup = RendererGroups[rendererGroupSlice.GroupIndexAllocation.Index];
			ref GPUDrivenRendererGroupPool.RendererGroupKey key = ref rendererGroup.Key;
			GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo = MaterialInfos[key.MaterialAllocation.Index];
			GPUDrivenResourceRegistry.UnmanagedMeshInfo unmanagedMeshInfo = MeshInfos[key.MeshAllocation.Index];
			BatchDrawCommandFlags batchDrawCommandFlags = BatchDrawCommandFlags.None;
			batchDrawCommandFlags |= BatchDrawCommandFlags.LODCrossFadeValuePacked;
			if (FixedLODIndex == -1 && (key.RendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.LODCrossFade) != 0)
			{
				batchDrawCommandFlags |= BatchDrawCommandFlags.LODCrossFadeKeyword;
			}
			if ((key.RendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.FlipWinding) != 0)
			{
				batchDrawCommandFlags |= BatchDrawCommandFlags.FlipWinding;
			}
			if (ViewType == GPUDrivenRendererGroupPool.ViewType.CameraMotionVectors)
			{
				batchDrawCommandFlags |= BatchDrawCommandFlags.HasMotion;
			}
			BatchMaterialID materialID = unmanagedMaterialInfo.EffectiveBatchMaterialID;
			if (ViewType == GPUDrivenRendererGroupPool.ViewType.Camera && (key.RendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.UseLightMaps) != 0)
			{
				GPUDrivenLightmapping.SceneMaterialKey sceneMaterialKey = default(GPUDrivenLightmapping.SceneMaterialKey);
				sceneMaterialKey.Scene = key.RendererSettings.Scene;
				sceneMaterialKey.OriginalMaterialID = unmanagedMaterialInfo.OriginalBatchMaterialID;
				GPUDrivenLightmapping.SceneMaterialKey key2 = sceneMaterialKey;
				if (SceneLightmappedMaterials.TryGetValue(key2, out var item))
				{
					materialID = item;
				}
			}
			int sortingPosition = 0;
			if (OpaqueSorting || rendererGroupSlice.IsTransparent)
			{
				batchDrawCommandFlags |= BatchDrawCommandFlags.HasSortingPosition;
				int num = AllViewIndices[rendererGroupSlice.PersistentIndexOffset + IndicesOffset];
				float3 value;
				if (rendererGroupSlice.IsTransparent)
				{
					value = VisibilityInfo[num].BoundingSphere.xyz;
				}
				else
				{
					int num2 = math.min(rendererGroupSlice.IndexCount, 8);
					float num3 = math.rcp(num2);
					value = default(float3);
					for (int i = 0; i < num2; i++)
					{
						float3 xyz = VisibilityInfo[num + i].BoundingSphere.xyz;
						value += xyz * num3;
					}
				}
				SortingPositions[index] = value;
				sortingPosition = index * 3;
			}
			GPUDrivenMetadataAuthoring.MetadataComponents metadataComponents = key.RendererSettings.GetMetadataComponents();
			GPUDrivenResourceRegistry.MaterialBatchCollection.Batch batch = unmanagedMaterialInfo.BatchCollection.Get(in metadataComponents);
			ResultingCommands[index] = new BatchDrawCommandIndirect
			{
				visibleOffset = (uint)rendererGroupSlice.VisibleIndexOffset,
				batchID = batch.ID,
				materialID = materialID,
				meshID = unmanagedMeshInfo.BatchMeshID,
				splitVisibilityMask = 255,
				flags = batchDrawCommandFlags,
				sortingPosition = sortingPosition,
				topology = MeshTopology.Triangles,
				lightmapIndex = ushort.MaxValue
			};
		}
	}

	private const int kFloatsPerPosition = 3;

	private const int kBitsInULong = 64;

	private const int kBitsInUInt = 32;

	private static readonly Plane[] s_FrustumPlanes = new Plane[6];

	private readonly GPUDrivenBatchRendererGroup m_BRG;

	private readonly CommandCache[] m_CommandCaches;

	private readonly DebugUtility m_DebugUtility;

	private readonly GPUDrivenLightmapping m_Lightmapping;

	private readonly GPUDrivenLODGroupRepository m_LODGroupRepository;

	private readonly GPUDrivenPersistentData m_PersistentData;

	private readonly GPUDrivenRendererGroupPool m_RendererGroupPool;

	private readonly GPUDrivenResourceRegistry m_ResourceRegistry;

	private NativeArray<Plane> m_CameraReceiverPlanes;

	private NativeList<GPUDrivenCullingContext> m_CullingContexts;

	private NativeList<GPUDrivenCullingContext> m_CullingContextsToCleanup;

	public GPUDrivenCullingResourcesPool CullingResourcesPool { get; }

	public GPUDrivenVisibilityMaskPool VisibilityMaskPool { get; }

	public GPUDrivenVisibilityReadback VisibilityReadback { get; }

	public GPUDrivenBRGInstanceCuller(GPUDrivenBatchRendererGroup brg, GPUDrivenResourceRegistry resourceRegistry, GPUDrivenRendererGroupPool rendererGroupPool, GPUDrivenPersistentData persistentData, GPUDrivenLightmapping lightmapping, GPUDrivenBufferUtils bufferUtils, GPUDrivenCommandQueue commandQueue, GPUDrivenLODGroupRepository lodGroupRepository, [CanBeNull] WaaaghDebugData debugData)
	{
		m_BRG = brg;
		m_ResourceRegistry = resourceRegistry;
		m_RendererGroupPool = rendererGroupPool;
		m_PersistentData = persistentData;
		m_Lightmapping = lightmapping;
		m_CullingContexts = new NativeList<GPUDrivenCullingContext>(Allocator.Persistent);
		m_CullingContextsToCleanup = new NativeList<GPUDrivenCullingContext>(Allocator.Persistent);
		m_CommandCaches = new CommandCache[rendererGroupPool.ViewTypeInfos.Length];
		for (int i = 0; i < m_CommandCaches.Length; i++)
		{
			m_CommandCaches[i] = new CommandCache(brg.Settings.InitialRendererGroupsCapacity, Allocator.Persistent);
		}
		CullingResourcesPool = new GPUDrivenCullingResourcesPool(m_BRG.Settings, commandQueue);
		VisibilityMaskPool = new GPUDrivenVisibilityMaskPool(bufferUtils, commandQueue);
		VisibilityReadback = new GPUDrivenVisibilityReadback();
		m_LODGroupRepository = lodGroupRepository;
		m_DebugUtility = new DebugUtility(resourceRegistry, debugData);
		m_CameraReceiverPlanes = new NativeArray<Plane>(6, Allocator.Persistent);
	}

	public void Dispose()
	{
		m_CameraReceiverPlanes.Dispose();
		m_DebugUtility.Dispose();
		if (m_CommandCaches != null)
		{
			for (int i = 0; i < m_CommandCaches.Length; i++)
			{
				ref CommandCache reference = ref m_CommandCaches[i];
				reference.Invalidate();
				reference.Dispose();
			}
		}
		CullingResourcesPool.Dispose();
		VisibilityMaskPool.Dispose();
		VisibilityReadback.Dispose();
		if (m_CullingContexts.IsCreated)
		{
			ClearCPUInstanceVisibility(ref m_CullingContexts);
			ClearCullingContexts(ref m_CullingContexts);
			m_CullingContexts.Dispose();
			m_CullingContexts = default(NativeList<GPUDrivenCullingContext>);
		}
		if (m_CullingContextsToCleanup.IsCreated)
		{
			ClearCPUInstanceVisibility(ref m_CullingContextsToCleanup);
			ClearCullingContexts(ref m_CullingContextsToCleanup);
			m_CullingContextsToCleanup.Dispose();
			m_CullingContextsToCleanup = default(NativeList<GPUDrivenCullingContext>);
		}
	}

	public void FillMemoryCounters(Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory.Counters.CounterCollection counters)
	{
		counters.CollectBufferSize(counters.CullingBatchingCPU, m_CullingContexts);
		counters.CollectBufferSize(counters.CullingBatchingCPU, m_CullingContextsToCleanup);
		for (int i = 0; i < m_CommandCaches.Length; i++)
		{
			ref CommandCache reference = ref m_CommandCaches[i];
			reference.JobHandle?.Complete();
			reference.JobHandle = null;
			reference.FillMemoryCounters(counters);
		}
		CullingResourcesPool.FillMemoryCounters(counters);
		VisibilityMaskPool.FillMemoryCounters(counters);
		VisibilityReadback.FillMemoryCounters(counters);
	}

	public void InvalidateCommandCache(GPUDrivenRendererGroupPool.ViewType viewType)
	{
		m_CommandCaches[(int)viewType].Invalidate();
	}

	private void ScheduleCommandCacheRefresh(in ViewBatch viewBatch)
	{
		GPUDrivenRendererGroupPool.ViewType viewType = viewBatch.ViewType;
		ref CommandCache reference = ref m_CommandCaches[(int)viewType];
		if (!reference.JobHandle.HasValue && reference.Dirty)
		{
			m_RendererGroupPool.BeginRebuildingGroupInfo();
			m_RendererGroupPool.CompleteGroupSliceBuilding(viewType);
			reference.Reset();
			reference.Dirty = false;
			int length = m_RendererGroupPool.GetAllRendererGroupSlicesReadonly(viewType).Length;
			reference.EnsureCapacity(length);
			reference.SingleSplitDrawCommandCount = length;
			JobHandle value = ConstructSingleSplitDraws(in m_RendererGroupPool.ViewTypeInfos[(int)viewType], length, ref reference.SingleSplitCommands, ref reference.SingleSplitDrawRanges, ref reference.SortingPositions, viewBatch.BatchLayer);
			reference.JobHandle = value;
		}
	}

	[Conditional("DEBUG")]
	private void UpdateProfilerCounters()
	{
		Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Main.Counters.SingleSplitDrawCommands.Value = m_CommandCaches[0].SingleSplitDrawCommandCount;
		Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Main.Counters.SingleSplitDrawRanges.Value = m_CommandCaches[0].SingleSplitDrawRanges.Length;
		Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Main.Counters.SingleSplitShadowDrawCommands.Value = m_CommandCaches[3].SingleSplitDrawCommandCount;
		Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Main.Counters.SingleSplitShadowDrawRanges.Value = m_CommandCaches[3].SingleSplitDrawRanges.Length;
		Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Main.Counters.SingleSplitShadowCullLightmappedDrawCommands.Value = m_CommandCaches[4].SingleSplitDrawCommandCount;
	}

	public NativeArray<GPUDrivenCullingContext> GetCullingContextsAndClear(Allocator allocator)
	{
		NativeArray<GPUDrivenCullingContext> nativeArray = new NativeArray<GPUDrivenCullingContext>(m_CullingContexts.Length, allocator);
		nativeArray.CopyFrom(m_CullingContexts.AsArray());
		for (int i = 0; i < nativeArray.Length; i++)
		{
			ref GPUDrivenCullingContext reference = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray, i);
			ref GPUDrivenCullingContext reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in m_CullingContexts, i);
			m_CullingContextsToCleanup.Add(in reference2);
			NativeArray<Plane> frustumPlanes = reference2.FrustumPlanes;
			reference.FrustumPlanes = new NativeArray<Plane>(frustumPlanes.Length, allocator);
			reference.FrustumPlanes.CopyFrom(reference2.FrustumPlanes);
		}
		m_CullingContexts.Clear();
		return nativeArray;
	}

	private static void ClearCPUInstanceVisibility(ref NativeList<GPUDrivenCullingContext> cullingContexts)
	{
		for (int i = 0; i < cullingContexts.Length; i++)
		{
			UnsafeCollectionExtensions.ElementAsRef(in cullingContexts, i).ClearCPUInstanceVisibility();
		}
	}

	private static void ClearCullingContexts(ref NativeList<GPUDrivenCullingContext> cullingContexts)
	{
		foreach (GPUDrivenCullingContext cullingContext in cullingContexts)
		{
			NativeArray<Plane> frustumPlanes = cullingContext.FrustumPlanes;
			if (frustumPlanes.IsCreated)
			{
				frustumPlanes.Dispose();
			}
		}
		cullingContexts.Clear();
	}

	private bool PassesDebugFilter(in BatchCullingContext batchCullingContext)
	{
		if (m_BRG.DebugData != null && m_BRG.DebugData.ViewTypeFilter != 0 && batchCullingContext.viewType != m_BRG.DebugData.ViewTypeFilter)
		{
			return false;
		}
		return true;
	}

	public unsafe JobHandle OnPerformMainPassCulling(in BatchCullingContext batchCullingContext, BatchCullingOutput cullingOutput)
	{
		if (batchCullingContext.viewType == BatchCullingViewType.Camera)
		{
			m_CameraReceiverPlanes.CopyFrom(batchCullingContext.cullingPlanes);
		}
		GPUDrivenBRGDebug debugData = m_BRG.DebugData;
		if (debugData != null && debugData.SkipRendering)
		{
			return default(JobHandle);
		}
		if (!PassesDebugFilter(in batchCullingContext))
		{
			return default(JobHandle);
		}
		NativeArray<ViewTypeCullingInfo> nativeArray = BuildViewTypeCullingInfos(in batchCullingContext, Allocator.Temp);
		if (nativeArray.Length == 0)
		{
			return default(JobHandle);
		}
		BatchPackedCullingViewID viewID = batchCullingContext.viewID;
		foreach (ViewTypeCullingInfo item in nativeArray)
		{
			GetFixedLODIndexAndBias(item.ViewBatch.ViewType, out var fixedLODIndex, out var _);
			if (fixedLODIndex == -1)
			{
				m_LODGroupRepository.OnMetViewID(viewID);
			}
		}
		CullingOutputData cullingOutputData = AllocateCullingOutputData(in UnsafeCollectionExtensions.ElementAsRef(in nativeArray, nativeArray.Length - 1), Allocator.TempJob);
		BatchCullingOutputDrawCommands* unsafePtr = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();
		*unsafePtr = new BatchCullingOutputDrawCommands
		{
			indirectDrawCommandCount = 0,
			drawRanges = cullingOutputData.DrawRanges,
			drawRangeCount = 0,
			indirectDrawCommands = cullingOutputData.IndirectDrawCommands,
			instanceSortingPositions = cullingOutputData.SortingPositions,
			instanceSortingPositionFloatCount = cullingOutputData.TotalSortingPositionCount
		};
		UnsafeAtomicCounter32 drawCommandCounter = new UnsafeAtomicCounter32(&unsafePtr->indirectDrawCommandCount);
		UnsafeAtomicCounter32 drawRangeCounter = new UnsafeAtomicCounter32(&unsafePtr->drawRangeCount);
		UnityEngine.Object view = ObjectDispatcherService.FindByInstanceId<UnityEngine.Object>(viewID.GetInstanceID());
		ViewObject viewObject = new ViewObject(view);
		JobHandle jobHandle = ScheduleFillDrawCommands(nativeArray, in batchCullingContext, in viewObject, in cullingOutputData, drawCommandCounter, drawRangeCounter);
		debugData = m_BRG.DebugData;
		if (debugData != null && debugData.SkipSubmittingDrawCommands)
		{
			SkipSubmittingDrawCommandsJob jobData = default(SkipSubmittingDrawCommandsJob);
			jobData.DrawCommandsPtr = unsafePtr;
			jobHandle = jobData.Schedule(jobHandle);
		}
		return jobHandle;
	}

	[MustUseReturnValue]
	private unsafe static CullingOutputData AllocateCullingOutputData(in ViewTypeCullingInfo lastViewTypeCullingInfo, Allocator allocator)
	{
		CullingOutputData result = default(CullingOutputData);
		result.TotalDrawCommandCount = lastViewTypeCullingInfo.DrawCommandOffset + lastViewTypeCullingInfo.DrawCommandCount;
		result.IndirectDrawCommands = Malloc<BatchDrawCommandIndirect>((uint)result.TotalDrawCommandCount, allocator);
		result.TotalDrawRangeCount = lastViewTypeCullingInfo.DrawRangeOffset + lastViewTypeCullingInfo.DrawRangeCount;
		result.DrawRanges = Malloc<BatchDrawRange>((uint)result.TotalDrawRangeCount, allocator);
		result.TotalSortingPositionCount = lastViewTypeCullingInfo.SortingPositionOffset + lastViewTypeCullingInfo.SortingPositionCount;
		result.SortingPositions = Malloc<float>((uint)result.TotalSortingPositionCount, allocator);
		return result;
	}

	private NativeArray<ViewTypeCullingInfo> BuildViewTypeCullingInfos(in BatchCullingContext batchCullingContext, Allocator allocator)
	{
		GPUDrivenRendererGroupPool.ViewType mainViewType = BRGCullingContextToViewType(in batchCullingContext);
		NativeList<ViewTypeCullingInfo> nativeArray = new NativeList<ViewTypeCullingInfo>(2, allocator);
		EnsureCommandCacheAndFillViewTypes(mainViewType, nativeArray);
		for (int i = 0; i < nativeArray.Length; i++)
		{
			ref ViewTypeCullingInfo reference = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray, i);
			ref CommandCache reference2 = ref m_CommandCaches[(int)reference.ViewBatch.ViewType];
			int drawCommandOffset;
			int drawRangeOffset;
			int sortingPositionOffset;
			if (i == 0)
			{
				drawCommandOffset = 0;
				drawRangeOffset = 0;
				sortingPositionOffset = 0;
			}
			else
			{
				ref ViewTypeCullingInfo reference3 = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray, i - 1);
				drawCommandOffset = reference3.DrawCommandOffset + reference3.DrawCommandCount;
				drawRangeOffset = reference3.DrawRangeOffset + reference3.DrawRangeCount;
				sortingPositionOffset = reference3.SortingPositionOffset + reference3.SortingPositionCount;
			}
			reference.DrawCommandOffset = drawCommandOffset;
			reference.DrawCommandCount = reference2.SingleSplitDrawCommandCount * batchCullingContext.cullingSplits.Length;
			reference.DrawRangeOffset = drawRangeOffset;
			int singleSplitDrawCommandCount = reference2.SingleSplitDrawCommandCount;
			reference.DrawRangeCount = singleSplitDrawCommandCount * batchCullingContext.cullingSplits.Length;
			reference.SortingPositionOffset = sortingPositionOffset;
			reference.SortingPositionCount = reference2.SingleSplitDrawCommandCount * 3;
		}
		return nativeArray.AsArray();
	}

	private void EnsureCommandCacheAndFillViewTypes(GPUDrivenRendererGroupPool.ViewType mainViewType, NativeList<ViewTypeCullingInfo> resultViewTypes)
	{
		using (new ProfilingScope(Profiling.EnsureCommandCacheAndFillViewTypes))
		{
			NativeList<ViewBatch> nativeArray = new NativeList<ViewBatch>(3, Allocator.Temp);
			CollectViewBatches(mainViewType, nativeArray);
			for (int i = 0; i < nativeArray.Length; i++)
			{
				ViewTypeCullingInfo value = new ViewTypeCullingInfo
				{
					ViewBatch = UnsafeCollectionExtensions.ElementAsRef(in nativeArray, i)
				};
				resultViewTypes.Add(in value);
			}
			nativeArray.Clear();
			CollectViewBatches(GPUDrivenRendererGroupPool.ViewType.Camera, nativeArray);
			CollectViewBatches(GPUDrivenRendererGroupPool.ViewType.Shadows, nativeArray);
			CollectViewBatches(GPUDrivenRendererGroupPool.ViewType.ShadowsCullLightmapped, nativeArray);
			for (int j = 0; j < nativeArray.Length; j++)
			{
				ScheduleCommandCacheRefresh(in UnsafeCollectionExtensions.ElementAsRef(in nativeArray, j));
			}
			if (resultViewTypes.Length == 1)
			{
				ViewTypeCullingInfo value = resultViewTypes[0];
				GetCommandCacheJobHandleOrDefault(in value).Complete();
			}
			else
			{
				ViewTypeCullingInfo value = resultViewTypes[0];
				JobHandle job = GetCommandCacheJobHandleOrDefault(in value);
				value = resultViewTypes[1];
				JobHandle job2 = GetCommandCacheJobHandleOrDefault(in value);
				if (resultViewTypes.Length == 2)
				{
					JobHandle.CompleteAll(ref job, ref job2);
				}
				else
				{
					value = resultViewTypes[2];
					JobHandle job3 = GetCommandCacheJobHandleOrDefault(in value);
					JobHandle.CompleteAll(ref job, ref job2, ref job3);
				}
			}
			for (int k = 0; k < resultViewTypes.Length; k++)
			{
				ref ViewTypeCullingInfo reference = ref UnsafeCollectionExtensions.ElementAsRef(in resultViewTypes, k);
				m_CommandCaches[(int)reference.ViewBatch.ViewType].JobHandle = null;
			}
			int num = 0;
			while (num < resultViewTypes.Length)
			{
				ref ViewTypeCullingInfo reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in resultViewTypes, num);
				ref CommandCache reference3 = ref m_CommandCaches[(int)reference2.ViewBatch.ViewType];
				if (reference3.SingleSplitDrawCommandCount == 0 || reference3.SingleSplitCommands.Length == 0)
				{
					resultViewTypes.RemoveAtSwapBack(num);
				}
				else
				{
					num++;
				}
			}
		}
	}

	private JobHandle GetCommandCacheJobHandleOrDefault(in ViewTypeCullingInfo viewTypeCullingInfo)
	{
		return m_CommandCaches[(int)viewTypeCullingInfo.ViewBatch.ViewType].JobHandle.GetValueOrDefault();
	}

	private static void CollectViewBatches(GPUDrivenRendererGroupPool.ViewType mainViewType, NativeList<ViewBatch> viewBatches)
	{
		ViewBatch value = new ViewBatch
		{
			ViewType = mainViewType,
			BatchLayer = 1
		};
		viewBatches.Add(in value);
		if (mainViewType == GPUDrivenRendererGroupPool.ViewType.Camera)
		{
			value = new ViewBatch
			{
				ViewType = GPUDrivenRendererGroupPool.ViewType.DepthOnly,
				BatchLayer = 2
			};
			viewBatches.Add(in value);
			value = new ViewBatch
			{
				ViewType = GPUDrivenRendererGroupPool.ViewType.CameraMotionVectors,
				BatchLayer = 3
			};
			viewBatches.Add(in value);
		}
	}

	private static GPUDrivenRendererGroupPool.ViewType BRGCullingContextToViewType(in BatchCullingContext batchCullingContext)
	{
		if ((batchCullingContext.viewType == BatchCullingViewType.Light) & ((batchCullingContext.cullingFlags & BatchCullingFlags.CullLightmappedShadowCasters) != 0))
		{
			return GPUDrivenRendererGroupPool.ViewType.ShadowsCullLightmapped;
		}
		return batchCullingContext.viewType switch
		{
			BatchCullingViewType.Camera => GPUDrivenRendererGroupPool.ViewType.Camera, 
			BatchCullingViewType.Light => GPUDrivenRendererGroupPool.ViewType.Shadows, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private unsafe JobHandle ScheduleFillDrawCommands(NativeArray<ViewTypeCullingInfo> viewTypeCullingInfos, in BatchCullingContext batchCullingContext, in ViewObject viewObject, in CullingOutputData cullingOutputData, UnsafeAtomicCounter32 drawCommandCounter, UnsafeAtomicCounter32 drawRangeCounter)
	{
		using (new ProfilingScope(Profiling.ScheduleFillDrawCommands))
		{
			NativeList<JobHandle> nativeList = new NativeList<JobHandle>(viewTypeCullingInfos.Length * 3 * 4, Allocator.Temp);
			ReceiverPlanes receiverPlanes = default(ReceiverPlanes);
			receiverPlanes.Planes = new NativeList<Plane>(36, Allocator.TempJob);
			receiverPlanes.LightFacingPlaneCount = 0;
			ReceiverPlanes receiverPlanes2 = receiverPlanes;
			PrepareCullingDataJob prepareCullingDataJob = default(PrepareCullingDataJob);
			prepareCullingDataJob.BatchCullingContext = batchCullingContext;
			prepareCullingDataJob.ReceiverPlanes = receiverPlanes2;
			prepareCullingDataJob.CameraReceiverPlanes = m_CameraReceiverPlanes;
			PrepareCullingDataJob jobData = prepareCullingDataJob;
			IJobExtensions.RunByRef(ref jobData);
			ConstructDrawRangesAndCommandsContext constructDrawRangesAndCommandsContext = default(ConstructDrawRangesAndCommandsContext);
			constructDrawRangesAndCommandsContext.VisibleIndicesCount = m_BRG.InstancesCount;
			constructDrawRangesAndCommandsContext.LODInfo = BuildLODInfo(in batchCullingContext.lodParameters, batchCullingContext.viewID, viewTypeCullingInfos[0].ViewBatch.ViewType);
			constructDrawRangesAndCommandsContext.CullingSpheresAreValid = jobData.CullingSpheresAreValid;
			ConstructDrawRangesAndCommandsContext constructContext = constructDrawRangesAndCommandsContext;
			foreach (ViewTypeCullingInfo item in viewTypeCullingInfos)
			{
				ViewTypeCullingInfo viewTypeCullingInfo = item;
				NativeArray<CPUInstanceVisibilityData> nativeArray = new NativeArray<CPUInstanceVisibilityData>(batchCullingContext.cullingSplits.Length, Allocator.Temp);
				GPUDrivenRendererGroupPool.ViewType viewType = viewTypeCullingInfo.ViewBatch.ViewType;
				ref CommandCache reference = ref m_CommandCaches[(int)viewType];
				NativeSlice<BatchDrawCommandIndirect> singleSplitCommands = reference.SingleSplitCommands.Slice(0, reference.SingleSplitDrawCommandCount);
				NativeList<BatchDrawRange> singleSplitDrawRanges = reference.SingleSplitDrawRanges;
				NativeArray<float3> singleSplitSortingPositions = reference.SortingPositions;
				JobHandle value = ConstructDrawRangesAndCommands(in m_RendererGroupPool.ViewTypeInfos[(int)viewType], in viewTypeCullingInfo, in constructContext, ref singleSplitCommands, ref singleSplitDrawRanges, ref singleSplitSortingPositions, in batchCullingContext, in receiverPlanes2, in viewObject, cullingOutputData.IndirectDrawCommands, cullingOutputData.DrawRanges, cullingOutputData.SortingPositions, default(JobHandle), nativeArray.AsSpan(), drawCommandCounter, drawRangeCounter);
				nativeList.Add(in value);
			}
			using (new ProfilingScope(Profiling.CombineJobs))
			{
				JobHandle job = JobHandle.CombineDependencies(nativeList.AsArray());
				return receiverPlanes2.Dispose(job);
			}
		}
	}

	private unsafe JobHandle ConstructSingleSplitDraws(in GPUDrivenRendererGroupPool.ViewTypeInfo viewTypeInfo, int splitDrawCommandCount, ref NativeArray<BatchDrawCommandIndirect> singleSplitCommands, ref NativeList<BatchDrawRange> singleSplitDrawRanges, ref NativeArray<float3> sortingPositions, byte batchLayer)
	{
		using (new ProfilingScope(Profiling.BuildSingleSplitDraws))
		{
			NativeArray<GPUDrivenRendererGroupPool.RendererGroupSlice>.ReadOnly allRendererGroupSlicesReadonly = m_RendererGroupPool.GetAllRendererGroupSlicesReadonly(viewTypeInfo.ViewType);
			GetFixedLODIndexAndBias(viewTypeInfo.ViewType, out var fixedLODIndex, out var _);
			bool opaqueSortingCPU = m_BRG.Settings.OpaqueSortingCPU;
			DebugUtility.DrawingOverrides? drawingOverrides = null;
			BuildSingleSplitDrawCommandsJob jobData = default(BuildSingleSplitDrawCommandsJob);
			jobData.ViewType = viewTypeInfo.ViewType;
			jobData.FixedLODIndex = fixedLODIndex;
			jobData.IndicesOffset = -viewTypeInfo.IndicesOffset;
			jobData.OpaqueSorting = opaqueSortingCPU;
			jobData.DrawingOverrides = drawingOverrides;
			jobData.AllViewIndices = m_RendererGroupPool.GetAllIndicesReadonly(viewTypeInfo.ViewType);
			jobData.SceneLightmappedMaterials = m_Lightmapping.GetSceneLightmappedMaterialsReadonly();
			jobData.ResultingCommands = singleSplitCommands;
			jobData.SortingPositions = sortingPositions;
			jobData.RendererGroupSlices = allRendererGroupSlicesReadonly;
			jobData.MaterialInfos = m_ResourceRegistry.GetInnerUnmanagedMaterialPool();
			jobData.MeshInfos = m_ResourceRegistry.GetInnerUnmanagedMeshPool();
			jobData.RendererGroups = m_RendererGroupPool.GetInnerPool();
			jobData.VisibilityInfo = m_PersistentData.GetAllVisibilityInfoReadonly();
			JobHandle job = IJobParallelForExtensions.Schedule(jobData, allRendererGroupSlicesReadonly.Length, 4);
			NativeList<int> drawRangeGroupSliceIndices = new NativeList<int>(splitDrawCommandCount, Allocator.TempJob);
			int num = Alignment.AlignUp(allRendererGroupSlicesReadonly.Length, 64) / 64;
			ulong* ptr = Malloc<ulong>((uint)num);
			JobHandle jobHandle = IJobParallelForExtensions.Schedule(dependsOn: IJobExtensions.Schedule(dependsOn: IJobParallelForExtensions.Schedule(new MarkSingleSplitDrawRangesForMergeJob
			{
				RendererGroups = m_RendererGroupPool.GetInnerPool(),
				CanBeMergedMaskPtr = ptr,
				RendererGroupSlices = allRendererGroupSlicesReadonly
			}, num, 4), jobData: new CreateSingleSplitDrawRangesJob
			{
				RendererGroupSlicesCount = allRendererGroupSlicesReadonly.Length,
				CanBeMergedMaskPtr = ptr,
				BatchDrawRanges = singleSplitDrawRanges,
				DrawRangeGroupSliceIndices = drawRangeGroupSliceIndices
			}), jobData: new FillSingleSplitBatchDrawRangesJob
			{
				DrawRangeGroupSliceIndices = drawRangeGroupSliceIndices.AsDeferredJobArray(),
				RendererGroups = m_RendererGroupPool.GetInnerPool(),
				BatchDrawRanges = singleSplitDrawRanges.AsDeferredJobArray(),
				RendererGroupSlices = allRendererGroupSlicesReadonly,
				BatchLayer = batchLayer,
				OpaqueSorting = opaqueSortingCPU
			}, arrayLength: splitDrawCommandCount, innerloopBatchCount: 16);
			drawRangeGroupSliceIndices.Dispose(jobHandle);
			new FreeUnsafePtrJob
			{
				Ptr = ptr,
				Allocator = Allocator.TempJob
			}.Schedule(jobHandle);
			return JobHandle.CombineDependencies(job, jobHandle);
		}
	}

	private unsafe JobHandle ConstructDrawRangesAndCommands(in GPUDrivenRendererGroupPool.ViewTypeInfo viewTypeInfo, in ViewTypeCullingInfo viewTypeCullingInfo, in ConstructDrawRangesAndCommandsContext constructContext, ref NativeSlice<BatchDrawCommandIndirect> singleSplitCommands, ref NativeList<BatchDrawRange> singleSplitDrawRanges, ref NativeArray<float3> singleSplitSortingPositions, in BatchCullingContext batchCullingContext, in ReceiverPlanes receiverPlanes, in ViewObject viewObject, BatchDrawCommandIndirect* pIndirectDrawCommands, BatchDrawRange* pDrawRanges, float* pSortingPositions, JobHandle dependency, Span<CPUInstanceVisibilityData> instanceVisibilityMaskData, UnsafeAtomicCounter32 drawCommandCounter, UnsafeAtomicCounter32 drawRangeCounter)
	{
		using (new ProfilingScope(Profiling.ConstructDrawRangesAndCommands))
		{
			int cpuInstanceVisibilityMaskCapacity = Alignment.AlignUp(m_RendererGroupPool.GetAllIndicesReadonly(viewTypeInfo.ViewType).Length, 32) / 32;
			int length = m_CullingContexts.Length;
			int num = 0;
			Span<CullingSplit> span = batchCullingContext.cullingSplits.AsSpan();
			using (new ProfilingScope(Profiling.AllocateCullingContexts))
			{
				Span<CullingSplit> span2 = span;
				for (int i = 0; i < span2.Length; i++)
				{
					ref CullingSplit cullingSplit = ref span2[i];
					int length2 = singleSplitCommands.Length;
					AllocateCullingContext(in viewTypeInfo, in cullingSplit, in batchCullingContext, in constructContext, in viewObject, cpuInstanceVisibilityMaskCapacity, length2).LOD = constructContext.LODInfo;
					num++;
				}
			}
			Span<GPUDrivenCullingContext> cullingContexts = UnsafeCollectionExtensions.AsSpan(in m_CullingContexts).Slice(length, num);
			DebugUtility.DrawingOverrides? drawingOverrides = null;
			NativeArray<GPUDrivenLODViewCollection.ViewDependentLODGroupData>.ReadOnly viewData;
			JobHandle job = m_LODGroupRepository.LaunchTickJobs(batchCullingContext.viewID, in cullingContexts[0].LOD, out viewData);
			if (!job.Equals(default(JobHandle)))
			{
				dependency = JobHandle.CombineDependencies(dependency, job);
			}
			return CoarseCPUFrustumCulling(in batchCullingContext, receiverPlanes, in viewTypeInfo, in viewTypeCullingInfo, ref singleSplitCommands, singleSplitDrawRanges.AsArray(), ref singleSplitSortingPositions, pIndirectDrawCommands, pDrawRanges, pSortingPositions, drawCommandCounter, drawRangeCounter, dependency, instanceVisibilityMaskData, cullingContexts, viewData, drawingOverrides);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void GetFixedLODIndexAndBias(GPUDrivenRendererGroupPool.ViewType viewType, out int fixedLODIndex, out float lodBias)
	{
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if ((object)asset != null)
		{
			ShadowSettings shadowSettings = asset.ShadowSettings;
			if (shadowSettings != null && (viewType == GPUDrivenRendererGroupPool.ViewType.Shadows || viewType == GPUDrivenRendererGroupPool.ViewType.ShadowsCullLightmapped))
			{
				fixedLODIndex = shadowSettings.FixedLODIndex;
				lodBias = shadowSettings.LODBias;
				return;
			}
		}
		fixedLODIndex = -1;
		lodBias = 0f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsSceneViewCamera(BatchPackedCullingViewID viewID)
	{
		return false;
	}

	private static GPUDrivenCullingContext.LODInfo BuildLODInfo(in LODParameters lodParameters, BatchPackedCullingViewID viewID, GPUDrivenRendererGroupPool.ViewType viewType)
	{
		float num = LODGroupRenderingUtils.CalculateScreenRelativeMetric(lodParameters, QualitySettings.lodBias);
		bool useSelectionForcedLOD = IsSceneViewCamera(viewID);
		GetFixedLODIndexAndBias(viewType, out var fixedLODIndex, out var lodBias);
		GPUDrivenCullingContext.LODInfo result = default(GPUDrivenCullingContext.LODInfo);
		result.SqrScreenRelativeMetric = num * num;
		result.IsOrtho = lodParameters.isOrthographic;
		result.UseSelectionForcedLOD = useSelectionForcedLOD;
		result.CameraPosition = lodParameters.cameraPosition;
		result.MaxLOD = QualitySettings.maximumLODLevel;
		result.FixedLODIndex = fixedLODIndex;
		result.LODBias = lodBias;
		return result;
	}

	private unsafe JobHandle CoarseCPUFrustumCulling(in BatchCullingContext batchCullingContext, ReceiverPlanes receiverPlanes, in GPUDrivenRendererGroupPool.ViewTypeInfo viewTypeInfo, in ViewTypeCullingInfo viewTypeCullingInfo, ref NativeSlice<BatchDrawCommandIndirect> singleSplitCommands, NativeArray<BatchDrawRange> singleSplitDrawRanges, ref NativeArray<float3> singleSplitSortingPositions, BatchDrawCommandIndirect* pIndirectDrawCommands, BatchDrawRange* pDrawRanges, float* pSortingPositions, UnsafeAtomicCounter32 drawCommandCounter, UnsafeAtomicCounter32 drawRangeCounter, JobHandle dependency, Span<CPUInstanceVisibilityData> instanceVisibilityMaskData, Span<GPUDrivenCullingContext> cullingContexts, NativeArray<GPUDrivenLODViewCollection.ViewDependentLODGroupData>.ReadOnly viewLODGroupData, DebugUtility.DrawingOverrides? drawingOverrides)
	{
		using (new ProfilingScope(Profiling.CoarseCPUFrustumCulling))
		{
			NativeArray<int>.ReadOnly allIndicesReadonly = m_RendererGroupPool.GetAllIndicesReadonly(viewTypeInfo.ViewType);
			NativeArray<GPUDrivenRendererGroupPool.RendererGroupSlice>.ReadOnly allRendererGroupSlicesReadonly = m_RendererGroupPool.GetAllRendererGroupSlicesReadonly(viewTypeInfo.ViewType);
			int* cullingStatsCounterPtr = null;
			PrepareSplitCPUCullingInfo(batchCullingContext, in viewTypeInfo, in receiverPlanes, allRendererGroupSlicesReadonly, out var splitCPUCullingInfos, out var allCullingPlanes, out var drawCommandVisibilityMasks);
			GPUDrivenBRGDebug debugData = m_BRG.DebugData;
			if (debugData != null && debugData.CullingStats)
			{
				ref GPUDrivenCullingPassSharedData.CPUCullingStatsData cPUCullingStats = ref m_BRG.SharedPassData.CPUCullingStats;
				int* ptr;
				switch (viewTypeInfo.ViewType)
				{
				case GPUDrivenRendererGroupPool.ViewType.Camera:
					ptr = (int*)cPUCullingStats.MainFrustumCulled.GetUnsafePtr();
					break;
				case GPUDrivenRendererGroupPool.ViewType.DepthOnly:
				case GPUDrivenRendererGroupPool.ViewType.CameraMotionVectors:
					ptr = null;
					break;
				case GPUDrivenRendererGroupPool.ViewType.Shadows:
					ptr = (int*)cPUCullingStats.ShadowFrustumCulled.GetUnsafePtr();
					break;
				case GPUDrivenRendererGroupPool.ViewType.ShadowsCullLightmapped:
					ptr = (int*)cPUCullingStats.ShadowFrustumCulled.GetUnsafePtr();
					break;
				default:
					ptr = null;
					break;
				}
				cullingStatsCounterPtr = ptr;
			}
			NativeArray<DrawCommandMaskingJob.SplitInfo> splits = new NativeArray<DrawCommandMaskingJob.SplitInfo>(splitCPUCullingInfos.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeList<JobHandle> nativeList = new NativeList<JobHandle>(splitCPUCullingInfos.Length, Allocator.Temp);
			NativeArray<IntPtr> instanceVisibilityMasks = new NativeArray<IntPtr>(splitCPUCullingInfos.Length, Allocator.TempJob);
			for (int i = 0; i < splitCPUCullingInfos.Length; i++)
			{
				SplitCPUCullingInfo splitCPUCullingInfo = splitCPUCullingInfos[i];
				ref CPUInstanceVisibilityData reference = ref instanceVisibilityMaskData[i];
				ref GPUDrivenCullingContext reference2 = ref cullingContexts[i];
				splits[i] = new DrawCommandMaskingJob.SplitInfo
				{
					VisibleIndicesOffset = (uint)reference2.VisibleIndicesOffset,
					VisibleIndicesBufferHandle = reference2.VisibleIndicesBuffer.Handle,
					IndirectArgsOffset = reference2.IndirectArgsOffset,
					IndirectArgsBufferHandle = reference2.IndirectArgsBuffer.Handle
				};
				if (!reference.Mask.IsCreated)
				{
					reference.Mask = new NativeArray<uint>(Alignment.AlignUp(allIndicesReadonly.Length, 32) / 32, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
					reference2.OwnsCPUInstanceVisibilityMask = true;
				}
				instanceVisibilityMasks[i] = (IntPtr)reference.Mask.GetUnsafePtr();
				reference2.CPUInstanceVisibilityMask = reference.Mask;
				if (reference.JobHandle.Equals(default(JobHandle)))
				{
					CoarseInstanceCullingJob coarseInstanceCullingJob = default(CoarseInstanceCullingJob);
					coarseInstanceCullingJob.CullingLayerMask = batchCullingContext.cullingLayerMask;
					coarseInstanceCullingJob.SceneCullingMask = batchCullingContext.sceneCullingMask;
					coarseInstanceCullingJob.LODInfo = reference2.LOD;
					coarseInstanceCullingJob.AllIndices = allIndicesReadonly;
					coarseInstanceCullingJob.VisibilityInfo = m_PersistentData.GetAllVisibilityInfoReadonly();
					coarseInstanceCullingJob.LODGroups = m_LODGroupRepository.GetGroupData().AsReadOnly();
					coarseInstanceCullingJob.ViewLODGroupData = viewLODGroupData;
					coarseInstanceCullingJob.CullingPlanes = allCullingPlanes.GetSubArray(splitCPUCullingInfo.CullingPlanesOffset, splitCPUCullingInfo.CullingPlanesCount).AsReadOnly();
					coarseInstanceCullingJob.InstanceVisibilityMask = reference.Mask;
					coarseInstanceCullingJob.CullingStatsCounterPtr = cullingStatsCounterPtr;
					coarseInstanceCullingJob.SplitInfo = new GPUDrivenCullingUtils.SplitInfo
					{
						CullingSphereLS = reference2.CullingSphereLS,
						WorldToLightSpaceRotation = reference2.WorldToLightSpaceRotation
					};
					CoarseInstanceCullingJob jobData = coarseInstanceCullingJob;
					if (viewTypeInfo.ViewType == GPUDrivenRendererGroupPool.ViewType.CameraMotionVectors)
					{
						jobData.RequiredDynamicFlags |= GPUDrivenDynamicFlags.DrawMotionVectors;
					}
					reference.JobHandle = IJobParallelForBatchExtensions.Schedule(jobData, allIndicesReadonly.Length, 32, dependency);
				}
				reference2.CPUInstanceVisibilityJobHandle = reference.JobHandle;
				nativeList.Add(in reference.JobHandle);
			}
			JobHandle dependsOn = JobHandle.CombineDependencies(nativeList.AsArray());
			DrawCommandMaskingJob jobData2 = default(DrawCommandMaskingJob);
			jobData2.SplitCPUCullingInfos = splitCPUCullingInfos;
			jobData2.AllIndices = allIndicesReadonly;
			jobData2.InstanceVisibilityMasks = instanceVisibilityMasks;
			jobData2.RendererGroupSlices = allRendererGroupSlicesReadonly;
			jobData2.DrawCommandVisibilityMasks = drawCommandVisibilityMasks;
			jobData2.PersistentIndexOffset = -viewTypeInfo.IndicesOffset;
			jobData2.ReadbackInstanceVisibilityMask = GetReadbackInstanceVisibilityMask(in viewTypeInfo, batchCullingContext.viewID);
			jobData2.Splits = splits;
			jobData2.DrawCommandOffset = viewTypeCullingInfo.DrawCommandOffset;
			jobData2.DrawCommandsPerSplit = singleSplitCommands.Length;
			jobData2.IndirectDrawCommandsPtr = pIndirectDrawCommands;
			jobData2.SingleSplitDrawCommands = singleSplitCommands;
			dependsOn = IJobParallelForBatchExtensions.Schedule(jobData2, allRendererGroupSlicesReadonly.Length, 64, dependsOn);
			FixupBatchDrawRangesJob jobData3 = default(FixupBatchDrawRangesJob);
			jobData3.SplitCPUCullingInfos = splitCPUCullingInfos;
			jobData3.BaseDrawCommandsOffset = (uint)viewTypeCullingInfo.DrawCommandOffset;
			jobData3.DrawCommandsPerSplit = (uint)singleSplitCommands.Length;
			jobData3.SceneCullingMask = batchCullingContext.sceneCullingMask;
			jobData3.DrawingOverrides = drawingOverrides;
			jobData3.DestinationDrawCommandCounter = drawCommandCounter;
			jobData3.DestinationDrawRangeCounter = drawRangeCounter;
			jobData3.DestinationDrawRangesPtr = pDrawRanges;
			jobData3.SingleSplitDrawRanges = singleSplitDrawRanges;
			jobData3.DrawCommandVisibilityMasks = drawCommandVisibilityMasks;
			jobData3.AllCullingPlanes = allCullingPlanes;
			jobData3.SortingPositionsCount = viewTypeCullingInfo.SortingPositionCount;
			jobData3.DestinationSortingPositionsPtr = pSortingPositions + viewTypeCullingInfo.SortingPositionOffset;
			jobData3.SourceSortingPositionsPtr = (float*)singleSplitSortingPositions.GetUnsafePtr();
			return jobData3.Schedule(dependsOn);
		}
	}

	private void PrepareSplitCPUCullingInfo(BatchCullingContext batchCullingContext, in GPUDrivenRendererGroupPool.ViewTypeInfo viewTypeInfo, in ReceiverPlanes receiverPlanes, NativeArray<GPUDrivenRendererGroupPool.RendererGroupSlice>.ReadOnly rendererGroupSlices, out NativeArray<SplitCPUCullingInfo> splitCPUCullingInfos, out NativeArray<Plane> allCullingPlanes, out NativeArray<ulong> drawCommandVisibilityMasks)
	{
		Span<CullingSplit> cullingSplits2 = batchCullingContext.cullingSplits.AsSpan();
		splitCPUCullingInfos = new NativeArray<SplitCPUCullingInfo>(cullingSplits2.Length, Allocator.TempJob);
		Span<SplitCPUCullingInfo> splitCPUCullingInfos2 = splitCPUCullingInfos.AsSpan();
		allCullingPlanes = PrepareCullingPlanes(m_BRG, batchCullingContext, cullingSplits2, in viewTypeInfo, in receiverPlanes, splitCPUCullingInfos2);
		drawCommandVisibilityMasks = PrepareDrawCommandVisibilityMasks(rendererGroupSlices, splitCPUCullingInfos2);
		static NativeArray<Plane> PrepareCullingPlanes(GPUDrivenBatchRendererGroup brg, BatchCullingContext batchCullingContext, Span<CullingSplit> cullingSplits, in GPUDrivenRendererGroupPool.ViewTypeInfo viewTypeInfo, in ReceiverPlanes receiverPlanes, Span<SplitCPUCullingInfo> splitCPUCullingInfos)
		{
			int num2 = 0;
			NativeArray<Plane> cullingPlanes = batchCullingContext.cullingPlanes;
			NativeArray<Plane> result = new NativeArray<Plane>((cullingPlanes.Length + receiverPlanes.Planes.Length) * cullingSplits.Length, Allocator.TempJob);
			for (int j = 0; j < cullingSplits.Length; j++)
			{
				ref CullingSplit reference2 = ref cullingSplits[j];
				NativeArray<Plane> subArray = cullingPlanes.GetSubArray(reference2.cullingPlaneOffset, reference2.cullingPlaneCount);
				ref SplitCPUCullingInfo reference3 = ref splitCPUCullingInfos[j];
				reference3.CullingPlanesOffset = num2;
				foreach (Plane item in subArray)
				{
					result[num2++] = item;
				}
				reference3.CullingPlanesCount += subArray.Length;
				if (batchCullingContext.viewType == BatchCullingViewType.Light)
				{
					foreach (Plane plane in receiverPlanes.Planes)
					{
						result[num2++] = plane;
					}
				}
				reference3.CullingPlanesCount = num2 - reference3.CullingPlanesOffset;
			}
			return result;
		}
		static NativeArray<ulong> PrepareDrawCommandVisibilityMasks(NativeArray<GPUDrivenRendererGroupPool.RendererGroupSlice>.ReadOnly rendererGroupSlices, Span<SplitCPUCullingInfo> splitCPUCullingInfos)
		{
			int num = Alignment.AlignUp(rendererGroupSlices.Length, 64) / 64;
			for (int i = 0; i < splitCPUCullingInfos.Length; i++)
			{
				ref SplitCPUCullingInfo reference = ref splitCPUCullingInfos[i];
				reference.DrawCommandVisibilityMaskOffset = i * num;
				reference.DrawCommandVisibilityMaskCount = num;
			}
			return new NativeArray<ulong>(num * splitCPUCullingInfos.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		}
	}

	private NativeArray<int> GetReadbackInstanceVisibilityMask(in GPUDrivenRendererGroupPool.ViewTypeInfo viewTypeInfo, BatchPackedCullingViewID viewID)
	{
		if (!m_BRG.Settings.OcclusionCulling)
		{
			return default(NativeArray<int>);
		}
		GPUDrivenVisibilityMaskReadbackMode visibilityMaskReadbackMode = m_BRG.Settings.VisibilityMaskReadbackMode;
		bool flag = !FrameDebugger.enabled && m_BRG.Settings.OcclusionCulling;
		if (flag)
		{
			flag = visibilityMaskReadbackMode switch
			{
				GPUDrivenVisibilityMaskReadbackMode.Off => false, 
				GPUDrivenVisibilityMaskReadbackMode.HDB => viewTypeInfo.ViewType == GPUDrivenRendererGroupPool.ViewType.DepthOnly, 
				GPUDrivenVisibilityMaskReadbackMode.FullUnsafe => viewTypeInfo.ViewType.IsMainView(), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		if (!flag)
		{
			return default(NativeArray<int>);
		}
		if (!VisibilityReadback.TryGetVisibilityMask(viewID.GetInstanceID(), out var visibilityMask))
		{
			return default(NativeArray<int>);
		}
		return visibilityMask;
	}

	private void AllocateCullingResources(int visibleIndicesCount, int indirectArgsCount, int cpuInstanceVisibilityMaskCapacity, out int visibleIndicesOffset, out int indirectArgsOffset, out int cpuInstanceVisibilityMaskOffset, out int cullingResourcesIndex, out GPUDrivenCullingContext.CullingBufferInfo visibleIndicesBuffer, out GPUDrivenCullingContext.CullingBufferInfo indirectArgsBuffer, out GPUDrivenCullingContext.CullingBufferInfo cpuInstanceVisibilityMaskBuffer)
	{
		if (m_CullingContexts.Length > 0)
		{
			ref GPUDrivenCullingContext reference = ref m_CullingContexts.ElementAt(m_CullingContexts.Length - 1);
			visibleIndicesOffset = reference.VisibleIndicesOffset + reference.VisibleIndicesCount;
			indirectArgsOffset = reference.IndirectArgsOffset + reference.IndirectArgsCount;
			cpuInstanceVisibilityMaskOffset = reference.CPUInstanceVisibilityMaskOffset + reference.CPUInstanceVisibilityMaskCount;
			if (visibleIndicesOffset + visibleIndicesCount <= reference.VisibleIndicesBuffer.Capacity && indirectArgsOffset + indirectArgsCount <= reference.IndirectArgsBuffer.Capacity / 5 && cpuInstanceVisibilityMaskOffset + cpuInstanceVisibilityMaskCapacity <= reference.CPUInstanceVisibilityMaskBuffer.Capacity)
			{
				cullingResourcesIndex = reference.CullingResourcesIndex;
				visibleIndicesBuffer = reference.VisibleIndicesBuffer;
				indirectArgsBuffer = reference.IndirectArgsBuffer;
				cpuInstanceVisibilityMaskBuffer = reference.CPUInstanceVisibilityMaskBuffer;
				return;
			}
		}
		GPUDrivenCullingResourcesPool.CullingResourceSet orAllocateResources = CullingResourcesPool.GetOrAllocateResources(visibleIndicesCount, indirectArgsCount, cpuInstanceVisibilityMaskCapacity);
		visibleIndicesOffset = 0;
		indirectArgsOffset = 0;
		cpuInstanceVisibilityMaskOffset = 0;
		cullingResourcesIndex = orAllocateResources.Index;
		visibleIndicesBuffer = GPUDrivenCullingContext.CullingBufferInfo.FromGraphicsBuffer(orAllocateResources.VisibleIndices);
		indirectArgsBuffer = GPUDrivenCullingContext.CullingBufferInfo.FromGraphicsBuffer(orAllocateResources.IndirectArgs);
		cpuInstanceVisibilityMaskBuffer = GPUDrivenCullingContext.CullingBufferInfo.FromGraphicsBuffer(orAllocateResources.CPUInstanceVisibilityMask);
	}

	private ref GPUDrivenCullingContext AllocateCullingContext(in GPUDrivenRendererGroupPool.ViewTypeInfo viewTypeInfo, in CullingSplit cullingSplit, in BatchCullingContext batchCullingContext, in ConstructDrawRangesAndCommandsContext constructContext, in ViewObject viewObject, int cpuInstanceVisibilityMaskCapacity, int indirectArgsCount)
	{
		int visibleIndicesCount = constructContext.VisibleIndicesCount;
		GPUDrivenCullingContext gPUDrivenCullingContext = default(GPUDrivenCullingContext);
		gPUDrivenCullingContext.PersistentIndicesOffset = viewTypeInfo.IndicesOffset;
		gPUDrivenCullingContext.VisibleIndicesCount = visibleIndicesCount;
		gPUDrivenCullingContext.IndirectArgsCount = indirectArgsCount;
		gPUDrivenCullingContext.CPUInstanceVisibilityMaskCount = cpuInstanceVisibilityMaskCapacity;
		gPUDrivenCullingContext.ViewID = batchCullingContext.viewID;
		gPUDrivenCullingContext.BatchCullingViewType = batchCullingContext.viewType;
		gPUDrivenCullingContext.ViewType = viewTypeInfo.ViewType;
		gPUDrivenCullingContext.SceneCullingMask = batchCullingContext.sceneCullingMask;
		gPUDrivenCullingContext.FrustumPlanes = new NativeArray<Plane>(cullingSplit.cullingPlaneCount, Allocator.TempJob);
		GPUDrivenCullingContext value = gPUDrivenCullingContext;
		AllocateCullingResources(visibleIndicesCount, indirectArgsCount, cpuInstanceVisibilityMaskCapacity, out value.VisibleIndicesOffset, out value.IndirectArgsOffset, out value.CPUInstanceVisibilityMaskOffset, out value.CullingResourcesIndex, out value.VisibleIndicesBuffer, out value.IndirectArgsBuffer, out value.CPUInstanceVisibilityMaskBuffer);
		if (value.BatchCullingViewType == BatchCullingViewType.Camera && m_BRG.Settings.OcclusionCulling)
		{
			value.InstanceVisibilityMaskIndex = VisibilityMaskPool.GetOrAllocateVisibilityMasks(value.ViewID, m_BRG.InstanceCapacity);
		}
		Camera asCamera = viewObject.AsCamera;
		if ((object)asCamera != null)
		{
			value.CameraType = asCamera.cameraType;
		}
		value.FrustumPlanes.CopyFrom(batchCullingContext.cullingPlanes, cullingSplit.cullingPlaneOffset, cullingSplit.cullingPlaneCount);
		GetSplitCullingSphereAndLightMatrix(in batchCullingContext, in constructContext, in cullingSplit, out value.CullingSphereLS, out value.WorldToLightSpaceRotation);
		value.CullingMatrix = cullingSplit.cullingMatrix;
		value.CameraPosition = viewObject.Position;
		m_CullingContexts.Add(in value);
		return ref UnsafeCollectionExtensions.ElementAsRef(in m_CullingContexts, m_CullingContexts.Length - 1);
	}

	private static void GetSplitCullingSphereAndLightMatrix(in BatchCullingContext batchCullingContext, in ConstructDrawRangesAndCommandsContext constructContext, in CullingSplit cullingSplit, out float4 cullingSphereLS, out float3x3 worldToLightSpaceRotation)
	{
		using (new ProfilingScope(Profiling.GetCullingSphereAndLightMatrix))
		{
			if (!constructContext.CullingSpheresAreValid)
			{
				cullingSphereLS = default(float4);
				worldToLightSpaceRotation = default(float3x3);
			}
			else
			{
				worldToLightSpaceRotation = math.transpose((float3x3)batchCullingContext.localToWorldMatrix);
				cullingSphereLS = math.float4(math.mul(worldToLightSpaceRotation, cullingSplit.sphereCenter), cullingSplit.sphereRadius);
			}
		}
	}

	public JobHandle OnPerformPickingCulling(in BatchCullingContext batchCullingContext, BatchCullingOutput cullingOutput)
	{
		PassesDebugFilter(in batchCullingContext);
		return default(JobHandle);
	}

	public JobHandle OnPerformFilteringCulling(in BatchCullingContext batchCullingContext, BatchCullingOutput cullingOutput)
	{
		PassesDebugFilter(in batchCullingContext);
		return default(JobHandle);
	}

	private unsafe static T* Malloc<T>(uint count, Allocator allocator = Allocator.TempJob) where T : unmanaged
	{
		return (T*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * count, UnsafeUtility.AlignOf<T>(), allocator);
	}

	public void PreRender()
	{
		ClearCPUInstanceVisibility(ref m_CullingContexts);
		ClearCullingContexts(ref m_CullingContexts);
		ClearCPUInstanceVisibility(ref m_CullingContextsToCleanup);
		ClearCullingContexts(ref m_CullingContextsToCleanup);
		CullingResourcesPool.PreRender();
		m_BRG.SharedPassData.CPUCullingStats.Reset();
		NativeList<ViewBatch> viewBatches = new NativeList<ViewBatch>(4, Allocator.Temp);
		CollectViewBatches(GPUDrivenRendererGroupPool.ViewType.Camera, viewBatches);
		CollectViewBatches(GPUDrivenRendererGroupPool.ViewType.Shadows, viewBatches);
		CollectViewBatches(GPUDrivenRendererGroupPool.ViewType.ShadowsCullLightmapped, viewBatches);
		foreach (ViewBatch item in viewBatches)
		{
			ViewBatch viewBatch = item;
			if (!m_RendererGroupPool.ViewTypeInfos[(int)viewBatch.ViewType].PendingRebuild.InProgress)
			{
				ScheduleCommandCacheRefresh(in viewBatch);
			}
		}
	}

	public void EarlyPostRender()
	{
		using (new ProfilingScope(Profiling.PostRender))
		{
			for (int i = 0; i < m_CommandCaches.Length; i++)
			{
				ref CommandCache reference = ref m_CommandCaches[i];
				reference.JobHandle?.Complete();
				reference.JobHandle = null;
			}
		}
	}

	public void PostRender()
	{
		using (new ProfilingScope(Profiling.PostRender))
		{
			VisibilityMaskPool.PostRender();
			VisibilityReadback.PostRender();
			ClearCPUInstanceVisibility(ref m_CullingContexts);
			ClearCullingContexts(ref m_CullingContexts);
			ClearCPUInstanceVisibility(ref m_CullingContextsToCleanup);
			ClearCullingContexts(ref m_CullingContextsToCleanup);
		}
	}
}
