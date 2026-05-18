using System;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility.Attributes;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.PatternAttack;

[Serializable]
public class AbilityAoEPatternSettings : IAbilityAoEPatternProvider
{
	public AoEPattern Pattern;

	public TargetType Targets;

	public bool CastOnSameLevel;

	[Header("Spreading settings")]
	[SerializeField]
	private bool m_IgnoreLos;

	[SerializeField]
	[HideIf("m_IgnoreLos")]
	private bool m_UseMeleeLos;

	[Tooltip("Если true, то паттерн не будет включать в себя клетки позади кавера")]
	[SerializeField]
	[HideIf("IsIgnoreLosOrMeleeLos")]
	private bool m_RespectCovers;

	[Tooltip("Если true, то паттерн не будет включать в себя клетки позади кавера даже если центр паттерна находится менее чем в 3х клетках от этих клеток. По умолчанию на короткой дистанции кавера игнорируются.")]
	[SerializeField]
	[ShowIf("RespectCovers")]
	private bool m_RespectCoversEvenInCloseRange;

	[SerializeField]
	private bool m_IgnoreLevelDifference;

	[Tooltip("'Directional' spreading works only with Sector, Cone and Ray patterns")]
	[SerializeField]
	[ShowIf("CanBeDirectional")]
	private bool m_Directional;

	public bool CalculateAttackFromPatternCentre;

	public bool Directional
	{
		get
		{
			return m_Directional;
		}
		set
		{
			m_Directional = value;
		}
	}

	private AoEPattern CurrentPattern => Pattern;

	private bool IsIgnoreLosOrMeleeLos
	{
		get
		{
			if (!IsIgnoreLos)
			{
				return UseMeleeLos;
			}
			return true;
		}
	}

	public bool IsIgnoreLos => m_IgnoreLos;

	public bool RespectCovers
	{
		get
		{
			if (!IsIgnoreLos && !UseMeleeLos)
			{
				return m_RespectCovers;
			}
			return false;
		}
	}

	public bool RespectCoversEvenInCloseRange
	{
		get
		{
			if (RespectCovers)
			{
				return m_RespectCoversEvenInCloseRange;
			}
			return false;
		}
	}

	public bool UseMeleeLos => m_UseMeleeLos;

	public bool IsIgnoreLevelDifference => m_IgnoreLevelDifference;

	public int PatternAngle => CurrentPattern.Angle;

	bool IAbilityAoEPatternProvider.CalculateAttackFromPatternCentre => CalculateAttackFromPatternCentre;

	TargetType IAbilityAoEPatternProvider.Targets => Targets;

	AoEPattern IAbilityAoEPatternProvider.Pattern => CurrentPattern;

	public int Radius => CurrentPattern.Radius;

	private bool CanBeDirectional => CurrentPattern.CanBeDirectional;

	public int? HaloSize { get; private set; }

	public void OverrideHaloSize(int? haloSize)
	{
		HaloSize = haloSize;
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		GridNodeBase actualCastNode;
		return AoEPatternHelper.GetOrientedPattern(ability, ability.Caster, CurrentPattern, this, casterNode, targetNode, CastOnSameLevel, m_Directional, coveredTargetsOnly, targetSize, out actualCastNode, HaloSize);
	}

	public OrientedPatternData GetOrientedHaloPattern(IAbilityDataProviderForPattern ability, int haloSize, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		GridNodeBase actualCastNode;
		return AoEPatternHelper.GetOrientedPattern(ability, ability.Caster, CurrentPattern, this, casterNode, targetNode, CastOnSameLevel, m_Directional, coveredTargetsOnly, targetSize, out actualCastNode, HaloSize, haloSize);
	}
}
