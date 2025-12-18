using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Owlcat.Runtime.Core.Logging;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Groups;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class UnitGroup : ICombatGroup, IDisposable, IComparable<UnitGroup>, IHashable, IOwlPackable, IOwlPackable<UnitGroup>
{
	private readonly List<UnitReference> m_Units = new List<UnitReference>();

	private readonly MultiSet<BlueprintFaction> m_Factions = new MultiSet<BlueprintFaction>();

	private readonly List<BlueprintFaction> m_AttackFactions = new List<BlueprintFaction>();

	private bool m_IsEnemyForEveryone;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitGroup",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("Id", typeof(string)),
			new FieldInfo("Memory", typeof(UnitGroupMemory)),
			new FieldInfo("IsPlayerParty", typeof(bool)),
			new FieldInfo("IsPeaceful", typeof(bool)),
			new FieldInfo("Disposed", typeof(bool))
		}
	};

	[OwlPackInclude]
	public string Id { get; private set; }

	[OwlPackInclude]
	public UnitGroupMemory Memory { get; private set; }

	[OwlPackInclude]
	public bool IsPlayerParty { get; private set; }

	[OwlPackInclude]
	public bool IsPeaceful { get; private set; }

	[OwlPackInclude]
	public bool Disposed { get; private set; }

	[NotNull]
	public CountableFlag IsInCombat { get; } = new CountableFlag();


	public float LeaveCombatTimer { get; set; }

	public bool IsFollowingUnitInCombat { get; set; }

	public ReadonlyList<BlueprintFaction> AttackFactions => m_AttackFactions;

	public bool IsFake => Id == null;

	public int Count => m_Units.Count;

	public bool IsInFogOfWar => All((BaseUnitEntity u) => u.IsInFogOfWar);

	public ReadonlyList<UnitReference> Units => m_Units;

	[CanBeNull]
	public BaseUnitEntity this[int index] => m_Units[index].Entity.ToBaseUnitEntity();

	public UnitGroup(string id)
	{
		Id = id;
		Memory = new UnitGroupMemory(id);
		IsPlayerParty = Id == "<directly-controllable-unit>";
		IsPeaceful = Id == "<peaceful-unit>";
	}

	private UnitGroup(OwlPackConstructorParameter _)
	{
	}

	public bool Empty()
	{
		return m_Units.Count <= 0;
	}

	public void Add(BaseUnitEntity unit)
	{
		if (!IsFake && unit.CombatGroup.Id != Id)
		{
			PFLog.Default.Error($"Adding unit to wrong group: {unit}");
			return;
		}
		if (m_Units.Contains(unit.FromBaseUnitEntity()))
		{
			PFLog.Default.Error($"Group already contains unit: {unit}");
			return;
		}
		if (Disposed)
		{
			PFLog.Default.Error($"Adding unit to disposed group: {unit}");
			return;
		}
		m_Units.Add(unit.FromBaseUnitEntity());
		m_Units.Sort();
		if (unit.IsInCombat)
		{
			IsInCombat.Retain();
		}
		m_Factions.Add(unit.Faction.Blueprint);
		UpdateAttackFactionsCache();
	}

	public void Remove(BaseUnitEntity unit)
	{
		if (!Disposed)
		{
			m_Units.Remove(unit.FromBaseUnitEntity());
			if (unit.IsInCombat)
			{
				IsInCombat.Release();
			}
			m_Factions.Remove(unit.Faction.Blueprint);
			UpdateAttackFactionsCache();
		}
	}

	private static bool IsEnemy(UnitGroup g1, UnitGroup g2)
	{
		using (ProfileScope.New("UnitGroup.IsEnemy"))
		{
			if (g1.IsPeaceful || g2.IsPeaceful)
			{
				return false;
			}
			if (g1 == g2)
			{
				return false;
			}
			if (g1.m_IsEnemyForEveryone || g2.m_IsEnemyForEveryone)
			{
				return true;
			}
			if (g2.IsPlayerParty || g2.m_AttackFactions == null)
			{
				return false;
			}
			for (int i = 0; i < g2.m_AttackFactions.Count; i++)
			{
				BlueprintFaction blueprintFaction = g2.m_AttackFactions[i];
				if (blueprintFaction.AlwaysEnemy)
				{
					return true;
				}
				if (g1.m_Factions.Contains(blueprintFaction))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsEnemy(UnitGroup group)
	{
		if (!IsEnemy(this, group))
		{
			return IsEnemy(group, this);
		}
		return true;
	}

	public bool IsEnemy(MechanicEntity entity)
	{
		UnitGroup unitGroup = entity.GetCombatGroupOptional()?.Group;
		if (unitGroup != null)
		{
			return IsEnemy(unitGroup);
		}
		return false;
	}

	public bool Any(Func<BaseUnitEntity, bool> pred)
	{
		for (int i = 0; i < m_Units.Count; i++)
		{
			IAbstractUnitEntity entity = m_Units[i].Entity;
			if (entity != null && pred(entity.ToBaseUnitEntity()))
			{
				return true;
			}
		}
		return false;
	}

	public bool All(Func<BaseUnitEntity, bool> pred)
	{
		try
		{
			for (int i = 0; i < m_Units.Count; i++)
			{
				IAbstractUnitEntity entity = m_Units[i].Entity;
				if (entity != null && !pred(entity.ToBaseUnitEntity()))
				{
					return false;
				}
			}
			return true;
		}
		finally
		{
		}
	}

	public IEnumerable<T> Select<T>(Func<BaseUnitEntity, T> select)
	{
		for (int i = 0; i < m_Units.Count; i++)
		{
			IAbstractUnitEntity entity = m_Units[i].Entity;
			if (entity != null)
			{
				yield return select(entity.ToBaseUnitEntity());
			}
		}
	}

	public void ForEach(Action<BaseUnitEntity> action)
	{
		for (int i = 0; i < m_Units.Count; i++)
		{
			IAbstractUnitEntity entity = m_Units[i].Entity;
			if (entity != null)
			{
				action(entity.ToBaseUnitEntity());
			}
		}
	}

	public bool HasLOS(BaseUnitEntity unit)
	{
		for (int i = 0; i < m_Units.Count; i++)
		{
			BaseUnitEntity baseUnitEntity = m_Units[i].Entity.ToBaseUnitEntity();
			if (baseUnitEntity != null && baseUnitEntity.IsInGame && !baseUnitEntity.IsHelpless && baseUnitEntity.Vision.HasLOS(unit))
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateAttackFactionsCache()
	{
		m_IsEnemyForEveryone = false;
		m_AttackFactions.Clear();
		if (IsPeaceful)
		{
			return;
		}
		foreach (UnitReference unit in m_Units)
		{
			BaseUnitEntity baseUnitEntity = unit.Entity.ToBaseUnitEntity();
			if (baseUnitEntity == null)
			{
				LogChannel.Default.Error("UnitGroup.UpdateAttackFactionsCache: can't dereference " + unit.Id);
				continue;
			}
			if (baseUnitEntity.Faction.EnemyForEveryone)
			{
				m_IsEnemyForEveryone = true;
			}
			foreach (BlueprintFaction attackFaction in baseUnitEntity.Faction.AttackFactions)
			{
				if (!m_AttackFactions.HasItem(attackFaction))
				{
					m_AttackFactions.Add(attackFaction);
				}
			}
		}
		Memory.ClearEnemies();
	}

	public void ResetFactionSet()
	{
		m_Factions.Clear();
		foreach (UnitReference unit in m_Units)
		{
			if (unit.Entity != null)
			{
				m_Factions.Add(unit.Entity.ToBaseUnitEntity().Faction.Blueprint);
			}
		}
	}

	public void ExtractNearUnits(HashSet<BaseUnitEntity> result)
	{
		UnitGroupEnumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IList<BaseUnitEntity> values = enumerator.Current.Vision.CanBeInRange.Values;
			for (int i = 0; i < values.Count; i++)
			{
				BaseUnitEntity baseUnitEntity = values[i];
				if (baseUnitEntity != null)
				{
					BaseUnitEntity item = baseUnitEntity;
					result.Add(item);
				}
			}
		}
	}

	public bool HasEnemyInCombat()
	{
		return Game.Instance.UnitGroups.Any((UnitGroup other) => other != this && (bool)other.IsInCombat && other.IsEnemy(this));
	}

	public void Dispose()
	{
		m_Units.Clear();
		Memory.Clear();
		Disposed = true;
	}

	public UnitGroupEnumerator GetEnumerator()
	{
		return new UnitGroupEnumerator(m_Units);
	}

	public override string ToString()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append("UnitGroup[");
		bool flag = true;
		foreach (UnitReference unit in m_Units)
		{
			if (!flag)
			{
				builder.Append(", ");
			}
			flag = false;
			string value = ((unit.Entity != null) ? unit.Entity.ToBaseUnitEntity().Blueprint.name : "<missing>");
			builder.Append(value);
		}
		builder.Append("]");
		return builder.ToString();
	}

	int IComparable<UnitGroup>.CompareTo(UnitGroup other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		return string.Compare(Id, other.Id, StringComparison.Ordinal);
	}

	public Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitGroup source = new UnitGroup(default(OwlPackConstructorParameter));
		result = Unsafe.As<UnitGroup, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<UnitGroup>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = Id;
		formatter.StringField(0, "Id", ref value, state);
		UnitGroupMemory value2 = Memory;
		formatter.Field(1, "Memory", ref value2, state);
		bool value3 = IsPlayerParty;
		formatter.UnmanagedField(2, "IsPlayerParty", ref value3, state);
		bool value4 = IsPeaceful;
		formatter.UnmanagedField(3, "IsPeaceful", ref value4, state);
		bool value5 = Disposed;
		formatter.UnmanagedField(4, "Disposed", ref value5, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitGroup>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				Id = formatter.ReadString(state);
				break;
			case 1:
				Memory = formatter.ReadPackable<UnitGroupMemory>(state);
				break;
			case 2:
				IsPlayerParty = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				IsPeaceful = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				Disposed = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
