using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.UnitLogic.Levelup.Components;

[Serializable]
[ClassInfoBox("Добавляет группы талантов в список рекомендованных для выбора в левелапе")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b956f8f3d00d4a1ca5e29f7516bc16f4")]
public sealed class RecommendedTalentGroup : UnitFactComponentDelegate
{
	[EnumFlagsAsButtons]
	public TalentGroup Group;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Progression.AddRecommendedTalentGroup(Group, base.Runtime);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Progression.RemoveRecommendedTalentGroup(base.Runtime);
	}
}
