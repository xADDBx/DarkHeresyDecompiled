using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("7c3693f332324ec4f94935a796c848f9")]
public class WarhammerOverrideGroupCooldown : UnitBuffComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public int Charges = -1;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("Charges", typeof(int))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref Charges);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ComponentData source = new ComponentData();
			result = Unsafe.As<ComponentData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ComponentData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.UnmanagedField(0, "Charges", ref Charges, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ComponentData>();
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
					Charges = formatter.ReadUnmanaged<int>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	private enum OverrideStrategyType
	{
		Override,
		Increase,
		Reduce
	}

	[SerializeField]
	private BlueprintAbilityGroupReference m_AffectedGroup;

	[SerializeField]
	private OverrideStrategyType m_OverrideStrategy;

	[SerializeField]
	[ShowIf("m_IsOverrideStrategy")]
	private bool m_Infinite;

	[SerializeField]
	[HideIf("m_Infinite")]
	private int m_Value;

	public bool LimitedCharges;

	[ShowIf("LimitedCharges")]
	public ContextValue Charges = -1;

	public bool CostRestricted;

	[ShowIf("CostRestricted")]
	public int MaxActionPointsCost = -1;

	public bool RefundActionPointCost;

	public ActionList ActionsOnOverride;

	[ShowIf("LimitedCharges")]
	public ActionList ActionsAfterCharges;

	public bool ForbidOtherAbilities;

	[SerializeField]
	private BlueprintAbilityGroupReference m_FilterGroup;

	public bool OnlyChosenWeapon;

	public bool OnlyCheapestAbilities;

	public BlueprintAbilityGroup AffectedGroup => m_AffectedGroup?.Get();

	private bool m_IsOverrideStrategy => m_OverrideStrategy == OverrideStrategyType.Override;

	public BlueprintAbilityGroup FilterGroup => m_FilterGroup;
}
