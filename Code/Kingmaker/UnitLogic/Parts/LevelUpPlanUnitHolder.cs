using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class LevelUpPlanUnitHolder : BaseUnitPart, IHashable, IOwlPackable<LevelUpPlanUnitHolder>
{
	[CanBeNull]
	private BaseUnitEntity m_PlanUnit;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "LevelUpPlanUnitHolder",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	[CanBeNull]
	public BaseUnitEntity RequestPlan()
	{
		if (m_PlanUnit == null)
		{
			using (ContextData<GameLogDisabled>.Request())
			{
				using (ContextData<AddClassLevels.DoNotCreatePlan>.Request())
				{
					m_PlanUnit = base.Owner.CreatePreview(createView: false);
				}
			}
		}
		return m_PlanUnit;
	}

	protected override void OnDetach()
	{
		m_PlanUnit?.Dispose();
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
		LevelUpPlanUnitHolder source = new LevelUpPlanUnitHolder();
		result = Unsafe.As<LevelUpPlanUnitHolder, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<LevelUpPlanUnitHolder>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<LevelUpPlanUnitHolder>();
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
