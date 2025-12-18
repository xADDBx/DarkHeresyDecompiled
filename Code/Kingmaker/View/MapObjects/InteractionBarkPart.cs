using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Localization;
using Kingmaker.UI.Sound;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class InteractionBarkPart : InteractionPart<InteractionBarkSettings>, IHashable, IOwlPackable<InteractionBarkPart>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InteractionBarkPart",
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

	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Info;
	}

	public override bool CanInteract()
	{
		ConditionsHolder conditionsHolder = base.Settings.Condition?.Get();
		if (conditionsHolder != null)
		{
			ConditionsChecker conditions = conditionsHolder.Conditions;
			if (conditions != null && conditions.HasConditions)
			{
				using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
				{
					if (!conditionsHolder.Conditions.Check())
					{
						return false;
					}
				}
			}
		}
		return base.CanInteract();
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		SharedStringAsset bark = base.Settings.GetBark();
		if (bark == null)
		{
			return;
		}
		Entity entity = (base.Settings.ShowOnUser ? ((MechanicEntity)user) : ((MechanicEntity)base.Owner));
		float duration = UtilityBark.DefaultBarkTime;
		if (base.Settings.BarkDurationByText)
		{
			duration = UtilityBark.GetBarkDuration(bark.String);
		}
		if (base.Settings.OverrideBarkDuration)
		{
			duration = base.Settings.BarkDuration;
		}
		string voGuidBySourceAndTarget = VoiceOverController.GetVoGuidBySourceAndTarget(base.Settings, entity);
		BarkPlayer.Bark(entity, bark.String, VoiceOverType.Bark, voGuidBySourceAndTarget, duration, user);
		ActionsHolder actionsHolder = base.Settings.BarkActions?.Get();
		if (actionsHolder == null)
		{
			return;
		}
		ActionList actions = actionsHolder.Actions;
		if (actions == null || !actions.HasActions || (base.Settings.RunActionsOnce && base.Settings.ActionsRan))
		{
			return;
		}
		using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(user))
			{
				actionsHolder.Actions.Run();
				base.Settings.ActionsRan = true;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InteractionBarkPart source = new InteractionBarkPart();
		result = Unsafe.As<InteractionBarkPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<InteractionBarkPart>(OwlPackTypeInfo);
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
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InteractionBarkPart>();
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
