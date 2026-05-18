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
public sealed class FogOfWarRevealerTriggerGameCommand : GameCommand, IMemoryPackable<FogOfWarRevealerTriggerGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<FogOfWarRevealerTriggerGameCommand>
{
	[Preserve]
	private sealed class FogOfWarRevealerTriggerGameCommandFormatter : MemoryPackFormatter<FogOfWarRevealerTriggerGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref FogOfWarRevealerTriggerGameCommand value)
		{
			FogOfWarRevealerTriggerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref FogOfWarRevealerTriggerGameCommand value)
		{
			FogOfWarRevealerTriggerGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref FogOfWarRevealerTriggerGameCommand value)
		{
			FogOfWarRevealerTriggerGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref FogOfWarRevealerTriggerGameCommand value)
		{
			FogOfWarRevealerTriggerGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	public readonly string Id;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public FogOfWarRevealerTriggerGameCommand(string id)
	{
		Id = id;
	}

	private FogOfWarRevealerTriggerGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		if (!FogOfWarRevealerTrigger.AllTriggers.TryGetValue(Id, out var value))
		{
			PFLog.GameCommands.Error("FogOfWarRevealerTrigger #" + Id + " was not found!");
		}
		else
		{
			value.Reveal();
		}
	}

	static FogOfWarRevealerTriggerGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "FogOfWarRevealerTriggerGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("Id", typeof(string))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<FogOfWarRevealerTriggerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new FogOfWarRevealerTriggerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<FogOfWarRevealerTriggerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<FogOfWarRevealerTriggerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref FogOfWarRevealerTriggerGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.Id);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref FogOfWarRevealerTriggerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string id;
		if (memberCount == 1)
		{
			if (value == null)
			{
				id = reader.ReadString();
			}
			else
			{
				id = value.Id;
				id = reader.ReadString();
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(FogOfWarRevealerTriggerGameCommand), 1, memberCount);
				return;
			}
			id = ((value != null) ? value.Id : null);
			if (memberCount != 0)
			{
				id = reader.ReadString();
				_ = 1;
			}
			_ = value;
		}
		value = new FogOfWarRevealerTriggerGameCommand(id);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref FogOfWarRevealerTriggerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("Id");
		writer.WriteString(value.Id);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref FogOfWarRevealerTriggerGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		string id = ((value != null) ? value.Id : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "Id")
				{
					id = reader.ReadString();
					array[0] = true;
				}
			}
			else if (text == "Id")
			{
				id = reader.ReadString();
			}
		}
		_ = value;
		value = new FogOfWarRevealerTriggerGameCommand(id);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FogOfWarRevealerTriggerGameCommand source = new FogOfWarRevealerTriggerGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<FogOfWarRevealerTriggerGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FogOfWarRevealerTriggerGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = Id;
		formatter.StringField(0, "Id", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FogOfWarRevealerTriggerGameCommand>();
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
				Unsafe.AsRef(in Id) = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
