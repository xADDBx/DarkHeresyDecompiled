using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartPropheticIntervention : BaseUnitPart, IHashable, IOwlPackable<UnitPartPropheticIntervention>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class PropheticInterventionEntry : IHashable, IOwlPackable, IOwlPackable<PropheticInterventionEntry>
	{
		[JsonProperty]
		[OwlPackInclude]
		private EntityRef<UnitEntity> m_DeadTargetRef;

		[JsonProperty]
		[OwlPackInclude]
		private Vector3 m_OldPosition;

		[JsonProperty]
		[OwlPackInclude]
		private bool m_IsOldPositionSet;

		[JsonProperty(PropertyName = "DeadTarget")]
		[OwlPackInclude]
		private UnitEntity m_DeadTarget;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "PropheticInterventionEntry",
			OldNames = null,
			Fields = new FieldInfo[5]
			{
				new FieldInfo("m_DeadTargetRef", typeof(EntityRef<UnitEntity>)),
				new FieldInfo("m_OldPosition", typeof(Vector3)),
				new FieldInfo("m_IsOldPositionSet", typeof(bool)),
				new FieldInfo("WoundsBeforeLastAttack", typeof(int)),
				new FieldInfo("m_DeadTarget", typeof(UnitEntity))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public int WoundsBeforeLastAttack { get; set; }

		public UnitEntity DeadTarget
		{
			get
			{
				return m_DeadTargetRef;
			}
			set
			{
				m_DeadTargetRef = value;
			}
		}

		public GridNodeBase OldPosition
		{
			[CanBeNull]
			get
			{
				if (!m_IsOldPositionSet)
				{
					return null;
				}
				return m_OldPosition.GetNearestNodeXZUnwalkable();
			}
			set
			{
				m_OldPosition = value.Vector3Position();
				m_IsOldPositionSet = true;
			}
		}

		public void OnPostLoad()
		{
			if (m_DeadTarget != null)
			{
				m_DeadTargetRef = new EntityRef<UnitEntity>(m_DeadTarget);
				m_DeadTarget = null;
				PFLog.Default.Log($"Convert DeadTarget property to ref. DeadTarget={DeadTarget}");
			}
			if (!m_IsOldPositionSet && m_DeadTargetRef.Entity != null)
			{
				m_OldPosition = m_DeadTargetRef.Entity.Position;
				m_IsOldPositionSet = true;
				PFLog.Default.Log($"Convert OldPosition property to Vector3. Defaulting to DeadTarget position {m_OldPosition}. DeadTarget={DeadTarget}");
			}
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			EntityRef<UnitEntity> obj = m_DeadTargetRef;
			Hash128 val = StructHasher<EntityRef<UnitEntity>>.GetHash128(ref obj);
			result.Append(ref val);
			result.Append(ref m_OldPosition);
			result.Append(ref m_IsOldPositionSet);
			int val2 = WoundsBeforeLastAttack;
			result.Append(ref val2);
			Hash128 val3 = ClassHasher<UnitEntity>.GetHash128(m_DeadTarget);
			result.Append(ref val3);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			PropheticInterventionEntry source = new PropheticInterventionEntry();
			result = Unsafe.As<PropheticInterventionEntry, TPossiblyBase>(ref source);
		}

		public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<PropheticInterventionEntry>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "m_DeadTargetRef", ref m_DeadTargetRef, state);
			formatter.Field(1, "m_OldPosition", ref m_OldPosition, state);
			formatter.UnmanagedField(2, "m_IsOldPositionSet", ref m_IsOldPositionSet, state);
			int value = WoundsBeforeLastAttack;
			formatter.UnmanagedField(3, "WoundsBeforeLastAttack", ref value, state);
			formatter.Field(4, "m_DeadTarget", ref m_DeadTarget, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PropheticInterventionEntry>();
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
					m_DeadTargetRef = formatter.ReadPackable<EntityRef<UnitEntity>>(state);
					break;
				case 1:
					m_OldPosition = formatter.ReadPackable<Vector3>(state);
					break;
				case 2:
					m_IsOldPositionSet = formatter.ReadUnmanaged<bool>(state);
					break;
				case 3:
					WoundsBeforeLastAttack = formatter.ReadUnmanaged<int>(state);
					break;
				case 4:
					m_DeadTarget = formatter.ReadPackable<UnitEntity>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	public List<PropheticInterventionEntry> Entries = new List<PropheticInterventionEntry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartPropheticIntervention",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Entries", typeof(List<PropheticInterventionEntry>))
		}
	};

	public void NewEntry(UnitEntity deadTarget, int woundsBeforeLastAttack, GridNodeBase oldPosition)
	{
		PropheticInterventionEntry item = new PropheticInterventionEntry
		{
			DeadTarget = deadTarget,
			WoundsBeforeLastAttack = woundsBeforeLastAttack,
			OldPosition = oldPosition
		};
		Entries.Add(item);
	}

	public void RemoveEntry(UnitEntity deadTarget)
	{
		Entries.RemoveAll((PropheticInterventionEntry p) => p.DeadTarget == deadTarget);
	}

	public bool HasEntry(UnitEntity deadTarget)
	{
		return Entries.Any((PropheticInterventionEntry p) => p.DeadTarget == deadTarget);
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		foreach (PropheticInterventionEntry entry in Entries)
		{
			entry.OnPostLoad();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<PropheticInterventionEntry> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<PropheticInterventionEntry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartPropheticIntervention source = new UnitPartPropheticIntervention();
		result = Unsafe.As<UnitPartPropheticIntervention, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartPropheticIntervention>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "Entries", ref Entries, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartPropheticIntervention>();
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
				Entries = formatter.ReadPackable<List<PropheticInterventionEntry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
