using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Code.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("39b5c25438ca44a199b88f0592ab7252")]
[PlayerUpgraderAllowed(false)]
public class PlanetScanned : Condition
{
	[SerializeField]
	private BlueprintPlanet.Reference m_Planet;

	private BlueprintPlanet Planet => m_Planet?.Get();

	protected override string GetConditionCaption()
	{
		return "Check planet " + Planet?.Name + " scanned";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
