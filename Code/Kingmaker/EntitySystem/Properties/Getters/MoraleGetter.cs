using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("1c3a23630df043f0b61a214ef5bd3eef")]
public class MoraleGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.GetOptional<PartMorale>()?.Value ?? 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Morale";
	}
}
