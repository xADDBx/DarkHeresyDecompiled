using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class ItemsCollectionRef : IOwlPackable, IOwlPackable<ItemsCollectionRef>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef m_OwnerRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ItemsCollectionRef",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_OwnerRef", typeof(EntityRef))
		}
	};

	public ItemsCollection ItemsCollection
	{
		get
		{
			if (m_OwnerRef.Entity == null)
			{
				return null;
			}
			IEntity entity = m_OwnerRef.Entity;
			if (!(entity is Player))
			{
				if (entity is PartInventory.IOwner owner)
				{
					return owner.Inventory.Collection;
				}
				return m_OwnerRef.Entity.ToEntity().GetOptional<InteractionLootPart>()?.Items;
			}
			return Game.Instance.PartySharedInventory.Collection;
		}
	}

	private ItemsCollectionRef()
	{
	}

	[JsonConstructor]
	public ItemsCollectionRef([CanBeNull] ItemsCollection collection)
	{
		m_OwnerRef = new EntityRef(collection?.Owner);
	}

	public bool Equals(ItemsCollectionRef other)
	{
		if (other != null)
		{
			return m_OwnerRef.Equals(other.m_OwnerRef);
		}
		return false;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ItemsCollectionRef source = new ItemsCollectionRef();
		result = Unsafe.As<ItemsCollectionRef, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<ItemsCollectionRef>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_OwnerRef", ref m_OwnerRef, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ItemsCollectionRef>();
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
				m_OwnerRef = formatter.ReadPackable<EntityRef>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
