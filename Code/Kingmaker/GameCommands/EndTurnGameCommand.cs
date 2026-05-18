using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class EndTurnGameCommand : GameCommand, IMemoryPackable<EndTurnGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<EndTurnGameCommand>
{
	[Preserve]
	private sealed class EndTurnGameCommandFormatter : MemoryPackFormatter<EndTurnGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EndTurnGameCommand value)
		{
			EndTurnGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref EndTurnGameCommand value)
		{
			EndTurnGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EndTurnGameCommand value)
		{
			EndTurnGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref EndTurnGameCommand value)
		{
			EndTurnGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	public readonly EntityRef<MechanicEntity> MechanicEntity;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private EndTurnGameCommand(EntityRef<MechanicEntity> mechanicEntity)
	{
		MechanicEntity = mechanicEntity;
	}

	private EndTurnGameCommand(OwlPackConstructorParameter _)
	{
	}

	public EndTurnGameCommand([NotNull] MechanicEntity mechanicEntity)
		: this((EntityRef<MechanicEntity>)mechanicEntity)
	{
		if (mechanicEntity == null)
		{
			throw new ArgumentNullException("mechanicEntity");
		}
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Controllers.TurnController.TryEndPlayerTurn(MechanicEntity);
	}

	static EndTurnGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "EndTurnGameCommand",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("MechanicEntity", typeof(EntityRef<MechanicEntity>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EndTurnGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new EndTurnGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EndTurnGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EndTurnGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref EndTurnGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.MechanicEntity);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EndTurnGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<MechanicEntity> value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<MechanicEntity>>();
			}
			else
			{
				value2 = value.MechanicEntity;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EndTurnGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.MechanicEntity : default(EntityRef<MechanicEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new EndTurnGameCommand(value2);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref EndTurnGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("MechanicEntity");
		writer.WritePackable(value.MechanicEntity);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref EndTurnGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<MechanicEntity> val = ((value != null) ? value.MechanicEntity : default(EntityRef<MechanicEntity>));
		bool[] array = new bool[1];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (text == "MechanicEntity")
				{
					val = reader.ReadPackable<EntityRef<MechanicEntity>>();
					array[0] = true;
				}
			}
			else if (text == "MechanicEntity")
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new EndTurnGameCommand(val);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EndTurnGameCommand source = new EndTurnGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<EndTurnGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EndTurnGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<MechanicEntity> value = MechanicEntity;
		formatter.Field(0, "MechanicEntity", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EndTurnGameCommand>();
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
				Unsafe.AsRef(in MechanicEntity) = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
