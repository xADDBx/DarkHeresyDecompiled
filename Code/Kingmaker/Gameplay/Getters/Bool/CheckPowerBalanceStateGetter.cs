using System;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Bool;

[Serializable]
[TypeId("77d8065416504c05a363a9113ee22b6c")]
public sealed class CheckPowerBalanceStateGetter : BoolPropertyGetter
{
	public PowerBalanceState State;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Power Balance state of " + FormulaTargetScope.Current;
	}

	protected override bool GetBaseValue()
	{
		return Game.Instance.Controllers.MoraleController.GetPowerBalanceState(base.CurrentEntity) == State;
	}
}
