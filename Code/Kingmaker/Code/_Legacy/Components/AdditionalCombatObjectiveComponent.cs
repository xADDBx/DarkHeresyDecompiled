using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Code._Legacy.Components;

[Serializable]
[ClassInfoBox("Включает обводку для юнита, который является ВАЖНОЙ целью в бою")]
[ComponentName("UI/AdditionalCombatObjectiveComponent")]
[TypeId("906ce4b44eea49fd915120cc94dd0ed9")]
public sealed class AdditionalCombatObjectiveComponent : EntityFactComponentDelegate<AbstractUnitEntity>
{
	public SharedStringAsset Description;

	public HighlightType HighlightType;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAdditionalCombatObjectiveUnit>().Register(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAdditionalCombatObjectiveUnit>()?.Unregister(base.Fact, this);
	}
}
