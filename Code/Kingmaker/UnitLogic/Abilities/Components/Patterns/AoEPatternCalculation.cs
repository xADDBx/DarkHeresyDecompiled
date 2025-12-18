using Kingmaker.Enums;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

public struct AoEPatternCalculation
{
	public GridNodeBase CheckLosFromNode { get; private set; }

	public GridNodeBase ApplicationNode { get; private set; }

	public Vector3 Direction { get; private set; }

	public bool IsIgnoreLos { get; private set; }

	public bool IsRespectCovers { get; private set; }

	public bool IsRespectCoversEvenInCloseRange { get; private set; }

	public bool IsIgnoreLevelDifference { get; private set; }

	public bool IsDirectional { get; private set; }

	public bool CoveredTargetsOnly { get; private set; }

	public bool UseMeleeLos { get; private set; }

	public Size EntitySizeRect { get; private set; }

	public int? BuiltInHaloSize { get; private set; }

	public int? HaloSize { get; private set; }

	public AoEPatternCalculation(GridNodeBase applicationNode, Vector3 direction)
		: this(applicationNode, applicationNode, direction)
	{
	}

	public AoEPatternCalculation(GridNodeBase checkLosFromNode, GridNodeBase applicationNode, Vector3 direction)
	{
		CheckLosFromNode = checkLosFromNode;
		ApplicationNode = applicationNode;
		Direction = direction;
		IsIgnoreLos = false;
		IsRespectCovers = false;
		IsRespectCoversEvenInCloseRange = false;
		IsIgnoreLevelDifference = false;
		IsDirectional = false;
		CoveredTargetsOnly = false;
		UseMeleeLos = false;
		EntitySizeRect = Size.Medium;
		BuiltInHaloSize = null;
		HaloSize = null;
	}

	public AoEPatternCalculation SetIgnoreLos(bool ignoreLos)
	{
		IsIgnoreLos = ignoreLos;
		return this;
	}

	public AoEPatternCalculation SetRespectCovers(bool respectCovers, bool evenInCloseRange)
	{
		IsRespectCovers = respectCovers;
		IsRespectCoversEvenInCloseRange = evenInCloseRange;
		return this;
	}

	public AoEPatternCalculation SetIgnoreLevelDifference(bool ignoreLevelDifference)
	{
		IsIgnoreLevelDifference = ignoreLevelDifference;
		return this;
	}

	public AoEPatternCalculation SetDirectional(bool directional)
	{
		IsDirectional = directional;
		return this;
	}

	public AoEPatternCalculation SetCoveredTargetsOnly(bool coveredTargetsOnly)
	{
		CoveredTargetsOnly = coveredTargetsOnly;
		return this;
	}

	public AoEPatternCalculation SetUseMeleeLos(bool useMeleeLos)
	{
		UseMeleeLos = useMeleeLos;
		return this;
	}

	public AoEPatternCalculation SetEntitySizeRect(Size entitySizeRect)
	{
		EntitySizeRect = entitySizeRect;
		return this;
	}

	public AoEPatternCalculation SetBuiltInHaloSize(int? builtInHaloSize)
	{
		BuiltInHaloSize = builtInHaloSize;
		return this;
	}

	public AoEPatternCalculation SetHaloSize(int? haloSize)
	{
		HaloSize = haloSize;
		return this;
	}
}
