using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Allocators.OffsetAllocator;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Core.ObjectTracking;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Unity.Burst;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VertexPaint;

public class VertexPaintManager : IDisposable
{
	public enum ContainerPropertyBlockUsage
	{
		None,
		Regular,
		Forced
	}

	public struct DataSourceAllocation
	{
		public OffsetAllocator.Allocation Allocation;

		public HashSet<VertexColorContainer> Usages;
	}

	public struct ContainerData : IEquatable<ContainerData>
	{
		public VertexColorContainer VertexColorContainer;

		public DataSourceKey DataSourceKey;

		public ContainerPropertyBlockUsage PropertyBlockUsage;

		[BurstDiscard]
		public bool Equals(ContainerData other)
		{
			if (object.Equals(VertexColorContainer, other.VertexColorContainer) && DataSourceKey.Equals(other.DataSourceKey))
			{
				int propertyBlockUsage = (int)PropertyBlockUsage;
				return propertyBlockUsage.Equals((int)other.PropertyBlockUsage);
			}
			return false;
		}

		[BurstDiscard]
		public override bool Equals(object obj)
		{
			if (obj is ContainerData other)
			{
				return Equals(other);
			}
			return false;
		}

		[BurstDiscard]
		public override int GetHashCode()
		{
			int num = (((VertexColorContainer != null) ? VertexColorContainer.GetHashCode() : 0) * 397) ^ DataSourceKey.GetHashCode();
			int propertyBlockUsage = (int)PropertyBlockUsage;
			return num ^ propertyBlockUsage.GetHashCode();
		}
	}

	public struct DataSourceKey : IEquatable<DataSourceKey>
	{
		public int InstanceID;

		public int Size;

		public bool Equals(DataSourceKey other)
		{
			if (InstanceID == other.InstanceID)
			{
				return Size == other.Size;
			}
			return false;
		}

		[BurstDiscard]
		public override bool Equals(object obj)
		{
			if (obj is DataSourceKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (InstanceID * 397) ^ Size;
		}

		public bool IsValid()
		{
			if (InstanceID != 0)
			{
				return Size > 0;
			}
			return false;
		}

		public static DataSourceKey CreateFromColors(in VertexColorContainer.RawColorsData rawColorsData)
		{
			if (rawColorsData.Colors.Length == 0)
			{
				return default(DataSourceKey);
			}
			DataSourceKey result = default(DataSourceKey);
			result.InstanceID = rawColorsData.SourceInstanceID;
			result.Size = rawColorsData.Colors.Length;
			return result;
		}

		[BurstDiscard]
		public override string ToString()
		{
			return string.Format("{0}{{InstanceID={1}; Size={2}; ({3})}}", "DataSourceKey", InstanceID, Size, ObjectDispatcherService.FindByInstanceId<UnityEngine.Object>(InstanceID));
		}
	}

	private static class ShaderID
	{
		public static readonly int _PerInstance_VertexColorContainer_Offset = Shader.PropertyToID("_PerInstance_VertexColorContainer_Offset");

		public static readonly int _VertexPaintBuffer = Shader.PropertyToID("_VertexPaintBuffer");
	}

	public const string kOffsetPropertyName = "_PerInstance_VertexColorContainer_Offset";

	private const int kBufferStride = 4;

	private static VertexPaintManager s_Instance;

	[ItemCanBeNull]
	private static readonly List<VertexColorContainer> s_DelayedContainerInits = new List<VertexColorContainer>();

	private readonly List<VertexColorContainer> m_AllContainers = new List<VertexColorContainer>();

	private readonly Dictionary<VertexColorContainer, ContainerData> m_Data = new Dictionary<VertexColorContainer, ContainerData>();

	private readonly Dictionary<DataSourceKey, DataSourceAllocation> m_DataSourceAllocations = new Dictionary<DataSourceKey, DataSourceAllocation>();

	private readonly HashSet<DataSourceKey> m_DirtyDataSources = new HashSet<DataSourceKey>();

	private OffsetAllocator m_OffsetAllocator;

	private readonly VertexPaintManagerParameters m_Parameters;

	private readonly WaaaghPipeline m_Pipeline;

	private readonly MaterialPropertyBlock m_PropertyBlock;

	private ResizableGraphicsBuffer m_Buffer;

	public static bool IsInitialized => s_Instance != null;

	private bool LoggingEnabled => m_Parameters.Logging;

