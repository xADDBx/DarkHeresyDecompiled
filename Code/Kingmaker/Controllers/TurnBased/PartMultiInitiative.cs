using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

[OwlPackable(OwlPackableMode.Generate)]
public class PartMultiInitiative : MechanicEntityPart, IUnitDeathHandler, ISubscriber, ITurnEndHandler, ISubscriber<IMechanicEntity>, IHashable, IOwlPackable<PartMultiInitiative>
{
	public int AdditionalTurnsCount;

	public bool ByEnemiesCount;

	[JsonProperty]
	[OwlPackInclude]
	public MechanicEntity CorrespondingEnemy;

	[JsonProperty]
	[OwlPackInclude]
	public List<InitiativePlaceholderEntity> Placeholders;

	[JsonProperty]
	[OwlPackInclude]
	public bool SwitchCorrespondingEnemyOnTurnEnd;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartMultiInitiative",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("CorrespondingEnemy", typeof(MechanicEntity)),
			new FieldInfo("Placeholders", typeof(List<InitiativePlaceholderEntity>)),
			new FieldInfo("SwitchCorrespondingEnemyOnTurnEnd", typeof(bool))
		}
	};

	public void Setup(int additionalTurns)
	{
		AdditionalTurnsCount = additionalTurns;
		Placeholders = new List<InitiativePlaceholderEntity>();
		ByEnemiesCount = false;
	}

	public void SetupByEnemiesCount()
	{
		if (Placeholders == null)
		{
			Placeholders = new List<InitiativePlaceholderEntity>();
		}
		ByEnemiesCount = true;
	}

	public IEnumerable<InitiativePlaceholderEntity> EnsurePlaceholders()
	{
		return from i in Enumerable.Range(0, AdditionalTurnsCount)
			select InitiativePlaceholderEntity.Ensure(base.Owner, i);
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (unitEntity == CorrespondingEnemy)
		{
			SwitchCorrespondingEnemyOnTurnEnd = true;
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (!SwitchCorrespondingEnemyOnTurnEnd)
		{
			return;
		}
		SwitchCorrespondingEnemyOnTurnEnd = false;
		if (Placeholders.Any((InitiativePlaceholderEntity p) => !p.CorrespondingEnemy.IsDeadOrUnconscious))
		{
			InitiativePlaceholderEntity initiativePlaceholderEntity = Placeholders.First((InitiativePlaceholderEntity p) => !p.CorrespondingEnemy.IsDeadOrUnconscious);
			base.Owner.Initiative.SwapPlaces(initiativePlaceholderEntity.Initiative);
			CorrespondingEnemy = initiativePlaceholderEntity.CorrespondingEnemy;
			Placeholders.Remove(initiativePlaceholderEntity);
			initiativePlaceholderEntity.Dispose();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicEntity>.GetHash128(CorrespondingEnemy);
		result.Append(ref val2);
		List<InitiativePlaceholderEntity> placeholders = Placeholders;
		if (placeholders != null)
		{
			for (int i = 0; i < placeholders.Count; i++)
			{
				Hash128 val3 = ClassHasher<InitiativePlaceholderEntity>.GetHash128(placeholders[i]);
				result.Append(ref val3);
			}
		}
		result.Append(ref SwitchCorrespondingEnemyOnTurnEnd);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartMultiInitiative source = new PartMultiInitiative();
		result = Unsafe.As<PartMultiInitiative, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartMultiInitiative>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "CorrespondingEnemy", ref CorrespondingEnemy, state);
		formatter.Field(1, "Placeholders", ref Placeholders, state);
		formatter.UnmanagedField(2, "SwitchCorrespondingEnemyOnTurnEnd", ref SwitchCorrespondingEnemyOnTurnEnd, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartMultiInitiative>();
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
				CorrespondingEnemy = formatter.ReadPackable<MechanicEntity>(state);
				break;
			case 1:
				Placeholders = formatter.ReadPackable<List<InitiativePlaceholderEntity>>(state);
				break;
			case 2:
				SwitchCorrespondingEnemyOnTurnEnd = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
