using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("0685a3943801c8c40b0122676324df22")]
public class LevelGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " Character Level";
	}

	protected override int GetBaseValue()
	{
		return base.CurrentEntity.GetProgressionOptional()?.CharacterLevel ?? 0;
	}
}