	private VertexPaintManager(WaaaghPipeline pipeline, VertexPaintManagerParameters parameters)
	{
		m_Parameters = parameters;
		m_Pipeline = pipeline;
		m_OffsetAllocator = new OffsetAllocator((uint)parameters.MaxCapacity, (uint)parameters.MaxAllocs);
		m_PropertyBlock = new MaterialPropertyBlock();
		m_Buffer.CreateOrResize(GraphicsBuffer.Target.Raw, GetBufferSize(Mathf.Min(parameters.InitialCapacity, parameters.MaxCapacity)), 4);
		Shader.SetGlobalBuffer(ShaderID._VertexPaintBuffer, m_Buffer.InternalBuffer);
	}

	public void Dispose()
	{
		m_Buffer.Dispose();
		m_OffsetAllocator.Dispose();
	}

	public static bool TryGetInstance(out VertexPaintManager vertexPaintManager)
	{
		vertexPaintManager = s_Instance;
		return s_Instance != null;
	}

	public void ForcePropertyBlockUsage()
	{
		foreach (VertexColorContainer allContainer in m_AllContainers)
		{
			ContainerData containerData = m_Data[allContainer];
			VertexColorContainer.RawColorsData rawColorsData = allContainer.GetRawColorsData();
			DataSourceKey key = DataSourceKey.CreateFromColors(in rawColorsData);
			if (key.IsValid() && m_DataSourceAllocations.TryGetValue(key, out var value) && containerData.PropertyBlockUsage == ContainerPropertyBlockUsage.None)
			{
				SetOffset(ref containerData, in value, allContainer, ContainerPropertyBlockUsage.Forced);
				if (allContainer.TryGetComponent<MeshRenderer>(out var component))
				{
					RendererUtils.SetAllowGPUDrivenRendering(component, allow: false);
				}
			}
			m_Data[allContainer] = containerData;
		}
	}

	public void RevertForcedPropertyBlockUsage()
	{
		foreach (VertexColorContainer allContainer in m_AllContainers)
		{
			ContainerData containerData = m_Data[allContainer];
			if (containerData.PropertyBlockUsage == ContainerPropertyBlockUsage.Forced)
			{
				ResetOffset(allContainer, ref containerData);
				if (allContainer.TryGetComponent<MeshRenderer>(out var component))
				{
					RendererUtils.SetAllowGPUDrivenRendering(component, allow: true);
				}
				VertexColorContainer.RawColorsData rawColorsData = allContainer.GetRawColorsData();
				DataSourceKey key = DataSourceKey.CreateFromColors(in rawColorsData);
				if (key.IsValid() && m_DataSourceAllocations.TryGetValue(key, out var value))
				{
					SetOffset(ref containerData, in value, allContainer);
				}
			}
			m_Data[allContainer] = containerData;
		}
	}

	public static bool TryGetBuffer(out GraphicsBuffer graphicsBuffer)
	{
		VertexPaintManager vertexPaintManager = s_Instance;
		if (vertexPaintManager != null)
		{
			ResizableGraphicsBuffer buffer = vertexPaintManager.m_Buffer;
			if (buffer.IsCreated)
			{
				graphicsBuffer = s_Instance.m_Buffer.InternalBuffer;
				return graphicsBuffer != null;
			}
		}
		graphicsBuffer = null;
		return false;
	}

	private static int GetBufferSize(int count)
	{
		return Alignment.AlignUp(count, 4) / 4;
	}

	private void InitContainer([NotNull] VertexColorContainer vertexColorContainer)
	{
		if (!m_Data.TryGetValue(vertexColorContainer, out var value))
		{
			VertexColorContainer.RawColorsData rawColorsData = vertexColorContainer.GetRawColorsData();
			value = default(ContainerData);
			value.VertexColorContainer = vertexColorContainer;
			ContainerData containerData = value;
			if (rawColorsData.Colors.Length == 0)
			{
				containerData.DataSourceKey = default(DataSourceKey);
				ResetOffset(vertexColorContainer, ref containerData);
			}
			else
			{
				DataSourceAllocation dataSourceAllocation;
				DataSourceKey key = (containerData.DataSourceKey = GetOrAllocateForSource(vertexColorContainer, in rawColorsData, out dataSourceAllocation));
				dataSourceAllocation = m_DataSourceAllocations[key];
				SetOffset(ref containerData, in dataSourceAllocation, vertexColorContainer);
			}
			m_Data.Add(vertexColorContainer, containerData);
			m_AllContainers.Add(vertexColorContainer);
			vertexColorContainer.ColorsChanged += OnContainerColorsChanged;
		}
	}

