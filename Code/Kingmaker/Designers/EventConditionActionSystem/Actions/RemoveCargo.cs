using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("44344b3713d94f1d93f6397345c67794")]
[PlayerUpgraderAllowed(false)]
public class RemoveCargo : GameAction
{
	[SerializeField]
	[ShowIf("ByBlueprint")]
	private BlueprintCargoReference m_Cargo;

	[SerializeField]
	[HideIf("ByBlueprint")]
	private ItemsItemOrigin m_OriginType;

	[SerializeField]
	private bool m_ByBlueprint;

	public bool ByBlueprint => m_ByBlueprint;

	public BlueprintCargo Cargo => m_Cargo?.Get();

	public override string GetCaption()
	{
		if (!m_ByBlueprint)
		{
			return $"Remove cargo {m_OriginType}";
		}
		return $"Remove cargo {Cargo}";
	}

	protected override void RunAction()
	{
	}
}
