using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Overrides.Vfx;

[Serializable]
public class VfxPropertyParameter
{
	[NonSerialized]
	private int m_NameId;

	[NonSerialized]
	private object m_CurrentValue;

	[SerializeField]
	[HideInInspector]
	private VfxPropertyValue m_DefaultValue;

	public bool OverrideState;

	public string Name;

	public VfxPropertyValue Value;

	public Vector2 Range;

	public VfxPropertyParameter(string name, VfxPropertyType type)
	{
		Name = name;
		Value = new VfxPropertyValue
		{
			VfxPropertyType = type
		};
		m_DefaultValue = new VfxPropertyValue
		{
			VfxPropertyType = type
		};
		m_NameId = Shader.PropertyToID(name);
	}

	internal static VfxPropertyType TypeToVfxPropertyType(string typeName)
	{
		if (typeName == typeof(bool).Name)
		{
			return VfxPropertyType.Bool;
		}
		if (typeName == typeof(int).Name)
		{
			return VfxPropertyType.Int;
		}
		if (typeName == typeof(uint).Name)
		{
			return VfxPropertyType.UInt;
		}
		if (typeName == typeof(float).Name)
		{
			return VfxPropertyType.Float;
		}
		if (typeName == typeof(Vector2).Name)
		{
			return VfxPropertyType.Vector2;
		}
		if (typeName == typeof(Vector3).Name)
		{
			return VfxPropertyType.Vector3;
		}
		if (typeName == typeof(Vector4).Name)
		{
			return VfxPropertyType.Vector4;
		}
		if (typeName == typeof(Color).Name)
		{
			return VfxPropertyType.Color;
		}
		if (typeName == typeof(Texture2D).Name)
		{
			return VfxPropertyType.Texture2D;
		}
		if (typeName == typeof(Mesh).Name)
		{
			return VfxPropertyType.Mesh;
		}
		if (typeName == typeof(AnimationCurve).Name)
		{
			return VfxPropertyType.Curve;
		}
		if (typeName == typeof(Gradient).Name)
		{
			return VfxPropertyType.Gradient;
		}
		if (typeName == typeof(Matrix4x4).Name)
		{
			return VfxPropertyType.Matrix4x4;
		}
		return VfxPropertyType.Unsupported;
	}

	public void Apply(VisualEffect vfx)
	{
		switch (Value.VfxPropertyType)
		{
		case VfxPropertyType.Bool:
			if (vfx.HasBool(m_NameId))
			{
				vfx.SetBool(m_NameId, (bool)m_CurrentValue);
			}
			break;
		case VfxPropertyType.Int:
			if (vfx.HasInt(m_NameId))
			{
				vfx.SetInt(m_NameId, (int)m_CurrentValue);
			}
			break;
		case VfxPropertyType.UInt:
			if (vfx.HasUInt(m_NameId))
			{
				vfx.SetUInt(m_NameId, (uint)m_CurrentValue);
			}
			break;
		case VfxPropertyType.Float:
			if (vfx.HasFloat(m_NameId))
			{
				vfx.SetFloat(m_NameId, (float)m_CurrentValue);
			}
			break;
		case VfxPropertyType.Vector2:
			if (vfx.HasVector2(m_NameId))
			{
				vfx.SetVector2(m_NameId, (Vector2)m_CurrentValue);
			}
			break;
		case VfxPropertyType.Vector3:
			if (vfx.HasVector3(m_NameId))
			{
				vfx.SetVector3(m_NameId, (Vector3)m_CurrentValue);
			}
			break;
		case VfxPropertyType.Vector4:
			if (vfx.HasVector4(m_NameId))
			{
				vfx.SetVector4(m_NameId, (Vector4)m_CurrentValue);
			}
			break;
		case VfxPropertyType.Color:
			if (vfx.HasVector4(m_NameId))
			{
				vfx.SetVector4(m_NameId, (Color)m_CurrentValue);
			}
			break;
		case VfxPropertyType.Texture2D:
		{
			Texture texture = m_CurrentValue as Texture;
			if (texture != null && vfx.HasTexture(m_NameId))
			{
				vfx.SetTexture(m_NameId, texture);
			}
			break;
		}
		case VfxPropertyType.Mesh:
		{
			Mesh mesh = m_CurrentValue as Mesh;
			if (mesh != null && vfx.HasMesh(m_NameId))
			{
				vfx.SetMesh(m_NameId, mesh);
			}
			break;
		}
		case VfxPropertyType.Curve:
			if (m_CurrentValue is AnimationCurve c && vfx.HasAnimationCurve(m_NameId))
			{
				vfx.SetAnimationCurve(m_NameId, c);
			}
			break;
		case VfxPropertyType.Gradient:
			if (m_CurrentValue is Gradient g && vfx.HasGradient(m_NameId))
			{
				vfx.SetGradient(m_NameId, g);
			}
			break;
		case VfxPropertyType.Matrix4x4:
			if (vfx.HasMatrix4x4(m_NameId))
			{
				vfx.SetMatrix4x4(m_NameId, (Matrix4x4)m_CurrentValue);
			}
			break;
		default:
			throw new NotImplementedException();
		}
	}

