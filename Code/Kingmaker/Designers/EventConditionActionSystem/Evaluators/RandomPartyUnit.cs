using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("4a0b4f8a27ed6e74c940f821cc147af9")]
[OwlPackable(OwlPackableMode.Generate)]
public class RandomPartyUnit : AbstractUnitEvaluator, IOwlPackable<RandomPartyUnit>
{
	public ConditionsChecker Conditions;

	[SerializeReference]
	public AbstractUnitEvaluator UnitIfNoVariants;

	[SerializeField]
	[FormerlySerializedAs("ForbiddenBlueprints")]
	private BlueprintUnitReference[] m_ForbiddenBlueprints;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RandomPartyUnit",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public ReferenceArrayProxy<BlueprintUnit> ForbiddenBlueprints
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] array = m_ForbiddenBlueprints ?? Array.Empty<BlueprintUnitReference>();
			return array;
		}
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		List<BaseUnitEntity> list = (from u in Game.Instance.Player.Party
			where u != null
			where !ForbiddenBlueprints.Contains(u.Blueprint)
			select u).ToList();
		List<BaseUnitEntity> list2 = new List<BaseUnitEntity>();
		foreach (BaseUnitEntity item in list)
		{
			using (ContextData<PartyUnitData>.Request().Setup(item))
			{
				if (Conditions.Check())
				{
					list2.Add(item);
				}
			}
		}
		if (list2.Count == 0)
		{
			return UnitIfNoVariants?.GetValue() ?? Game.Instance.Player.MainCharacterEntity;
		}
		return list2.Random(PFStatefulRandom.Designers) ?? Game.Instance.Player.MainCharacterEntity;
	}

	public override string GetCaption()
	{
		return "Random Party Unit";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RandomPartyUnit source = new RandomPartyUnit();
		result = Unsafe.As<RandomPartyUnit, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RandomPartyUnit>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RandomPartyUnit>();
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
