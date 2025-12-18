using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartMachineTrait : MechanicEntityPart, IHashable, IOwlPackable<PartMachineTrait>
{
	public interface IOwner : IEntityPartOwner<PartMachineTrait>, IEntityPartOwner
	{
		[CanBeNull]
		PartMachineTrait MaybeMachineTrait { get; }
	}

	private int m_RetainCount;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartMachineTrait",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValue MachineTrait => StatsContainer.GetStat(StatType.MachineTrait);

	public static int GetBaseStatValue(MechanicEntity unit)
	{
		return unit.GetStatBaseValue(StatType.MachineTrait).Value;
	}

	public void Retain()
	{
		m_RetainCount++;
		PFLog.History.Party.Log($"PartMachineTrait.Retain: {base.Owner}, count {m_RetainCount}");
	}

	public void Release()
	{
		m_RetainCount--;
		PFLog.History.Party.Log($"PartMachineTrait.Release: {base.Owner}, count {m_RetainCount}");
		if (m_RetainCount < 1)
		{
			RemoveSelf();
		}
	}

	protected override void OnAttachOrPrePostLoad()
	{
		StatsContainer.Register(StatType.MachineTrait);
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
		PartMachineTrait source = new PartMachineTrait();
		result = Unsafe.As<PartMachineTrait, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartMachineTrait>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartMachineTrait>();
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
