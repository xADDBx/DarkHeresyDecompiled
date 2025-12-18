using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b2a17e3404004ded87b8179ff8634ef2")]
public class OverrideAnimationSet : UnitFactComponentDelegate
{
	[SerializeField]
	private AnimationSetLink m_AnimationSetMaleLink;

	[SerializeField]
	private AnimationSetLink m_AnimationSetFemaleLink;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartVisualChange>().SetAnimationSet((base.Owner.Gender == Gender.Male) ? m_AnimationSetMaleLink : m_AnimationSetFemaleLink);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<UnitPartVisualChange>();
		base.Owner.View.CharacterAvatar.Or(null)?.InitAnimSet();
	}
}
