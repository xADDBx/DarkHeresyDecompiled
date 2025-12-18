using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Squads;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitSquad : MechanicEntity, ICombatParticipant, IHashable, IOwlPackable<UnitSquad>
{
	public class SquadTurnControlData
	{
		private readonly UnitSquad m_Squad;

		private readonly List<Entity> m_UnitsReady = new List<Entity>();

		private readonly HashSet<GraphNode> m_ReservedNodes = new HashSet<GraphNode>();

		public TargetWrapper CommonTarget { get; set; }

		public bool IsAllReady => m_Squad.Units.Where((UnitReference ur) => ur.ToBaseUnitEntity().CanActInTurnBased).All((UnitReference ur) => m_UnitsReady.Contains(ur.ToBaseUnitEntity()));

		public SquadTurnControlData(UnitSquad squad)
		{
			m_Squad = squad;
		}

		public bool IsReserved(GraphNode node)
		{
			return m_ReservedNodes.Contains(node);
		}

		public bool TryReserve(GraphNode node)
		{
			return m_ReservedNodes.Add(node);
		}

		public void SetReady(Entity unit)
		{
			m_UnitsReady.Add(unit);
		}

		public void Reset()
		{
			m_UnitsReady.Clear();
			m_ReservedNodes.Clear();
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	private readonly List<UnitReference> m_Units = new List<UnitReference>();

	[JsonProperty]
	[OwlPackInclude]
	private UnitReference m_Leader;

	[NotNull]
	public readonly SquadTurnControlData Data;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitSquad",
		OldNames = null,
		Fields = new FieldInfo[17]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_Initiative", typeof(Initiative)),
			new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("MainFact", typeof(MechanicEntityFact)),
			new FieldInfo("Id", typeof(string)),
			new FieldInfo("m_Units", typeof(List<UnitReference>)),
			new FieldInfo("m_Leader", typeof(UnitReference))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public string Id { get; private set; }

	[CanBeNull]
	public BaseUnitEntity Leader
	{
		get
		{
			return m_Leader.ToBaseUnitEntity();
		}
		set
		{
			m_Leader = value.FromBaseUnitEntity();
		}
	}

	public ReadonlyList<UnitReference> Units => m_Units.Where((UnitReference x) => x.Entity.ToBaseUnitEntity().IsInGame).ToList();

	public int Count => Units.Count;

	public override bool IsInCombat => Units.HasItem((UnitReference i) => i.Entity?.IsInCombat ?? false);

	[NotNull]
	public MechanicEntity InitiativeRoller => (MechanicEntity)(Leader ?? ((object)m_Units.FirstItem((UnitReference i) => i.Entity != null).Entity.ToBaseUnitEntity()) ?? ((object)this));

	public override bool NeedsView => false;

	public static UnitSquad GetOrCreate(string id)
	{
		MechanicEntity mechanicEntity = Game.Instance.EntityPools.CombatParticipants.All.FirstItem((MechanicEntity s) => s is UnitSquad unitSquad2 && unitSquad2.Id.Equals(id));
		if (mechanicEntity != null)
		{
			return mechanicEntity as UnitSquad;
		}
		UnitSquad unitSquad = Entity.Initialize(new UnitSquad(id));
		Game.Instance.State.LoadedAreaState.MainState.AddEntityData(unitSquad);
		return unitSquad;
	}

	private UnitSquad(string id)
		: base(id, isInGame: true, ConfigRoot.Instance.SystemMechanics.EmptyMechanicEntity)
	{
		Id = id;
		Data = new SquadTurnControlData(this);
	}

	protected UnitSquad()
	{
		Data = new SquadTurnControlData(this);
	}

	public void Add(BaseUnitEntity unit)
	{
		if (string.IsNullOrEmpty(Id) || unit.GetSquadOptional()?.Id != Id)
		{
			return;
		}
		UnitReference item = unit.FromBaseUnitEntity();
		if (m_Units.Contains(item))
		{
			PFLog.Default.Error($"Squad already contains unit: {unit}");
			return;
		}
		if (base.Initiative.Empty)
		{
			base.Initiative.Value = unit.Initiative.Value;
			base.Initiative.LastTurn = unit.Initiative.LastTurn;
		}
		m_Units.Add(item);
	}

	public void Remove(BaseUnitEntity unit)
	{
		UnitReference item = unit.FromBaseUnitEntity();
		m_Units.Remove(item);
		if (unit == Leader)
		{
			Leader = null;
		}
		if (m_Units.Empty())
		{
			HandleDestroy();
			Dispose();
		}
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(Id);
		List<UnitReference> units = m_Units;
		if (units != null)
		{
			for (int i = 0; i < units.Count; i++)
			{
				UnitReference obj = units[i];
				Hash128 val2 = UnitReferenceHasher.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		UnitReference obj2 = m_Leader;
		Hash128 val3 = UnitReferenceHasher.GetHash128(ref obj2);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitSquad source = new UnitSquad();
		result = Unsafe.As<UnitSquad, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<UnitSquad>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "m_Initiative", ref m_Initiative, state);
		formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
		formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
		MechanicEntityFact value2 = base.MainFact;
		formatter.Field(13, "MainFact", ref value2, state);
		string value3 = Id;
		formatter.StringField(14, "Id", ref value3, state);
		List<UnitReference> value4 = m_Units;
		formatter.Field(15, "m_Units", ref value4, state);
		formatter.Field(16, "m_Leader", ref m_Leader, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitSquad>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				m_Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 11:
				m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 12:
				m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 13:
				base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
				break;
			case 14:
				Id = formatter.ReadString(state);
				break;
			case 15:
				Unsafe.AsRef(in m_Units) = formatter.ReadPackable<List<UnitReference>>(state);
				break;
			case 16:
				m_Leader = formatter.ReadPackable<UnitReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
