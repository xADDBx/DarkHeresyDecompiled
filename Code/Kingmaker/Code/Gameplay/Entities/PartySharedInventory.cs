using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public class PartySharedInventory : Entity, IHashable, IOwlPackable<PartySharedInventory>
{
	public const string ID = "party-shared-inventory-id";

	public new static readonly EntityRef<PartySharedInventory> Ref = new EntityRef<PartySharedInventory>("party-shared-inventory-id");

	[JsonProperty]
	[OwlPackInclude]
	private ItemsCollection m_Collection;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartySharedInventory",
		OldNames = null,
		Fields = new FieldInfo[11]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_Collection", typeof(ItemsCollection))
		}
	};

	[NotNull]
	public ItemsCollection Collection => m_Collection;

	public override bool NeedsView => false;

	public PartySharedInventory()
		: base("party-shared-inventory-id", isInGame: true)
	{
		m_Collection = new ItemsCollection(this);
	}

	[JsonConstructor]
	public PartySharedInventory(JsonConstructorMark _)
		: base(_)
	{
	}

	public PartySharedInventory(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	protected override void OnPrePostLoad()
	{
		m_Collection.PrePostLoad();
	}

	protected override void OnPostLoad()
	{
		m_Collection.PostLoad();
	}

	protected override void OnPreSave()
	{
		m_Collection.PreSave();
	}

	protected override void OnSubscribe()
	{
		m_Collection.Subscribe();
	}

	protected override void OnUnsubscribe()
	{
		m_Collection.Unsubscribe();
	}

	protected override void OnDispose()
	{
		m_Collection.Dispose();
	}

	protected override void OnApplyPostLoadFixes()
	{
		m_Collection.ApplyPostLoadFixes();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<ItemsCollection>.GetHash128(m_Collection);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartySharedInventory source = new PartySharedInventory(default(OwlPackConstructorParameter));
		result = Unsafe.As<PartySharedInventory, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartySharedInventory>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "m_Collection", ref m_Collection, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartySharedInventory>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				m_Collection = formatter.ReadPackable<ItemsCollection>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
