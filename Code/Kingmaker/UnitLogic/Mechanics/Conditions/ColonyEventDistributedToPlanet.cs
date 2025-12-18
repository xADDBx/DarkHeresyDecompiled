using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("052035f45ba843d4964321fff1192ad4")]
public class ColonyEventDistributedToPlanet : Condition
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintColonyEventReference m_Event;

	[SerializeField]
	private BlueprintPlanet.Reference m_Planet;

	[SerializeField]
	private bool m_Except;

	[SerializeField]
	private bool m_AnyPlanet;

	[SerializeField]
	private bool m_CheckStates;

	[SerializeField]
	[EnumFlagsAsButtons]
	private ColonyEventState m_States;

	public BlueprintColonyEvent Event => m_Event?.Get();

	public BlueprintPlanet Planet => m_Planet?.Get();

	protected override string GetConditionCaption()
	{
		return "";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
