using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class AreaTransitionGameCommand : GameCommand, IMemoryPackable<AreaTransitionGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<AreaTransitionGameCommand>
{
	[Preserve]
	private sealed class AreaTransitionGameCommandFormatter : MemoryPackFormatter<AreaTransitionGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AreaTransitionGameCommand value)
		{
			AreaTransitionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref AreaTransitionGameCommand value)
		{
			AreaTransitionGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AreaTransitionGameCommand value)
		{
			AreaTransitionGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref AreaTransitionGameCommand value)
		{
			AreaTransitionGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private BlueprintMultiEntranceEntryReference m_MultiEntranceEntryRef;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private AreaTransitionGameCommand()
	{
	}

	public AreaTransitionGameCommand([NotNull] BlueprintMultiEntranceEntry multiEntrance)
	{
		m_MultiEntranceEntryRef = multiEntrance.ToReference<BlueprintMultiEntranceEntryReference>();
	}

	protected override void ExecuteInternal()
	{
		BlueprintMultiEntranceEntry blueprintMultiEntranceEntry = m_MultiEntranceEntryRef?.Get();
		if (blueprintMultiEntranceEntry != null)
		{
			blueprintMultiEntranceEntry.Enter();
			EventBus.RaiseEvent(delegate(IAreaTransitionHandler h)
			{
				h.HandleAreaTransition();
			});
		}
	}

	static AreaTransitionGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "AreaTransitionGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_MultiEntranceEntryRef", typeof(BlueprintMultiEntranceEntryReference))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AreaTransitionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AreaTransitionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AreaTransitionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AreaTransitionGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AreaTransitionGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_MultiEntranceEntryRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AreaTransitionGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintMultiEntranceEntryReference value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_MultiEntranceEntryRef;
				reader.ReadPackable(ref value2);
				goto IL_006a;
			}
			value2 = reader.ReadPackable<BlueprintMultiEntranceEntryReference>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AreaTransitionGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_MultiEntranceEntryRef : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new AreaTransitionGameCommand
		{
			m_MultiEntranceEntryRef = value2
		};
		return;
		IL_006a:
		value.m_MultiEntranceEntryRef = value2;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref AreaTransitionGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_MultiEntranceEntryRef");
		writer.WritePackable(value.m_MultiEntranceEntryRef);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref AreaTransitionGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintMultiEntranceEntryReference val = ((value != null) ? value.m_MultiEntranceEntryRef : null);
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_MultiEntranceEntryRef")
				{
					val = reader.ReadPackable<BlueprintMultiEntranceEntryReference>();
					array[0] = true;
				}
			}
			else if (text == "m_MultiEntranceEntryRef")
			{
				reader.ReadPackable(ref val);
			}
		}
		if (value != null)
		{
			value.m_MultiEntranceEntryRef = val;
		}
		else
		{
			value = new AreaTransitionGameCommand
			{
				m_MultiEntranceEntryRef = val
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
		AreaTransitionGameCommand source = new AreaTransitionGameCommand();
		result = Unsafe.As<AreaTransitionGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AreaTransitionGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_MultiEntranceEntryRef", ref m_MultiEntranceEntryRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaTransitionGameCommand>();
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
				m_MultiEntranceEntryRef = formatter.ReadPackable<BlueprintMultiEntranceEntryReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
