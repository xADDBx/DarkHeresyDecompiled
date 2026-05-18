using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartLifeState : AbstractUnitPart, IHashable, IOwlPackable<PartLifeState>
{
	public interface IOwner : IEntityPartOwner<PartLifeState>, IEntityPartOwner
	{
		PartLifeState LifeState { get; }
	}

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsDeathRevealed;

	private bool m_isRessurecting;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartLifeState",
		OldNames = null,
		Fields = new FieldInfo[9]
		{
			new FieldInfo("IsHiddenBecauseDead", typeof(bool)),
			new FieldInfo("m_IsDeathRevealed", typeof(bool)),
			new FieldInfo("DeathTime", typeof(TimeSpan)),
			new FieldInfo("State", typeof(UnitLifeState)),
			new FieldInfo("MarkedForDeath", typeof(bool)),
			new FieldInfo("IsManualDeath", typeof(bool)),
			new FieldInfo("ScriptedKill", typeof(bool)),
			new FieldInfo("ForceDismember", typeof(UnitDismemberType)),
			new FieldInfo("DismembermentLimbsApartType", typeof(DismembermentLimbsApartType?))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool IsHiddenBecauseDead { get; private set; }

	public bool IsDeathRevealed
	{
		get
		{
			return m_IsDeathRevealed;
		}
		set
		{
			if (m_IsDeathRevealed != value)
			{
				m_IsDeathRevealed = value;
				base.Owner.View?.UpdateViewActive();
			}
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan DeathTime { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public UnitLifeState State { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool MarkedForDeath { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsManualDeath { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool ScriptedKill { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public UnitDismemberType ForceDismember { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public DismembermentLimbsApartType? DismembermentLimbsApartType { get; set; }

	public PartHealth Health => base.Owner.GetRequired<PartHealth>();

	public PartArmor Armor => base.Owner.GetOptional<PartArmor>();

	public bool IsConscious => State == UnitLifeState.Conscious;

	public bool IsUnconscious => State == UnitLifeState.Unconscious;

	public bool IsDead => State == UnitLifeState.Dead;

	public bool IsDeadOrUnconscious => State != UnitLifeState.Conscious;

	public bool IsFinallyDead
	{
		get
		{
			if (IsDead)
			{
				if ((bool)base.Owner.Features.Immortality)
				{
					return ScriptedKill;
				}
				return true;
			}
			return false;
		}
	}

	public void ManualDeath()
	{
		ForceDismember = UnitDismemberType.None;
		MarkedForDeath = true;
		IsManualDeath = true;
	}

	public bool Set(UnitLifeState newLifeState)
	{
		if (State == newLifeState)
		{
			return false;
		}
		State = newLifeState;
		if (newLifeState == UnitLifeState.Dead)
		{
			DeathTime = Game.Instance.Controllers.TimeController.GameTime;
		}
		else
		{
			IsDeathRevealed = false;
		}
		return true;
	}

	public void Resurrect(int resultHealth = 1, bool restoreHealth = true)
	{
		Resurrect(resultHealth, restoreHealth, fullRestore: false);
	}

	public void ResurrectAndFullRestore()
	{
		Resurrect(0);
	}

	private void Resurrect(int resultHealth, bool restoreHealth, bool fullRestore)
	{
		if (!m_isRessurecting)
		{
			IsHiddenBecauseDead = false;
			m_isRessurecting = true;
			UnitLifeController.ForceUnitConscious(base.Owner);
			if (fullRestore)
			{
				Health.HealDamageAll();
			}
			else if (restoreHealth)
			{
				Health.SetHitPointsLeft(resultHealth);
			}
			UpdateUnitViewOnResurrect(base.Owner.View);
			m_isRessurecting = false;
			base.EventBus.RaiseEvent((IAbstractUnitEntity)(IBaseUnitEntity)base.Owner, (Action<IUnitResurrectedHandler>)delegate(IUnitResurrectedHandler h)
			{
				h.HandleUnitResurrected();
			}, isCheckRuntime: true);
		}
	}

	private static void UpdateUnitViewOnResurrect([NotNull] IAbstractUnitEntityView view)
	{
		UnitEntityView unitEntityView = view as UnitEntityView;
		if (unitEntityView != null)
		{
			unitEntityView.RigidbodyController.Or(null)?.CancelRagdoll();
		}
		view.LeaveProneState();
		if (unitEntityView != null)
		{
			Game.Instance.Controllers.HandsEquipmentController.ScheduleUpdate(unitEntityView.HandsEquipment);
		}
		view.ResetHoverHighlighted();
	}

	public void HideIfDead()
	{
		IsHiddenBecauseDead |= IsFinallyDead;
	}

	public void RevealDeath()
	{
		IsDeathRevealed = true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = IsHiddenBecauseDead;
		result.Append(ref val2);
		result.Append(ref m_IsDeathRevealed);
		TimeSpan val3 = DeathTime;
		result.Append(ref val3);
		UnitLifeState val4 = State;
		result.Append(ref val4);
		bool val5 = MarkedForDeath;
		result.Append(ref val5);
		bool val6 = IsManualDeath;
		result.Append(ref val6);
		bool val7 = ScriptedKill;
		result.Append(ref val7);
		UnitDismemberType val8 = ForceDismember;
		result.Append(ref val8);
		if (DismembermentLimbsApartType.HasValue)
		{
			DismembermentLimbsApartType val9 = DismembermentLimbsApartType.Value;
			result.Append(ref val9);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartLifeState source = new PartLifeState();
		result = Unsafe.As<PartLifeState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartLifeState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = IsHiddenBecauseDead;
		formatter.UnmanagedField(0, "IsHiddenBecauseDead", ref value, state);
		formatter.UnmanagedField(1, "m_IsDeathRevealed", ref m_IsDeathRevealed, state);
		TimeSpan value2 = DeathTime;
		formatter.Field(2, "DeathTime", ref value2, state);
		UnitLifeState value3 = State;
		formatter.EnumField(3, "State", ref value3, state);
		bool value4 = MarkedForDeath;
		formatter.UnmanagedField(4, "MarkedForDeath", ref value4, state);
		bool value5 = IsManualDeath;
		formatter.UnmanagedField(5, "IsManualDeath", ref value5, state);
		bool value6 = ScriptedKill;
		formatter.UnmanagedField(6, "ScriptedKill", ref value6, state);
		UnitDismemberType value7 = ForceDismember;
		formatter.EnumField(7, "ForceDismember", ref value7, state);
		DismembermentLimbsApartType? value8 = DismembermentLimbsApartType;
		formatter.EnumNullableField(8, "DismembermentLimbsApartType", ref value8, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartLifeState>();
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
				IsHiddenBecauseDead = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				m_IsDeathRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				DeathTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 3:
				State = formatter.ReadEnum<UnitLifeState>(state);
				break;
			case 4:
				MarkedForDeath = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				IsManualDeath = formatter.ReadUnmanaged<bool>(state);
				break;
			case 6:
				ScriptedKill = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				ForceDismember = formatter.ReadEnum<UnitDismemberType>(state);
				break;
			case 8:
				DismembermentLimbsApartType = formatter.ReadNullableEnum<DismembermentLimbsApartType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
