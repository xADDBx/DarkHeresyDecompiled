using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class WarhammerUnitPartBuffLimit : BaseUnitPart, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable, IOwlPackable<WarhammerUnitPartBuffLimit>
{
	[JsonProperty]
	[OwlPackInclude]
	public HashSet<BlueprintBuff> WatchedBuffs = new HashSet<BlueprintBuff>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "WarhammerUnitPartBuffLimit",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("WatchedBuffs", typeof(HashSet<BlueprintBuff>))
		}
	};

	public void AddWatchedBuff(BlueprintBuff buffBlueprint)
	{
		WatchedBuffs.Add(buffBlueprint);
	}

	public void HandleBuffDidAdded(Buff buff, MechanicEntity caster)
	{
		if (buff.Context?.MaybeCaster == base.Owner && WatchedBuffs.Contains(buff.Blueprint))
		{
			((buff.Context?.MaybeOwner)?.Buffs.RawFacts.FirstOrDefault((Buff x) => buff.Blueprint == x.Blueprint && buff != x))?.Remove();
		}
	}

	public void HandleBuffDidRemoved(Buff buff, MechanicEntity caster)
	{
	}

	public void HandleBuffRankIncreased(Buff buff, int delta, MechanicEntity caster)
	{
	}

	public void HandleBuffRankDecreased(Buff buff, int delta, MechanicEntity caster)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		HashSet<BlueprintBuff> watchedBuffs = WatchedBuffs;
		if (watchedBuffs != null)
		{
			int num = 0;
			foreach (BlueprintBuff item in watchedBuffs)
			{
				num ^= SimpleBlueprintHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		WarhammerUnitPartBuffLimit source = new WarhammerUnitPartBuffLimit();
		result = Unsafe.As<WarhammerUnitPartBuffLimit, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<WarhammerUnitPartBuffLimit>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "WatchedBuffs", ref WatchedBuffs, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<WarhammerUnitPartBuffLimit>();
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
				WatchedBuffs = formatter.ReadPackable<HashSet<BlueprintBuff>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
