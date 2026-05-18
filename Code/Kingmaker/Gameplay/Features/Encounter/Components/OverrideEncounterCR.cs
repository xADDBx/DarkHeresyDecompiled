using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Encounter.Components;

[AllowedOn(typeof(BlueprintEtude))]
[ComponentName("Combat/OverrideEncounterCR")]
[TypeId("596bf8a1b253aaa44aa0f55b68e41ecf")]
public class OverrideEncounterCR : EntityFactComponentDelegate
{
	[SerializeField]
	private BpRef<BlueprintEncounter> m_Encounter = new BpRef<BlueprintEncounter>();

	[SerializeField]
	private int m_NewCR;

	public BlueprintEncounter Encounter => m_Encounter?.MaybeBlueprint;

	public int NewCR => m_NewCR;

	protected override void OnActivateOrPostLoad()
	{
		BlueprintEncounter encounter = Encounter;
		if (encounter != null)
		{
			EncounterCROverrideHelper.Push(encounter.AssetGuid, this);
		}
	}

	protected override void OnDeactivate()
	{
		BlueprintEncounter encounter = Encounter;
		if (encounter != null)
		{
			EncounterCROverrideHelper.Pop(encounter.AssetGuid, this);
		}
	}
}
