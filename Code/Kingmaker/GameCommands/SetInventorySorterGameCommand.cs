using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class SetInventorySorterGameCommand : GameCommand, IMemoryPackable<SetInventorySorterGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<SetInventorySorterGameCommand>
{
	[Preserve]
	private sealed class SetInventorySorterGameCommandFormatter : MemoryPackFormatter<SetInventorySorterGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetInventorySorterGameCommand value)
		{
			SetInventorySorterGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SetInventorySorterGameCommand value)
		{
			SetInventorySorterGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetInventorySorterGameCommand value)
		{
			SetInventorySorterGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SetInventorySorterGameCommand value)
		{
			SetInventorySorterGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private ItemsSorterType m_SorterType;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackConstructor]
	private SetInventorySorterGameCommand()
	{
	}

	[JsonConstructor]
	public SetInventorySorterGameCommand(ItemsSorterType sorterType)
	{
		m_SorterType = sorterType;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.UISettings.InventorySorter = m_SorterType;
		EventBus.RaiseEvent(delegate(ISetInventorySorterHandler h)
		{
			h.HandleSetInventorySorter(m_SorterType);
		});
	}

	static SetInventorySorterGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "SetInventorySorterGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_SorterType", typeof(ItemsSorterType))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetInventorySorterGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetInventorySorterGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetInventorySorterGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetInventorySorterGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ItemsSorterType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<ItemsSorterType>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SetInventorySorterGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_SorterType);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetInventorySorterGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemsSorterType value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_SorterType;
				reader.ReadUnmanaged<ItemsSorterType>(out value2);
				goto IL_006b;
			}
			reader.ReadUnmanaged<ItemsSorterType>(out value2);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetInventorySorterGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_SorterType : ItemsSorterType.NotSorted);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<ItemsSorterType>(out value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006b;
			}
		}
		value = new SetInventorySorterGameCommand
		{
			m_SorterType = value2
		};
		return;
		IL_006b:
		value.m_SorterType = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SetInventorySorterGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_SorterType");
		writer.WriteUnmanaged(value.m_SorterType);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SetInventorySorterGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		ItemsSorterType v = ((value != null) ? value.m_SorterType : ItemsSorterType.NotSorted);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_SorterType")
				{
					reader.ReadUnmanaged<ItemsSorterType>(out v);
					array[0] = true;
				}
			}
			else if (text == "m_SorterType")
			{
				reader.ReadUnmanaged<ItemsSorterType>(out v);
			}
		}
		if (value != null)
		{
			value.m_SorterType = v;
		}
		else
		{
			value = new SetInventorySorterGameCommand
			{
				m_SorterType = v
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
		SetInventorySorterGameCommand source = new SetInventorySorterGameCommand();
		result = Unsafe.As<SetInventorySorterGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SetInventorySorterGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EnumField(0, "m_SorterType", ref m_SorterType, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SetInventorySorterGameCommand>();
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
				m_SorterType = formatter.ReadEnum<ItemsSorterType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
