using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class DrawMovePredictionGameCommand : GameCommand, IMemoryPackable<DrawMovePredictionGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<DrawMovePredictionGameCommand>
{
	[Preserve]
	private sealed class DrawMovePredictionGameCommandFormatter : MemoryPackFormatter<DrawMovePredictionGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DrawMovePredictionGameCommand value)
		{
			DrawMovePredictionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref DrawMovePredictionGameCommand value)
		{
			DrawMovePredictionGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DrawMovePredictionGameCommand value)
		{
			DrawMovePredictionGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref DrawMovePredictionGameCommand value)
		{
			DrawMovePredictionGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly ForcedPath m_Path;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly float[] m_CostPerEveryCell;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly UnitCommandParams m_UnitCommandParams;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private DrawMovePredictionGameCommand()
	{
	}

	[MemoryPackConstructor]
	private DrawMovePredictionGameCommand(EntityRef<BaseUnitEntity> m_unit, [NotNull] Path m_path, [CanBeNull] float[] m_costPerEveryCell, [CanBeNull] UnitCommandParams m_unitCommandParams)
	{
		if (m_path == null)
		{
			throw new ArgumentNullException("m_path");
		}
		m_Unit = m_unit;
		m_Path = ForcedPath.Construct(m_path);
		m_CostPerEveryCell = m_costPerEveryCell;
		m_UnitCommandParams = m_unitCommandParams;
		m_Path.Claim(this);
	}

	public DrawMovePredictionGameCommand([NotNull] BaseUnitEntity unit, [NotNull] Path path, [CanBeNull] float[] costPerEveryCell, [CanBeNull] UnitCommandParams unitCommandParams)
		: this((EntityRef<BaseUnitEntity>)unit, path, costPerEveryCell, unitCommandParams)
	{
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
	}

	protected override void ExecuteInternal()
	{
		BaseUnitEntity baseUnitEntity = m_Unit;
		if (baseUnitEntity == null)
		{
			PFLog.GameCommands.Error("Unit #" + m_Unit.Id + " not found!");
			return;
		}
		UnitHelper.DrawMovePredictionLocal(baseUnitEntity, m_Path, m_CostPerEveryCell);
		m_Path.Release(this);
		if (m_UnitCommandParams != null)
		{
			UnitCommandsRunner.SetVirtualMoveCommand(baseUnitEntity, m_UnitCommandParams);
		}
	}

	public override void AfterDeserialization()
	{
		base.AfterDeserialization();
		m_UnitCommandParams?.AfterDeserialization();
	}

	static DrawMovePredictionGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "DrawMovePredictionGameCommand",
			OldNames = null,
			Fields = new FieldInfo[4]
			{
				new FieldInfo("m_Unit", typeof(EntityRef<BaseUnitEntity>)),
				new FieldInfo("m_Path", typeof(ForcedPath)),
				new FieldInfo("m_CostPerEveryCell", typeof(float[])),
				new FieldInfo("m_UnitCommandParams", typeof(UnitCommandParams))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DrawMovePredictionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new DrawMovePredictionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DrawMovePredictionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DrawMovePredictionGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<float[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<float>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref DrawMovePredictionGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(4);
		writer.WritePackable(in value.m_Unit);
		writer.WritePackable(in value.m_Path);
		writer.WriteUnmanagedArray(value.m_CostPerEveryCell);
		writer.WriteValue(in value.m_UnitCommandParams);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DrawMovePredictionGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity> value2;
		ForcedPath value3;
		float[] value4;
		UnitCommandParams value5;
		if (memberCount == 4)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				value3 = reader.ReadPackable<ForcedPath>();
				value4 = reader.ReadUnmanagedArray<float>();
				value5 = reader.ReadValue<UnitCommandParams>();
			}
			else
			{
				value2 = value.m_Unit;
				value3 = value.m_Path;
				value4 = value.m_CostPerEveryCell;
				value5 = value.m_UnitCommandParams;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanagedArray(ref value4);
				reader.ReadValue(ref value5);
			}
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DrawMovePredictionGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<BaseUnitEntity>);
				value3 = null;
				value4 = null;
				value5 = null;
			}
			else
			{
				value2 = value.m_Unit;
				value3 = value.m_Path;
				value4 = value.m_CostPerEveryCell;
				value5 = value.m_UnitCommandParams;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanagedArray(ref value4);
						if (memberCount != 3)
						{
							reader.ReadValue(ref value5);
							_ = 4;
						}
					}
				}
			}
			_ = value;
		}
		value = new DrawMovePredictionGameCommand(value2, value3, value4, value5);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref DrawMovePredictionGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_Unit");
		writer.WritePackable(value.m_Unit);
		writer.WriteProperty("m_Path");
		writer.WritePackable(value.m_Path);
		writer.WriteProperty("m_CostPerEveryCell");
		writer.WriteArray(value.m_CostPerEveryCell);
		writer.WriteProperty("m_UnitCommandParams");
		writer.WriteValue(value.m_UnitCommandParams);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref DrawMovePredictionGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityRef<BaseUnitEntity> val;
		ForcedPath val2;
		float[] value2;
		UnitCommandParams val3;
		if (value == null)
		{
			val = default(EntityRef<BaseUnitEntity>);
			val2 = null;
			value2 = null;
			val3 = null;
		}
		else
		{
			val = value.m_Unit;
			val2 = value.m_Path;
			value2 = value.m_CostPerEveryCell;
			val3 = value.m_UnitCommandParams;
		}
		bool[] array = new bool[4];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_Unit":
					val = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
					array[0] = true;
					break;
				case "m_Path":
					val2 = reader.ReadPackable<ForcedPath>();
					array[1] = true;
					break;
				case "m_CostPerEveryCell":
					value2 = reader.ReadArray<float>();
					array[2] = true;
					break;
				case "m_UnitCommandParams":
					val3 = reader.ReadValue<UnitCommandParams>();
					array[3] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_Unit":
					reader.ReadPackable(ref val);
					break;
				case "m_Path":
					reader.ReadPackable(ref val2);
					break;
				case "m_CostPerEveryCell":
					reader.ReadArray(ref value2);
					break;
				case "m_UnitCommandParams":
					reader.ReadValue(ref val3);
					break;
				}
			}
		}
		_ = value;
		value = new DrawMovePredictionGameCommand(val, val2, value2, val3);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DrawMovePredictionGameCommand source = new DrawMovePredictionGameCommand();
		result = Unsafe.As<DrawMovePredictionGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DrawMovePredictionGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<BaseUnitEntity> value = m_Unit;
		formatter.Field(0, "m_Unit", ref value, state);
		ForcedPath value2 = m_Path;
		formatter.Field(1, "m_Path", ref value2, state);
		float[] value3 = m_CostPerEveryCell;
		formatter.Field(2, "m_CostPerEveryCell", ref value3, state);
		UnitCommandParams value4 = m_UnitCommandParams;
		formatter.Field(3, "m_UnitCommandParams", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DrawMovePredictionGameCommand>();
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
				Unsafe.AsRef(in m_Unit) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Path) = formatter.ReadPackable<ForcedPath>(state);
				break;
			case 2:
				Unsafe.AsRef(in m_CostPerEveryCell) = formatter.ReadPackable<float[]>(state);
				break;
			case 3:
				Unsafe.AsRef(in m_UnitCommandParams) = formatter.ReadPackable<UnitCommandParams>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
