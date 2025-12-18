using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class TriggerLootGameCommand : GameCommandWithSynchronized, IOwlPackable<TriggerLootGameCommand>
{
	public enum TriggerType : byte
	{
		None,
		Put,
		Take,
		Close
	}

	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityPartRef<Entity, InteractionLootPart> m_InteractionLootPartRef;

	[JsonProperty]
	[OwlPackInclude]
	private readonly byte m_Type;

	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityRef<ItemEntity> m_Item;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "TriggerLootGameCommand",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_InteractionLootPartRef", typeof(EntityPartRef<Entity, InteractionLootPart>)),
			new FieldInfo("m_Type", typeof(byte)),
			new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>))
		}
	};

	[JsonConstructor]
	private TriggerLootGameCommand()
	{
	}

	private TriggerLootGameCommand(EntityPartRef<Entity, InteractionLootPart> m_interactionLootPartRef, byte m_type, EntityRef<ItemEntity> m_item)
	{
		m_InteractionLootPartRef = m_interactionLootPartRef;
		m_Type = m_type;
		m_Item = m_item;
	}

	public TriggerLootGameCommand(InteractionLootPart interactionLootPart, TriggerType type, ItemEntity item)
		: this(interactionLootPart, (byte)type, item)
	{
		m_IsSynchronized = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
	}

	protected override void ExecuteInternal()
	{
		InteractionLootPart entityPart = m_InteractionLootPartRef.EntityPart;
		if (entityPart != null)
		{
			switch ((TriggerType)m_Type)
			{
			case TriggerType.Put:
				entityPart.HandleItemsAddedImplementation(m_Item.Entity);
				break;
			case TriggerType.Take:
				entityPart.HandleItemsRemovedImplementation(m_Item.Entity);
				break;
			case TriggerType.Close:
				entityPart.OnLootClosedImplementation();
				break;
			default:
				throw new ArgumentOutOfRangeException(string.Format("{0}={1}", "m_Type", m_Type));
			}
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TriggerLootGameCommand source = new TriggerLootGameCommand();
		result = Unsafe.As<TriggerLootGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<TriggerLootGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityPartRef<Entity, InteractionLootPart> value = m_InteractionLootPartRef;
		formatter.Field(0, "m_InteractionLootPartRef", ref value, state);
		byte value2 = m_Type;
		formatter.UnmanagedField(1, "m_Type", ref value2, state);
		EntityRef<ItemEntity> value3 = m_Item;
		formatter.Field(2, "m_Item", ref value3, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TriggerLootGameCommand>();
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
				Unsafe.AsRef(in m_InteractionLootPartRef) = formatter.ReadPackable<EntityPartRef<Entity, InteractionLootPart>>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Type) = formatter.ReadUnmanaged<byte>(state);
				break;
			case 2:
				Unsafe.AsRef(in m_Item) = formatter.ReadPackable<EntityRef<ItemEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
