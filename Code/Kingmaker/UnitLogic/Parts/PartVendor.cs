using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartVendor : PartItemsCollection, IHashable, IOwlPackable<PartVendor>
{
	[JsonProperty]
	[OwlPackInclude]
	private BlueprintSharedVendorTable m_VendorTable;

	[JsonProperty]
	[OwlPackInclude]
	private BlueprintVendorFaction m_VendorFaction;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartVendor",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("CollectionConverter", typeof(ItemsCollection)),
			new FieldInfo("m_VendorTable", typeof(BlueprintSharedVendorTable)),
			new FieldInfo("m_VendorFaction", typeof(BlueprintVendorFaction))
		}
	};

	public BlueprintSharedVendorTable VendorTable => m_VendorTable;

	public FactionType FactionType => Faction.FactionType;

	public BlueprintVendorFaction Faction => m_VendorFaction;

	public bool AutoIdentifyPlayersInventory => m_VendorTable?.AutoIdentifyAllItems ?? false;

	[Obsolete]
	public int GetCurrentFactionReputationPoints()
	{
		return 0;
	}

	public bool IsLockedByReputation(ItemEntity item)
	{
		VendorTableSlot vendorSlot = item.GetVendorSlot();
		if (vendorSlot == null)
		{
			return false;
		}
		return !vendorSlot.ReputationRestriction.IsPassed(FactionType);
	}

	[Obsolete]
	public int GetReputationToUnlock(ItemEntity item)
	{
		return item.GetVendorSlot()?.ReputationRestriction.Value ?? 0;
	}

	protected override ItemsCollection SetupInternal(ItemsCollection currentCollection)
	{
		if (m_VendorTable != null)
		{
			return Game.Instance.Player.VendorsManager.GetSharedCollection(m_VendorTable);
		}
		return currentCollection ?? new ItemsCollection(base.ConcreteOwner)
		{
			ForceStackable = true,
			IsVendorTable = true
		};
	}

	public void SetSharedInventory(BlueprintSharedVendorTable table)
	{
		m_VendorTable = table;
		Setup();
	}

	public void SetVendorFaction(BlueprintVendorFaction vendorFaction)
	{
		m_VendorFaction = vendorFaction.Reference();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(m_VendorTable);
		result.Append(ref val2);
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(m_VendorFaction);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartVendor source = new PartVendor();
		result = Unsafe.As<PartVendor, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartVendor>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		ItemsCollection value = base.CollectionConverter;
		formatter.Field(0, "CollectionConverter", ref value, state);
		formatter.Field(1, "m_VendorTable", ref m_VendorTable, state);
		formatter.Field(2, "m_VendorFaction", ref m_VendorFaction, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartVendor>();
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
				base.CollectionConverter = formatter.ReadPackable<ItemsCollection>(state);
				break;
			case 1:
				m_VendorTable = formatter.ReadPackable<BlueprintSharedVendorTable>(state);
				break;
			case 2:
				m_VendorFaction = formatter.ReadPackable<BlueprintVendorFaction>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
