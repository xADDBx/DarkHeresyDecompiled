using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[Obsolete]
[OwlPackable(OwlPackableMode.Generate)]
public class PartUnitStealth : BaseUnitPart, IHashable, IOwlPackable<PartUnitStealth>
{
	public interface IOwner : IEntityPartOwner<PartUnitStealth>, IEntityPartOwner
	{
		PartUnitStealth Stealth { get; }
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartUnitStealth",
		OldNames = null,
		Fields = new FieldInfo[13]
		{
			new FieldInfo("Active", typeof(bool)),
			new FieldInfo("WantActivate", typeof(bool)),
			new FieldInfo("CachedRoll", typeof(int)),
			new FieldInfo("FullSpeed", typeof(bool)),
			new FieldInfo("ShouldExitStealth", typeof(bool)),
			new FieldInfo("LastNearEnemyTime", typeof(TimeSpan)),
			new FieldInfo("NearEnemyPenalty", typeof(float)),
			new FieldInfo("ForceEnterStealth", typeof(bool)),
			new FieldInfo("BecameInvisibleThisFrame", typeof(bool)),
			new FieldInfo("InAmbush", typeof(bool)),
			new FieldInfo("AmbushJoinCombatDistance", typeof(float)),
			new FieldInfo("AmbushTake20", typeof(bool)),
			new FieldInfo("m_SpottedBy", typeof(List<UnitReference>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool Active { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool WantActivate { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int CachedRoll { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool FullSpeed { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool ShouldExitStealth { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan LastNearEnemyTime { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public float NearEnemyPenalty { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool ForceEnterStealth { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool BecameInvisibleThisFrame { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool InAmbush { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public float AmbushJoinCombatDistance { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool AmbushTake20 { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	private List<UnitReference> m_SpottedBy { get; set; } = new List<UnitReference>();


	public IEnumerable<BaseUnitEntity> SpottedBy
	{
		get
		{
			int i = 0;
			while (i < m_SpottedBy.Count)
			{
				UnitReference unitReference = m_SpottedBy[i];
				if (unitReference.Entity != null && unitReference.Entity.ToBaseUnitEntity().LifeState.IsConscious)
				{
					yield return unitReference.Entity.ToBaseUnitEntity();
				}
				int num = i + 1;
				i = num;
			}
		}
	}

	public bool AddSpottedBy(BaseUnitEntity unit)
	{
		if (!m_SpottedBy.Contains(unit.FromBaseUnitEntity()))
		{
			m_SpottedBy.Add(unit.FromBaseUnitEntity());
			return true;
		}
		return false;
	}

	public bool IsSpottedBy(UnitGroup group)
	{
		for (int i = 0; i < m_SpottedBy.Count; i++)
		{
			IAbstractUnitEntity entity = m_SpottedBy[i].Entity;
			if (entity != null && entity.ToBaseUnitEntity().LifeState.IsConscious && entity.ToBaseUnitEntity().CombatGroup.Group == group)
			{
				return true;
			}
		}
		return false;
	}

	public void Clear()
	{
		m_SpottedBy.Clear();
		CachedRoll = 0;
		FullSpeed = false;
		ShouldExitStealth = false;
	}

	public void ForceExitStealth()
	{
		if (Active)
		{
			WantActivate = false;
			Clear();
			Active = false;
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitStealthHandler>)delegate(IUnitStealthHandler h)
			{
				h.HandleUnitSwitchStealthCondition(inStealth: false);
			}, isCheckRuntime: true);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = Active;
		result.Append(ref val2);
		bool val3 = WantActivate;
		result.Append(ref val3);
		int val4 = CachedRoll;
		result.Append(ref val4);
		bool val5 = FullSpeed;
		result.Append(ref val5);
		bool val6 = ShouldExitStealth;
		result.Append(ref val6);
		TimeSpan val7 = LastNearEnemyTime;
		result.Append(ref val7);
		float val8 = NearEnemyPenalty;
		result.Append(ref val8);
		bool val9 = ForceEnterStealth;
		result.Append(ref val9);
		bool val10 = BecameInvisibleThisFrame;
		result.Append(ref val10);
		bool val11 = InAmbush;
		result.Append(ref val11);
		float val12 = AmbushJoinCombatDistance;
		result.Append(ref val12);
		bool val13 = AmbushTake20;
		result.Append(ref val13);
		List<UnitReference> spottedBy = m_SpottedBy;
		if (spottedBy != null)
		{
			for (int i = 0; i < spottedBy.Count; i++)
			{
				UnitReference obj = spottedBy[i];
				Hash128 val14 = UnitReferenceHasher.GetHash128(ref obj);
				result.Append(ref val14);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartUnitStealth source = new PartUnitStealth();
		result = Unsafe.As<PartUnitStealth, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartUnitStealth>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = Active;
		formatter.UnmanagedField(0, "Active", ref value, state);
		bool value2 = WantActivate;
		formatter.UnmanagedField(1, "WantActivate", ref value2, state);
		int value3 = CachedRoll;
		formatter.UnmanagedField(2, "CachedRoll", ref value3, state);
		bool value4 = FullSpeed;
		formatter.UnmanagedField(3, "FullSpeed", ref value4, state);
		bool value5 = ShouldExitStealth;
		formatter.UnmanagedField(4, "ShouldExitStealth", ref value5, state);
		TimeSpan value6 = LastNearEnemyTime;
		formatter.Field(5, "LastNearEnemyTime", ref value6, state);
		float value7 = NearEnemyPenalty;
		formatter.UnmanagedField(6, "NearEnemyPenalty", ref value7, state);
		bool value8 = ForceEnterStealth;
		formatter.UnmanagedField(7, "ForceEnterStealth", ref value8, state);
		bool value9 = BecameInvisibleThisFrame;
		formatter.UnmanagedField(8, "BecameInvisibleThisFrame", ref value9, state);
		bool value10 = InAmbush;
		formatter.UnmanagedField(9, "InAmbush", ref value10, state);
		float value11 = AmbushJoinCombatDistance;
		formatter.UnmanagedField(10, "AmbushJoinCombatDistance", ref value11, state);
		bool value12 = AmbushTake20;
		formatter.UnmanagedField(11, "AmbushTake20", ref value12, state);
		List<UnitReference> value13 = m_SpottedBy;
		formatter.Field(12, "m_SpottedBy", ref value13, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartUnitStealth>();
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
				Active = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				WantActivate = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				CachedRoll = formatter.ReadUnmanaged<int>(state);
				break;
			case 3:
				FullSpeed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 4:
				ShouldExitStealth = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				LastNearEnemyTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 6:
				NearEnemyPenalty = formatter.ReadUnmanaged<float>(state);
				break;
			case 7:
				ForceEnterStealth = formatter.ReadUnmanaged<bool>(state);
				break;
			case 8:
				BecameInvisibleThisFrame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				InAmbush = formatter.ReadUnmanaged<bool>(state);
				break;
			case 10:
				AmbushJoinCombatDistance = formatter.ReadUnmanaged<float>(state);
				break;
			case 11:
				AmbushTake20 = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				m_SpottedBy = formatter.ReadPackable<List<UnitReference>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
