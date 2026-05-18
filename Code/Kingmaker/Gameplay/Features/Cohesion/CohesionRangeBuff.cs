using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Cohesion;

[Serializable]
[AllowMultipleComponents]
[ComponentName("Cohesion/CohesionRangeBuff")]
[TypeId("afcd2ddb2ad74dd5876efd7531090d4b")]
public sealed class CohesionRangeBuff : UnitFactComponentDelegate, IEntityEnterCohesionRangeHandler<EntitySubscriber>, IEntityEnterCohesionRangeHandler, ISubscriber<IUnitEntity>, ISubscriber, IEventTag<IEntityEnterCohesionRangeHandler, EntitySubscriber>, IEntityExitCohesionRangeHandler<EntitySubscriber>, IEntityExitCohesionRangeHandler, IEventTag<IEntityExitCohesionRangeHandler, EntitySubscriber>, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>
{
	[OwlPackable(OwlPackableMode.Generate)]
	private sealed class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		[OwlPackInclude]
		public readonly List<EntityFactRef<Buff>> Buffs = new List<EntityFactRef<Buff>>();

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("Buffs", typeof(List<EntityFactRef<Buff>>))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ComponentData source = new ComponentData();
			result = Unsafe.As<ComponentData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ComponentData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			List<EntityFactRef<Buff>> value = Buffs;
			formatter.Field(0, "Buffs", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ComponentData>();
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
					Unsafe.AsRef(in Buffs) = formatter.ReadPackable<List<EntityFactRef<Buff>>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public TargetType Filter;

	[Tooltip("Накладывать/снимать бафф в начале хода если условие начало/перестало выполняться")]
	public bool UpdateAtTurnStart;

	public bool TriggerOnEnterWhenActivated = true;

	public bool TriggerOnExitWhenDeactivated = true;

	[SerializeField]
	public BpRef<BlueprintBuff> Buff;

	void IEntityEnterCohesionRangeHandler.HandleEntityEnterCohesionRange(MechanicEntity entity)
	{
		TryApplyBuff(entity);
	}

	void IEntityExitCohesionRangeHandler.HandleEntityExitCohesionRange(MechanicEntity entity)
	{
		TryRemoveBuff(entity);
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (!UpdateAtTurnStart)
		{
			return;
		}
		foreach (UnitEntity item in base.Owner.GetRequired<PartCohesion>().UnitsInRange)
		{
			if (IsSuitable(item))
			{
				ApplyBuff(item);
			}
			else
			{
				RemoveBuff(item);
			}
		}
	}

	private void TryApplyBuff(MechanicEntity entity)
	{
		if (IsSuitable(entity))
		{
			ApplyBuff(entity);
		}
	}

	private void TryRemoveBuff(MechanicEntity entity)
	{
		if (IsSuitable(entity))
		{
			RemoveBuff(entity);
		}
	}

	private void ApplyBuff(MechanicEntity entity)
	{
		List<EntityFactRef<Buff>> buffs = RequestSavableData<ComponentData>().Buffs;
		if (!buffs.HasItem((EntityFactRef<Buff> i) => i.Entity == entity))
		{
			Buff buff = entity.Buffs.Add(Buff, base.Owner);
			if (buff != null)
			{
				buff.AddSource(base.Fact, this);
				buffs.Add(buff);
			}
		}
	}

	private void RemoveBuff(MechanicEntity entity)
	{
		RequestSavableData<ComponentData>().Buffs.RemoveAll(delegate(EntityFactRef<Buff> i)
		{
			bool num = i.Entity == entity;
			Buff fact = i.Fact;
			if (num && fact != null)
			{
				fact.RemoveRank(1, base.Owner);
				fact.RemoveSource(base.Fact, this);
			}
			return num;
		});
	}

	private bool IsSuitable(MechanicEntity entity)
	{
		if (Filter switch
		{
			TargetType.Enemy => base.Owner.IsEnemy(entity), 
			TargetType.Ally => base.Owner.IsAlly(entity), 
			TargetType.Any => true, 
			_ => throw new ArgumentOutOfRangeException(), 
		})
		{
			return Restriction.IsPassed(base.Context, entity);
		}
		return false;
	}

	protected override void OnActivate()
	{
		if (!TriggerOnEnterWhenActivated)
		{
			return;
		}
		PartCohesion optional = base.Owner.GetOptional<PartCohesion>();
		if (optional == null)
		{
			return;
		}
		foreach (UnitEntity item in optional.UnitsInRange)
		{
			TryApplyBuff(item);
		}
	}

	protected override void OnDeactivate()
	{
		if (!TriggerOnExitWhenDeactivated)
		{
			return;
		}
		PartCohesion optional = base.Owner.GetOptional<PartCohesion>();
		if (optional == null)
		{
			return;
		}
		foreach (UnitEntity item in optional.UnitsInRange)
		{
			TryRemoveBuff(item);
		}
	}
}
