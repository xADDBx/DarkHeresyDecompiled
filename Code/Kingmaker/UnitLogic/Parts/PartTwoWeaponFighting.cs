using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartTwoWeaponFighting : UnitPart, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>, IInterruptTurnStartHandler<EntitySubscriber>, IInterruptTurnStartHandler, IEventTag<IInterruptTurnStartHandler, EntitySubscriber>, IUnitCommandEndHandler, IHashable, IOwlPackable<PartTwoWeaponFighting>
{
	private static BlueprintAbilityGroup PrimaryHandAbilityGroup;

	private static BlueprintAbilityGroup SecondaryHandAbilityGroup;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsAttackedThisTurn;

	public static readonly TypeInfo OwlPackTypeInfo;

	public bool EnableAttackWithPairedWeapon { get; set; }

	public bool IsAttackedThisTurn => m_IsAttackedThisTurn;

	static PartTwoWeaponFighting()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PartTwoWeaponFighting",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("m_IsAttackedThisTurn", typeof(bool))
			}
		};
		BlueprintCombatRoot combatRoot = ConfigRoot.Instance.CombatRoot;
		PrimaryHandAbilityGroup = combatRoot.PrimaryHandAbilityGroup.Get();
		SecondaryHandAbilityGroup = combatRoot.SecondaryHandAbilityGroup.Get();
	}

	public bool IsOtherAbilityGroupOnCooldown(AbilityData abilityData)
	{
		if (abilityData.Weapon == null)
		{
			return false;
		}
		if (GetCroupCooldown(SecondaryHandAbilityGroup) > 0 && abilityData.Caster.GetFirstWeapon() == abilityData.Weapon)
		{
			return true;
		}
		if (GetCroupCooldown(PrimaryHandAbilityGroup) > 0 && abilityData.Caster.GetSecondWeapon() == abilityData.Weapon)
		{
			return true;
		}
		return false;
	}

	private int GetCroupCooldown(BlueprintAbilityGroup group)
	{
		return base.Owner.GetAbilityCooldownsOptional()?.GroupCooldown(group) ?? 0;
	}

	public void HandleGroupCooldownRemoved(BlueprintAbilityGroup group)
	{
		BlueprintCombatRoot combatRoot = ConfigRoot.Instance.CombatRoot;
		if (group == combatRoot.PrimaryHandAbilityGroup.Get() || group == combatRoot.SecondaryHandAbilityGroup.Get())
		{
			UpdateIsAttackedThisTurn();
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command is UnitUseAbility unitUseAbility && unitUseAbility.Executor == base.Owner)
		{
			UpdateIsAttackedThisTurn();
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		ResetAttacks();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		ResetAttacks();
	}

	public void ResetAttacks()
	{
		m_IsAttackedThisTurn = false;
	}

	private void UpdateIsAttackedThisTurn()
	{
		m_IsAttackedThisTurn = base.Owner.AbilityCooldowns.GroupIsOnCooldown(PrimaryHandAbilityGroup) || base.Owner.AbilityCooldowns.GroupIsOnCooldown(SecondaryHandAbilityGroup);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_IsAttackedThisTurn);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartTwoWeaponFighting source = new PartTwoWeaponFighting();
		result = Unsafe.As<PartTwoWeaponFighting, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartTwoWeaponFighting>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_IsAttackedThisTurn", ref m_IsAttackedThisTurn, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartTwoWeaponFighting>();
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
				m_IsAttackedThisTurn = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
