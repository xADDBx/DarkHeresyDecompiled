using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.BarkBanters;

[Obsolete]
[AllowedOn(typeof(BlueprintBarkBanter))]
[TypeId("829809919e5d4c5da7a345c8931a52a8")]
public class AstropathBriefComponent : BlueprintComponent
{
	[SerializeField]
	private BlueprintAstropathBrief.Reference m_AstropathBrief;

	public BlueprintAstropathBrief AstropathBrief => m_AstropathBrief?.Get();
}
