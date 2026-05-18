using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CameraFollowTimeScaleGameCommand : GameCommand, IMemoryPackable<CameraFollowTimeScaleGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CameraFollowTimeScaleGameCommand>
{
	[Preserve]
	private sealed class CameraFollowTimeScaleGameCommandFormatter : MemoryPackFormatter<CameraFollowTimeScaleGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CameraFollowTimeScaleGameCommand value)
		{
			CameraFollowTimeScaleGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CameraFollowTimeScaleGameCommand value)
		{
			CameraFollowTimeScaleGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CameraFollowTimeScaleGameCommand value)
		{
			CameraFollowTimeScaleGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CameraFollowTimeScaleGameCommand value)
		{
			CameraFollowTimeScaleGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private byte m_Scale;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private bool m_Force;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CameraFollowTimeScaleGameCommand()
	{
	}

	private CameraFollowTimeScaleGameCommand(byte m_scale, bool m_force)
	{
		m_Scale = m_scale;
		m_Force = m_force;
	}

	public CameraFollowTimeScaleGameCommand(float scale, bool force)
		: this(FloatToByte(scale), force)
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Controllers.TimeController.SetCameraFollowTimeScale(ByteToFloat(m_Scale), m_Force);
	}

	private static byte FloatToByte(float scale)
	{
		return (byte)(255f * Mathf.Clamp01(scale));
	}

	private static float ByteToFloat(byte scale)
	{
		return (float)(int)scale / 255f;
	}

	static CameraFollowTimeScaleGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CameraFollowTimeScaleGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_Scale", typeof(byte)),
				new FieldInfo("m_Force", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CameraFollowTimeScaleGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CameraFollowTimeScaleGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CameraFollowTimeScaleGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CameraFollowTimeScaleGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CameraFollowTimeScaleGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_Scale, in value.m_Force);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CameraFollowTimeScaleGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		byte value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_Scale;
				value3 = value.m_Force;
				reader.ReadUnmanaged<byte>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
				goto IL_0096;
			}
			reader.ReadUnmanaged<byte, bool>(out value2, out value3);
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CameraFollowTimeScaleGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = false;
			}
			else
			{
				value2 = value.m_Scale;
				value3 = value.m_Force;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<byte>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0096;
			}
		}
		value = new CameraFollowTimeScaleGameCommand
		{
			m_Scale = value2,
			m_Force = value3
		};
		return;
		IL_0096:
		value.m_Scale = value2;
		value.m_Force = value3;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CameraFollowTimeScaleGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Scale");
		writer.WriteUnmanaged(value.m_Scale);
		writer.WriteProperty("m_Force");
		writer.WriteUnmanaged(value.m_Force);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CameraFollowTimeScaleGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		byte v;
		bool v2;
		if (value == null)
		{
			v = 0;
			v2 = false;
		}
		else
		{
			v = value.m_Scale;
			v2 = value.m_Force;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_Scale"))
				{
					if (text == "m_Force")
					{
						reader.ReadUnmanaged<bool>(out v2);
						array[1] = true;
					}
				}
				else
				{
					reader.ReadUnmanaged<byte>(out v);
					array[0] = true;
				}
			}
			else if (!(text == "m_Scale"))
			{
				if (text == "m_Force")
				{
					reader.ReadUnmanaged<bool>(out v2);
				}
			}
			else
			{
				reader.ReadUnmanaged<byte>(out v);
			}
		}
		if (value != null)
		{
			value.m_Scale = v;
			value.m_Force = v2;
		}
		else
		{
			value = new CameraFollowTimeScaleGameCommand
			{
				m_Scale = v,
				m_Force = v2
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CameraFollowTimeScaleGameCommand source = new CameraFollowTimeScaleGameCommand();
		result = Unsafe.As<CameraFollowTimeScaleGameCommand, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<CameraFollowTimeScaleGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_Scale", ref m_Scale, state);
		formatter.UnmanagedField(1, "m_Force", ref m_Force, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CameraFollowTimeScaleGameCommand>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_Scale = formatter.ReadUnmanaged<byte>(state);
				break;
			case 1:
				m_Force = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
