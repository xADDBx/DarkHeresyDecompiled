using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers.Units;

public class UnitGroupsHolder : IEnumerable<UnitGroup>, IEnumerable
{
	private readonly List<UnitGroup> m_All = new List<UnitGroup>();

	private readonly List<UnitGroup> m_Awake = new List<UnitGroup>();

	public ReadonlyList<UnitGroup> All => m_All;

	public ReadonlyList<UnitGroup> Awake => m_Awake;

	public UnitGroup Party => GetGroup("<directly-controllable-unit>");

	public UnitGroup GetGroup(string id)
	{
		string id = id;
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		UnitGroup unitGroup = m_All.FirstOrDefault((UnitGroup g) => g.Id.Equals(id));
		if (unitGroup == null)
		{
			unitGroup = new UnitGroup(id);
			m_All.Add(unitGroup);
			m_All.Sort();
		}
		return unitGroup;
	}

	public void RestoreGroup(UnitGroup group)
	{
		UnitGroup group = group;
		UnitGroup unitGroup = m_All.FirstItem<UnitGroup>((UnitGroup i) => i.Id == group.Id);
		if (unitGroup != null)
		{
			IAbstractUnitEntity[] array = unitGroup.Units.Select((UnitReference i) => i.Entity).NotNull().ToArray();
			m_All.Remove(unitGroup);
			unitGroup.Dispose();
			IAbstractUnitEntity[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				BaseUnitEntity baseUnitEntity = (BaseUnitEntity)array2[j];
				group.Add(baseUnitEntity);
				baseUnitEntity.CombatGroup.EnsureAndUpdateGroup();
			}
		}
		m_All.Add(group);
		m_All.Sort();
	}

	public void Cleanup()
	{
		m_All.RemoveAll(delegate(UnitGroup g)
		{
			bool num = g.Empty();
			if (num)
			{
				g.Dispose();
			}
			return num;
		});
	}

	public void Clear()
	{
		foreach (UnitGroup item in All)
		{
			item.Dispose();
		}
		m_All.Clear();
		m_Awake.Clear();
	}

	public void UpdateAwakeGroups()
	{
		m_Awake.Clear();
		foreach (UnitGroup item in m_All)
		{
			for (int i = 0; i < item.Count; i++)
			{
				BaseUnitEntity baseUnitEntity = item[i];
				if (baseUnitEntity != null && !baseUnitEntity.IsSleeping && !baseUnitEntity.Faction.Peaceful && (!baseUnitEntity.IsInFogOfWar || baseUnitEntity.IsInCombat || baseUnitEntity.AwakeTimer > 0f))
				{
					m_Awake.Add(item);
					break;
				}
			}
		}
	}

	IEnumerator<UnitGroup> IEnumerable<UnitGroup>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public List<UnitGroup>.Enumerator GetEnumerator()
	{
		return m_All.GetEnumerator();
	}
}
