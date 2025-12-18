using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("aa60d691e1a94f45b999e2d16f190214")]
public class IsMainCharacterGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		return baseUnitEntity.IsMainCharacter;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is " + FormulaTargetScope.Current + " is Main Character";
	}
}