	private DataSourceKey GetOrAllocateForSource(VertexColorContainer vertexColorContainer, in VertexColorContainer.RawColorsData rawColorsData, out DataSourceAllocation dataSourceAllocation)
	{
		DataSourceKey dataSourceKey = DataSourceKey.CreateFromColors(in rawColorsData);
		if (!m_DataSourceAllocations.TryGetValue(dataSourceKey, out dataSourceAllocation))
		{
			m_DirtyDataSources.Add(dataSourceKey);
			dataSourceAllocation = new DataSourceAllocation
			{
				Allocation = Allocate(rawColorsData.Colors)
			};
			LogAllocation(dataSourceKey);
		}
		ref HashSet<VertexColorContainer> usages = ref dataSourceAllocation.Usages;
		if (usages == null)
		{
			usages = new HashSet<VertexColorContainer>();
		}
		dataSourceAllocation.Usages.Add(vertexColorContainer);
		LogDataSourceUsages(dataSourceKey, dataSourceAllocation);
		m_DataSourceAllocations[dataSourceKey] = dataSourceAllocation;
		return dataSourceKey;
	}

	private OffsetAllocator.Allocation Allocate(Span<byte> rawColors)
	{
		OffsetAllocator.Allocation result = m_OffsetAllocator.Allocate((uint)rawColors.Length);
		EnsureSufficientBufferCapacity((int)(result.Offset + result.Size));
		return result;
	}

	private void EnsureSufficientBufferCapacity(int capacity)
	{
		m_Buffer.ResizeKeepContentsAndClear(GetBufferSize(capacity), m_Pipeline.BufferUtils, m_Pipeline.CommandQueue, 0);
		Shader.SetGlobalBuffer(ShaderID._VertexPaintBuffer, m_Buffer.InternalBuffer);
	}

	private void SetOffset(ref ContainerData containerData, in DataSourceAllocation dataSourceAllocation, VertexColorContainer vertexColorContainer, ContainerPropertyBlockUsage propertyBlockUsage = ContainerPropertyBlockUsage.Regular)
	{
		if (vertexColorContainer == null || !vertexColorContainer.TryGetComponent<MeshRenderer>(out var component))
		{
			return;
		}
		int perInstance_VertexColorContainer_Offset = ShaderID._PerInstance_VertexColorContainer_Offset;
		int value = (int)(dataSourceAllocation.Allocation.Offset + 1);
		int num;
		if (IsBRGEnabled())
		{
			num = (RendererUtils.AllowGPUDrivenRendering(component) ? 1 : 0);
			if (num != 0)
			{
				if (!vertexColorContainer.TryGetComponent<GPUDrivenRenderer>(out var component2))
				{
					component2 = vertexColorContainer.gameObject.AddComponent<GPUDrivenRenderer>();
				}
				GPUDrivenRenderer gPUDrivenRenderer = component2;
				GPUDrivenRenderer.PropertyData data = GPUDrivenRenderer.PropertyData.Int(perInstance_VertexColorContainer_Offset, value);
				gPUDrivenRenderer.AddPropertyData(in data);
			}
		}
		else
		{
			num = 0;
		}
		if (num == 0 || propertyBlockUsage == ContainerPropertyBlockUsage.Forced)
		{
			m_PropertyBlock.Clear();
			component.GetPropertyBlock(m_PropertyBlock);
			m_PropertyBlock.SetInt(perInstance_VertexColorContainer_Offset, value);
			component.SetPropertyBlock(m_PropertyBlock);
			containerData.PropertyBlockUsage = propertyBlockUsage;
		}
		else
		{
			containerData.PropertyBlockUsage = ContainerPropertyBlockUsage.None;
		}
	}

	private void ResetOffset(VertexColorContainer vertexColorContainer, ref ContainerData containerData)
	{
		if (vertexColorContainer == null)
		{
			return;
		}
		int perInstance_VertexColorContainer_Offset = ShaderID._PerInstance_VertexColorContainer_Offset;
		if (vertexColorContainer.TryGetComponent<GPUDrivenRenderer>(out var component))
		{
			component.RemovePropertyData(perInstance_VertexColorContainer_Offset);
		}
		if (containerData.PropertyBlockUsage != 0 && vertexColorContainer.TryGetComponent<MeshRenderer>(out var component2))
		{
			if (containerData.PropertyBlockUsage == ContainerPropertyBlockUsage.Forced)
			{
				component2.SetPropertyBlock(null);
				containerData.PropertyBlockUsage = ContainerPropertyBlockUsage.None;
				return;
			}
			m_PropertyBlock.Clear();
			component2.GetPropertyBlock(m_PropertyBlock);
			m_PropertyBlock.SetInt(perInstance_VertexColorContainer_Offset, 0);
			component2.SetPropertyBlock(m_PropertyBlock);
		}
	}

	private static bool IsBRGEnabled()
	{
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if ((object)asset != null)
		{
			GPUDrivenBRGSettings gPUDrivenBRGSettings = asset.GPUDrivenBRGSettings;
			if (gPUDrivenBRGSettings != null)
			{
				return gPUDrivenBRGSettings.IsEnabledAndSupported;
			}
		}
		return false;
	}

