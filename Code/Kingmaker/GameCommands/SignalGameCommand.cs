using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using Kingmaker.Signals;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class SignalGameCommand : GameCommand, IMemoryPackable<SignalGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SignalGameCommand>
{
	[Preserve]
	private sealed class SignalGameCommandFormatter : MemoryPackFormatter<SignalGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SignalGameCommand value)
		{
			SignalGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SignalGameCommand value)
		{
			SignalGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SignalGameCommand value)
		{
			SignalGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SignalGameCommand value)
		{
			SignalGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	public readonly uint Key;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	public SignalGameCommand(uint key)
	{
		Key = key;
	}

	[JsonConstructor]
	private SignalGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		SignalService.Instance.Write(Key, playerOrEmpty);
	}

	static SignalGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SignalGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("Key", typeof(uint))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SignalGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SignalGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SignalGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SignalGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SignalGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.Key);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SignalGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		uint value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<uint>(out value2);
			}
			else
			{
				value2 = value.Key;
				reader.ReadUnmanaged<uint>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SignalGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.Key : 0u);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<uint>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new SignalGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SignalGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("Key");
		writer.WriteUnmanaged(value.Key);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SignalGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		uint v = ((value != null) ? value.Key : 0u);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "Key")
				{
					reader.ReadUnmanaged<uint>(out v);
					array[0] = true;
				}
			}
			else if (text == "Key")
			{
				reader.ReadUnmanaged<uint>(out v);
			}
		}
		_ = value;
		value = new SignalGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SignalGameCommand source = new SignalGameCommand();
		result = Unsafe.As<SignalGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SignalGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		uint value = Key;
		formatter.UnmanagedField(0, "Key", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SignalGameCommand>();
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
				Unsafe.AsRef(in Key) = formatter.ReadUnmanaged<uint>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
