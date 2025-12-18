using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components.OverrideStat;

[Serializable]
[ClassInfoBox("Подменяет один атрибут другим.")]
[AllowMultipleComponents]
[ComponentName("Stats/OverrideStat")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d553b17d00fd44b085c1eb5c79c8e94f")]
public sealed class OverrideStat : UnitFactComponentDelegate
{
	public AttributeType Target;

	public AttributeType Override;

	public bool OnlyIfHigher = true;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Stats.GetAttribute(Target.ToStatType()).AddOverride(Override.ToStatType(), base.Runtime, OnlyIfHigher);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetAttribute(Target.ToStatType()).RemoveOverride(base.Runtime);
	}
}
