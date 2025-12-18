using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("fad8e98d61f54ac7ac8ce20cc204b9ca")]
[PlayerUpgraderAllowed(false)]
public class ChangeColonyStats : GameAction
{
	[ShowIf("ApplyToSpecificColony")]
	[SerializeField]
	private BlueprintColonyReference m_Colony;

	[HideIf("m_ApplyToAllColonies")]
	[SerializeField]
	private bool m_ApplyToCurrentColony;

	[HideIf("m_ApplyToCurrentColony")]
	[SerializeField]
	private bool m_ApplyToAllColonies;

	[SerializeField]
	private int m_ContentmentModifier;

	[SerializeField]
	private int m_SecurityModifier;

	[SerializeField]
	private int m_EfficiencyModifier;

	private BlueprintColony Colony => m_Colony?.Get();

	private bool ApplyToSpecificColony
	{
		get
		{
			if (!m_ApplyToAllColonies)
			{
				return !m_ApplyToCurrentColony;
			}
			return false;
		}
	}

	public override string GetCaption()
	{
		return "Change colony stats";
	}

	protected override void RunAction()
	{
	}
}
