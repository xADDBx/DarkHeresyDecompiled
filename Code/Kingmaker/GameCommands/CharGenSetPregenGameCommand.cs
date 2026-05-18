using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetPregenGameCommand : GameCommand, IMemoryPackable<CharGenSetPregenGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CharGenSetPregenGameCommand>
{
	[Preserve]
	private sealed class CharGenSetPregenGameCommandFormatter : MemoryPackFormatter<CharGenSetPregenGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetPregenGameCommand value)
		{
			CharGenSetPregenGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetPregenGameCommand value)
		{
			CharGenSetPregenGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetPregenGameCommand value)
		{
			CharGenSetPregenGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetPregenGameCommand value)
		{
			CharGenSetPregenGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_UnitRef;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSetPregenGameCommand(EntityRef<BaseUnitEntity> m_unitRef)
	{
		m_UnitRef = m_unitRef;
	}

	private CharGenSetPregenGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenSetPregenGameCommand([CanBeNull] BaseUnitEntity unit)
		: this((EntityRef<BaseUnitEntity>)unit)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenPregenHandler h)
		{
			h.HandleSetPregen(m_UnitRef.Entity);
		});
	}

	static CharGenSetPregenGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CharGenSetPregenGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_UnitRef", typeof(EntityRef<BaseUnitEntity>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPregenGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetPregenGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPregenGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetPregenGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CharGenSetPregenGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_UnitRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetPregenGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity> value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			}
			else
			{
				value2 = value.m_UnitRef;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetPregenGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_UnitRef : default(EntityRef<BaseUnitEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenSetPregenGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CharGenSetPregenGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_UnitRef");
		writer.WritePackable(value.m_UnitRef);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CharGenSetPregenGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<BaseUnitEntity> val = ((value != null) ? value.m_UnitRef : default(EntityRef<BaseUnitEntity>));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_UnitRef")
				{
					val = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
					array[0] = true;
				}
			}
			else if (text == "m_UnitRef")
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new CharGenSetPregenGameCommand(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetPregenGameCommand source = new CharGenSetPregenGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetPregenGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetPregenGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<BaseUnitEntity> value = m_UnitRef;
		formatter.Field(0, "m_UnitRef", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetPregenGameCommand>();
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
				Unsafe.AsRef(in m_UnitRef) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
