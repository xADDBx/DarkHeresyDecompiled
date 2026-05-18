using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class InteractionActionPart : InteractionPart<InteractionActionSettings>, IInteractionVariantActor, IInteractionRestriction, IHashable, IOwlPackable<InteractionActionPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InteractionActionPart",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("AlreadyUnlocked", typeof(bool)),
			new FieldInfo("AlreadyVisited", typeof(bool)),
			new FieldInfo("m_LastCombatRoundInteractionAttempt", typeof(int)),
			new FieldInfo("m_Enabled", typeof(bool))
		}
	};

	public int? InteractionDC => null;

	InteractionActorType IInteractionVariantActor.Type => InteractionActorType.Default;

	public UIInteractionType UIType => base.Settings.UIType;

	public AbstractInteractionPart InteractionPart => this;

	public BlueprintAdditionalCombatObjective CombatObjective => null;

	public bool ShowInteractFx => false;

	public int? RequiredItemsCount => null;

	public BlueprintItem RequiredItem => null;

	public StatType Skill => StatType.Unknown;

	public bool CheckOnlyOnce => false;

	public bool CanUse => Enabled;

	public bool AlreadyUsed => false;

	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	public override bool CanInteract()
	{
		if (base.Settings.UseGlobalCooldown)
		{
			InteractionGlobalCooldownController controller = Game.Instance.GetController<InteractionGlobalCooldownController>();
			if (controller != null && !controller.CheckGlobalCooldown())
			{
				return false;
			}
		}
		ConditionsHolder conditionsHolder = base.Settings.Condition.Get();
		if ((bool)conditionsHolder && conditionsHolder.Conditions.HasConditions)
		{
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				if (!conditionsHolder.Conditions.Check())
				{
					return false;
				}
			}
		}
		return base.CanInteract();
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		ActionsHolder actionsHolder = base.Settings.Actions?.Get();
		if (actionsHolder != null && actionsHolder.HasActions)
		{
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				using (ContextData<InteractingUnitData>.Request().Setup(user))
				{
					actionsHolder.Run();
				}
			}
		}
		if (base.Settings.UseGlobalCooldown)
		{
			Game.Instance.GetController<InteractionGlobalCooldownController>()?.UpdateGlobalCooldown(base.Owner);
		}
	}

	public override string ToString()
	{
		return string.Format("{0}[{1}]", "InteractionActionPart", base.Owner);
	}

	public string GetInteractionName()
	{
		return base.Settings.DisplayName?.Text;
	}

	public bool CheckRestriction(BaseUnitEntity user)
	{
		return CanInteract();
	}

	public void ShowSuccessBark(BaseUnitEntity user)
	{
	}

	public void ShowRestrictionBark(BaseUnitEntity user)
	{
	}

	void IInteractionVariantActor.OnDidInteract(BaseUnitEntity user)
	{
		OnDidInteract(user);
	}

	public void OnFailedInteract(BaseUnitEntity user)
	{
	}

	bool IInteractionVariantActor.TryInteract(BaseUnitEntity user)
	{
		return true;
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
		InteractionActionPart source = new InteractionActionPart();
		result = Unsafe.As<InteractionActionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<InteractionActionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = base.AlreadyUnlocked;
		formatter.UnmanagedField(1, "AlreadyUnlocked", ref value2, state);
		bool value3 = AlreadyVisited;
		formatter.UnmanagedField(2, "AlreadyVisited", ref value3, state);
		formatter.UnmanagedField(3, "m_LastCombatRoundInteractionAttempt", ref m_LastCombatRoundInteractionAttempt, state);
		formatter.UnmanagedField(4, "m_Enabled", ref m_Enabled, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InteractionActionPart>();
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
			}
		}
		formatter.LeaveObject();
	}
}
