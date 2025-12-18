using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
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
public class UnitPartVigilBeyondTime : BaseUnitPart, IHashable, IOwlPackable<UnitPartVigilBeyondTime>
{
	[JsonProperty]
	[OwlPackInclude]
	public List<VigilEntry> Entries = new List<VigilEntry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartVigilBeyondTime",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Entries", typeof(List<VigilEntry>))
		}
	};

	public void AddEntry(Buff buff, BlueprintAbility teleportAbility)
	{
		VigilEntry vigilEntry = new VigilEntry();
		vigilEntry.OldPosition = buff.Owner.Position.GetNearestNodeXZUnwalkable();
		vigilEntry.OldDamage = buff.Owner.LifeState.Health.Damage;
		vigilEntry.Buff = buff;
		vigilEntry.TeleportAbility = teleportAbility;
		Entries.Add(vigilEntry);
	}

	public bool HasEntries()
	{
		return !Entries.Empty();
	}

	public void RemoveEntry(Buff buff)
	{
		Entries.RemoveAll((VigilEntry p) => p.Buff == buff || p.Buff == null);
	}

	public void ActivateVigil(UnitEntity unit)
	{
		VigilEntry entry = Entries.FirstOrDefault((VigilEntry p) => p.Buff?.Owner == unit);
		if (entry == null)
		{
			return;
		}
		AbilityData ability = new AbilityData(entry.TeleportAbility, unit);
		Vector3 vector = entry.OldPosition?.Vector3Position() ?? unit.Position;
		int distanceToInCells = unit.DistanceToInCells(vector);
		if (distanceToInCells > 0)
		{
			unit.LifeState.Health.SetDamage(entry.OldDamage);
			if (unit.IsEnemy(base.Owner))
			{
				EventBus.RaiseEvent(delegate(IUnitAbilityNonPushForceMoveHandler h)
				{
					h.HandleUnitNonPushForceMove(distanceToInCells, entry.Buff.Context, unit);
				});
			}
			Rulebook.Trigger(new RulePerformAbility(ability, vector)
			{
				IgnoreCooldown = true,
				ForceFreeAction = true
			});
			RemoveEntry(entry.Buff);
		}
		else
		{
			unit.LifeState.Health.SetDamage(entry.OldDamage);
			entry.Buff.Remove();
			RemoveEntry(entry.Buff);
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		foreach (VigilEntry entry in Entries)
		{
			entry.OnPostLoad();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<VigilEntry> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<VigilEntry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartVigilBeyondTime source = new UnitPartVigilBeyondTime();
		result = Unsafe.As<UnitPartVigilBeyondTime, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartVigilBeyondTime>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "Entries", ref Entries, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartVigilBeyondTime>();
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
				Entries = formatter.ReadPackable<List<VigilEntry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
