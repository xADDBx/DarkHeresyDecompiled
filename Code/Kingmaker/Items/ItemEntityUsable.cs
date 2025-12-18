using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items;

[OwlPackable(OwlPackableMode.Generate)]
public class ItemEntityUsable : ItemEntity<BlueprintItemEquipmentUsable>, IHashable, IOwlPackable<ItemEntityUsable>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ItemEntityUsable",
		OldNames = null,
		Fields = new FieldInfo[28]
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
			new FieldInfo("m_Initiative", typeof(Initiative)),
			new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("MainFact", typeof(MechanicEntityFact)),
			new FieldInfo("m_Count", typeof(int)),
			new FieldInfo("m_InventorySlotIndex", typeof(int)),
			new FieldInfo("m_FactsAppliedToWielder", typeof(EntityFact[])),
			new FieldInfo("m_SkinningSuccessful", typeof(bool)),
			new FieldInfo("m_WielderRef", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("m_IdentifyRolls", typeof(List<IdentifyRollData>)),
			new FieldInfo("m_NotLootable", typeof(bool)),
			new FieldInfo("Time", typeof(TimeSpan)),
			new FieldInfo("Charges", typeof(int)),
			new FieldInfo("IsIdentified", typeof(bool)),
			new FieldInfo("SellTime", typeof(TimeSpan?)),
			new FieldInfo("OriginArea", typeof(BlueprintArea)),
			new FieldInfo("VendorBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("IsNonRemovable", typeof(bool))
		}
	};

	protected override bool RemoveFromSlotWhenNoCharges => base.Blueprint.RemoveFromSlotWhenNoCharges;

	public ItemEntityUsable([NotNull] BlueprintItemEquipmentUsable bpItem)
		: base(bpItem)
	{
	}

	public ItemEntityUsable(JsonConstructorMark _)
		: base(_)
	{
	}

	protected ItemEntityUsable()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ItemEntityUsable source = new ItemEntityUsable();
		result = Unsafe.As<ItemEntityUsable, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ItemEntityUsable>(OwlPackTypeInfo);
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
		formatter.Field(10, "m_Initiative", ref m_Initiative, state);
		formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
		formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
		MechanicEntityFact value2 = base.MainFact;
		formatter.Field(13, "MainFact", ref value2, state);
		formatter.UnmanagedField(14, "m_Count", ref m_Count, state);
		formatter.UnmanagedField(15, "m_InventorySlotIndex", ref m_InventorySlotIndex, state);
		formatter.Field(16, "m_FactsAppliedToWielder", ref m_FactsAppliedToWielder, state);
		formatter.UnmanagedField(17, "m_SkinningSuccessful", ref m_SkinningSuccessful, state);
		formatter.Field(18, "m_WielderRef", ref m_WielderRef, state);
		List<IdentifyRollData> value3 = base.m_IdentifyRolls;
		formatter.Field(19, "m_IdentifyRolls", ref value3, state);
		formatter.UnmanagedField(20, "m_NotLootable", ref m_NotLootable, state);
		TimeSpan value4 = base.Time;
		formatter.Field(21, "Time", ref value4, state);
		int value5 = base.Charges;
		formatter.UnmanagedField(22, "Charges", ref value5, state);
		bool value6 = base.IsIdentified;
		formatter.UnmanagedField(23, "IsIdentified", ref value6, state);
		TimeSpan? value7 = base.SellTime;
		formatter.NullableField(24, "SellTime", ref value7, state);
		BlueprintArea value8 = base.OriginArea;
		formatter.Field(25, "OriginArea", ref value8, state);
		BlueprintMechanicEntityFact value9 = base.VendorBlueprint;
		formatter.Field(26, "VendorBlueprint", ref value9, state);
		bool value10 = base.IsNonRemovable;
		formatter.UnmanagedField(27, "IsNonRemovable", ref value10, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ItemEntityUsable>();
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
				m_Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 11:
				m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 12:
				m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 13:
				base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
				break;
			case 14:
				m_Count = formatter.ReadUnmanaged<int>(state);
				break;
			case 15:
				m_InventorySlotIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 16:
				m_FactsAppliedToWielder = formatter.ReadPackable<EntityFact[]>(state);
				break;
			case 17:
				m_SkinningSuccessful = formatter.ReadUnmanaged<bool>(state);
				break;
			case 18:
				m_WielderRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 19:
				base.m_IdentifyRolls = formatter.ReadPackable<List<IdentifyRollData>>(state);
				break;
			case 20:
				m_NotLootable = formatter.ReadUnmanaged<bool>(state);
				break;
			case 21:
				base.Time = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 22:
				base.Charges = formatter.ReadUnmanaged<int>(state);
				break;
			case 23:
				base.IsIdentified = formatter.ReadUnmanaged<bool>(state);
				break;
			case 24:
				base.SellTime = formatter.ReadNullablePackable<TimeSpan>(state);
				break;
			case 25:
				base.OriginArea = formatter.ReadPackable<BlueprintArea>(state);
				break;
			case 26:
				base.VendorBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 27:
				base.IsNonRemovable = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
