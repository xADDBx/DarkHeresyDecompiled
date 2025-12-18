using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.ObjectTracking;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[DisallowMultipleComponent]
public class GPUDrivenRenderer : MonoBehaviour
{
	public enum PropertyDataType
	{
		Float,
		Int,
		Vector,
		Color,
		Matrix
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct PropertyValue
	{
		[FieldOffset(0)]
		public float Float;

		[FieldOffset(0)]
		public int Int;

		[FieldOffset(0)]
		public Vector4 Vector;

		[FieldOffset(0)]
		public Color Color;

		[FieldOffset(0)]
		public Matrix4x4 Matrix;
	}

	public struct PropertyData
	{
		public PropertyValue Value;

		public int NameID;

		public PropertyDataType Type;

		public static bool AreStoredValuesEqual(in PropertyData data1, in PropertyData data2)
		{
			if (data1.Type == data2.Type)
			{
				return AreStoredValuesEqual(in data1.Value, in data2.Value, data1.Type);
			}
			return false;
		}

		private static bool AreStoredValuesEqual(in PropertyValue value1, in PropertyValue value2, PropertyDataType type)
		{
			return type switch
			{
				PropertyDataType.Float => Mathf.Approximately(value1.Float, value2.Float), 
				PropertyDataType.Int => value1.Int == value2.Int, 
				PropertyDataType.Vector => value1.Vector == value2.Vector, 
				PropertyDataType.Color => value1.Color == value2.Color, 
				PropertyDataType.Matrix => value1.Matrix == value2.Matrix, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		public static PropertyData Float(int nameID, float value)
		{
			PropertyData result = default(PropertyData);
			result.NameID = nameID;
			result.Type = PropertyDataType.Float;
			result.Value = new PropertyValue
			{
				Float = value
			};
			return result;
		}

		public static PropertyData Int(int nameID, int value)
		{
			PropertyData result = default(PropertyData);
			result.NameID = nameID;
			result.Type = PropertyDataType.Int;
			result.Value = new PropertyValue
			{
				Int = value
			};
			return result;
		}

		public static PropertyData Vector(int nameID, Vector4 value)
		{
			PropertyData result = default(PropertyData);
			result.NameID = nameID;
			result.Type = PropertyDataType.Vector;
			result.Value = new PropertyValue
			{
				Vector = value
			};
			return result;
		}

		public static PropertyData Color(int nameID, Color value)
		{
			PropertyData result = default(PropertyData);
			result.NameID = nameID;
			result.Type = PropertyDataType.Color;
			result.Value = new PropertyValue
			{
				Color = value
			};
			return result;
		}

		public static PropertyData Matrix(int nameID, Matrix4x4 value)
		{
			PropertyData result = default(PropertyData);
			result.NameID = nameID;
			result.Type = PropertyDataType.Matrix;
			result.Value = new PropertyValue
			{
				Matrix = value
			};
			return result;
		}
	}

	private class WrappedNativeList<T> : IDisposable where T : unmanaged
	{
		private NativeList<T> m_List = new NativeList<T>(Allocator.Persistent);

		public ref NativeList<T> List => ref m_List;

		public void Dispose()
		{
			if (m_List.IsCreated)
			{
				m_List.Dispose();
			}
		}

		~WrappedNativeList()
		{
			Dispose();
		}
	}

	[Serializable]
	public struct SerializedPropertyData
	{
		public string Name;

		public PropertyDataType Type;

		public float FloatValue;

		public int IntValue;

		public Vector4 VectorValue;

		[ColorUsage(true, true)]
		public Color ColorValue;

		public Matrix4x4 MatrixValue;

		public static implicit operator PropertyData(SerializedPropertyData serializedPropertyData)
		{
			PropertyData propertyData = default(PropertyData);
			propertyData.NameID = ((serializedPropertyData.Name != null) ? Shader.PropertyToID(serializedPropertyData.Name) : 0);
			propertyData.Type = serializedPropertyData.Type;
			PropertyData result = propertyData;
			switch (serializedPropertyData.Type)
			{
			case PropertyDataType.Float:
				result.Value.Float = serializedPropertyData.FloatValue;
				break;
			case PropertyDataType.Int:
				result.Value.Int = serializedPropertyData.IntValue;
				break;
			case PropertyDataType.Vector:
				result.Value.Vector = serializedPropertyData.VectorValue;
				break;
			case PropertyDataType.Color:
				result.Value.Color = serializedPropertyData.ColorValue;
				break;
			case PropertyDataType.Matrix:
				result.Value.Matrix = serializedPropertyData.MatrixValue;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return result;
		}

		public SerializedPropertyData(string name, PropertyDataType type)
		{
			Name = name;
			Type = type;
			FloatValue = 0f;
			IntValue = 0;
			VectorValue = default(Vector4);
			ColorValue = default(Color);
			MatrixValue = Matrix4x4.identity;
		}
	}

	[NonSerialized]
	[CanBeNull]
	private WrappedNativeList<PropertyData> m_PropertyData;

	public int PropertyCount => m_PropertyData?.List.Length ?? 0;

	private void OnDestroy()
	{
		m_PropertyData?.Dispose();
	}

	public void AddPropertyData(in PropertyData data)
	{
		if (m_PropertyData != null)
		{
			Span<PropertyData> span = UnsafeCollectionExtensions.AsSpan(in m_PropertyData.List);
			for (int i = 0; i < span.Length; i++)
			{
				ref PropertyData reference = ref span[i];
				if (data.NameID == reference.NameID && data.Type == reference.Type)
				{
					if (!PropertyData.AreStoredValuesEqual(in reference, in data))
					{
						reference = data;
						MarkDataDirty();
					}
					return;
				}
			}
		}
		else
		{
			m_PropertyData = new WrappedNativeList<PropertyData>();
		}
		m_PropertyData.List.Add(in data);
		MarkDataDirty();
	}

	public void RemovePropertyData(int nameID)
	{
		if (m_PropertyData == null)
		{
			return;
		}
		Span<PropertyData> span = UnsafeCollectionExtensions.AsSpan(in m_PropertyData.List);
		for (int i = 0; i < span.Length; i++)
		{
			if (nameID == span[i].NameID)
			{
				m_PropertyData.List.RemoveAtSwapBack(i);
				MarkDataDirty();
				break;
			}
		}
	}

	public void SetPropertyData(List<PropertyData> propertyData)
	{
		if (propertyData != null && propertyData.Count > 0)
		{
			if (m_PropertyData != null)
			{
				m_PropertyData.List.Clear();
			}
			else
			{
				m_PropertyData = new WrappedNativeList<PropertyData>();
			}
			foreach (PropertyData propertyDatum in propertyData)
			{
				PropertyData value = propertyDatum;
				m_PropertyData.List.Add(in value);
			}
			MarkDataDirty();
		}
		else if (m_PropertyData != null)
		{
			m_PropertyData.List.Clear();
			MarkDataDirty();
		}
	}

	public ReadOnlySpan<PropertyData> GetInstanceData()
	{
		if (m_PropertyData == null)
		{
			return ReadOnlySpan<PropertyData>.Empty;
		}
		return UnsafeCollectionExtensions.AsSpan(in m_PropertyData.List);
	}

	public bool TryGetInstanceData(int nameId, out PropertyData propertyData)
	{
		if (m_PropertyData != null)
		{
			Span<PropertyData> span = UnsafeCollectionExtensions.AsSpan(in m_PropertyData.List);
			for (int i = 0; i < span.Length; i++)
			{
				PropertyData propertyData2 = span[i];
				if (propertyData2.NameID == nameId)
				{
					propertyData = propertyData2;
					return true;
				}
			}
		}
		propertyData = default(PropertyData);
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void MarkDataDirty()
	{
		UnityObjectUtils.MarkDirty(this);
	}
}
