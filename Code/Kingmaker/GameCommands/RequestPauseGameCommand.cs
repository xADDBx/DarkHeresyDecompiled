using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class RequestPauseGameCommand : GameCommand, IMemoryPackable<RequestPauseGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<RequestPauseGameCommand>
{
	[Preserve]
	private sealed class RequestPauseGameCommandFormatter : MemoryPackFormatter<RequestPauseGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref RequestPauseGameCommand value)
		{
			RequestPauseGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref RequestPauseGameCommand value)
		{
			RequestPauseGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RequestPauseGameCommand value)
		{
			RequestPauseGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref RequestPauseGameCommand value)
		{
			RequestPauseGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	public readonly bool ToPause;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private RequestPauseGameCommand()
	{
	}

	[MemoryPackConstructor]
	public RequestPauseGameCommand(bool toPause)
	{
		ToPause = toPause;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		Game.Instance.Controllers.PauseController.SetPlayer(playerOrEmpty, ToPause);
	}

	static RequestPauseGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "RequestPauseGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("ToPause", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RequestPauseGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RequestPauseGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RequestPauseGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RequestPauseGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref RequestPauseGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.ToPause);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RequestPauseGameCommand? value)
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
				value2 = value.ToPause;
				reader.ReadUnmanaged<bool>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RequestPauseGameCommand), 1, memberCount);
				return;
			}
			value2 = value != null && value.ToPause;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new RequestPauseGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref RequestPauseGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("ToPause");
		writer.WriteUnmanaged(value.ToPause);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref RequestPauseGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		bool v = value != null && value.ToPause;
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "ToPause")
				{
					reader.ReadUnmanaged<bool>(out v);
					array[0] = true;
				}
			}
			else if (text == "ToPause")
			{
				reader.ReadUnmanaged<bool>(out v);
			}
		}
		_ = value;
		value = new RequestPauseGameCommand(v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RequestPauseGameCommand source = new RequestPauseGameCommand();
		result = Unsafe.As<RequestPauseGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RequestPauseGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = ToPause;
		formatter.UnmanagedField(0, "ToPause", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RequestPauseGameCommand>();
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
				Unsafe.AsRef(in ToPause) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
