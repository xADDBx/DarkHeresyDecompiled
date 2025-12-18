using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("6ffdadf391194bd1960f6bd0ee30a785")]
public class RequirementProfitFactorMinimum : Requirement
{
	[SerializeField]
	private int m_ProfitFactorMinimum;

	public int ProfitFactorMinimum => m_ProfitFactorMinimum;
}
