using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.View.Roaming;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartRoaming : EntityPart<AbstractUnitEntity>, IHashable, IOwlPackable<UnitPartRoaming>
{
	[CanBeNull]
	[JsonProperty]
	[HasherCustom(Type = typeof(IRoamingPointHasher))]
	[OwlPackInclude]
	private IRoamingPoint m_PersistentNextPoint;

	[JsonProperty]
	[OwlPackInclude]
	private string m_PersistentNextPointId = "";

	[JsonProperty]
	[OwlPackInclude]
	private bool m_Disabled;

	[CanBeNull]
	private IRoamingPoint m_NextPoint;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartRoaming",
		OldNames = null,
		Fields = new FieldInfo[7]
		{
			new FieldInfo("m_PersistentNextPoint", typeof(IRoamingPoint)),
			new FieldInfo("m_PersistentNextPointId", typeof(string)),
			new FieldInfo("m_Disabled", typeof(bool)),
			new FieldInfo("OriginalPoint", typeof(Vector3)),
			new FieldInfo("ReverseDirection", typeof(bool)),
			new FieldInfo("IdleCutscene", typeof(CutscenePlayerData)),
			new FieldInfo("IdleEndTime", typeof(TimeSpan))
		}
	};

	[CanBeNull]
	public IRoamingPoint NextPoint
	{
		get
		{
			return m_NextPoint;
		}
		set
		{
			m_NextPoint = value;
			if (value is RoamingWaypointData roamingWaypointData)
			{
				m_PersistentNextPointId = roamingWaypointData.UniqueId;
				m_PersistentNextPoint = null;
			}
			else
			{
				m_PersistentNextPointId = "";
				m_PersistentNextPoint = value;
			}
		}
	}

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public Vector3 OriginalPoint { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool ReverseDirection { get; set; }

	[CanBeNull]
	[JsonProperty]
	[OwlPackInclude]
	public CutscenePlayerData IdleCutscene { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan IdleEndTime { get; set; }

	[CanBeNull]
	public RoamingUnitSettings Settings { get; set; }

	public Vector3? CachedTargetPosition { get; set; }

	public ABPath PathInProcess { get; set; }

	public bool Disabled
	{
		get
		{
			if (!m_Disabled)
			{
				return base.Owner.GetOptional<UnitPartFollowUnit>() != null;
			}
			return true;
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		IRoamingPoint nextPoint;
		if (string.IsNullOrEmpty(m_PersistentNextPointId))
		{
			nextPoint = m_PersistentNextPoint;
		}
		else
		{
			IRoamingPoint entity = EntityService.Instance.GetEntity<RoamingWaypointData>(m_PersistentNextPointId);
			nextPoint = entity;
		}
		m_NextPoint = nextPoint;
	}

	public void SetEnabled(bool enabled)
	{
		m_Disabled = !enabled;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = IRoamingPointHasher.GetHash128(m_PersistentNextPoint);
		result.Append(ref val2);
		result.Append(m_PersistentNextPointId);
		result.Append(ref m_Disabled);
		Vector3 val3 = OriginalPoint;
		result.Append(ref val3);
		bool val4 = ReverseDirection;
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<CutscenePlayerData>.GetHash128(IdleCutscene);
		result.Append(ref val5);
		TimeSpan val6 = IdleEndTime;
		result.Append(ref val6);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartRoaming source = new UnitPartRoaming();
		result = Unsafe.As<UnitPartRoaming, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartRoaming>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_PersistentNextPoint", ref m_PersistentNextPoint, state);
		formatter.StringField(1, "m_PersistentNextPointId", ref m_PersistentNextPointId, state);
		formatter.UnmanagedField(2, "m_Disabled", ref m_Disabled, state);
		Vector3 value = OriginalPoint;
		formatter.Field(3, "OriginalPoint", ref value, state);
		bool value2 = ReverseDirection;
		formatter.UnmanagedField(4, "ReverseDirection", ref value2, state);
		CutscenePlayerData value3 = IdleCutscene;
		formatter.Field(5, "IdleCutscene", ref value3, state);
		TimeSpan value4 = IdleEndTime;
		formatter.Field(6, "IdleEndTime", ref value4, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartRoaming>();
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
				m_PersistentNextPoint = formatter.ReadPackable<IRoamingPoint>(state);
				break;
			case 1:
				m_PersistentNextPointId = formatter.ReadString(state);
				break;
			case 2:
				m_Disabled = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				OriginalPoint = formatter.ReadPackable<Vector3>(state);
				break;
			case 4:
				ReverseDirection = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				IdleCutscene = formatter.ReadPackable<CutscenePlayerData>(state);
				break;
			case 6:
				IdleEndTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
