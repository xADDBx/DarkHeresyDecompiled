using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.Gameplay.ContextActions;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("a0dfa475bb7444e0bba3732248cd3120")]
public sealed class RepeatAbilityCastNextTurn : UnitBuffComponentDelegate, IUnitCommandEndHandler<EntitySubscriber>, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitCommandEndHandler, EntitySubscriber>, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>
{
	[OwlPackable(OwlPackableMode.Generate)]
	private sealed class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		[CanBeNull]
		[OwlPackInclude]
		public AbilityData Ability;

		[CanBeNull]
		[OwlPackInclude]
		public TargetWrapper Target;

		[CanBeNull]
		public UnitCommandHandle RepeatCmd;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("Ability", typeof(AbilityData)),
				new FieldInfo("Target", typeof(TargetWrapper))
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
			formatter.Field(0, "Ability", ref Ability, state);
			formatter.Field(1, "Target", ref Target, state);
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
					Ability = formatter.ReadPackable<AbilityData>(state);
					break;
				case 1:
					Target = formatter.ReadPackable<TargetWrapper>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public bool RemoveBuffAfterRepeat = true;

	public bool HierophantRhyme;

	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public ActionList OnSchedule = new ActionList();

	public ActionList OnRepeated = new ActionList();

	protected override void OnActivate()
	{
		AbilityData current = SimpleContextData<AbilityData, ContextActionRhymeAbility.Scope.Ability>.Current;
		TargetWrapper current2 = SimpleContextData<TargetWrapper, ContextActionRhymeAbility.Scope.Target>.Current;
		if (current != null && current2 != null)
		{
			ComponentData componentData = RequestSavableData<ComponentData>();
			componentData.Ability = current;
			componentData.Target = current2;
		}
	}

	void IUnitCommandEndHandler.HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		ComponentData componentData = RequestSavableData<ComponentData>();
		if (componentData.RepeatCmd?.Cmd == command)
		{
			componentData.RepeatCmd = null;
		}
		else
		{
			if (componentData.Ability != null || !(command is UnitUseAbility { Ability: { } ability } unitUseAbility))
			{
				return;
			}
			TargetWrapper target = command.Target;
			if ((object)target == null)
			{
				return;
			}
			AbilityExecutionProcess executionProcess = unitUseAbility.ExecutionProcess;
			if (executionProcess != null && Restriction.IsPassed(base.Context, null, target, null, ability))
			{
				AbilityData abilityData = ability.Clone();
				abilityData.IsRhymed = HierophantRhyme;
				abilityData.UnrestrictedRanged = HierophantRhyme;
				abilityData.IgnoreRestrictions = true;
				componentData.Ability = abilityData;
				componentData.Target = target;
				using (EvalContext.Current.PushSourceAbility(executionProcess.Context.Ability))
				{
					OnSchedule.Run();
				}
			}
		}
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if ((bool)ContextData<TurnController.InterruptTurnEndMark>.Current)
		{
			return;
		}
		ComponentData componentData = RequestSavableData<ComponentData>();
		if (componentData == null)
		{
			return;
		}
		AbilityData ability = componentData.Ability;
		if ((object)ability == null)
		{
			return;
		}
		TargetWrapper target = componentData.Target;
		if ((object)target == null)
		{
			return;
		}
		componentData.RepeatCmd = base.Owner.Commands.AddToQueue(new UnitUseAbilityParams(ability, target)
		{
			IgnoreCooldown = true,
			FreeAction = true,
			NeedLoS = !HierophantRhyme
		});
		componentData.Ability = null;
		componentData.Target = null;
		if (RemoveBuffAfterRepeat)
		{
			base.Buff.MarkExpired();
		}
		using (EvalContext.Current.PushTarget(target))
		{
			OnRepeated.Run();
		}
	}
}
