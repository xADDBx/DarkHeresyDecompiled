using System;
using JetBrains.Annotations;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Items.Components;

[TypeId("425db89e8c7149dd84cfe3fd3d807fe1")]
public class OverrideWarhammerMovementApPerCellThreateningArea : UnitFactComponentDelegate
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public int WarhammerMovementApPerCellThreateningArea = 3;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartWarhammerMovementApPerCellThreateningArea>().Add(this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartWarhammerMovementApPerCellThreateningArea>()?.Remove(this);
	}

	[CanBeNull]
	public float? GetWarhammerMovementApPerCellThreateningArea()
	{
		try
		{
			return Restriction.IsPassed(base.Context) ? new float?(WarhammerMovementApPerCellThreateningArea) : null;
		}
		catch (Exception arg)
		{
			PFLog.Pathfinding.Error($"Error while calculating override of ApPerCellThreateningArea :\n{arg}");
			return null;
		}
	}
}
