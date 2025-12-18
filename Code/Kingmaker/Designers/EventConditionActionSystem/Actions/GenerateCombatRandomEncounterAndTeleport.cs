using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("585e0999b522401abc033c1e23c651f4")]
public class GenerateCombatRandomEncounterAndTeleport : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintAreaReference m_Area;

	[SerializeField]
	[CanBeNull]
	private BlueprintAreaEnterPointReference m_OverrideEnterPoint;

	[SerializeField]
	[CanBeNull]
	private BlueprintRandomGroupOfUnits.Reference m_OverrideRandomGroup;

	[SerializeField]
	private bool m_SpecifyCoverGroup;

	[SerializeField]
	[ShowIf("m_SpecifyCoverGroup")]
	private EntityReference m_OverrideCoverGroup;

	[SerializeField]
	private bool m_SpecifyTrapGroup;

	[SerializeField]
	[ShowIf("m_SpecifyTrapGroup")]
	private EntityReference m_OverrideTrapGroup;

	[SerializeField]
	private bool m_SpecifyAreaEffectGroup;

	[SerializeField]
	[ShowIf("m_SpecifyAreaEffectGroup")]
	private EntityReference m_OverrideAreaEffectGroup;

	[SerializeField]
	private bool m_SpecifyOtherMapObjectGroup;

	[SerializeField]
	[ShowIf("m_SpecifyOtherMapObjectGroup")]
	private EntityReference m_OverrideOtherMapObjectGroup;

	[SerializeField]
	private BlueprintUnlockableFlagReference m_UnlockFlag;

	public BlueprintUnlockableFlag UnlockFlag => m_UnlockFlag?.Get();

	public override string GetCaption()
	{
		return "Run CombatRandomEncounterGenerator and teleport party to area";
	}

	protected override void RunAction()
	{
	}
}
