using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("2ac5d227b9415e94d959e0c8198646db")]
[PlayerUpgraderAllowed(false)]
public class ApplyTraitToColony : GameAction
{
	public BlueprintColonyTrait.Reference Trait;

	[SerializeField]
	private BlueprintColonyReference m_Colony;

	private BlueprintColony Colony => m_Colony?.Get();

	public override string GetCaption()
	{
		return "Apply trait " + Trait.Get().Name + " to current colony in context";
	}

	protected override void RunAction()
	{
	}
}
