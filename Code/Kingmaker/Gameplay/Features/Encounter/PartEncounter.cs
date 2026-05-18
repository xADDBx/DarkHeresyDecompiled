using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Gameplay.Features.Encounter.Events;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Encounter;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartEncounter : UnitPart, IHashable, IOwlPackable<PartEncounter>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartEncounter",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("Blueprint", typeof(BlueprintEncounter)),
			new FieldInfo("SpawnerId", typeof(string)),
			new FieldInfo("Joined", typeof(bool))
		}
	};

	[OwlPackInclude]
	public BlueprintEncounter Blueprint { get; private set; }

	[CanBeNull]
	[OwlPackInclude]
	public string SpawnerId { get; private set; }

	[OwlPackInclude]
	public bool Joined { get; private set; }

	public bool IsDefault => Blueprint.IsDefault;

	public void SetupOnJoin([NotNull] BlueprintEncounter blueprint, int? combatGroup = null)
	{
		BlueprintEncounter blueprint2 = Blueprint;
		if (blueprint2 != null && !blueprint2.IsDefault && Blueprint != blueprint)
		{
			PFLog.Default.ErrorWithReport($"{base.Owner} is linked to encounter {Blueprint} and can't participate in encounter {blueprint}");
			return;
		}
		Blueprint = blueprint;
		if (combatGroup.HasValue)
		{
			base.Owner.CombatGroup.Id = $"{Blueprint.AssetGuid}_{combatGroup}";
		}
		else if (base.Owner.CombatGroup.IsPeaceful)
		{
			base.Owner.CombatGroup.Id = Blueprint.AssetGuid + "_" + base.Owner.UniqueId;
		}
	}

	public void SetupOnSpawn([NotNull] BlueprintEncounter blueprint, [CanBeNull] string spawnerId)
	{
		if (Blueprint != null)
		{
			throw new InvalidOperationException("Unit is already linked to an encounter");
		}
		Blueprint = blueprint;
		SpawnerId = spawnerId;
		if (spawnerId != null)
		{
			BlueprintEncounter.Group group = FindGroup();
			if (group != null)
			{
				base.Owner.CombatGroup.Id = $"{Blueprint.AssetGuid}_{group.CombatGroup}";
				SetupSquad(group);
			}
		}
	}

	public void Join([NotNull] BlueprintEncounter blueprint)
	{
		BlueprintEncounter blueprint2 = Blueprint;
		if (blueprint2 != null && !blueprint2.IsDefault && Blueprint != blueprint)
		{
			PFLog.Default.ErrorWithReport($"{base.Owner} is linked to encounter {Blueprint} and can't join encounter {blueprint}");
			return;
		}
		Blueprint = blueprint;
		Joined = true;
		base.EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IJoinEncounterHandler>)delegate(IJoinEncounterHandler h)
		{
			h.HandleJoinEncounter();
		}, isCheckRuntime: true);
	}

	protected override void OnApplyPostLoadFixes()
	{
		if (Game.Instance.Player.CompletedEncounters.Contains(Blueprint))
		{
			PFLog.Default.Log($"Encounter {Blueprint} was completed. Removing PartEncounter.");
			RemoveSelf();
		}
	}

	private void SetupSquad([NotNull] BlueprintEncounter.Group group)
	{
		if (group.IsSquad)
		{
			PartSquad orCreate = base.Owner.GetOrCreate<PartSquad>();
			orCreate.Id = base.Owner.CombatGroup.Id;
			if (group.SquadLeader.Ref.UniqueId == SpawnerId)
			{
				orCreate.Squad.Leader = base.Owner;
			}
		}
	}

	private BlueprintEncounter.Group FindGroup()
	{
		return Blueprint.Groups.FirstItem((BlueprintEncounter.Group i) => i.Spawners.HasItem((BlueprintEncounter.SpawnerEntry s) => s.Ref.UniqueId == SpawnerId));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartEncounter source = new PartEncounter();
		result = Unsafe.As<PartEncounter, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartEncounter>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintEncounter value = Blueprint;
		formatter.Field(0, "Blueprint", ref value, state);
		string value2 = SpawnerId;
		formatter.StringField(1, "SpawnerId", ref value2, state);
		bool value3 = Joined;
		formatter.UnmanagedField(2, "Joined", ref value3, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartEncounter>();
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
				Blueprint = formatter.ReadPackable<BlueprintEncounter>(state);
				break;
			case 1:
				SpawnerId = formatter.ReadString(state);
				break;
			case 2:
				Joined = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
