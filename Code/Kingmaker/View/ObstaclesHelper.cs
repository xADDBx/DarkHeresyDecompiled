using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View;

public static class ObstaclesHelper
{
	private class ObstacleGroupData
	{
		[CanBeNull]
		public List<UnitMovementAgent> ObstaclesGroup;

		[CanBeNull]
		public List<UnitMovementAgent> UnitContacts;
	}

	public const float GroupingDistance = 0.65f;

	public const float MaxNavmeshContactDistance = 2f;

	private static readonly Dictionary<UnitMovementAgent, ObstacleGroupData> s_UnitsAsObstacles = new Dictionary<UnitMovementAgent, ObstacleGroupData>();

	private static readonly List<UnitMovementAgent> s_EmptyList = new List<UnitMovementAgent>();

	public static IReadOnlyList<UnitMovementAgent> GetObstaclesGroup(this UnitMovementAgent forUnit)
	{
		if (!s_UnitsAsObstacles.TryGetValue(forUnit, out var value))
		{
			return s_EmptyList;
		}
		return value.ObstaclesGroup ?? s_EmptyList;
	}

	public static IReadOnlyList<UnitMovementAgent> GetUnitContacts(this UnitMovementAgent forUnit)
	{
		if (!s_UnitsAsObstacles.TryGetValue(forUnit, out var value))
		{
			return s_EmptyList;
		}
		return value.UnitContacts ?? s_EmptyList;
	}

	public static void RemoveFromGroup([NotNull] UnitMovementAgent unit)
	{
		if (!s_UnitsAsObstacles.Remove(unit, out var value))
		{
			return;
		}
		List<UnitMovementAgent> obstaclesGroup = value.ObstaclesGroup;
		if (obstaclesGroup == null)
		{
			return;
		}
		value.ObstaclesGroup = null;
		foreach (UnitMovementAgent item in obstaclesGroup)
		{
			if (s_UnitsAsObstacles.TryGetValue(item, out var value2))
			{
				value2.ObstaclesGroup = null;
				value2.UnitContacts = null;
			}
		}
		for (int i = 0; i < obstaclesGroup.Count; i++)
		{
			UnitMovementAgent unitMovementAgent = obstaclesGroup[i];
			if (GroupingForbidden(unitMovementAgent) || unitMovementAgent == unit)
			{
				continue;
			}
			for (int j = i + 1; j < obstaclesGroup.Count; j++)
			{
				UnitMovementAgent unitMovementAgent2 = obstaclesGroup[j];
				if (!GroupingForbidden(unitMovementAgent2) && !(unitMovementAgent2 == unit))
				{
					TryConnectUnits(unitMovementAgent, unitMovementAgent2);
				}
			}
		}
	}

	public static void ConnectToGroups([NotNull] UnitMovementAgent unit)
	{
		if (unit == null || GroupingForbidden(unit) || s_UnitsAsObstacles.ContainsKey(unit))
		{
			return;
		}
		s_UnitsAsObstacles[unit] = new ObstacleGroupData();
		if (unit.Unit == null)
		{
			return;
		}
		PartCombatGroup combatGroupOptional = unit.Unit.EntityData.GetCombatGroupOptional();
		if (combatGroupOptional?.Memory == null)
		{
			return;
		}
		foreach (UnitMovementAgent item in (from u in combatGroupOptional.Memory.UnitsList
			select u.Unit into u
			where u?.View != null
			select u.View.MovementAgent).Concat(combatGroupOptional.Select((BaseUnitEntity u) => u.View.MovementAgent)).Distinct())
		{
			TryConnectUnits(unit, item);
		}
	}

	private static bool GroupingForbidden(UnitMovementAgent u)
	{
		if (!(u.Unit == null) && !u.IsReallyMoving)
		{
			return u.Unit.IsProne;
		}
		return true;
	}

