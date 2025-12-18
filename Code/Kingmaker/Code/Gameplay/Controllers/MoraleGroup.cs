using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;

namespace Kingmaker.Code.Gameplay.Controllers;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class MoraleGroup : IOwlPackable, IOwlPackable<MoraleGroup>
{
	public readonly int ID;

	[OwlPackInclude]
	private List<EntityRef<BaseUnitEntity>> _units = new List<EntityRef<BaseUnitEntity>>();

	[OwlPackInclude]
	private EntityRef<BaseUnitEntity?> _leader;

	private HashSet<UnitGroup>? _combatGroupsCache;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MoraleGroup",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("_units", typeof(List<EntityRef<BaseUnitEntity>>)),
			new FieldInfo("_leader", typeof(EntityRef<BaseUnitEntity>)),
			new FieldInfo("PowerValue", typeof(float)),
			new FieldInfo("MostPowerfulEnemy", typeof(float)),
			new FieldInfo("PowerBalanceState", typeof(PowerBalanceState))
		}
	};

	[OwlPackInclude]
	public float PowerValue { get; set; }

	[OwlPackInclude]
	public float MostPowerfulEnemy { get; set; }

	[OwlPackInclude]
	public PowerBalanceState PowerBalanceState { get; set; }

	private HashSet<UnitGroup> CombatGroups => EnsureCombatGroupsCache();

	public IEnumerable<BaseUnitEntity> Units => _units.Select<EntityRef<BaseUnitEntity>, BaseUnitEntity>((EntityRef<BaseUnitEntity> i) => i.Entity).NotNull();

	public BaseUnitEntity? Leader => _leader.Entity;

	public IEnumerable<BlueprintFaction> Factions => Units.Select((BaseUnitEntity x) => x.Faction.Blueprint).Distinct();

	public bool IsEmpty => !_units.HasItem<EntityRef<BaseUnitEntity>>((EntityRef<BaseUnitEntity> i) => i.Entity != null);

	public bool IsPlayerGroup => _units.HasItem<EntityRef<BaseUnitEntity>>((EntityRef<BaseUnitEntity> i) => i.Entity?.IsPlayerFaction ?? false);

	public bool IsPlayerEnemy => _units.HasItem<EntityRef<BaseUnitEntity>>((EntityRef<BaseUnitEntity> i) => i.Entity?.IsPlayerEnemy ?? false);

	public bool AllUnitsHasBrokenMorale => Units.All((BaseUnitEntity i) => i.IsDeadOrUnconscious || i.Morale.Phase == MoralePhaseType.Broken);

	public bool GroupCanSurrender => Units.All((BaseUnitEntity i) => i.IsDeadOrUnconscious || !i.Features.DoesNotSurrender);

	public bool IsLeaderDeadOrUnconscious => Leader?.IsDeadOrUnconscious ?? false;

	public MoraleGroup(int id)
	{
		ID = id;
	}

	private MoraleGroup(OwlPackConstructorParameter _)
	{
	}

	public bool Contains(BaseUnitEntity unit)
	{
		return _units.Contains(unit);
	}

	public void Add(BaseUnitEntity unit)
	{
		_units.AddUnique(unit);
		CombatGroups.Add(unit.CombatGroup.Group);
	}

	public void Remove(BaseUnitEntity unit)
	{
		BaseUnitEntity unit = unit;
		if (_units.Remove(unit))
		{
			if (Leader == unit)
			{
				ClearLeader();
			}
			if (!_units.HasItem<EntityRef<BaseUnitEntity>>((EntityRef<BaseUnitEntity> i) => i.Entity?.CombatGroup?.Group == unit.CombatGroup.Group))
			{
				CombatGroups.Remove(unit.CombatGroup.Group);
			}
		}
	}

	public bool ContainsCombatGroup(UnitGroup group)
	{
		return CombatGroups.Contains(group);
	}

	public bool IsSuitableFor(BaseUnitEntity unit)
	{
		if (ContainsCombatGroup(unit.CombatGroup.Group))
		{
			return true;
		}
		foreach (UnitGroup combatGroup in CombatGroups)
		{
			if (combatGroup.IsEnemy(unit))
			{
				return false;
			}
			bool flag = combatGroup.IsEnemy(Game.Instance.Player.MainCharacterEntity.CombatGroup.Group);
			if (unit.IsPlayerEnemy != flag)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEnemy(MoraleGroup otherGroup)
	{
		foreach (UnitGroup combatGroup in CombatGroups)
		{
			foreach (UnitGroup combatGroup2 in otherGroup.CombatGroups)
			{
				if (combatGroup.IsEnemy(combatGroup2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public HashSet<UnitGroup> EnsureCombatGroupsCache()
	{
		if (_combatGroupsCache == null)
		{
			_combatGroupsCache = new HashSet<UnitGroup>();
			foreach (BaseUnitEntity unit in Units)
			{
				_combatGroupsCache.Add(unit.CombatGroup.Group);
			}
		}
		return _combatGroupsCache;
	}

	public void SetLeader(BaseUnitEntity unit)
	{
		if (Leader != unit)
		{
			if (!_units.Contains(unit))
			{
				throw new InvalidOperationException();
			}
			if (Leader != null)
			{
				ClearLeader();
			}
			unit.Buffs.Add(ConfigRoot.Instance.MoraleRoot.LeaderBuff, unit);
			_leader = unit;
		}
	}

	public void ClearLeader()
	{
		BaseUnitEntity leader = Leader;
		if (leader != null)
		{
			leader.Buffs.Remove(ConfigRoot.Instance.MoraleRoot.LeaderBuff);
			_leader = null;
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MoraleGroup source = new MoraleGroup(default(OwlPackConstructorParameter));
		result = Unsafe.As<MoraleGroup, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MoraleGroup>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "_units", ref _units, state);
		formatter.Field(1, "_leader", ref _leader, state);
		float value = PowerValue;
		formatter.UnmanagedField(2, "PowerValue", ref value, state);
		float value2 = MostPowerfulEnemy;
		formatter.UnmanagedField(3, "MostPowerfulEnemy", ref value2, state);
		PowerBalanceState value3 = PowerBalanceState;
		formatter.EnumField(4, "PowerBalanceState", ref value3, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MoraleGroup>();
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
				_units = formatter.ReadPackable<List<EntityRef<BaseUnitEntity>>>(state);
				break;
			case 1:
				_leader = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 2:
				PowerValue = formatter.ReadUnmanaged<float>(state);
				break;
			case 3:
				MostPowerfulEnemy = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				PowerBalanceState = formatter.ReadEnum<PowerBalanceState>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
