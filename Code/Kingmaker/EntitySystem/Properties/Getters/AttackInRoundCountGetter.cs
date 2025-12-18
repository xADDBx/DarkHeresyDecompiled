using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("8b598d75e29a452599a67c43ccc941bb")]
public class AttackInRoundCountGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.GetCombatStateOptional()?.AttackInRoundCount ?? 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count of attacks made by " + FormulaTargetScope.Current + " this round";
	}
}
