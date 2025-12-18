using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("847bad5ab8fbb6f4da7728b64b1245f8")]
public class StarshipModifyHitChancesWithDistance : BlueprintComponent
{
	private enum TriggerType
	{
		AsInitiator,
		AsTarget,
		Both
	}

	[SerializeField]
	private int hitPerTilePctBase;

	[SerializeField]
	private int hitPerTilePctPerTile;

	[SerializeField]
	[Tooltip("When calculating, ignore leading number of tiles")]
	private int tilesToIgnore;

	[SerializeField]
	private int tilesLimit;

	[SerializeField]
	private TriggerType triggerType;

	[SerializeField]
	private bool followDifficultySettings;
}
