using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartCompanion : BaseUnitPart, IHashable, IOwlPackable<UnitPartCompanion>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef m_Spawner;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartCompanion",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("State", typeof(CompanionState)),
			new FieldInfo("m_Spawner", typeof(EntityRef))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public CompanionState State { get; private set; }

	public bool IsCompanion
	{
		get
		{
			CompanionState state = State;
			return state == CompanionState.InParty || state == CompanionState.InPartyDetached || state == CompanionState.Remote;
		}
	}

	public bool CanRemoveFromParty
	{
		get
		{
			if (base.Owner is UnitEntity @this)
			{
				return @this.IsCustomCompanion();
			}
			return false;
		}
	}

	public void SetState(CompanionState s)
	{
		State = s;
		if (s == CompanionState.ExCompanion)
		{
			base.Owner.CombatGroup.Id = Uuid.Instance.CreateString();
			Metrics.Recruitment.Id(base.Owner.Blueprint?.AssetGuid).RecruitState(RecruitMetricsEvent.CompanionRecruitStates.Dismiss).Send();
		}
		base.Owner.GetOrCreate<UnitPartNonStackBonuses>();
		Game.Instance.Player.InvalidateCharacterLists();
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<ICompanionStateChanged>)delegate(ICompanionStateChanged h)
		{
			h.HandleCompanionStateChanged();
		}, isCheckRuntime: true);
	}

	public bool IsControllableInParty()
	{
		if (!base.Owner.IsMainCharacter)
		{
			if (State == CompanionState.InParty)
			{
				return !Game.Instance.LoadedAreaState.Settings.CapitalPartyMode;
			}
			return false;
		}
		return true;
	}

	public static BaseUnitEntity FindCompanion(BlueprintUnit bp, CompanionState state)
	{
		return Game.Instance.Player.AllCrossSceneUnits.FirstOrDefault(delegate(BaseUnitEntity u)
		{
			if (u.Blueprint == bp)
			{
				UnitPartCompanion optional = u.GetOptional<UnitPartCompanion>();
				if (optional == null)
				{
					return false;
				}
				return optional.State == state;
			}
			return false;
		});
	}

	public void SetSpawner([CanBeNull] CompanionSpawner spawner)
	{
		CompanionSpawner currentSpawner = GetCurrentSpawner();
		if (!(currentSpawner == spawner))
		{
			currentSpawner?.ReleaseCompanion();
			m_Spawner = new EntityRef(spawner?.UniqueId);
		}
	}

	public CompanionSpawner GetCurrentSpawner()
	{
		return m_Spawner.Entity?.View as CompanionSpawner;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		CompanionState val2 = State;
		result.Append(ref val2);
		EntityRef obj = m_Spawner;
		Hash128 val3 = EntityRefHasher.GetHash128(ref obj);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartCompanion source = new UnitPartCompanion();
		result = Unsafe.As<UnitPartCompanion, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartCompanion>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		CompanionState value = State;
		formatter.EnumField(0, "State", ref value, state);
		formatter.Field(1, "m_Spawner", ref m_Spawner, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartCompanion>();
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
				State = formatter.ReadEnum<CompanionState>(state);
				break;
			case 1:
				m_Spawner = formatter.ReadPackable<EntityRef>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