	internal void ResetToDefault()
	{
		m_CurrentValue = m_DefaultValue.GetValue();
	}

	internal void InitNameId()
	{
		m_NameId = Shader.PropertyToID(Name);
	}

	internal int GetContentHash()
	{
		return HashCode.Combine(Name.GetHashCode(), Value.VfxPropertyType.GetHashCode());
	}

	internal void Interp(VfxPropertyParameter toParam, float t)
	{
		switch (Value.VfxPropertyType)
		{
		case VfxPropertyType.Bool:
		{
			bool flag = (bool)m_CurrentValue;
			m_CurrentValue = ((t > 0.5f) ? toParam.Value.BoolValue : flag);
			break;
		}
		case VfxPropertyType.Int:
		{
			int num = (int)m_CurrentValue;
			m_CurrentValue = (int)math.lerp(num, toParam.Value.IntValue, t);
			break;
		}
		case VfxPropertyType.UInt:
		{
			uint num2 = (uint)m_CurrentValue;
			m_CurrentValue = (uint)math.lerp(num2, toParam.Value.UIntValue, t);
			break;
		}
		case VfxPropertyType.Float:
		{
			float start = (float)m_CurrentValue;
			m_CurrentValue = math.lerp(start, toParam.Value.FloatValue, t);
			break;
		}
		case VfxPropertyType.Vector2:
		{
			Vector2 a3 = (Vector2)m_CurrentValue;
			m_CurrentValue = Vector2.Lerp(a3, toParam.Value.Vector2Value, t);
			break;
		}
		case VfxPropertyType.Vector3:
		{
			Vector3 a4 = (Vector3)m_CurrentValue;
			m_CurrentValue = Vector3.Lerp(a4, toParam.Value.Vector3Value, t);
			break;
		}
		case VfxPropertyType.Vector4:
		{
			Vector4 a2 = (Vector4)m_CurrentValue;
			m_CurrentValue = Vector4.Lerp(a2, toParam.Value.Vector4Value, t);
			break;
		}
		case VfxPropertyType.Color:
		{
			Color a = (Color)m_CurrentValue;
			m_CurrentValue = Color.Lerp(a, toParam.Value.ColorValue, t);
			break;
		}
		case VfxPropertyType.Texture2D:
		{
			Texture texture = (Texture)m_CurrentValue;
			m_CurrentValue = ((t > 0.5f) ? toParam.Value.Texture2DValue : texture);
			break;
		}
		case VfxPropertyType.Mesh:
		{
			Mesh mesh = (Mesh)m_CurrentValue;
			m_CurrentValue = ((t > 0.5f) ? toParam.Value.MeshValue : mesh);
			break;
		}
		case VfxPropertyType.Curve:
		{
			AnimationCurve animationCurve = (AnimationCurve)m_CurrentValue;
			m_CurrentValue = ((t > 0.5f) ? toParam.Value.CurveValue : animationCurve);
			break;
		}
		case VfxPropertyType.Gradient:
		{
			Gradient gradient = (Gradient)m_CurrentValue;
			m_CurrentValue = ((t > 0.5f) ? toParam.Value.GradientValue : gradient);
			break;
		}
		case VfxPropertyType.Matrix4x4:
		{
			Matrix4x4 matrix4x = (Matrix4x4)m_CurrentValue;
			Matrix4x4 matrixValue = toParam.Value.MatrixValue;
			m_CurrentValue = new Matrix4x4(math.lerp(matrix4x.GetColumn(0), matrixValue.GetColumn(0), t), math.lerp(matrix4x.GetColumn(1), matrixValue.GetColumn(1), t), math.lerp(matrix4x.GetColumn(2), matrixValue.GetColumn(2), t), math.lerp(matrix4x.GetColumn(3), matrixValue.GetColumn(3), t));
			break;
		}
		}
	}

