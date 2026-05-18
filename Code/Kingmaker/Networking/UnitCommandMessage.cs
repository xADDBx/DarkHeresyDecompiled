using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.Net;
using Kingmaker.GameCommands;
using Kingmaker.UnitLogic.Commands.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public struct UnitCommandMessage : IMemoryPackable<UnitCommandMessage>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<UnitCommandMessage>
{
	[Preserve]
	private sealed class UnitCommandMessageFormatter : MemoryPackFormatter<UnitCommandMessage>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref UnitCommandMessage value)
		{
			UnitCommandMessage.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref UnitCommandMessage value)
		{
			UnitCommandMessage.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitCommandMessage value)
		{
			UnitCommandMessage.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref UnitCommandMessage value)
		{
			UnitCommandMessage.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "i")]
	public int tickIndex;

	[JsonProperty(PropertyName = "g")]
	public List<GameCommand> gameCommandList;

	[JsonProperty(PropertyName = "c")]
	public List<UnitCommandParams> unitCommandList;

	[JsonProperty(PropertyName = "d")]
	public List<SynchronizedData> synchronizedDataList;

	public static readonly TypeInfo OwlPackTypeInfo;

	public UnitCommandMessage(int tickIndex, List<GameCommand> gameCommandList, List<UnitCommandParams> unitCommandList, List<SynchronizedData> synchronizedDataList)
	{
		this.tickIndex = tickIndex;
		this.gameCommandList = ((0 < gameCommandList?.Count) ? gameCommandList : null);
		this.unitCommandList = ((0 < unitCommandList?.Count) ? unitCommandList : null);
		this.synchronizedDataList = ((0 < synchronizedDataList?.Count) ? synchronizedDataList : null);
	}

	public void AfterDeserialization()
	{
		if (gameCommandList != null)
		{
			foreach (GameCommand gameCommand in gameCommandList)
			{
				gameCommand.AfterDeserialization();
			}
		}
		if (unitCommandList == null)
		{
			return;
		}
		foreach (UnitCommandParams unitCommand in unitCommandList)
		{
			unitCommand.AfterDeserialization();
		}
	}

	static UnitCommandMessage()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "UnitCommandMessage",
			Fields = new FieldInfo[0]
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitCommandMessage>())
		{
			MemoryPackFormatterProvider.Register(new UnitCommandMessageFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitCommandMessage[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitCommandMessage>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<GameCommand>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<GameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<UnitCommandParams>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<UnitCommandParams>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<SynchronizedData>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<SynchronizedData>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref UnitCommandMessage value) where TBufferWriter : class, IBufferWriter<byte>
	{
		writer.WriteUnmanagedWithObjectHeader(4, in value.tickIndex);
		writer.WriteValue(in value.gameCommandList);
		writer.WriteValue(in value.unitCommandList);
		ListFormatter.SerializePackable(ref writer, value.synchronizedDataList);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitCommandMessage value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(UnitCommandMessage);
			return;
		}
		int value2;
		List<GameCommand> value3;
		List<UnitCommandParams> value4;
		List<SynchronizedData> value5;
		if (memberCount == 4)
		{
			reader.ReadUnmanaged<int>(out value2);
			value3 = reader.ReadValue<List<GameCommand>>();
			value4 = reader.ReadValue<List<UnitCommandParams>>();
			value5 = ListFormatter.DeserializePackable<SynchronizedData>(ref reader);
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitCommandMessage), 4, memberCount);
				return;
			}
			value2 = 0;
			value3 = null;
			value4 = null;
			value5 = null;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					if (memberCount != 2)
					{
						reader.ReadValue(ref value4);
						if (memberCount != 3)
						{
							ListFormatter.DeserializePackable(ref reader, ref value5);
							_ = 4;
						}
					}
				}
			}
		}
		value = new UnitCommandMessage(value2, value3, value4, value5);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref UnitCommandMessage value)
	{
		writer.WriteObjectHeader();
		writer.WriteProperty("tickIndex");
		writer.WriteUnmanaged(value.tickIndex);
		writer.WriteProperty("gameCommandList");
		writer.WriteValue(value.gameCommandList);
		writer.WriteProperty("unitCommandList");
		writer.WriteValue(value.unitCommandList);
		writer.WriteProperty("synchronizedDataList");
		ListFormatter.SerializePackableJson(ref writer, value.synchronizedDataList);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref UnitCommandMessage value)
	{
		if (!reader.CheckObjectStart())
		{
			value = default(UnitCommandMessage);
			reader.Advance();
			return;
		}
		reader.Advance();
		int v = 0;
		List<GameCommand> list = null;
		List<UnitCommandParams> list2 = null;
		List<SynchronizedData> list3 = null;
		bool[] array = new bool[4];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			switch (text)
			{
			case "tickIndex":
				reader.ReadUnmanaged<int>(out v);
				array[0] = true;
				break;
			case "gameCommandList":
				list = reader.ReadValue<List<GameCommand>>();
				array[1] = true;
				break;
			case "unitCommandList":
				list2 = reader.ReadValue<List<UnitCommandParams>>();
				array[2] = true;
				break;
			case "synchronizedDataList":
				list3 = ListFormatter.DeserializePackableJson<SynchronizedData>(ref reader);
				array[3] = true;
				break;
			}
		}
		value = new UnitCommandMessage(v, list, list2, list3);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitCommandMessage source = default(UnitCommandMessage);
		result = Unsafe.As<UnitCommandMessage, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<UnitCommandMessage>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitCommandMessage>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
