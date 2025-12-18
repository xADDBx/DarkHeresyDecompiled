using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitAttackOfOpportunity : UnitUseAbilityAbstract<UnitAttackOfOpportunityParams>
{
	public static readonly HashSet<UnitAttackOfOpportunity> AllActive = new HashSet<UnitAttackOfOpportunity>();

	private ItemEntityWeapon _weapon;

	public BlueprintFact Reason => base.Params.Reason;

	public new MechanicEntity Target => base.Target?.Entity;

	public override bool ShouldBeInterrupted
	{
		get
		{
			if (base.Executor != null)
			{
				return !base.Executor.CanAttack(_weapon);
			}
			return false;
		}
	}

	public override bool IsMoveUnit => false;

	public override bool IsWaitingForAnimation
	{
		get
		{
			if (base.Executor?.AnimationManager != null && !base.Executor.AnimationManager.CanRunIdleAction())
			{
				return !base.Executor.AnimationManager.InCover;
			}
			return false;
		}
	}

	public UnitAttackOfOpportunity(UnitAttackOfOpportunityParams @params)
		: base(@params)
	{
	}

	protected override void OnInit(AbstractUnitEntity executor)
	{
		_weapon = base.Params.Weapon ?? base.Executor.GetThreatHand()?.Weapon ?? throw new Exception($"{base.Executor} can't make attack of opportunity: has no threat hand");
		BlueprintAbility blueprint = _weapon.AttackOfOpportunityAbility ?? throw new Exception($"{base.Executor} can't make attack of opportunity: weapon in threat hand doesn't have any ability for AOO");
		base.Params.AssignAbility(new AbilityData(blueprint, base.Executor)
		{
			OverrideWeapon = _weapon,
			IsAttackOfOpportunity = true,
			FXSettingsOverride = _weapon.AttackOfOpportunityAbilityFXSettings
		});
		base.OnInit(executor);
		AllActive.Add(this);
	}

	public override void Clear()
	{
		AllActive.Remove(this);
		base.Clear();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (base.Executor != null)
		{
			AllActive.Add(this);
		}
	}
}
