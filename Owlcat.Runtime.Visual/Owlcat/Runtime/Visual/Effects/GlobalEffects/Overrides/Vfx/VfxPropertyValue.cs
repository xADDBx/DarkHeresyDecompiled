using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Overrides.Vfx;

[Serializable]
public class VfxPropertyValue
{
	public VfxPropertyType VfxPropertyType;

	public bool BoolValue;

	public int IntValue;

	public uint UIntValue;

	public float FloatValue;

	public Vector2 Vector2Value;

	public Vector3 Vector3Value;

	public Vector4 Vector4Value;

	public Color ColorValue;

	public Texture2D Texture2DValue;

	public Mesh MeshValue;

	public AnimationCurve CurveValue;

	public Gradient GradientValue;

	public Matrix4x4 MatrixValue;

	internal void SetValue(object value)
	{
		switch (VfxPropertyType)
		{
		case VfxPropertyType.Bool:
			BoolValue = (bool)value;
			break;
		case VfxPropertyType.Int:
			IntValue = (int)value;
			break;
		case VfxPropertyType.UInt:
			UIntValue = (uint)value;
			break;
		case VfxPropertyType.Float:
			FloatValue = (float)value;
			break;
		case VfxPropertyType.Vector2:
			Vector2Value = (Vector2)value;
			break;
		case VfxPropertyType.Vector3:
			Vector3Value = (Vector3)value;
			break;
		case VfxPropertyType.Vector4:
			Vector4Value = (Vector4)value;
			break;
		case VfxPropertyType.Color:
			ColorValue = (Color)value;
			break;
		case VfxPropertyType.Texture2D:
			Texture2DValue = (Texture2D)value;
			break;
		case VfxPropertyType.Mesh:
			MeshValue = (Mesh)value;
			break;
		case VfxPropertyType.Curve:
			CurveValue = (AnimationCurve)value;
			break;
		case VfxPropertyType.Gradient:
			GradientValue = (Gradient)value;
			break;
		case VfxPropertyType.Matrix4x4:
			MatrixValue = (Matrix4x4)value;
			break;
		default:
			throw new NotImplementedException($"Unsupported VFX property type: {VfxPropertyType}");
		}
	}

	internal void SetValue(VfxPropertyValue value)
	{
		switch (VfxPropertyType)
		{
		case VfxPropertyType.Bool:
			BoolValue = value.BoolValue;
			break;
		case VfxPropertyType.Int:
			IntValue = value.IntValue;
			break;
		case VfxPropertyType.UInt:
			UIntValue = value.UIntValue;
			break;
		case VfxPropertyType.Float:
			FloatValue = value.FloatValue;
			break;
		case VfxPropertyType.Vector2:
			Vector2Value = value.Vector2Value;
			break;
		case VfxPropertyType.Vector3:
			Vector3Value = value.Vector3Value;
			break;
		case VfxPropertyType.Vector4:
			Vector4Value = value.Vector4Value;
			break;
		case VfxPropertyType.Color:
			ColorValue = value.ColorValue;
			break;
		case VfxPropertyType.Texture2D:
			Texture2DValue = value.Texture2DValue;
			break;
		case VfxPropertyType.Mesh:
			MeshValue = value.MeshValue;
			break;
		case VfxPropertyType.Curve:
			CurveValue = value.CurveValue;
			break;
		case VfxPropertyType.Gradient:
			GradientValue = value.GradientValue;
			break;
		case VfxPropertyType.Matrix4x4:
			MatrixValue = value.MatrixValue;
			break;
		}
	}

	internal object GetValue()
	{
		return VfxPropertyType switch
		{
			VfxPropertyType.Bool => BoolValue, 
			VfxPropertyType.Int => IntValue, 
			VfxPropertyType.UInt => UIntValue, 
			VfxPropertyType.Float => FloatValue, 
			VfxPropertyType.Vector2 => Vector2Value, 
			VfxPropertyType.Vector3 => Vector3Value, 
			VfxPropertyType.Vector4 => Vector4Value, 
			VfxPropertyType.Color => ColorValue, 
			VfxPropertyType.Texture2D => Texture2DValue, 
			VfxPropertyType.Mesh => MeshValue, 
			VfxPropertyType.Curve => CurveValue, 
			VfxPropertyType.Gradient => GradientValue, 
			VfxPropertyType.Matrix4x4 => MatrixValue, 
			_ => throw new NotImplementedException($"Unsupported VFX property type: {VfxPropertyType}"), 
		};
	}
}
