using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.SectorMap;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("0ef23848acd648b3a4ea8fd6b2ae8962")]
public class LowerSectorMapPassageDifficulty : GameAction
{
	public BlueprintSectorMapPointReference PassageFrom;

	public BlueprintSectorMapPointReference PassageTo;

	public int RequiredNavigatorResource;

	[CanBeNull]
	public BlueprintItemReference RequiredItem;

	public int Quantity;

	public SectorMapPassageEntity.PassageDifficulty Difficulty;

	public override string GetCaption()
	{
		return $"Lower difficulty of passage from {PassageFrom} to {PassageTo}";
	}

	protected override void RunAction()
	{
	}
}
