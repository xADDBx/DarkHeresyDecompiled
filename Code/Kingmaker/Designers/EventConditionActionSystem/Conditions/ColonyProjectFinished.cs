using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("59303f79a33946b0a3cb099809b0804b")]
public class ColonyProjectFinished : Condition
{
	[SerializeField]
	private BlueprintColonyProjectReference m_Project;

	private BlueprintColonyProject Project => m_Project?.Get();

	protected override string GetConditionCaption()
	{
		return "Project " + Project.Name + " is finished";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
