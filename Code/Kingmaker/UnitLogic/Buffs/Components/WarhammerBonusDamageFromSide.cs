using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Gameplay.Features.Weakpoints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[TypeId("b821b468558e4e41beaa8bf08f9178b4")]
public class WarhammerBonusDamageFromSide : UnitBuffComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Data : IEntityFactComponentSavableData, IHashable, IOwlPackable<Data>
	{
		[JsonProperty]
		[OwlPackInclude]
		public WeakpointSide ChosenSide;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Data",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("ChosenSide", typeof(WeakpointSide))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref ChosenSide);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			Data source = new Data();
			result = Unsafe.As<Data, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<Data>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.EnumField(0, "ChosenSide", ref ChosenSide, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Data>();
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
					ChosenSide = formatter.ReadEnum<WeakpointSide>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public bool IsRandomSide;

	[HideIf("IsRandomSide")]
	public bool ClosestToCasterSide;

	[HideIf("IsRandomSide")]
	public WeakpointSide Side;

	[SerializeField]
	[HideIf("IsRandomSide")]
	private bool OtherSidesButSelected;

	public int BonusDamagePercent;

	public ContextValue PercentValue;

	public ContextValue FlatValue;

	public ContextValue UnmodifiableFlatValue;

	public ContextValueModifierWithType Modifier;

	public bool CreateOppositeOnProk;

	public bool DestroyOnProk;

	public bool CanStack;

	public GameObject BonusDamageMarker;

	public ConditionsChecker ApplyConditions;

	public ActionList ActionsOnAttackInitiator;

	public ActionList ActionsOnAttackTarget;

	public ActionList ActionsOnNotAttackInitiator;

	public ActionList ActionsOnNotAttackTarget;

	[SerializeField]
	private BlueprintUnitFactReference m_MarkedForDestruction;

	[SerializeField]
	private BlueprintAbilityGroupReference m_DoTAbilityGroup;

	public BlueprintUnitFact MarkedForDestruction => m_MarkedForDestruction?.Get();

	public BlueprintAbilityGroup DoTAbilityGroup => m_DoTAbilityGroup?.Get();
}
