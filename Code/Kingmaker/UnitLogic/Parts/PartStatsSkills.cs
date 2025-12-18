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
public class PartStatsSkills : EntityPart, IHashable, IOwlPackable<PartStatsSkills>
{
	public interface IOwner : IEntityPartOwner<PartStatsSkills>, IEntityPartOwner
	{
		PartStatsSkills Skills { get; }
	}

	private ModifiableValueSkill[] m_List;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartStatsSkills",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	private StatsContainer Container => base.ConcreteOwner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValueSkill SkillAthletics => Container.GetSkill(StatType.SkillAthletics);

	public ModifiableValueSkill SkillTenacity => Container.GetSkill(StatType.SkillTenacity);

	public ModifiableValueSkill SkillMobility => Container.GetSkill(StatType.SkillMobility);

	public ModifiableValueSkill SkillResistance => Container.GetSkill(StatType.SkillResistance);

	public ModifiableValueSkill SkillDemolition => Container.GetSkill(StatType.SkillDemolition);

	public ModifiableValueSkill SkillReflexes => Container.GetSkill(StatType.SkillReflexes);

	public ModifiableValueSkill SkillSleightOfHand => Container.GetSkill(StatType.SkillSleightOfHand);

	public ModifiableValueSkill SkillLoreHeresy => Container.GetSkill(StatType.SkillLoreHeresy);

	public ModifiableValueSkill SkillLoreXenos => Container.GetSkill(StatType.SkillLoreXenos);

	public ModifiableValueSkill SkillLoreWarp => Container.GetSkill(StatType.SkillLoreWarp);

	public ModifiableValueSkill SkillTechUse => Container.GetSkill(StatType.SkillTechUse);

	public ModifiableValueSkill SkillInterrogation => Container.GetSkill(StatType.SkillInterrogation);

	public ModifiableValueSkill SkillMettle => Container.GetSkill(StatType.SkillMettle);

	public ModifiableValueSkill SkillAwareness => Container.GetSkill(StatType.SkillAwareness);

	public ModifiableValueSkill SkillWits => Container.GetSkill(StatType.SkillWits);

	public ModifiableValueSkill SkillIntimidation => Container.GetSkill(StatType.SkillIntimidation);

	public ModifiableValueSkill SkillDiplomacy => Container.GetSkill(StatType.SkillDiplomacy);

	public ModifiableValueSkill SkillMedicae => Container.GetSkill(StatType.SkillMedicae);

	protected override void OnAttachOrPrePostLoad()
	{
		m_List = new ModifiableValueSkill[18]
		{
			Container.RegisterSkill(StatType.SkillAthletics),
			Container.RegisterSkill(StatType.SkillTenacity),
			Container.RegisterSkill(StatType.SkillResistance),
			Container.RegisterSkill(StatType.SkillDemolition),
			Container.RegisterSkill(StatType.SkillMobility),
			Container.RegisterSkill(StatType.SkillReflexes),
			Container.RegisterSkill(StatType.SkillSleightOfHand),
			Container.RegisterSkill(StatType.SkillLoreHeresy),
			Container.RegisterSkill(StatType.SkillLoreXenos),
			Container.RegisterSkill(StatType.SkillLoreWarp),
			Container.RegisterSkill(StatType.SkillTechUse),
			Container.RegisterSkill(StatType.SkillInterrogation),
			Container.RegisterSkill(StatType.SkillMettle),
			Container.RegisterSkill(StatType.SkillAwareness),
			Container.RegisterSkill(StatType.SkillWits),
			Container.RegisterSkill(StatType.SkillIntimidation),
			Container.RegisterSkill(StatType.SkillDiplomacy),
			Container.RegisterSkill(StatType.SkillMedicae)
		};
	}

	public ListEnumerator<ModifiableValueSkill> GetEnumerator()
	{
		return new ListEnumerator<ModifiableValueSkill>(m_List);
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
		PartStatsSkills source = new PartStatsSkills();
		result = Unsafe.As<PartStatsSkills, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartStatsSkills>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartStatsSkills>();
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
