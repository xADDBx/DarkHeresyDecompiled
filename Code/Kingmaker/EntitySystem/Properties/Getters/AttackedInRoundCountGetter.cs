using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("8b734b0a49094c9f8957d90790e3d8fb")]
public class AttackedInRoundCountGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.GetCombatStateOptional()?.AttackedInRoundCount ?? 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count of attacks suffered by " + FormulaTargetScope.Current + " this round";
	}
}
