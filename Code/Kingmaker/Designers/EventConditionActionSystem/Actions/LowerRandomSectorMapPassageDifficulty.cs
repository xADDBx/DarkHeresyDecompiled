using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("461df2474ffe423e928241c6645b57db")]
public class LowerRandomSectorMapPassageDifficulty : GameAction
{
	public BlueprintSectorMapPointReference SectorMapPoint;

	public int RequiredNavigatorResource;

	[CanBeNull]
	public BlueprintItemReference RequiredItem;

	public int Quantity;

	public override string GetCaption()
	{
		return $"Lower difficulty of passage from {SectorMapPoint} to random system";
	}

	protected override void RunAction()
	{
	}
}
