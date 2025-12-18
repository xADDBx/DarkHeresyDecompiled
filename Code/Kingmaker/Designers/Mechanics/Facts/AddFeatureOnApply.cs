using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[ComponentName("Facts And Buffs/AddFeatureOnApply")]
[TypeId("0a53ee09075a237408f1347648c1e91a")]
public class AddFeatureOnApply : UnitFactComponentDelegate
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintFeatureReference m_Feature;

	public BlueprintFeature Feature => m_Feature?.Get();

	protected override void OnActivate()
	{
		if (m_Restrictions.IsPassed(base.Context))
		{
			base.Owner.Progression.Features.Add(Feature);
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.Progression.Features.Remove(Feature);
	}
}