	internal void SetDefaultValue(object defaultValue)
	{
		Value.SetValue(defaultValue);
		m_DefaultValue.SetValue(defaultValue);
	}

	internal void ResetDefaultValue(object defaultValue)
	{
		m_DefaultValue.SetValue(defaultValue);
	}

	public VfxPropertyParameter Clone()
	{
		VfxPropertyParameter vfxPropertyParameter = new VfxPropertyParameter(Name, Value.VfxPropertyType);
		vfxPropertyParameter.m_DefaultValue = m_DefaultValue;
		vfxPropertyParameter.ResetToDefault();
		vfxPropertyParameter.InitNameId();
		return vfxPropertyParameter;
	}

	internal bool IsValid()
	{
		return m_DefaultValue.VfxPropertyType == Value.VfxPropertyType;
	}

	internal bool IsDefaultValueEquals(object defaultValue)
	{
		return m_DefaultValue.VfxPropertyType switch
		{
			VfxPropertyType.Unsupported => false, 
			VfxPropertyType.Bool => m_DefaultValue.BoolValue == (bool)defaultValue, 
			VfxPropertyType.Int => m_DefaultValue.IntValue == (int)defaultValue, 
			VfxPropertyType.UInt => m_DefaultValue.UIntValue == (uint)defaultValue, 
			VfxPropertyType.Float => m_DefaultValue.FloatValue == (float)defaultValue, 
			VfxPropertyType.Vector2 => m_DefaultValue.Vector2Value == (Vector2)defaultValue, 
			VfxPropertyType.Vector3 => m_DefaultValue.Vector3Value == (Vector3)defaultValue, 
			VfxPropertyType.Vector4 => m_DefaultValue.Vector4Value == (Vector4)defaultValue, 
			VfxPropertyType.Color => m_DefaultValue.ColorValue == (Color)defaultValue, 
			VfxPropertyType.Texture2D => m_DefaultValue.Texture2DValue == (Texture2D)defaultValue, 
			VfxPropertyType.Mesh => m_DefaultValue.MeshValue == (Mesh)defaultValue, 
			VfxPropertyType.Curve => CompareCurves(m_DefaultValue.CurveValue, (AnimationCurve)defaultValue), 
			VfxPropertyType.Gradient => CompareGradients(m_DefaultValue.GradientValue, (Gradient)defaultValue), 
			VfxPropertyType.Matrix4x4 => m_DefaultValue.MatrixValue == (Matrix4x4)defaultValue, 
			_ => false, 
		};
	}

	private bool CompareCurves(AnimationCurve c1, AnimationCurve c2)
	{
		if (c1.preWrapMode == c2.preWrapMode && c1.postWrapMode == c2.postWrapMode && c1.length == c2.length)
		{
			for (int i = 0; i < c1.length; i++)
			{
				Keyframe k = c1.keys[i];
				Keyframe k2 = c2.keys[i];
				if (!CompareKeys(in k, in k2))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private bool CompareKeys(in Keyframe k1, in Keyframe k2)
	{
		if (k1.time == k2.time && k1.value == k2.value && k1.weightedMode == k2.weightedMode && k1.inTangent == k2.inTangent && k1.outTangent == k2.outTangent && k1.inWeight == k2.inWeight)
		{
			return k1.outWeight == k2.outWeight;
		}
		return false;
	}

	private bool CompareGradients(Gradient g1, Gradient g2)
	{
		if (g1.mode == g2.mode && g1.colorSpace == g2.colorSpace && g1.alphaKeys.Length == g2.alphaKeys.Length && g1.colorKeys.Length == g2.colorKeys.Length)
		{
			for (int i = 0; i < g1.alphaKeys.Length; i++)
			{
				GradientAlphaKey k = g1.alphaKeys[i];
				GradientAlphaKey k2 = g2.alphaKeys[i];
				if (!CompareAlphaKeys(in k, in k2))
				{
					return false;
				}
			}
			for (int j = 0; j < g1.colorKeys.Length; j++)
			{
				GradientColorKey k3 = g1.colorKeys[j];
				GradientColorKey k4 = g2.colorKeys[j];
				if (!CompareColorKeys(k3, k4))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private bool CompareAlphaKeys(in GradientAlphaKey k1, in GradientAlphaKey k2)
	{
		if (k1.alpha == k2.alpha)
		{
			return k1.time == k2.time;
		}
		return false;
	}

	private bool CompareColorKeys(GradientColorKey k1, GradientColorKey k2)
	{
		if (k1.color == k2.color)
		{
			return k1.time == k2.time;
		}
		return false;
	}
}
