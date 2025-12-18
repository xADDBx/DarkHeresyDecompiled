using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public static class GPUDrivenMetadataAuthoring
{
	[Flags]
	public enum MetadataComponents
	{
		Default = 1,
		LightMaps = 3,
		LightProbes = 5
	}

	public struct DataField
	{
		public int NameID;

		public int Offset;
	}

	public struct MaterialMetadata : IDisposable
	{
		public NativeArray<MetadataValue> MetadataValues;

		public int BuiltInPerInstanceDataSize;

		public void Dispose()
		{
			if (MetadataValues.IsCreated)
			{
				MetadataValues.Dispose();
			}
		}
	}

	private enum PropertyKind
	{
		PerMaterial,
		PerInstance
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct DefaultPerInstanceData
	{
		public float3x4 unity_ObjectToWorld;

		public float3x4 unity_WorldToObject;

		public float3x4 unity_MatrixPreviousM;

		public float3x4 unity_MatrixPreviousMI;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct LightMapsPerInstanceData
	{
		public float4 unity_LightmapST;

		public float4 unity_LightmapIndex;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct LightProbesPerInstanceData
	{
		public SHCoefficients unity_SHCoefficients;
	}

	private static readonly Dictionary<Type, List<DataField>> DataFieldsCache = new Dictionary<Type, List<DataField>>();

	public static bool Includes(this MetadataComponents metadataComponents, MetadataComponents components)
	{
		return (metadataComponents & components) == components;
	}

	public static MaterialMetadata FromMaterialLayout(MetadataComponents metadataComponents, in GPUDrivenRenderingUtils.PropertyLayout propertyLayout, Allocator allocator)
	{
		MaterialMetadata result = default(MaterialMetadata);
		result.BuiltInPerInstanceDataSize = 0;
		NativeArray<MetadataValue> nativeArray = ReflectInOnFlagOrDefault<DefaultPerInstanceData>(metadataComponents, MetadataComponents.Default, allocator, PropertyKind.PerInstance, ref result.BuiltInPerInstanceDataSize);
		NativeArray<MetadataValue> nativeArray2 = ReflectInOnFlagOrDefault<LightMapsPerInstanceData>(metadataComponents, MetadataComponents.LightMaps, allocator, PropertyKind.PerInstance, ref result.BuiltInPerInstanceDataSize);
		NativeArray<MetadataValue> nativeArray3 = ReflectInOnFlagOrDefault<LightProbesPerInstanceData>(metadataComponents, MetadataComponents.LightProbes, allocator, PropertyKind.PerInstance, ref result.BuiltInPerInstanceDataSize);
		int count2 = propertyLayout.PerMaterialData.Length + propertyLayout.PerInstanceData.Length;
		AddCountIfCreated(nativeArray, ref count2);
		AddCountIfCreated(nativeArray2, ref count2);
		AddCountIfCreated(nativeArray3, ref count2);
		NativeList<MetadataValue> metadata = new NativeList<MetadataValue>(count2, allocator);
		AddRangeIfCreated(metadata, nativeArray);
		AddRangeIfCreated(metadata, nativeArray2);
		AddRangeIfCreated(metadata, nativeArray3);
		AddPropertyMetadata(ref metadata, in propertyLayout, PropertyKind.PerInstance, result.BuiltInPerInstanceDataSize);
		AddPropertyMetadata(ref metadata, in propertyLayout, PropertyKind.PerMaterial, 0);
		result.MetadataValues = metadata.AsArray();
		return result;
		static void AddCountIfCreated(NativeArray<MetadataValue> metadataValues, ref int count)
		{
			if (metadataValues.IsCreated)
			{
				count += metadataValues.Length;
			}
		}
		static void AddRangeIfCreated(NativeList<MetadataValue> destination, NativeArray<MetadataValue> range)
		{
			if (range.IsCreated)
			{
				destination.AddRange(range);
			}
		}
		static NativeArray<MetadataValue> ReflectInOnFlagOrDefault<T>(MetadataComponents metadataComponents, MetadataComponents mask, Allocator allocator, PropertyKind propertyKind, ref int totalSize) where T : struct
		{
			if (metadataComponents.Includes(mask))
			{
				NativeArray<MetadataValue> result2 = DataFieldsToMetadata<T>(totalSize, allocator, propertyKind);
				totalSize += UnsafeUtility.SizeOf<T>();
				return result2;
			}
			return default(NativeArray<MetadataValue>);
		}
	}

	private static void AddPropertyMetadata(ref NativeList<MetadataValue> metadata, in GPUDrivenRenderingUtils.PropertyLayout propertyLayout, PropertyKind propertyKind, int offset)
	{
		foreach (GPUDrivenRenderer.PropertyData item in propertyKind switch
		{
			PropertyKind.PerMaterial => propertyLayout.PerMaterialData, 
			PropertyKind.PerInstance => propertyLayout.PerInstanceData, 
			_ => throw new ArgumentOutOfRangeException("propertyKind", propertyKind, null), 
		})
		{
			MetadataValue value = PackMetadata(item.NameID, (uint)offset, propertyKind);
			metadata.Add(in value);
			offset += item.Type.GetPropertyDataSize();
		}
	}

	private static NativeArray<MetadataValue> DataFieldsToMetadata<T>(int fieldsOffset, Allocator allocator, PropertyKind propertyKind) where T : struct
	{
		Type typeFromHandle = typeof(T);
		if (!DataFieldsCache.TryGetValue(typeFromHandle, out var value))
		{
			value = (DataFieldsCache[typeFromHandle] = ReflectDataFields<T>());
		}
		NativeArray<MetadataValue> result = new NativeArray<MetadataValue>(value.Count, allocator, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < value.Count; i++)
		{
			DataField dataField = value[i];
			result[i] = PackMetadata(dataField.NameID, (uint)(fieldsOffset + dataField.Offset), propertyKind);
		}
		return result;
	}

	private static List<DataField> ReflectDataFields<T>() where T : struct
	{
		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		List<DataField> list = new List<DataField>();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			string name = fieldInfo.Name;
			if (!name.StartsWith("pad_"))
			{
				int nameID = Shader.PropertyToID(name);
				list.Add(new DataField
				{
					NameID = nameID,
					Offset = UnsafeUtility.GetFieldOffset(fieldInfo)
				});
			}
		}
		return list;
	}

	private static MetadataValue PackMetadata(int nameId, uint offset, PropertyKind propertyKind)
	{
		MetadataValue result = default(MetadataValue);
		result.NameID = nameId;
		result.Value = propertyKind switch
		{
			PropertyKind.PerMaterial => offset, 
			PropertyKind.PerInstance => offset | 0x80000000u, 
			_ => throw new ArgumentOutOfRangeException("propertyKind", propertyKind, null), 
		};
		return result;
	}
}
