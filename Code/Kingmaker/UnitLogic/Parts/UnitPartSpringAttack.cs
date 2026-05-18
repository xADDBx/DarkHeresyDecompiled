using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartSpringAttack : BaseUnitPart, ITurnBasedModeHandler, ISubscriber, IUnitJumpHandler, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, IEventTag<ITurnEndHandler, EntitySubscriber>, IInterruptTurnStartHandler<EntitySubscriber>, IInterruptTurnStartHandler, IEventTag<IInterruptTurnStartHandler, EntitySubscriber>, IInterruptTurnEndHandler<EntitySubscriber>, IInterruptTurnEndHandler, IEventTag<IInterruptTurnEndHandler, EntitySubscriber>, IHashable, IOwlPackable<UnitPartSpringAttack>
{
	[JsonProperty]
	[OwlPackInclude]
	public int LastIndex;

	[JsonProperty]
	[OwlPackInclude]
	public EntityRef<AreaEffectEntity> TurnStartMark;

	[JsonProperty]
	[OwlPackInclude]
	public List<SpringAttackEntry> Entries = new List<SpringAttackEntry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartSpringAttack",
		OldNames = null,
		Fields = new FieldInfo[8]
		{
			new FieldInfo("DeathWaltzBlueprint", typeof(BlueprintAbility)),
			new FieldInfo("DeathWaltzUltimateBlueprint", typeof(BlueprintAbility)),
			new FieldInfo("SpringAttackFeature", typeof(UnitFact)),
			new FieldInfo("TurnStartPosition", typeof(Vector3)),
			new FieldInfo("LastIndex", typeof(int)),
			new FieldInfo("AreaMark", typeof(BlueprintAreaEffect)),
			new FieldInfo("TurnStartMark", typeof(EntityRef<AreaEffectEntity>)),
			new FieldInfo("Entries", typeof(List<SpringAttackEntry>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintAbility DeathWaltzBlueprint { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintAbility DeathWaltzUltimateBlueprint { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public UnitFact SpringAttackFeature { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public Vector3 TurnStartPosition { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintAreaEffect AreaMark { get; set; }

	public void AddEntry(Vector3 oldPosition, Vector3 newPosition)
	{
		SpringAttackEntry springAttackEntry = new SpringAttackEntry();
		springAttackEntry.OldPosition = oldPosition;
		springAttackEntry.NewPosition = newPosition;
		springAttackEntry.AreaMark = AreaEffectsController.CreateSpawner(AreaMark, SpringAttackFeature.Context, oldPosition).Duration(1.Rounds().Seconds).Spawn();
		LastIndex++;
		springAttackEntry.Index = LastIndex;
		if (Entries == null)
		{
			Entries = new List<SpringAttackEntry>();
		}
		Entries.Add(springAttackEntry);
	}

	public bool HasEntries()
	{
		return !Entries.Empty();
	}

	public void RemoveEntries()
	{
		foreach (SpringAttackEntry entry in Entries)
		{
			if (entry.AreaMark.Entity != null)
			{
				entry.AreaMark.Entity.ForceEnded = true;
			}
		}
		Entries.Clear();
		LastIndex = 0;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			RemoveEntries();
			if (TurnStartMark.Entity != null)
			{
				TurnStartMark.Entity.ForceEnded = true;
			}
		}
	}

	public void HandleUnitJump(int distanceInCells, Vector3 startPoint, Vector3 targetPoint, MechanicEntity caster, BlueprintAbility ability)
	{
		if (caster == base.Owner && (ability == DeathWaltzBlueprint || ability == DeathWaltzUltimateBlueprint))
		{
			AddEntry(startPoint, targetPoint);
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased && !ContextData<TurnController.InterruptTurnEndMark>.Current)
		{
			RemoveEntries();
			TurnStartPosition = base.Owner.Position;
			TurnStartMark = AreaEffectsController.CreateSpawner(AreaMark, SpringAttackFeature.MaybeContext, TurnStartPosition).Duration(1.Rounds().Seconds).Spawn();
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		RemoveEntries();
		TurnStartPosition = base.Owner.Position;
		TurnStartMark = AreaEffectsController.CreateSpawner(AreaMark, SpringAttackFeature.MaybeContext, TurnStartPosition).Duration(1.Rounds().Seconds).Spawn();
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		RemoveEntries();
		if (TurnStartMark.Entity != null)
		{
			TurnStartMark.Entity.ForceEnded = true;
		}
	}

	public void HandleUnitEndInterruptTurn()
	{
		RemoveEntries();
		if (TurnStartMark.Entity != null)
		{
			TurnStartMark.Entity.ForceEnded = true;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(DeathWaltzBlueprint);
		result.Append(ref val2);
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(DeathWaltzUltimateBlueprint);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<UnitFact>.GetHash128(SpringAttackFeature);
		result.Append(ref val4);
		Vector3 val5 = TurnStartPosition;
		result.Append(ref val5);
		result.Append(ref LastIndex);
		Hash128 val6 = SimpleBlueprintHasher.GetHash128(AreaMark);
		result.Append(ref val6);
		EntityRef<AreaEffectEntity> obj = TurnStartMark;
		Hash128 val7 = StructHasher<EntityRef<AreaEffectEntity>>.GetHash128(ref obj);
		result.Append(ref val7);
		List<SpringAttackEntry> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val8 = ClassHasher<SpringAttackEntry>.GetHash128(entries[i]);
				result.Append(ref val8);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartSpringAttack source = new UnitPartSpringAttack();
		result = Unsafe.As<UnitPartSpringAttack, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartSpringAttack>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintAbility value = DeathWaltzBlueprint;
		formatter.Field(0, "DeathWaltzBlueprint", ref value, state);
		BlueprintAbility value2 = DeathWaltzUltimateBlueprint;
		formatter.Field(1, "DeathWaltzUltimateBlueprint", ref value2, state);
		UnitFact value3 = SpringAttackFeature;
		formatter.Field(2, "SpringAttackFeature", ref value3, state);
		Vector3 value4 = TurnStartPosition;
		formatter.Field(3, "TurnStartPosition", ref value4, state);
		formatter.UnmanagedField(4, "LastIndex", ref LastIndex, state);
		BlueprintAreaEffect value5 = AreaMark;
		formatter.Field(5, "AreaMark", ref value5, state);
		formatter.Field(6, "TurnStartMark", ref TurnStartMark, state);
		formatter.Field(7, "Entries", ref Entries, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartSpringAttack>();
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
				DeathWaltzBlueprint = formatter.ReadPackable<BlueprintAbility>(state);
				break;
			case 1:
				DeathWaltzUltimateBlueprint = formatter.ReadPackable<BlueprintAbility>(state);
				break;
			case 2:
				SpringAttackFeature = formatter.ReadPackable<UnitFact>(state);
				break;
			case 3:
				TurnStartPosition = formatter.ReadPackable<Vector3>(state);
				break;
			case 4:
				LastIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 5:
				AreaMark = formatter.ReadPackable<BlueprintAreaEffect>(state);
				break;
			case 6:
				TurnStartMark = formatter.ReadPackable<EntityRef<AreaEffectEntity>>(state);
				break;
			case 7:
				Entries = formatter.ReadPackable<List<SpringAttackEntry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
