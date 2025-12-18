using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

[Serializable]
public class AoEPattern
{
	[SerializeField]
	private PatternType m_Type;

	[SerializeField]
	[ShowIf("IsCustom")]
	private BlueprintAttackPatternReference m_Blueprint;

	[HideIf("IsCustom")]
	[SerializeField]
	private int m_Radius;

	[ShowIf("IsCone")]
	[SerializeField]
	[RangeWithStep(30, 180, 30)]
	private int m_Angle;

	public static float SameLevelDiff => ConfigRoot.Instance.AoEPatternRoot.SameLevelDiff;

	public static float RayConeThickness => ConfigRoot.Instance.AoEPatternRoot.RayConeThickness;

	public PatternType Type
	{
		get
		{
			if (!IsCustom || Blueprint == null)
			{
				return m_Type;
			}
			return Blueprint.Type;
		}
	}

	public bool CanBeDirectional
	{
		get
		{
			PatternType type = m_Type;
			return type == PatternType.Ray || type == PatternType.Cone || type == PatternType.Sector;
		}
	}

	private bool IsCustom => m_Type == PatternType.Custom;

	private bool IsCone
	{
		get
		{
			if (m_Type != PatternType.Cone)
			{
				return m_Type == PatternType.Sector;
			}
			return true;
		}
	}

	private BlueprintAttackPattern Blueprint => m_Blueprint?.Get();

