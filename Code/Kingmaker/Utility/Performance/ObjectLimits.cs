using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Utility.Performance;

public static class ObjectLimits
{
	public class Entry
	{
		public readonly Func<int> Getter;

		public readonly string Name;

		public readonly int Threshold;

		public Entry(string name, int threshold, [NotNull] Func<int> getter)
		{
			Getter = getter;
			Name = name;
			Threshold = threshold;
		}

		public static implicit operator Entry((string, int, Func<int>) value)
		{
			return new Entry(value.Item1, value.Item2, value.Item3);
		}
	}

	public static readonly Entry[] Entries = new Entry[11]
	{
		("TOTAL UNITS", 200, (Func<int>)(() => Game.Instance.EntityPools.AllUnits.Count())),
		("NORMAL UNITS", 75, (Func<int>)(() => Game.Instance.EntityPools.AllUnits.Count((AbstractUnitEntity i) => i is BaseUnitEntity && !i.IsExtra))),
		("EXTRA UNITS", 125, (Func<int>)(() => Game.Instance.EntityPools.AllUnits.Count((AbstractUnitEntity i) => i is LightweightUnitEntity || i.IsExtra))),
		("AWAKE UNITS", 45, (Func<int>)(() => Game.Instance.EntityPools.AllAwakeUnits.Count)),
		("COMBAT GROUPS", 10, (Func<int>)(() => Game.Instance.UnitGroups.Awake.Count)),
		("COMBAT UNITS", 35, (Func<int>)(() => Game.Instance.EntityPools.AllUnits.Count((AbstractUnitEntity i) => i.IsInCombat))),
		("MAP OBJECTS", 100, (Func<int>)(() => Game.Instance.EntityPools.MapObjects.Count((MapObjectEntity i) => !(i is ScriptZoneEntity)))),
		("SCRIPT ZONES", 50, (Func<int>)(() => Game.Instance.EntityPools.ScriptZones.Count() + Game.Instance.EntityPools.AreaEffects.Count())),
		("AREA EFFECTS", 25, (Func<int>)(() => Game.Instance.EntityPools.AreaEffects.Count())),
		("CROWD UNITS", 1000, (Func<int>)(() => Game.Instance.GpuCrowdController.CountAllCrowdsUnits(withShadows: false))),
		("CROWD UNITS W/SHADOWS", 500, (Func<int>)(() => Game.Instance.GpuCrowdController.CountAllCrowdsUnits(withShadows: true)))
	};

	private static PersistentState State => Game.Instance.State;
}
