using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Mechanics.Actions;

[TypeId("a81250c2e5754eb4ba2a6ebbde4b2e65")]
public class ContextActionMedicae : ContextAction
{
	public ContextValue BaseHeal;

	public override string GetCaption()
	{
		return "Perform medicae";
	}

	protected override void RunAction()
	{
		RuleHealDamage ruleHealDamage = RuleHealDamage.Setup(base.Caster, base.TargetEntity).Base(BaseHeal.Calculate(base.Context)).Create();
		int value = (int)base.Caster.Actor.GetStat(StatType.SkillMedicae, null, default(StatContext), "RunAction") / 3;
		ruleHealDamage.CalculateHealRule.Modifiers.Add(ModifierType.ValAdd, value, null, null, BonusType.None, StatType.SkillMedicae);
		Rulebook.Trigger(ruleHealDamage);
	}
}
