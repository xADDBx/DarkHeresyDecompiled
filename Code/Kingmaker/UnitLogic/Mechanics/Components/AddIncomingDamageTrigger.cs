using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Enum;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Obsolete]
[TypeId("76dd00a1f560ad2438ed8bf8cbfcd039")]
[AllowMultipleComponents]
public class AddIncomingDamageTrigger : BlueprintComponent
{
	public ActionList Actions;

	public ActionList ActionsToAttacker;

	public bool TriggerOnStatDamageOrEnergyDrain;

	public bool IgnoreDamageFromThisFact;

	public bool ReduceBelowZero;

	public bool CheckDamageDealt;

	[ShowIf("CheckDamageDealt")]
	public CompareOperation.Type CompareType;

	[ShowIf("CheckDamageDealt")]
	public ContextValue TargetValue;

	public bool CheckWeaponAttackType;

	[ShowIf("CheckWeaponAttackType")]
	[EnumFlagsAsButtons]
	public AttackTypeFlag AttackType;

	public bool CheckDamageType;

	[ShowIf("CheckDamageType")]
	public DamageType DamageType;

	public bool TriggersForDamageOverTime;
}
