using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("598966732faf44fe9b1de2bfcceeca90")]
public class MoralePhaseGetter : BoolPropertyGetter
{
	public MoralePhaseType TargetPhase;

	protected override bool GetBaseValue()
	{
		return base.CurrentEntity.GetOptional<PartMorale>()?.Phase == TargetPhase;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is Morale Phase = " + TargetPhase;
	}
}
