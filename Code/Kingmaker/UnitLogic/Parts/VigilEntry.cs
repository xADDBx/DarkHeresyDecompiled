using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.Pathfinding;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class VigilEntry : IHashable, IOwlPackable, IOwlPackable<VigilEntry>
{
	[JsonProperty]
	[OwlPackInclude]
	private Vector3 m_OldPosition;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsOldPositionSet;

	[JsonProperty]
	[OwlPackInclude]
	private EntityFactRef<Buff> m_BuffRef;

	[JsonProperty(PropertyName = "Buff")]
	[OwlPackInclude]
	private Buff m_Buff;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "VigilEntry",
		OldNames = null,
		Fields = new FieldInfo[6]
		{
			new FieldInfo("m_OldPosition", typeof(Vector3)),
			new FieldInfo("m_IsOldPositionSet", typeof(bool)),
			new FieldInfo("m_BuffRef", typeof(EntityFactRef<Buff>)),
			new FieldInfo("OldDamage", typeof(int)),
			new FieldInfo("TeleportAbility", typeof(BlueprintAbility)),
			new FieldInfo("m_Buff", typeof(Buff))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public int OldDamage { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintAbility TeleportAbility { get; set; }

	public Buff Buff
	{
		get
		{
			return m_BuffRef.Fact;
		}
		set
		{
			m_BuffRef = value;
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
		if (m_Buff != null)
		{
			m_BuffRef = new EntityFactRef<Buff>(m_Buff);
			m_Buff = null;
			PFLog.Default.Log($"Convert Buff property to ref. Buff={Buff}, buff owner={m_BuffRef.Entity}");
		}
		if (!m_IsOldPositionSet && m_BuffRef.Entity != null)
		{
			m_OldPosition = m_BuffRef.Entity.Position;
			m_IsOldPositionSet = true;
			PFLog.Default.Log($"Convert OldPosition property to Vector3. Defaulting to buff owner position {m_OldPosition}. Buff owner={m_BuffRef.Entity}");
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_OldPosition);
		result.Append(ref m_IsOldPositionSet);
		EntityFactRef<Buff> obj = m_BuffRef;
		Hash128 val = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
		result.Append(ref val);
		int val2 = OldDamage;
		result.Append(ref val2);
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(TeleportAbility);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<Buff>.GetHash128(m_Buff);
		result.Append(ref val4);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		VigilEntry source = new VigilEntry();
		result = Unsafe.As<VigilEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<VigilEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_OldPosition", ref m_OldPosition, state);
		formatter.UnmanagedField(1, "m_IsOldPositionSet", ref m_IsOldPositionSet, state);
		formatter.Field(2, "m_BuffRef", ref m_BuffRef, state);
		int value = OldDamage;
		formatter.UnmanagedField(3, "OldDamage", ref value, state);
		BlueprintAbility value2 = TeleportAbility;
		formatter.Field(4, "TeleportAbility", ref value2, state);
		formatter.Field(5, "m_Buff", ref m_Buff, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<VigilEntry>();
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
				m_OldPosition = formatter.ReadPackable<Vector3>(state);
				break;
			case 1:
				m_IsOldPositionSet = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_BuffRef = formatter.ReadPackable<EntityFactRef<Buff>>(state);
				break;
			case 3:
				OldDamage = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				TeleportAbility = formatter.ReadPackable<BlueprintAbility>(state);
				break;
			case 5:
				m_Buff = formatter.ReadPackable<Buff>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
