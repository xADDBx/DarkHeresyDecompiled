using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("292931bf836afcd4a801b15afc72d51c")]
public class StarshipSoulmarksLogic : BlueprintComponent
{
	public enum MarkMode
	{
		Chaos,
		Faith
	}

	[SerializeField]
	private MarkMode mode;

	[SerializeField]
	[ShowIf("IsFaith")]
	private int healRound;

	[SerializeField]
	[ShowIf("IsFaith")]
	private int healPct;

	[SerializeField]
	private ActionList Actions;

	public bool IsFaith => mode == MarkMode.Faith;
}
