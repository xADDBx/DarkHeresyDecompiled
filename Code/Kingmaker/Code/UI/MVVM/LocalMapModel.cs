using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public static class LocalMapModel
{
	private static readonly HashSet<ILocalMapMarker> s_MapObjectMarkers = new HashSet<ILocalMapMarker>();

	public static IEnumerable<ILocalMapMarker> MapObjectMarkers => s_MapObjectMarkers.Where((ILocalMapMarker m) => IsInCurrentArea(m.GetPosition()));

	public static IEnumerable<BaseUnitEntity> PartyForMarkers
	{
		get
		{
			IEnumerable<BaseUnitEntity> enumerable = Game.Instance.Player.PartyAndPets;
			if (Game.Instance.Player.CapitalPartyMode)
			{
				enumerable = enumerable.Concat(Game.Instance.Player.RemoteCompanions.Where((BaseUnitEntity u) => !u.IsCustomCompanion()));
			}
			return enumerable.Where((BaseUnitEntity u) => !u.LifeState.IsHiddenBecauseDead && u.IsInGame && IsInCurrentArea(u.Position));
		}
	}

	public static List<UnitGroupMemory.UnitInfo> ValidatedNonPartyUnitsForMarkers => PossibleNonPartyUnitsForMarkers.Where((UnitGroupMemory.UnitInfo u) => CheckValidForLocalMap(u.Unit)).ToList();

	public static List<UnitGroupMemory.UnitInfo> PossibleNonPartyUnitsForMarkers => Game.Instance.Player.MainCharacterEntity.CombatGroup.Memory.UnitsList;

	public static bool CheckValidForLocalMap(BaseUnitEntity unit)
	{
		if (!unit.Faction.IsPlayer && unit.IsVisibleForPlayer && !unit.LifeState.IsDead)
		{
			return IsInCurrentArea(unit.Position);
		}
		return false;
	}

	public static bool IsInCurrentArea(Vector3 pos)
	{
		return Game.Instance.CurrentlyLoadedAreaPart.Bounds.LocalMapBounds.ContainsXZ(pos);
	}

	public static void Add(ILocalMapMarker marker)
	{
		if (marker.GetMarkerType() != LocalMapMarkType.Invalid)
		{
			s_MapObjectMarkers.Add(marker);
		}
	}

	public static void Remove(ILocalMapMarker marker)
	{
		s_MapObjectMarkers.Remove(marker);
	}

	public static void Clear()
	{
		s_MapObjectMarkers.Clear();
	}
}
