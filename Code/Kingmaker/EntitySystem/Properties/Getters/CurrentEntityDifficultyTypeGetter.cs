using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("20908d2906855504abc36ab381c7ea6f")]
public class CurrentEntityDifficultyTypeGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Difficulty Type of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return 0;
		}
		return (int)baseUnitEntity.Blueprint.DifficultyType;
	}
}
