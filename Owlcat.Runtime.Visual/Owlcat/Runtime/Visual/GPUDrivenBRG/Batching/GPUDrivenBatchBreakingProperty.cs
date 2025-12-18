using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Batching;

[StructLayout(LayoutKind.Explicit)]
public struct GPUDrivenBatchBreakingProperty : IEquatable<GPUDrivenBatchBreakingProperty>, IComparable<GPUDrivenBatchBreakingProperty>
{
	public enum PropertyType
	{
		Float,
		Vector,
		Texture
	}

	[FieldOffset(0)]
	public int PropertyIndex;

	[FieldOffset(4)]
	public PropertyType Type;

	[FieldOffset(8)]
	public float FloatValue;

	[FieldOffset(8)]
	public float4 VectorValue;

	[FieldOffset(8)]
	public int TextureValue;

	public bool Equals(GPUDrivenBatchBreakingProperty other)
	{
		if (PropertyIndex != other.PropertyIndex)
		{
			return false;
		}
		if (Type != other.Type)
		{
			return false;
		}
		return Type switch
		{
			PropertyType.Float => FloatValue.Equals(other.FloatValue), 
			PropertyType.Vector => VectorValue.Equals(other.VectorValue), 
			PropertyType.Texture => TextureValue.Equals(other.TextureValue), 
			_ => true, 
		};
	}

	public override bool Equals(object obj)
	{
		if (obj is GPUDrivenBatchBreakingProperty other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int propertyIndex = PropertyIndex;
		return HashCode.Combine(propertyIndex, Type switch
		{
			PropertyType.Float => HashCode.Combine(Type, FloatValue), 
			PropertyType.Vector => HashCode.Combine(Type, VectorValue), 
			PropertyType.Texture => HashCode.Combine(Type, TextureValue), 
			_ => (int)Type, 
		});
	}

	public int CompareTo(GPUDrivenBatchBreakingProperty other)
	{
		int num = PropertyIndex.CompareTo(other.PropertyIndex);
		if (num != 0)
		{
			return num;
		}
		int type = (int)Type;
		int num2 = type.CompareTo((int)other.Type);
		if (num2 != 0)
		{
			return num2;
		}
		return Type switch
		{
			PropertyType.Float => FloatValue.CompareTo(other.FloatValue), 
			PropertyType.Vector => math.lengthsq(VectorValue).CompareTo(math.lengthsq(other.VectorValue)), 
			PropertyType.Texture => TextureValue.CompareTo(other.TextureValue), 
			_ => 0, 
		};
	}

	public static GPUDrivenBatchBreakingProperty Extract(Shader shader, Material material, int propertyIndex)
	{
		int propertyNameId = shader.GetPropertyNameId(propertyIndex);
		switch (shader.GetPropertyType(propertyIndex))
		{
		case ShaderPropertyType.Color:
		{
			GPUDrivenBatchBreakingProperty result = default(GPUDrivenBatchBreakingProperty);
			result.PropertyIndex = propertyIndex;
			result.Type = PropertyType.Vector;
			result.VectorValue = (material.HasColor(propertyNameId) ? ((Vector4)material.GetColor(propertyNameId)) : shader.GetPropertyDefaultVectorValue(propertyNameId));
			return result;
		}
		case ShaderPropertyType.Vector:
		{
			GPUDrivenBatchBreakingProperty result = default(GPUDrivenBatchBreakingProperty);
			result.PropertyIndex = propertyIndex;
			result.Type = PropertyType.Vector;
			result.VectorValue = (material.HasVector(propertyNameId) ? material.GetVector(propertyNameId) : shader.GetPropertyDefaultVectorValue(propertyNameId));
			return result;
		}
		case ShaderPropertyType.Float:
		case ShaderPropertyType.Range:
		{
			GPUDrivenBatchBreakingProperty result = default(GPUDrivenBatchBreakingProperty);
			result.PropertyIndex = propertyIndex;
			result.Type = PropertyType.Float;
			result.FloatValue = (material.HasFloat(propertyNameId) ? material.GetFloat(propertyNameId) : shader.GetPropertyDefaultFloatValue(propertyNameId));
			return result;
		}
		case ShaderPropertyType.Texture:
		{
			GPUDrivenBatchBreakingProperty result = default(GPUDrivenBatchBreakingProperty);
			result.PropertyIndex = propertyIndex;
			result.Type = PropertyType.Texture;
			result.TextureValue = GetMaterialTextureInstanceID(material, propertyNameId);
			return result;
		}
		case ShaderPropertyType.Int:
		{
			GPUDrivenBatchBreakingProperty result = default(GPUDrivenBatchBreakingProperty);
			result.PropertyIndex = propertyIndex;
			result.Type = PropertyType.Float;
			result.FloatValue = (material.HasInteger(propertyNameId) ? material.GetInteger(propertyNameId) : shader.GetPropertyDefaultIntValue(propertyNameId));
			return result;
		}
		default:
			return default(GPUDrivenBatchBreakingProperty);
		}
		static int GetMaterialTextureInstanceID(Material material, int nameID)
		{
			Texture texture = (material.GetTexture(nameID) ? material.GetTexture(nameID) : null);
			if (!(texture != null))
			{
				return 0;
			}
			return texture.GetInstanceID();
		}
	}
}
