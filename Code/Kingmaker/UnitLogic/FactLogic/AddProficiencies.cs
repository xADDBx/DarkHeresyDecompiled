using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[ComponentName("Add proficiencies")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("5fa95b92f4e484742ad84c926183372c")]
public class AddProficiencies : UnitFactComponentDelegate
{
	[SerializeField]
	[FormerlySerializedAs("RaceRestriction")]
	private BlueprintRaceReference m_RaceRestriction;

	public ArmorProficiencyGroup[] ArmorProficiencies;

	public WeaponProficiency[] WeaponProficiencies;

	public BlueprintRace RaceRestriction => m_RaceRestriction?.Get();

	protected override void OnActivateOrPostLoad()
	{
		if (!RaceRestriction || base.Owner.Progression.Race == RaceRestriction)
		{
			ArmorProficiencies.EmptyIfNull().ForEach(delegate(ArmorProficiencyGroup i)
			{
				base.Owner.Proficiencies.Add(i);
			});
			WeaponProficiencies.EmptyIfNull().ForEach(delegate(WeaponProficiency i)
			{
				base.Owner.Proficiencies.Add(in i);
			});
		}
	}

	protected override void OnDeactivate()
	{
		if (!RaceRestriction || base.Owner.Progression.Race == RaceRestriction)
		{
			ArmorProficiencies.EmptyIfNull().ForEach(delegate(ArmorProficiencyGroup i)
			{
				base.Owner.Proficiencies.Remove(i);
			});
			WeaponProficiencies.EmptyIfNull().ForEach(delegate(WeaponProficiency i)
			{
				base.Owner.Proficiencies.Remove(in i);
			});
		}
	}
}
