using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class FinishRespecGameCommand : GameCommand, IMemoryPackable<FinishRespecGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<FinishRespecGameCommand>
{
	[Preserve]
	private sealed class FinishRespecGameCommandFormatter : MemoryPackFormatter<FinishRespecGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref FinishRespecGameCommand value)
		{
			FinishRespecGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref FinishRespecGameCommand value)
		{
			FinishRespecGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref FinishRespecGameCommand value)
		{
			FinishRespecGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref FinishRespecGameCommand value)
		{
			FinishRespecGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_RespecEntity;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly bool m_ForFree;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public FinishRespecGameCommand()
	{
	}

	[MemoryPackConstructor]
	private FinishRespecGameCommand(EntityRef<BaseUnitEntity> m_respecEntity, bool m_forfree)
	{
		m_RespecEntity = m_respecEntity;
		m_ForFree = m_forfree;
	}

	public FinishRespecGameCommand(BaseUnitEntity respecEntity, bool forFree)
		: this((EntityRef<BaseUnitEntity>)respecEntity, forFree)
	{
	}

	protected override void ExecuteInternal()
	{
		_ = Game.Instance.Player;
		PartUnitProgression progression = m_RespecEntity.Entity.Progression;
		progression.Respec();
		if (!m_ForFree)
		{
			progression.CountRespecIn();
		}
		Game.Instance.AdvanceGameTime(1.Days());
	}

	static FinishRespecGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "FinishRespecGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_RespecEntity", typeof(EntityRef<BaseUnitEntity>)),
				new FieldInfo("m_ForFree", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<FinishRespecGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new FinishRespecGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<FinishRespecGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<FinishRespecGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref FinishRespecGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_RespecEntity);
		writer.WriteUnmanaged(in value.m_ForFree);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref FinishRespecGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity> value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				reader.ReadUnmanaged<bool>(out value3);
			}
			else
			{
				value2 = value.m_RespecEntity;
				value3 = value.m_ForFree;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<bool>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(FinishRespecGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<BaseUnitEntity>);
				value3 = false;
			}
			else
			{
				value2 = value.m_RespecEntity;
				value3 = value.m_ForFree;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new FinishRespecGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref FinishRespecGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_RespecEntity");
		writer.WritePackable(value.m_RespecEntity);
		writer.WriteProperty("m_ForFree");
		writer.WriteUnmanaged(value.m_ForFree);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref FinishRespecGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<BaseUnitEntity> val;
		bool v;
		if (value == null)
		{
			val = default(EntityRef<BaseUnitEntity>);
			v = false;
		}
		else
		{
			val = value.m_RespecEntity;
			v = value.m_ForFree;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_RespecEntity"))
				{
					if (text == "m_ForFree")
					{
						reader.ReadUnmanaged<bool>(out v);
						array[1] = true;
					}
				}
				else
				{
					val = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
					array[0] = true;
				}
			}
			else if (!(text == "m_RespecEntity"))
			{
				if (text == "m_ForFree")
				{
					reader.ReadUnmanaged<bool>(out v);
				}
			}
			else
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new FinishRespecGameCommand(val, v);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FinishRespecGameCommand source = new FinishRespecGameCommand();
		result = Unsafe.As<FinishRespecGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FinishRespecGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<BaseUnitEntity> value = m_RespecEntity;
		formatter.Field(0, "m_RespecEntity", ref value, state);
		bool value2 = m_ForFree;
		formatter.UnmanagedField(1, "m_ForFree", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FinishRespecGameCommand>();
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
				Unsafe.AsRef(in m_RespecEntity) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_ForFree) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
