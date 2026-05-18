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
public sealed class SetFlashlightTargetGameCommand : GameCommand, IMemoryPackable<SetFlashlightTargetGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SetFlashlightTargetGameCommand>
{
	[Preserve]
	private sealed class SetFlashlightTargetGameCommandFormatter : MemoryPackFormatter<SetFlashlightTargetGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetFlashlightTargetGameCommand value)
		{
			SetFlashlightTargetGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SetFlashlightTargetGameCommand value)
		{
			SetFlashlightTargetGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetFlashlightTargetGameCommand value)
		{
			SetFlashlightTargetGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SetFlashlightTargetGameCommand value)
		{
			SetFlashlightTargetGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private Vector3 m_Point;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	public SetFlashlightTargetGameCommand()
	{
	}

	[JsonConstructor]
	public SetFlashlightTargetGameCommand(Vector3 point)
	{
		m_Point = point;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Controllers.ServoskullFlashlightController.SetFlashlightPoint(m_Point);
	}

	static SetFlashlightTargetGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SetFlashlightTargetGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_Point", typeof(Vector3))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetFlashlightTargetGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetFlashlightTargetGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetFlashlightTargetGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetFlashlightTargetGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetFlashlightTargetGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Point);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetFlashlightTargetGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Vector3 value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_Point;
				reader.ReadUnmanaged<Vector3>(out value2);
				goto IL_0071;
			}
			reader.ReadUnmanaged<Vector3>(out value2);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetFlashlightTargetGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Point : default(Vector3));
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<Vector3>(out value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0071;
			}
		}
		value = new SetFlashlightTargetGameCommand
		{
			m_Point = value2
		};
		return;
		IL_0071:
		value.m_Point = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SetFlashlightTargetGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Point");
		writer.WriteUnmanaged(value.m_Point);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SetFlashlightTargetGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		Vector3 v = ((value != null) ? value.m_Point : default(Vector3));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_Point")
				{
					reader.ReadUnmanaged<Vector3>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_Point")
			{
				reader.ReadUnmanaged<Vector3>(out v);
			}
		}
		if (value != null)
		{
			value.m_Point = v;
		}
		else
		{
			value = new SetFlashlightTargetGameCommand
			{
				m_Point = v
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
		SetFlashlightTargetGameCommand source = new SetFlashlightTargetGameCommand();
		result = Unsafe.As<SetFlashlightTargetGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SetFlashlightTargetGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Point", ref m_Point, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SetFlashlightTargetGameCommand>();
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
				m_Point = formatter.ReadPackable<Vector3>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
