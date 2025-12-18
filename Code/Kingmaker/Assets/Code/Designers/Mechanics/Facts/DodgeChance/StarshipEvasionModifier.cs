using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Assets.Code.Designers.Mechanics.Facts.DodgeChance;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("bb50529527787d544b395cdbed76eda2")]
public class StarshipEvasionModifier : BlueprintComponent
{
	private enum ModifyWhen
	{
		IsInitiator,
		IsTarget
	}

	[SerializeField]
	private ModifyWhen modifyWhen = ModifyWhen.IsTarget;

	public int EvasionBonusPct;

	public bool GrantSuperEvasion;

	[SerializeField]
	private BlueprintFeatureReference m_CheckEnemyFeature;

	public BlueprintFeature CheckEnemyFeature => m_CheckEnemyFeature?.Get();
}