	public int Angle => m_Type switch
	{
		PatternType.Cone => m_Angle, 
		PatternType.Sector => m_Angle, 
		PatternType.Ray => 0, 
		PatternType.Custom => Blueprint.Angle, 
		PatternType.Circle => 360, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public int Radius
	{
		get
		{
			if (m_Type != PatternType.Custom)
			{
				return m_Radius;
			}
			if (Blueprint.Type != PatternType.Custom)
			{
				return Blueprint.Radius;
			}
			IntRect bounds = Bounds;
			return Math.Max(Math.Max(bounds.xmax, bounds.ymax), Math.Max(-bounds.xmin, -bounds.ymin));
		}
	}

	public IntRect Bounds
	{
		get
		{
			using PatternGridData patternGridData = GetGridData(Vector2.up);
			using PatternGridData patternGridData2 = GetGridData((Vector2.up + Vector2.right).normalized);
			return IntRect.Union(patternGridData.Bounds, patternGridData2.Bounds);
		}
	}

	public PatternGridData GetGridData(Vector2 direction, Size entitySizeRect = Size.Medium)
	{
		float sqrMagnitude = direction.sqrMagnitude;
		if (sqrMagnitude > 1.1f || sqrMagnitude < 0.9f)
		{
			throw new ArgumentException("Need nonzero vector", "direction");
		}
		if (m_Type != PatternType.Custom)
		{
			return GridPatterns.ConstructPattern(m_Type, m_Radius, m_Angle, direction, entitySizeRect);
		}
		return Blueprint.GetOrientedNodes(direction);
	}

	public OrientedPatternData GetOriented(GridNodeBase applicationNode, Vector3 direction)
	{
		AoEPatternCalculation @params = new AoEPatternCalculation(applicationNode, direction).SetIgnoreLos(ignoreLos: true).SetIgnoreLevelDifference(ignoreLevelDifference: true);
		return GetOriented(@params);
	}

	public PatternGridData GetOrientedGridData(AoEPatternCalculation @params)
	{
		GridNodeBase checkLosFromNode = @params.CheckLosFromNode;
		GridNodeBase applicationNode = @params.ApplicationNode;
		Vector3 direction = @params.Direction;
		bool isIgnoreLos = @params.IsIgnoreLos;
		bool isRespectCovers = @params.IsRespectCovers;
		bool isRespectCoversEvenInCloseRange = @params.IsRespectCoversEvenInCloseRange;
		bool isIgnoreLevelDifference = @params.IsIgnoreLevelDifference;
		bool isDirectional = @params.IsDirectional;
		bool coveredTargetsOnly = @params.CoveredTargetsOnly;
		bool useMeleeLos = @params.UseMeleeLos;
		Size entitySizeRect = @params.EntitySizeRect;
		int? builtInHaloSize = @params.BuiltInHaloSize;
		int? haloSize = @params.HaloSize;
		Vector2 direction2 = (((double)direction.sqrMagnitude < 0.0001) ? Vector2.down : direction.To2D().normalized);
		float num = 0f;
		bool flag = Math.Abs(direction2.x) > Math.Abs(direction2.y);
		GridGraph gridGraph = (GridGraph)applicationNode.Graph;
		if (isDirectional)
		{
			Vector3 normalized = direction.normalized;
			float nodeSize = gridGraph.nodeSize;
			float num2 = (flag ? (nodeSize / Math.Abs(normalized.x)) : (nodeSize / Math.Abs(normalized.z)));
			num = (normalized * num2).y;
		}
		float num3 = (isIgnoreLevelDifference ? float.MaxValue : (isDirectional ? RayConeThickness : SameLevelDiff));
		HashSet<Vector2Int> value;
		using (CollectionPool<HashSet<Vector2Int>, Vector2Int>.Get(out value))
		{
			HashSet<Vector2Int> value2;
			using (CollectionPool<HashSet<Vector2Int>, Vector2Int>.Get(out value2))
			{
				PatternGridData patternGridData = default(PatternGridData);
				patternGridData = GetGridData(direction2, entitySizeRect).Move(applicationNode.CoordinatesInGrid);
				bool preventBlowback = m_Type != PatternType.Circle;
				if (builtInHaloSize.HasValue)
				{
					PatternGridData patternGridData2 = patternGridData;
					patternGridData = patternGridData2.BuildHalo(builtInHaloSize.Value, PatternGridData.HaloMode.IncludeOriginalPattern, applicationNode.CoordinatesInGrid, direction, preventBlowback, disposable: true);
					patternGridData2.Dispose();
				}
				if (haloSize.HasValue)
				{
					PatternGridData patternGridData3 = patternGridData;
					patternGridData = patternGridData3.BuildHalo(haloSize.Value, PatternGridData.HaloMode.ExcludeOriginalPattern, applicationNode.CoordinatesInGrid, direction, preventBlowback, disposable: true);
					patternGridData3.Dispose();
				}
				NodeList nodeList = new NodeList(gridGraph, in patternGridData);
				float num4 = checkLosFromNode.Vector3Position().y - applicationNode.Vector3Position().y;
				foreach (GridNodeBase item in nodeList)
				{
					if (coveredTargetsOnly && !item.ContainsUnit())
					{
						continue;
					}
					Vector3 vector = item.Vector3Position() - applicationNode.Vector3Position();
					float num7;
					if (isDirectional)
					{
						int num5 = (flag ? Mathf.Abs(applicationNode.XCoordinateInGrid - item.XCoordinateInGrid) : Mathf.Abs(applicationNode.ZCoordinateInGrid - item.ZCoordinateInGrid));
						float num6 = applicationNode.Vector3Position().y + (float)num5 * num + num4;
						num7 = Mathf.Abs(item.Vector3Position().y - num6);
					}
					else
					{
						num7 = Mathf.Abs(vector.y);
					}
					if (num7 > num3)
					{
						continue;
					}
					if (!isIgnoreLos)
					{
						if (useMeleeLos && !LosCalculations.HasMeleeLos(checkLosFromNode, SizePathfindingHelper.GetRectForSize(entitySizeRect), item, default(IntRect)))
						{
							value2.Add(item.CoordinatesInGrid);
							continue;
						}
						LosDescription warhammerLos = LosCalculations.GetWarhammerLos(checkLosFromNode, SizePathfindingHelper.GetRectForSize(entitySizeRect), item, default(IntRect));
						LosCalculations.CoverType coverType = (isRespectCoversEvenInCloseRange ? warhammerLos.OriginalCoverType : warhammerLos.CoverType);
						if (coverType == LosCalculations.CoverType.LosBlocker)
						{
							value2.Add(item.CoordinatesInGrid);
							continue;
						}
						if (isRespectCovers && coverType == LosCalculations.CoverType.Cover)
						{
							value2.Add(item.CoordinatesInGrid);
							continue;
						}
					}
					value.Add(item.CoordinatesInGrid);
				}
				return PatternGridData.Create(value, value2, disposable: true);
			}
		}
	}

	public OrientedPatternData GetOriented(AoEPatternCalculation @params)
	{
		return new OrientedPatternData(GetOrientedGridData(@params), @params.ApplicationNode);
	}

	public static AoEPattern Ray(int length)
	{
		return new AoEPattern
		{
			m_Type = PatternType.Ray,
			m_Radius = length
		};
	}

	public static AoEPattern Cone(int angle, int radius)
	{
		return new AoEPattern
		{
			m_Type = PatternType.Cone,
			m_Radius = radius,
			m_Angle = angle
		};
	}

	public static AoEPattern Sector(int angle, int radius)
	{
		return new AoEPattern
		{
			m_Type = PatternType.Sector,
			m_Radius = radius,
			m_Angle = angle
		};
	}

	public static AoEPattern Circle(int radius)
	{
		return new AoEPattern
		{
			m_Type = PatternType.Circle,
			m_Radius = radius
		};
	}

	public static AoEPattern CopyAndOverrideRadius(AoEPattern pattern, int radius)
	{
		return new AoEPattern
		{
			m_Blueprint = pattern.m_Blueprint,
			m_Type = pattern.m_Type,
			m_Radius = radius,
			m_Angle = pattern.m_Angle
		};
	}

	public static AoEPattern FromBlueprint(BlueprintAttackPattern blueprint)
	{
		return new AoEPattern
		{
			m_Type = PatternType.Custom,
			m_Blueprint = blueprint.ToReference<BlueprintAttackPatternReference>()
		};
	}

	public static Vector3 GetCastDirection(PatternType patternType, GridNodeBase casterNode, GridNodeBase castNode, GridNodeBase targetNode)
	{
		Vector3 result = targetNode.Vector3Position() - casterNode.Vector3Position();
		if (patternType == PatternType.Ray || patternType == PatternType.Cone || patternType == PatternType.Sector)
		{
			Vector3 vector = (((targetNode.Vector3Position() - castNode.Vector3Position()).sqrMagnitude < 1f) ? (targetNode.Vector3Position() - casterNode.Vector3Position()) : (targetNode.Vector3Position() - castNode.Vector3Position()));
			result = new Vector3(vector.x, result.y, vector.z);
		}
		return result;
	}
}
