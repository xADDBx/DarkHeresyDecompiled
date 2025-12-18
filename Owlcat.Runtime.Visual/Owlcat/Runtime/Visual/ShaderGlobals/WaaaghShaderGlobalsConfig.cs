using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Visual.ShaderGlobals;

public sealed class WaaaghShaderGlobalsConfig : ScriptableObject
{
	public enum GraphicsBufferType
	{
		Int,
		Float,
		Vector4,
		Matrix4X4
	}

	public abstract class GlobalBase
	{
		public string Name;

		protected abstract Type Type { get; }

		public abstract ShaderGlobalsCommon.GlobalKind GlobalKind { get; }

		public abstract Vector4 MaterialSlotDefaultValue { get; }

		public void Validate()
		{
			Name = string.Join("", Name.Where((char c) => char.IsLetterOrDigit(c) || c == '_'));
		}

		public ShaderGlobalsCommon.ReferenceNameKey CreateReferenceNameKey()
		{
			return new ShaderGlobalsCommon.ReferenceNameKey(Name, Type, GlobalKind);
		}
	}

	[Serializable]
	public abstract class StaticGlobalBase : GlobalBase
	{
		public abstract int ComponentCount { get; }

		public abstract Type ComponentType { get; }

		public abstract object GetComponent(int index);
	}

	[Serializable]
	public abstract class StaticGlobal<T> : StaticGlobalBase where T : unmanaged
	{
		protected sealed override Type Type => typeof(T);

		public sealed override ShaderGlobalsCommon.GlobalKind GlobalKind => ShaderGlobalsCommon.GlobalKind.Static;

		public override int ComponentCount => 1;

		public override Type ComponentType => typeof(T);

		public abstract T GetValue();

		public override object GetComponent(int index)
		{
			return GetValue();
		}
	}

	[Serializable]
	public abstract class BasicStaticGlobal<T> : StaticGlobal<T> where T : unmanaged
	{
		public T Value;

		public sealed override T GetValue()
		{
			return Value;
		}
	}

	[Serializable]
	public sealed class StaticGlobalFloat : BasicStaticGlobal<float>
	{
		public override Vector4 MaterialSlotDefaultValue => new Vector4(Value, 0f, 0f, 0f);
	}

	[Serializable]
	public sealed class StaticGlobalInt : BasicStaticGlobal<int>
	{
		public override Vector4 MaterialSlotDefaultValue => new Vector4(Value, 0f, 0f, 0f);
	}

	[Serializable]
	public sealed class StaticGlobalVector4 : BasicStaticGlobal<Vector4>
	{
		public override int ComponentCount => 4;

		public override Type ComponentType => typeof(float);

		public override Vector4 MaterialSlotDefaultValue => Value;

		public override object GetComponent(int index)
		{
			return Value[index];
		}
	}

	[Serializable]
	public sealed class StaticGlobalColor : StaticGlobal<Color>
	{
		[ColorUsage(true, true)]
		public Color Value;

		public override int ComponentCount => 4;

		public override Type ComponentType => typeof(float);

		public override Vector4 MaterialSlotDefaultValue => Value;

		public override Color GetValue()
		{
			return Value;
		}

		public override object GetComponent(int index)
		{
			return Value[index];
		}
	}

	[Serializable]
	public abstract class DynamicGlobal<T> : GlobalBase
	{
		[CanBeNull]
		public virtual Type CBufferFieldType => null;

		protected sealed override Type Type => typeof(T);

		public sealed override ShaderGlobalsCommon.GlobalKind GlobalKind => ShaderGlobalsCommon.GlobalKind.Dynamic;

		public abstract T GetDefaultValue();
	}

	[Serializable]
	public abstract class BasicDynamicGlobal<T> : DynamicGlobal<T>
	{
		public T DefaultValue;

		public sealed override Type CBufferFieldType => typeof(T);

		public sealed override T GetDefaultValue()
		{
			return DefaultValue;
		}
	}

	[Serializable]
	public sealed class DynamicGlobalFloat : BasicDynamicGlobal<float>
	{
		public override Vector4 MaterialSlotDefaultValue => new Vector4(DefaultValue, 0f, 0f, 0f);
	}

	[Serializable]
	public sealed class DynamicGlobalInt : BasicDynamicGlobal<int>
	{
		public override Vector4 MaterialSlotDefaultValue => new Vector4(DefaultValue, 0f, 0f, 0f);
	}

	[Serializable]
	public sealed class DynamicGlobalVector4 : BasicDynamicGlobal<Vector4>
	{
		public override Vector4 MaterialSlotDefaultValue => DefaultValue;
	}

	[Serializable]
	public sealed class DynamicGlobalColor : DynamicGlobal<Color>
	{
		[ColorUsage(true, true)]
		public Color DefaultValue;

		public override Type CBufferFieldType => typeof(Vector4);

		public override Vector4 MaterialSlotDefaultValue => DefaultValue;

		public override Color GetDefaultValue()
		{
			return DefaultValue;
		}
	}

	[Serializable]
	public sealed class DynamicGlobalTexture : DynamicGlobal<Texture2D>
	{
		public Texture2D DefaultValue;

		public override Vector4 MaterialSlotDefaultValue => default(Vector4);

		public override Texture2D GetDefaultValue()
		{
			return DefaultValue;
		}
	}

	[Serializable]
	public sealed class DynamicGlobalGraphicsBuffer : DynamicGlobal<GraphicsBuffer>
	{
		public GraphicsBufferType ItemType;

		public override Vector4 MaterialSlotDefaultValue => default(Vector4);

		public override GraphicsBuffer GetDefaultValue()
		{
			return null;
		}
	}

	private const bool kColorsUseHDR = true;

	private const bool kColorsUseAlpha = true;

	[Header("Static Globals - compile-time shader constants.")]
	public List<StaticGlobalFloat> StaticFloats = new List<StaticGlobalFloat>();

	public List<StaticGlobalInt> StaticInts = new List<StaticGlobalInt>();

	public List<StaticGlobalVector4> StaticVectors = new List<StaticGlobalVector4>();

	public List<StaticGlobalColor> StaticColors = new List<StaticGlobalColor>();

	[Header("Dynamic Globals - get default values on start, can be overriden in runtime.")]
	public List<DynamicGlobalFloat> DynamicFloats = new List<DynamicGlobalFloat>();

	public List<DynamicGlobalInt> DynamicInts = new List<DynamicGlobalInt>();

	public List<DynamicGlobalVector4> DynamicVectors = new List<DynamicGlobalVector4>();

	public List<DynamicGlobalColor> DynamicColors = new List<DynamicGlobalColor>();

	public List<DynamicGlobalTexture> DynamicTextures = new List<DynamicGlobalTexture>();

	public List<DynamicGlobalGraphicsBuffer> DynamicGraphicsBuffers = new List<DynamicGlobalGraphicsBuffer>();

	private void OnValidate()
	{
		ValidateGlobals(StaticFloats);
		ValidateGlobals(StaticInts);
		ValidateGlobals(StaticVectors);
		ValidateGlobals(StaticColors);
		ValidateGlobals(DynamicFloats);
		ValidateGlobals(DynamicInts);
		ValidateGlobals(DynamicVectors);
		ValidateGlobals(DynamicColors);
		ValidateGlobals(DynamicTextures);
		ValidateGlobals(DynamicGraphicsBuffers);
		static void ValidateGlobals(IEnumerable<GlobalBase> globals)
		{
			if (globals != null)
			{
				foreach (GlobalBase global in globals)
				{
					global.Validate();
				}
			}
		}
	}
}