	private void FreeContainer(VertexColorContainer vertexColorContainer)
	{
		vertexColorContainer.ColorsChanged -= OnContainerColorsChanged;
		m_AllContainers.Remove(vertexColorContainer);
		if (m_Data.Remove(vertexColorContainer, out var value))
		{
			m_DirtyDataSources.Remove(value.DataSourceKey);
			RemoveDataSourceUsage(vertexColorContainer, value.DataSourceKey);
			ResetOffset(vertexColorContainer, ref value);
		}
	}

	private void RemoveDataSourceUsage(VertexColorContainer vertexColorContainer, DataSourceKey dataSourceKey)
	{
		if (m_DataSourceAllocations.TryGetValue(dataSourceKey, out var value))
		{
			value.Usages.Remove(vertexColorContainer);
			LogDataSourceUsages(dataSourceKey, value);
			if (value.Usages.Count == 0)
			{
				m_OffsetAllocator.Free(value.Allocation);
				m_DataSourceAllocations.Remove(dataSourceKey);
				LogDeallocation(dataSourceKey);
			}
		}
	}

	private void OnContainerColorsChanged(VertexColorContainer container)
	{
		if (!m_Data.TryGetValue(container, out var value))
		{
			return;
		}
		VertexColorContainer.RawColorsData rawColorsData = container.GetRawColorsData();
		if (rawColorsData.Colors.Length == 0)
		{
			ResetOffset(container, ref value);
			RemoveDataSourceUsage(container, value.DataSourceKey);
			value.DataSourceKey = default(DataSourceKey);
		}
		else
		{
			DataSourceKey dataSourceKey = DataSourceKey.CreateFromColors(in rawColorsData);
			if (!value.DataSourceKey.Equals(dataSourceKey))
			{
				RemoveDataSourceUsage(container, value.DataSourceKey);
				value.DataSourceKey = GetOrAllocateForSource(container, in rawColorsData, out var dataSourceAllocation);
				SetOffset(ref value, in dataSourceAllocation, container);
			}
			m_DirtyDataSources.Add(dataSourceKey);
		}
		m_Data[container] = value;
	}

	public static void OnEnableContainer([NotNull] VertexColorContainer vertexColorContainer)
	{
		if (s_Instance != null)
		{
			s_Instance.InitContainer(vertexColorContainer);
		}
		else
		{
			s_DelayedContainerInits.Add(vertexColorContainer);
		}
	}

	public static void OnDisableContainer(VertexColorContainer vertexColorContainer)
	{
		s_Instance?.FreeContainer(vertexColorContainer);
	}

	public static void GetDirtyDataAndClear(List<DataSourceAllocation> dirtySourceAllocations)
	{
		if (s_Instance == null)
		{
			return;
		}
		foreach (DataSourceKey dirtyDataSource in s_Instance.m_DirtyDataSources)
		{
			if (s_Instance.m_DataSourceAllocations.TryGetValue(dirtyDataSource, out var value))
			{
				dirtySourceAllocations.Add(value);
			}
		}
		s_Instance.m_DirtyDataSources.Clear();
	}

	public static void Init(WaaaghPipeline pipeline, VertexPaintManagerParameters parameters)
	{
		s_Instance = new VertexPaintManager(pipeline, parameters);
		foreach (VertexColorContainer s_DelayedContainerInit in s_DelayedContainerInits)
		{
			if (s_DelayedContainerInit != null && s_DelayedContainerInit.isActiveAndEnabled)
			{
				s_Instance.InitContainer(s_DelayedContainerInit);
			}
		}
		s_DelayedContainerInits.Clear();
	}

	public static void Cleanup()
	{
		if (s_Instance != null)
		{
			s_Instance.Dispose();
			s_Instance = null;
		}
	}

	private void LogDataSourceUsages(DataSourceKey dataSourceKey, DataSourceAllocation dataSourceAllocation)
	{
		if (LoggingEnabled)
		{
			Log($"{dataSourceKey} has {dataSourceAllocation.Usages.Count} usages.");
		}
	}

	private void LogAllocation(DataSourceKey dataSourceKey)
	{
		if (LoggingEnabled)
		{
			Log($"Allocated {dataSourceKey}.");
		}
	}

	private void LogDeallocation(DataSourceKey dataSourceKey)
	{
		if (LoggingEnabled)
		{
			Log($"Deallocated {dataSourceKey}.");
		}
	}

	private static void Log(string message)
	{
		Debug.Log("VertexPaintManager: " + message);
	}
}
