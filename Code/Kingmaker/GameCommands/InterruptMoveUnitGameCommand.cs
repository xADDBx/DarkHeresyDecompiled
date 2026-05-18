using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class InterruptMoveUnitGameCommand : GameCommand, IMemoryPackable<InterruptMoveUnitGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<InterruptMoveUnitGameCommand>
{
	[Preserve]
	private sealed class InterruptMoveUnitGameCommandFormatter : MemoryPackFormatter<InterruptMoveUnitGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref InterruptMoveUnitGameCommand value)
		{
			InterruptMoveUnitGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref InterruptMoveUnitGameCommand value)
		{
			InterruptMoveUnitGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref InterruptMoveUnitGameCommand value)
		{
			InterruptMoveUnitGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref InterruptMoveUnitGameCommand value)
		{
			InterruptMoveUnitGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly UnitReference m_UnitRef;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private InterruptMoveUnitGameCommand(UnitReference m_unitRef)
	{
		m_UnitRef = m_unitRef;
	}

	private InterruptMoveUnitGameCommand(OwlPackConstructorParameter _)
	{
	}

	public InterruptMoveUnitGameCommand([NotNull] AbstractUnitEntity unit)
		: this(unit.FromAbstractUnitEntity())
	{
	}

	protected override void ExecuteInternal()
	{
		AbstractUnitEntity abstractUnitEntity = m_UnitRef.ToAbstractUnitEntity();
		if (abstractUnitEntity == null)
		{
			PFLog.GameCommands.Error("Unit '{0}' not found!", m_UnitRef.Id);
		}
		else
		{
			abstractUnitEntity.GetOptional<PartUnitCommands>()?.ForceInterruptMove();
		}
	}

	static InterruptMoveUnitGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "InterruptMoveUnitGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_UnitRef", typeof(UnitReference))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<InterruptMoveUnitGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new InterruptMoveUnitGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<InterruptMoveUnitGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<InterruptMoveUnitGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref InterruptMoveUnitGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
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
	public static void Deserialize(ref MemoryPackReader reader, ref InterruptMoveUnitGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		UnitReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<UnitReference>();
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(InterruptMoveUnitGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_UnitRef : default(UnitReference));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new InterruptMoveUnitGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref InterruptMoveUnitGameCommand? value)
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
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref InterruptMoveUnitGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		UnitReference val = ((value != null) ? value.m_UnitRef : default(UnitReference));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "m_UnitRef")
				{
					val = reader.ReadPackable<UnitReference>();
					array[0] = true;
				}
			}
			else if (text == "m_UnitRef")
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new InterruptMoveUnitGameCommand(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InterruptMoveUnitGameCommand source = new InterruptMoveUnitGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<InterruptMoveUnitGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<InterruptMoveUnitGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		UnitReference value = m_UnitRef;
		formatter.Field(0, "m_UnitRef", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InterruptMoveUnitGameCommand>();
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
				Unsafe.AsRef(in m_UnitRef) = formatter.ReadPackable<UnitReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
