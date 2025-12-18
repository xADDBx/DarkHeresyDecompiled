using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Obsolete]
[AllowedOn(typeof(BlueprintCampaign))]
[TypeId("489e6e8233f1adb4bac778225fa022a4")]
public class BlueprintCampaignCustomCompanion : BlueprintComponent
{
	[SerializeField]
	private BlueprintUnitReference m_CustomCompanion;

	public BlueprintUnit CustomCompanion => m_CustomCompanion;
}
