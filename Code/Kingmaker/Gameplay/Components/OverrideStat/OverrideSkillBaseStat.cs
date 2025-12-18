using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components.OverrideStat;

[Serializable]
[ClassInfoBox("Подменяет базовый атрибут скилла.")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d503099978ed4daeb041048174fe66cb")]
public sealed class OverrideSkillBaseStat : UnitFactComponentDelegate
{
	public SkillType Target;

	public AttributeType Override;

	public bool OnlyIfHigher = true;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Stats.GetSkill(Target.ToStatType()).AddBaseStatOverride(Override.ToStatType(), base.Runtime, OnlyIfHigher);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetSkill(Target.ToStatType()).RemoveBaseStatOverride(base.Runtime);
	}
}
