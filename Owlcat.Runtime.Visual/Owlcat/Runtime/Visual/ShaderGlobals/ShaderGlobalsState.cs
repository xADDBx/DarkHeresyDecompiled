using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.ShaderGlobals;

public sealed class ShaderGlobalsState : IDisposable
{
	public abstract class ValueSetBase : IDisposable
	{
		public virtual void Dispose()
		{
		}

		internal abstract void Reset();

		internal abstract void LoadValuesFromConfig(WaaaghShaderGlobalsConfig config);

		public abstract void UploadToGPU(CommandBuffer cmd);
	}

	public abstract class ValueSet<T> : ValueSetBase
	{
		private static readonly string s_IndexOutOfRangeMessage = "Global Shader " + typeof(T).Name + " index out of range.";

		private readonly ShaderGlobalsState m_ShaderGlobalsState;

		private readonly List<GlobalValue<T>> m_Values = new List<GlobalValue<T>>();

		internal ValueSet(ShaderGlobalsState shaderGlobalsState)
		{
			m_ShaderGlobalsState = shaderGlobalsState;
		}

		internal sealed override void Reset()
		{
			m_Values.Clear();
		}

		public ShaderGlobalHandle<T> CreateHandle(string name)
		{
			ShaderGlobalHandle<T> shaderGlobalHandle = default(ShaderGlobalHandle<T>);
			shaderGlobalHandle.Name = name;
			shaderGlobalHandle.Generation = int.MinValue;
			ShaderGlobalHandle<T> handle = shaderGlobalHandle;
			if (!VerifyHandle(ref handle))
			{
				return default(ShaderGlobalHandle<T>);
			}
			return handle;
		}

		public void SetValue(ref ShaderGlobalHandle<T> handle, T value)
		{
			if (VerifyHandle(ref handle, throwOnNotFound: false))
			{
				GlobalValue<T> value2 = m_Values[handle.Index];
				value2.Value = value;
				m_Values[handle.Index] = value2;
			}
		}

		public T GetValue(ref ShaderGlobalHandle<T> handle)
		{
			if (!VerifyHandle(ref handle))
			{
				return default(T);
			}
			return m_Values[handle.Index].Value;
		}

		[MustUseReturnValue]
		private bool VerifyHandle(ref ShaderGlobalHandle<T> handle, bool throwOnNotFound = true)
		{
			if (handle.Name == null)
			{
				throw new ArgumentNullException("Name");
			}
			if (handle.Generation != m_ShaderGlobalsState.Generation && !TryFindByName(handle.Name, out handle))
			{
				string message = "Global Shader " + typeof(T).Name + " " + handle.Name + " could not be found.";
				if (throwOnNotFound)
				{
					throw new InvalidOperationException(message);
				}
				Debug.LogError(message, ShaderGlobalsCommon.TryLoadConfig(out var config) ? config : null);
				return false;
			}
			if (handle.Index < 0 || handle.Index >= m_Values.Count)
			{
				throw new InvalidOperationException(s_IndexOutOfRangeMessage);
			}
			return true;
		}

		private bool TryFindByName(string name, out ShaderGlobalHandle<T> handle)
		{
			for (int i = 0; i < m_Values.Count; i++)
			{
				GlobalValue<T> globalValue = m_Values[i];
				if (!(globalValue.Name != name))
				{
					handle = new ShaderGlobalHandle<T>
					{
						Name = globalValue.Name,
						ShaderID = globalValue.ShaderID,
						Index = i,
						Generation = m_ShaderGlobalsState.Generation
					};
					return true;
				}
			}
			handle = new ShaderGlobalHandle<T>
			{
				Name = null,
				Index = -1,
				Generation = -1
			};
			return false;
		}

