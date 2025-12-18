using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartOathOfVengeance : BaseUnitPart, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IGlobalRulebookSubscriber, ITurnBasedModeHandler, IHashable, IOwlPackable<UnitPartOathOfVengeance>
{
	[JsonProperty]
	[OwlPackInclude]
	public List<OathOfVengeanceEntry> Entries = new List<OathOfVengeanceEntry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartOathOfVengeance",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Entries", typeof(List<OathOfVengeanceEntry>))
		}
	};

	public void AddEntry(UnitEntity ally, UnitEntity enemy)
	{
		OathOfVengeanceEntry oathOfVengeanceEntry = new OathOfVengeanceEntry();
		oathOfVengeanceEntry.Ally = ally;
		oathOfVengeanceEntry.Enemy = enemy;
		Entries.Add(oathOfVengeanceEntry);
	}

	public bool HasEntries(UnitEntity ally)
	{
		return !GetEntries(ally).Empty();
	}

	public IEnumerable<OathOfVengeanceEntry> GetEntries(UnitEntity ally)
	{
		return Entries.Where((OathOfVengeanceEntry p) => p.Ally == ally);
	}

	public void RemoveEntries()
	{
		Entries.Clear();
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		UnitEntity unitEntity = evt.Initiator as UnitEntity;
		if (evt.Target is UnitEntity unitEntity2 && unitEntity != null && base.Owner.IsAlly(unitEntity2))
		{
			AddEntry(unitEntity2, unitEntity);
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			RemoveEntries();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<OathOfVengeanceEntry> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<OathOfVengeanceEntry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartOathOfVengeance source = new UnitPartOathOfVengeance();
		result = Unsafe.As<UnitPartOathOfVengeance, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartOathOfVengeance>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "Entries", ref Entries, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartOathOfVengeance>();
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
				Entries = formatter.ReadPackable<List<OathOfVengeanceEntry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
