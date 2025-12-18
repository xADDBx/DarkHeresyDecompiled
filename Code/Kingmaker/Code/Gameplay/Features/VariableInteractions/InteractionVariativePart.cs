using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Interaction;
using Kingmaker.Interaction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Features.VariableInteractions;

[OwlPackable(OwlPackableMode.Generate)]
public class InteractionVariativePart : NewInteractionPart<InteractionVariativeSettings>, IHasInteractionVariantActors, IInteractionObjectUIHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IHashable, IOwlPackable<InteractionVariativePart>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InteractionVariativePart",
		OldNames = null,
		Fields = new FieldInfo[7]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("AlreadyUnlocked", typeof(bool)),
			new FieldInfo("AlreadyVisited", typeof(bool)),
			new FieldInfo("m_LastCombatRoundInteractionAttempt", typeof(int)),
			new FieldInfo("m_Enabled", typeof(bool)),
			new FieldInfo("SelectedVariantMapObject", typeof(MapObjectEntity)),
			new FieldInfo("PassedConditions", typeof(Dictionary<MapObjectEntity, ConditionsHolder>))
		}
	};

	public override UIInteractionType UIInteractionType => base.Settings.UIType;

	public override InteractionType Type => base.Settings.InteractionType;

	public override float OvertipVerticalCorrection => base.Settings.OvertipVerticalCorrection;

	public bool InteractThroughVariants => false;

	public float OvertipCorrection => base.Settings.OvertipVerticalCorrection;

	public override bool NotInCombat => base.Settings.NotInCombat;

	public override int ApproachRadius => (int)base.Settings.ProximityRadius;

	public override UnitAnimationInteractionType UseAnimationState => UnitAnimationInteractionType.None;

	public InteractionVariativeSettings InteractionSettings => base.Settings;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public MapObjectEntity SelectedVariantMapObject { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public Dictionary<MapObjectEntity, ConditionsHolder> PassedConditions { get; private set; } = new Dictionary<MapObjectEntity, ConditionsHolder>();


	protected override InteractionProcess OnInteract(BaseUnitEntity user)
	{
		SetVisited();
		return null;
	}

	public override bool CanInteract()
	{
		if (base.CanInteract())
		{
			if (base.Settings.VariativeType != VariativeType.ToggleGroup)
			{
				return GetAvailableInteractions().Any();
			}
			return true;
		}
		return false;
	}

	public IEnumerable<IInteractionVariantActor> GetInteractionVariantActors()
	{
		return from v in base.Settings.InteractionsWithConditions
			select v.GetVariantActor() into a
			where a != null
			select a;
	}

	public void SetSelectedVariant(IInteractionVariantActor variant)
	{
		if (variant is InteractionSkillCheckPart interactionSkillCheckPart)
		{
			SelectedVariantMapObject = interactionSkillCheckPart.View.Data;
		}
	}

	public void TrySetPassedConditions(ConditionsHolder conditions)
	{
		if (conditions == null)
		{
			foreach (InteractionWithConditions item in base.Settings.InteractionsWithConditions.Where(delegate(InteractionWithConditions iwc)
			{
				List<InteractionWithConditions.ShowReason> showReasons = iwc.ShowReasons;
				return showReasons != null && showReasons.Count == 0;
			}))
			{
				MapObjectEntity mapObject = item.GetMapObject();
				if (!PassedConditions.TryGetValue(mapObject, out var _))
				{
					PassedConditions.Add(mapObject, null);
				}
			}
			return;
		}
		InteractionWithConditions interactionWithConditions = base.Settings.InteractionsWithConditions.FirstOrDefault((InteractionWithConditions iwc) => iwc.ShowReasons.Any((InteractionWithConditions.ShowReason r) => r.Conditions.Get() == conditions));
		if (interactionWithConditions == null)
		{
			PFLog.UI.Error("Trying to add unavailable conditions");
			return;
		}
		MapObjectEntity mapObject2 = interactionWithConditions.GetMapObject();
		if (PassedConditions.TryGetValue(mapObject2, out var value2))
		{
			if (interactionWithConditions.ShowReasons.Select((InteractionWithConditions.ShowReason r) => r.Conditions.Get()).Contains(value2))
			{
				return;
			}
			PassedConditions.Remove(mapObject2);
		}
		PassedConditions.TryAdd(mapObject2, conditions.Reference());
	}

	public IInteractionVariantActor GetSelectedVariantActor()
	{
		return base.Settings.InteractionsWithConditions.FirstOrDefault((InteractionWithConditions v) => v.GetMapObject() == SelectedVariantMapObject)?.GetVariantActor();
	}

	public void HandleObjectHighlightChange()
	{
	}

	public void HandleObjectInteractChanged()
	{
		if (base.Settings.InteractionsWithConditions.Select((InteractionWithConditions v) => v.GetMapObject()).Any((MapObjectEntity i) => i.View.Data == EventInvokerExtensions.MapObjectEntity))
		{
			EventBus.RaiseEvent((IMapObjectEntity)base.View.Data, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectInteractChanged();
			}, isCheckRuntime: true);
		}
	}

	public void HandleObjectInteract()
	{
	}

	public IEnumerable<InteractionWithConditions> GetAvailableInteractions()
	{
		return base.Settings.InteractionsWithConditions.Where((InteractionWithConditions iwc) => iwc.GetVariantActor().CanUse && (iwc.ShowReasons == null || iwc.ShowReasons.Count == 0 || iwc.ShowReasons.Any((InteractionWithConditions.ShowReason r) => r.Conditions.IsEmpty() || r.Conditions.Get().Check())));
	}

	public override void SetVisited()
	{
		foreach (InteractionWithConditions availableInteraction in GetAvailableInteractions())
		{
			ConditionsReference conditionsReference = availableInteraction.ShowReasons.FirstOrDefault((InteractionWithConditions.ShowReason r) => r.Conditions.IsEmpty() || r.Conditions.Get().Check())?.Conditions;
			TrySetPassedConditions(conditionsReference);
		}
		base.SetVisited();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MapObjectEntity>.GetHash128(SelectedVariantMapObject);
		result.Append(ref val2);
		Dictionary<MapObjectEntity, ConditionsHolder> passedConditions = PassedConditions;
		if (passedConditions != null)
		{
			int val3 = 0;
			foreach (KeyValuePair<MapObjectEntity, ConditionsHolder> item in passedConditions)
			{
				Hash128 hash = default(Hash128);
				Hash128 val4 = ClassHasher<MapObjectEntity>.GetHash128(item.Key);
				hash.Append(ref val4);
				Hash128 val5 = SimpleBlueprintHasher.GetHash128(item.Value);
				hash.Append(ref val5);
				val3 ^= hash.GetHashCode();
			}
			result.Append(ref val3);
		}
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InteractionVariativePart source = new InteractionVariativePart();
		result = Unsafe.As<InteractionVariativePart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<InteractionVariativePart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = base.AlreadyUnlocked;
		formatter.UnmanagedField(1, "AlreadyUnlocked", ref value2, state);
		bool value3 = AlreadyVisited;
		formatter.UnmanagedField(2, "AlreadyVisited", ref value3, state);
		formatter.UnmanagedField(3, "m_LastCombatRoundInteractionAttempt", ref m_LastCombatRoundInteractionAttempt, state);
		formatter.UnmanagedField(4, "m_Enabled", ref m_Enabled, state);
		MapObjectEntity value4 = SelectedVariantMapObject;
		formatter.Field(5, "SelectedVariantMapObject", ref value4, state);
		Dictionary<MapObjectEntity, ConditionsHolder> value5 = PassedConditions;
		formatter.Field(6, "PassedConditions", ref value5, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InteractionVariativePart>();
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
				base.SourceType = formatter.ReadString(state);
				break;
			case 1:
				base.AlreadyUnlocked = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				AlreadyVisited = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_LastCombatRoundInteractionAttempt = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				m_Enabled = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				SelectedVariantMapObject = formatter.ReadPackable<MapObjectEntity>(state);
				break;
			case 6:
				PassedConditions = formatter.ReadPackable<Dictionary<MapObjectEntity, ConditionsHolder>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
