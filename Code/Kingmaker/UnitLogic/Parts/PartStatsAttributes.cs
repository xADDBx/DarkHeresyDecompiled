using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartStatsAttributes : EntityPart, IHashable, IOwlPackable<PartStatsAttributes>
{
	public interface IOwner : IEntityPartOwner<PartStatsAttributes>, IEntityPartOwner
	{
		PartStatsAttributes Attributes { get; }
	}

	private ModifiableValueAttributeStat[] m_List;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartStatsAttributes",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	private StatsContainer Container => base.ConcreteOwner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValueAttributeStat WarhammerBallisticSkill => Container.GetAttribute(StatType.BallisticSkill);

	public ModifiableValueAttributeStat WarhammerWeaponSkill => Container.GetAttribute(StatType.WeaponSkill);

	public ModifiableValueAttributeStat WarhammerStrength => Container.GetAttribute(StatType.Strength);

	public ModifiableValueAttributeStat WarhammerToughness => Container.GetAttribute(StatType.Toughness);

	public ModifiableValueAttributeStat WarhammerAgility => Container.GetAttribute(StatType.Agility);

	public ModifiableValueAttributeStat WarhammerIntelligence => Container.GetAttribute(StatType.Intelligence);

	public ModifiableValueAttributeStat WarhammerWillpower => Container.GetAttribute(StatType.Willpower);

	public ModifiableValueAttributeStat WarhammerPerception => Container.GetAttribute(StatType.Perception);

	public ModifiableValueAttributeStat WarhammerFellowship => Container.GetAttribute(StatType.Fellowship);

	protected override void OnAttachOrPrePostLoad()
	{
		m_List = new ModifiableValueAttributeStat[9]
		{
			Container.RegisterAttribute(StatType.BallisticSkill),
			Container.RegisterAttribute(StatType.WeaponSkill),
			Container.RegisterAttribute(StatType.Strength),
			Container.RegisterAttribute(StatType.Toughness),
			Container.RegisterAttribute(StatType.Agility),
			Container.RegisterAttribute(StatType.Intelligence),
			Container.RegisterAttribute(StatType.Willpower),
			Container.RegisterAttribute(StatType.Perception),
			Container.RegisterAttribute(StatType.Fellowship)
		};
	}

	public ListEnumerator<ModifiableValueAttributeStat> GetEnumerator()
	{
		return new ListEnumerator<ModifiableValueAttributeStat>(m_List);
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
		PartStatsAttributes source = new PartStatsAttributes();
		result = Unsafe.As<PartStatsAttributes, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartStatsAttributes>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartStatsAttributes>();
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
