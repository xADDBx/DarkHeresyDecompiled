using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("94692eddaeccedc4b8ad9b0bc6304303")]
[PlayerUpgraderAllowed(false)]
public class CreateColony : GameAction
{
	[SerializeField]
	[NotNull]
	public BlueprintPlanet.Reference Planet;

	[CanBeNull]
	public BlueprintColonyTrait.Reference[] ApplyTraits;

	[SerializeField]
	public bool ChangeInitialContentment;

	[ShowIf("ChangeInitialContentment")]
	[SerializeField]
	public int InitialContentmentValue;

	[SerializeField]
	public bool ChangeInitialSecurity;

	[ShowIf("ChangeInitialSecurity")]
	[SerializeField]
	public int InitialSecurityValue;

	[SerializeField]
	public bool ChangeInitialEfficiency;

	[ShowIf("ChangeInitialEfficiency")]
	[SerializeField]
	public int InitialEfficiencyValue;

	public override string GetCaption()
	{
		return "Create colony from component";
	}

	protected override void RunAction()
	{
	}
}
