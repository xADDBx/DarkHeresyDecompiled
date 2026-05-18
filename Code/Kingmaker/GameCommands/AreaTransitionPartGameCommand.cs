using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.MapObjects;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class AreaTransitionPartGameCommand : GameCommandWithSynchronized, IMemoryPackable<AreaTransitionPartGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<AreaTransitionPartGameCommand>
{
	public class TransitionExecutorEntity : ContextData<TransitionExecutorEntity>
	{
		public EntityRef<BaseUnitEntity> EntityRef { get; private set; }

		public TransitionExecutorEntity Setup(EntityRef<BaseUnitEntity> entityRef)
		{
			EntityRef = entityRef;
			return this;
		}

		protected override void Reset()
		{
			EntityRef = null;
		}
	}

	[Preserve]
	private sealed class AreaTransitionPartGameCommandFormatter : MemoryPackFormatter<AreaTransitionPartGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AreaTransitionPartGameCommand value)
		{
			AreaTransitionPartGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref AreaTransitionPartGameCommand value)
		{
			AreaTransitionPartGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AreaTransitionPartGameCommand value)
		{
			AreaTransitionPartGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref AreaTransitionPartGameCommand value)
		{
			AreaTransitionPartGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityPartRef<Entity, AreaTransitionPart> m_AreaTransitionPartRef;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_ExecutorEntity;

	public static readonly TypeInfo OwlPackTypeInfo;

	[JsonConstructor]
	public AreaTransitionPartGameCommand()
	{
	}

	[MemoryPackConstructor]
	private AreaTransitionPartGameCommand(EntityPartRef<Entity, AreaTransitionPart> m_areaTransitionPartRef, EntityRef<BaseUnitEntity> m_executorEntity)
	{
		m_AreaTransitionPartRef = m_areaTransitionPartRef;
		m_ExecutorEntity = m_executorEntity;
	}

	public AreaTransitionPartGameCommand([NotNull] AreaTransitionPart areaTransitionPart, bool isPlayerCommand, BaseUnitEntity executorEntity)
		: this(areaTransitionPart, executorEntity)
	{
		m_IsSynchronized = isPlayerCommand;
	}

	protected override void ExecuteInternal()
	{
		AreaTransitionPart entityPart = m_AreaTransitionPartRef.EntityPart;
		if (entityPart == null)
		{
			return;
		}
		if (Game.Instance.Player.IsInCombat)
		{
			PFLog.GameCommands.Log("[AreaTransitionPartGameCommand] Canceled due to IsInCombat=true");
			return;
		}
		if (Game.Instance.CurrentModeType == GameModeType.Dialog)
		{
			PFLog.GameCommands.Log("[AreaTransitionPartGameCommand] Canceled due to CurrentMode=Dialog");
			return;
		}
		if (Game.Instance.CurrentModeType == GameModeType.Cutscene)
		{
			PFLog.GameCommands.Log("[AreaTransitionPartGameCommand] Canceled due to CurrentMode=Cutscene");
			return;
		}
		if (Game.Instance.IsPaused)
		{
			Game.Instance.IsPaused = false;
		}
		BaseUnitEntity user = Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity u) => u.IsDirectlyControllable).FirstOrDefault((BaseUnitEntity u) => !u.IsPet);
		if (!entityPart.CheckRestrictions(user))
		{
			return;
		}
		ConditionAction conditionAction = entityPart.Blueprint?.Actions.FirstOrDefault((ConditionAction ca) => ca.Condition?.Check() ?? true);
		if (conditionAction != null)
		{
			using (ContextData<TransitionExecutorEntity>.Request().Setup(m_ExecutorEntity))
			{
				conditionAction.Actions.Run();
				return;
			}
		}
		BlueprintArea currentArea = Game.Instance.CurrentlyLoadedArea;
		BlueprintAreaEnterPoint targetEnterPoint = entityPart.AreaEnterPoint;
		EventBus.RaiseEvent(delegate(IPartyLeaveAreaHandler h)
		{
			h.HandlePartyLeaveArea(currentArea, targetEnterPoint);
		});
		Game.Instance.LoadArea(targetEnterPoint, entityPart.Settings.AutoSaveMode);
	}

	static AreaTransitionPartGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "AreaTransitionPartGameCommand",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("m_AreaTransitionPartRef", typeof(EntityPartRef<Entity, AreaTransitionPart>)),
				new FieldInfo("m_ExecutorEntity", typeof(EntityRef<BaseUnitEntity>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AreaTransitionPartGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AreaTransitionPartGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AreaTransitionPartGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AreaTransitionPartGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref AreaTransitionPartGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_AreaTransitionPartRef);
		writer.WritePackable(in value.m_ExecutorEntity);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AreaTransitionPartGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityPartRef<Entity, AreaTransitionPart> value2;
		EntityRef<BaseUnitEntity> value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityPartRef<Entity, AreaTransitionPart>>();
				value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			}
			else
			{
				value2 = value.m_AreaTransitionPartRef;
				value3 = value.m_ExecutorEntity;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AreaTransitionPartGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityPartRef<Entity, AreaTransitionPart>);
				value3 = default(EntityRef<BaseUnitEntity>);
			}
			else
			{
				value2 = value.m_AreaTransitionPartRef;
				value3 = value.m_ExecutorEntity;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new AreaTransitionPartGameCommand(value2, value3);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref AreaTransitionPartGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_AreaTransitionPartRef");
		writer.WritePackable(value.m_AreaTransitionPartRef);
		writer.WriteProperty("m_ExecutorEntity");
		writer.WritePackable(value.m_ExecutorEntity);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref AreaTransitionPartGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		EntityPartRef<Entity, AreaTransitionPart> val;
		EntityRef<BaseUnitEntity> val2;
		if (value == null)
		{
			val = default(EntityPartRef<Entity, AreaTransitionPart>);
			val2 = default(EntityRef<BaseUnitEntity>);
		}
		else
		{
			val = value.m_AreaTransitionPartRef;
			val2 = value.m_ExecutorEntity;
		}
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				if (!(text == "m_AreaTransitionPartRef"))
				{
					if (text == "m_ExecutorEntity")
					{
						val2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
						array[1] = true;
					}
				}
				else
				{
					val = reader.ReadPackable<EntityPartRef<Entity, AreaTransitionPart>>();
					array[0] = true;
				}
			}
			else if (!(text == "m_AreaTransitionPartRef"))
			{
				if (text == "m_ExecutorEntity")
				{
					reader.ReadPackable(ref val2);
				}
			}
			else
			{
				reader.ReadPackable(ref val);
			}
		}
		_ = value;
		value = new AreaTransitionPartGameCommand(val, val2);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AreaTransitionPartGameCommand source = new AreaTransitionPartGameCommand();
		result = Unsafe.As<AreaTransitionPartGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AreaTransitionPartGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityPartRef<Entity, AreaTransitionPart> value = m_AreaTransitionPartRef;
		formatter.Field(0, "m_AreaTransitionPartRef", ref value, state);
		EntityRef<BaseUnitEntity> value2 = m_ExecutorEntity;
		formatter.Field(1, "m_ExecutorEntity", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaTransitionPartGameCommand>();
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
				Unsafe.AsRef(in m_AreaTransitionPartRef) = formatter.ReadPackable<EntityPartRef<Entity, AreaTransitionPart>>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_ExecutorEntity) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