		internal override void LoadValuesFromConfig(WaaaghShaderGlobalsConfig config)
		{
			IReadOnlyList<WaaaghShaderGlobalsConfig.DynamicGlobal<T>> sourceFromConfig = GetSourceFromConfig(config);
			if (sourceFromConfig == null)
			{
				return;
			}
			for (int i = 0; i < sourceFromConfig.Count; i++)
			{
				WaaaghShaderGlobalsConfig.DynamicGlobal<T> dynamicGlobal = sourceFromConfig[i];
				ShaderGlobalsCommon.ReferenceNameKey nameKey = dynamicGlobal.CreateReferenceNameKey();
				string text = ShaderGlobalsCommon.BuildReferenceName(in nameKey);
				GlobalValue<T> globalValue = default(GlobalValue<T>);
				globalValue.Name = dynamicGlobal.Name;
				globalValue.ShaderID = Shader.PropertyToID(text);
				globalValue.Value = dynamicGlobal.GetDefaultValue();
				GlobalValue<T> item = globalValue;
				if (dynamicGlobal is WaaaghShaderGlobalsConfig.DynamicGlobalGraphicsBuffer dynamicGlobalGraphicsBuffer)
				{
					item.BufferItemStride = dynamicGlobalGraphicsBuffer.ItemType switch
					{
						WaaaghShaderGlobalsConfig.GraphicsBufferType.Int => 4, 
						WaaaghShaderGlobalsConfig.GraphicsBufferType.Float => 4, 
						WaaaghShaderGlobalsConfig.GraphicsBufferType.Vector4 => UnsafeUtility.SizeOf<Vector4>(), 
						WaaaghShaderGlobalsConfig.GraphicsBufferType.Matrix4X4 => UnsafeUtility.SizeOf<Matrix4x4>(), 
						_ => throw new ArgumentOutOfRangeException(), 
					};
					item.BufferCountShaderID = Shader.PropertyToID(ShaderGlobalsCommon.BufferCountName(text));
				}
				m_Values.Add(item);
			}
		}

		protected abstract IReadOnlyList<WaaaghShaderGlobalsConfig.DynamicGlobal<T>> GetSourceFromConfig(WaaaghShaderGlobalsConfig config);

		public sealed override void UploadToGPU(CommandBuffer cmd)
		{
			foreach (GlobalValue<T> value2 in m_Values)
			{
				GlobalValue<T> value = value2;
				SetGlobalValue(cmd, in value);
			}
		}

		internal abstract void SetGlobalValue(CommandBuffer cmd, in GlobalValue<T> value);
	}

	public sealed class IntValueSet : ValueSet<int>
	{
		internal IntValueSet(ShaderGlobalsState shaderGlobalsState)
			: base(shaderGlobalsState)
		{
		}

		protected override IReadOnlyList<WaaaghShaderGlobalsConfig.DynamicGlobal<int>> GetSourceFromConfig(WaaaghShaderGlobalsConfig config)
		{
			return config.DynamicInts;
		}

		internal override void SetGlobalValue(CommandBuffer cmd, in GlobalValue<int> value)
		{
			cmd.SetGlobalInteger(value.ShaderID, value.Value);
		}
	}

	public sealed class FloatValueSet : ValueSet<float>
	{
		internal FloatValueSet(ShaderGlobalsState shaderGlobalsState)
			: base(shaderGlobalsState)
		{
		}

		protected override IReadOnlyList<WaaaghShaderGlobalsConfig.DynamicGlobal<float>> GetSourceFromConfig(WaaaghShaderGlobalsConfig config)
		{
			return config.DynamicFloats;
		}

		internal override void SetGlobalValue(CommandBuffer cmd, in GlobalValue<float> value)
		{
			cmd.SetGlobalFloat(value.ShaderID, value.Value);
		}
	}

	public sealed class VectorValueSet : ValueSet<Vector4>
	{
		internal VectorValueSet(ShaderGlobalsState shaderGlobalsState)
			: base(shaderGlobalsState)
		{
		}

		protected override IReadOnlyList<WaaaghShaderGlobalsConfig.DynamicGlobal<Vector4>> GetSourceFromConfig(WaaaghShaderGlobalsConfig config)
		{
			return config.DynamicVectors;
		}

		internal override void SetGlobalValue(CommandBuffer cmd, in GlobalValue<Vector4> value)
		{
			cmd.SetGlobalVector(value.ShaderID, value.Value);
		}
	}

	public sealed class ColorValueSet : ValueSet<Color>
	{
		internal ColorValueSet(ShaderGlobalsState shaderGlobalsState)
			: base(shaderGlobalsState)
		{
		}

		protected override IReadOnlyList<WaaaghShaderGlobalsConfig.DynamicGlobal<Color>> GetSourceFromConfig(WaaaghShaderGlobalsConfig config)
		{
			return config.DynamicColors;
		}

		internal override void SetGlobalValue(CommandBuffer cmd, in GlobalValue<Color> value)
		{
			cmd.SetGlobalVector(value.ShaderID, value.Value);
		}
	}

	public sealed class TextureValueSet : ValueSet<Texture2D>
	{
		public TextureValueSet(ShaderGlobalsState shaderGlobalsState)
			: base(shaderGlobalsState)
		{
		}

		protected override IReadOnlyList<WaaaghShaderGlobalsConfig.DynamicGlobal<Texture2D>> GetSourceFromConfig(WaaaghShaderGlobalsConfig config)
		{
			return config.DynamicTextures;
		}