	private static void TryConnectUnits(UnitMovementAgent o1, UnitMovementAgent o2)
	{
		if (o1 == o2 || o1 == null || o2 == null || GroupingForbidden(o1) || GroupingForbidden(o2) || !s_UnitsAsObstacles.ContainsKey(o1) || !s_UnitsAsObstacles.ContainsKey(o2))
		{
			return;
		}
		PartFaction partFaction = ((o1.Unit != null) ? o1.Unit.EntityData.GetFactionOptional() : null);
		PartFaction partFaction2 = ((o2.Unit != null) ? o2.Unit.EntityData.GetFactionOptional() : null);
		if (!(partFaction == null) && !(partFaction2 == null) && !(partFaction != partFaction2))
		{
			float sqrMagnitude = (o1.transform.position - o2.transform.position).To2D().sqrMagnitude;
			float num = o1.Corpulence + o2.Corpulence + 0.65f;
			if (!(sqrMagnitude > num * num))
			{
				MergeGroups(o1, o2);
				MarkContacts(o1, o2);
			}
		}
	}

	private static void MarkContacts(UnitMovementAgent o1, UnitMovementAgent o2)
	{
		ObstacleGroupData obstacleGroupData = s_UnitsAsObstacles[o1];
		ObstacleGroupData obstacleGroupData2 = s_UnitsAsObstacles[o2];
		ObstacleGroupData obstacleGroupData3 = obstacleGroupData;
		if (obstacleGroupData3.UnitContacts == null)
		{
			obstacleGroupData3.UnitContacts = new List<UnitMovementAgent>();
		}
		obstacleGroupData3 = obstacleGroupData2;
		if (obstacleGroupData3.UnitContacts == null)
		{
			obstacleGroupData3.UnitContacts = new List<UnitMovementAgent>();
		}
		if (obstacleGroupData.UnitContacts.Contains(o2))
		{
			Debug.LogError("Adding obstacle twice");
		}
		if (obstacleGroupData2.UnitContacts.Contains(o1))
		{
			Debug.LogError("Adding obstacle twice");
		}
		obstacleGroupData.UnitContacts.Add(o2);
		obstacleGroupData2.UnitContacts.Add(o1);
	}

	private static void MergeGroups(UnitMovementAgent o1, UnitMovementAgent o2)
	{
		try
		{
			ObstacleGroupData d1 = s_UnitsAsObstacles[o1];
			ObstacleGroupData obstacleGroupData = s_UnitsAsObstacles[o2];
			if (d1.ObstaclesGroup == null && obstacleGroupData.ObstaclesGroup == null)
			{
				List<UnitMovementAgent> obstaclesGroup = new List<UnitMovementAgent> { o1, o2 };
				d1.ObstaclesGroup = obstaclesGroup;
				obstacleGroupData.ObstaclesGroup = obstaclesGroup;
			}
			else
			{
				if (d1.ObstaclesGroup == obstacleGroupData.ObstaclesGroup)
				{
					return;
				}
				if (d1.ObstaclesGroup == null)
				{
					obstacleGroupData.ObstaclesGroup.Add(o1);
					d1.ObstaclesGroup = obstacleGroupData.ObstaclesGroup;
					return;
				}
				if (obstacleGroupData.ObstaclesGroup == null)
				{
					d1.ObstaclesGroup.Add(o2);
					obstacleGroupData.ObstaclesGroup = d1.ObstaclesGroup;
					return;
				}
				d1.ObstaclesGroup.AddRange(obstacleGroupData.ObstaclesGroup);
				d1.ObstaclesGroup.ForEach(delegate(UnitMovementAgent o)
				{
					s_UnitsAsObstacles[o].ObstaclesGroup = d1.ObstaclesGroup;
				});
				obstacleGroupData.ObstaclesGroup = d1.ObstaclesGroup;
			}
		}
		finally
		{
			ValidateSameGroup(o1, o2);
		}
	}

	public static void ValidateSameGroup(UnitMovementAgent o1, UnitMovementAgent o2)
	{
		ObstacleGroupData obstacleGroupData = s_UnitsAsObstacles[o1];
		ObstacleGroupData obstacleGroupData2 = s_UnitsAsObstacles[o2];
		if (obstacleGroupData.ObstaclesGroup != obstacleGroupData2.ObstaclesGroup)
		{
			PFLog.Default.Error("Groups are different after merging");
		}
		if (obstacleGroupData.ObstaclesGroup == null || obstacleGroupData2.ObstaclesGroup == null)
		{
			return;
		}
		foreach (UnitMovementAgent item in obstacleGroupData.ObstaclesGroup)
		{
			if (s_UnitsAsObstacles[item].ObstaclesGroup != obstacleGroupData.ObstaclesGroup)
			{
				PFLog.Default.Error("Different groups for obstacles in one group");
			}
		}
	}
}
