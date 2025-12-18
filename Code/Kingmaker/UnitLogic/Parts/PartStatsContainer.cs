using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartStatsContainer : MechanicEntityPart, IHashable, IOwlPackable<PartStatsContainer>
{
	public interface IOwner : IEntityPartOwner<PartStatsContainer>, IEntityPartOwner
	{
		PartStatsContainer Stats { get; }
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartStatsContainer",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public StatsContainer Container { get; private set; }

	public Dictionary<StatType, StatType> OverridenBaseStat { get; } = new Dictionary<StatType, StatType>();


	public IEnumerable<ModifiableValue> AllStats => Container.AllStats;

	protected override void OnAttachOrPrePostLoad()
	{
		Container = new StatsContainer(base.Owner);
	}

	protected override void OnDidPostLoad()
	{
		Container.OnDidPostLoad();
	}

	[CanBeNull]
	public TModifiableValue GetStatOptional<TModifiableValue>(StatType type) where TModifiableValue : ModifiableValue
	{
		return Container.GetStatOptional<TModifiableValue>(type);
	}

	[CanBeNull]
	public ModifiableValue GetStatOptional(StatType type)
	{
		return Container.GetStatOptional(type);
	}

	[CanBeNull]
	public ModifiableValueAttributeStat GetAttributeOptional(StatType type)
	{
		return Container.GetStatOptional<ModifiableValueAttributeStat>(type);
	}

	[CanBeNull]
	public ModifiableValueSkill GetSkillOptional(StatType type)
	{
		return Container.GetStatOptional<ModifiableValueSkill>(type);
	}

	[CanBeNull]
	public ModifiableValue GetStat(StatType type)
	{
		return Container.GetStat(type);
	}

	[NotNull]
	public TModifiableValue GetStat<TModifiableValue>(StatType type) where TModifiableValue : ModifiableValue
	{
		return Container.GetStat<TModifiableValue>(type);
	}

	[NotNull]
	public ModifiableValueAttributeStat GetAttribute(StatType type)
	{
		return Container.GetAttribute(type);
	}

	[NotNull]
	public ModifiableValueSkill GetSkill(StatType type)
	{
		return Container.GetSkill(type);
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
		PartStatsContainer source = new PartStatsContainer();
		result = Unsafe.As<PartStatsContainer, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartStatsContainer>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartStatsContainer>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