		internal override void SetGlobalValue(CommandBuffer cmd, in GlobalValue<Texture2D> value)
		{
			cmd.SetGlobalTexture(value.ShaderID, value.Value);
		}
	}

	public sealed class GraphicsBufferValueSet : ValueSet<GraphicsBuffer>
	{
		private readonly Dictionary<int, GraphicsBuffer> m_DefaultBuffers = new Dictionary<int, GraphicsBuffer>();

		public GraphicsBufferValueSet(ShaderGlobalsState shaderGlobalsState)
			: base(shaderGlobalsState)
		{
		}

		protected override IReadOnlyList<WaaaghShaderGlobalsConfig.DynamicGlobal<GraphicsBuffer>> GetSourceFromConfig(WaaaghShaderGlobalsConfig config)
		{
			return config.DynamicGraphicsBuffers;
		}

		internal override void SetGlobalValue(CommandBuffer cmd, in GlobalValue<GraphicsBuffer> value)
		{
			GraphicsBuffer graphicsBuffer = ((value.Value != null && value.Value.IsValid()) ? value.Value : GetOrCreateDefaultBuffer(value.BufferItemStride));
			if (graphicsBuffer.stride != value.BufferItemStride)
			{
				throw new InvalidOperationException($"Tried to set buffer with stride {graphicsBuffer.stride} while {value.BufferItemStride} was expected.");
			}
			cmd.SetGlobalBuffer(value.ShaderID, graphicsBuffer);
			cmd.SetGlobalInteger(value.BufferCountShaderID, graphicsBuffer.count);
		}

		private GraphicsBuffer GetOrCreateDefaultBuffer(int stride)
		{
			if (m_DefaultBuffers.TryGetValue(stride, out var value) && value != null && value.IsValid())
			{
				return value;
			}
			value?.Dispose();
			value = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, stride);
			m_DefaultBuffers[stride] = value;
			return value;
		}

		public override void Dispose()
		{
			base.Dispose();
			foreach (GraphicsBuffer value in m_DefaultBuffers.Values)
			{
				value?.Dispose();
			}
			m_DefaultBuffers.Clear();
		}
	}

	internal struct GlobalValue<T>
	{
		public string Name;

		public int ShaderID;

		public T Value;

		public int BufferItemStride;

		public int BufferCountShaderID;
	}

	private const int kInvalidGeneration = int.MinValue;

	private readonly List<ValueSetBase> m_AllValueSets = new List<ValueSetBase>();

	public ColorValueSet Colors { get; }

	public FloatValueSet Floats { get; }

	public IntValueSet Ints { get; }

	public TextureValueSet Textures { get; }

	public VectorValueSet Vectors { get; }

	public GraphicsBufferValueSet GraphicsBuffers { get; }

	public static ShaderGlobalsState Instance { get; private set; }

	private int Generation { get; set; } = -1;


	static ShaderGlobalsState()
	{
		Instance = new ShaderGlobalsState();
	}

	public ShaderGlobalsState()
	{
		Floats = AddValueSet(new FloatValueSet(this));
		Ints = AddValueSet(new IntValueSet(this));
		Vectors = AddValueSet(new VectorValueSet(this));
		Colors = AddValueSet(new ColorValueSet(this));
		Textures = AddValueSet(new TextureValueSet(this));
		GraphicsBuffers = AddValueSet(new GraphicsBufferValueSet(this));
		Reset();
	}

	public void Dispose()
	{
		foreach (ValueSetBase allValueSet in m_AllValueSets)
		{
			allValueSet.Dispose();
		}
		m_AllValueSets.Clear();
	}

	private TValueSet AddValueSet<TValueSet>(TValueSet valueSet) where TValueSet : ValueSetBase
	{
		m_AllValueSets.Add(valueSet);
		return valueSet;
	}

	internal void Reset()
	{
		int generation = Generation + 1;
		Generation = generation;
		foreach (ValueSetBase allValueSet in m_AllValueSets)
		{
			allValueSet.Reset();
		}
		if (ShaderGlobalsCommon.TryLoadConfig(out var config))
		{
			foreach (ValueSetBase allValueSet2 in m_AllValueSets)
			{
				allValueSet2.LoadValuesFromConfig(config);
			}
			return;
		}
		Debug.LogWarning("WaaaghShaderGlobalsConfig not found at Assets/Resources/ShaderGlobalsConfig.asset.");
	}

	internal void UploadToGPU(CommandBuffer cmd)
	{
		foreach (ValueSetBase allValueSet in m_AllValueSets)
		{
			allValueSet.UploadToGPU(cmd);
		}
	}
}
