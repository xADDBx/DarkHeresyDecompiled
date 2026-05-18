using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class SetPauseGameCommand : GameCommand, IMemoryPackable<SetPauseGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SetPauseGameCommand>
{
	[Preserve]
	private sealed class SetPauseGameCommandFormatter : MemoryPackFormatter<SetPauseGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetPauseGameCommand value)
		{
			SetPauseGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SetPauseGameCommand value)
		{
			SetPauseGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetPauseGameCommand value)
		{
			SetPauseGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SetPauseGameCommand value)
		{
			SetPauseGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private bool m_ToPause;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	protected SetPauseGameCommand()
	{
	}

	[MemoryPackConstructor]
	public SetPauseGameCommand(bool m_toPause)
	{
		m_ToPause = m_toPause;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Controllers.PauseController.SetManualPause(m_ToPause);
	}

	static SetPauseGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SetPauseGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_ToPause", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetPauseGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetPauseGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetPauseGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetPauseGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetPauseGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_ToPause);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetPauseGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<bool>(out value2);
			}
			else
			{
				value2 = value.m_ToPause;
				reader.ReadUnmanaged<bool>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetPauseGameCommand), 1, memberCount);
				return;
			}
			value2 = value != null && value.m_ToPause;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new SetPauseGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SetPauseGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_ToPause");
		writer.WriteUnmanaged(value.m_ToPause);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SetPauseGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		bool v = value != null && value.m_ToPause;
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_ToPause")
				{
					reader.ReadUnmanaged<bool>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_ToPause")
			{
				reader.ReadUnmanaged<bool>(out v);
			}
		}
		_ = value;
		value = new SetPauseGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SetPauseGameCommand source = new SetPauseGameCommand();
		result = Unsafe.As<SetPauseGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SetPauseGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_ToPause", ref m_ToPause, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SetPauseGameCommand>();
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
				m_ToPause = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
