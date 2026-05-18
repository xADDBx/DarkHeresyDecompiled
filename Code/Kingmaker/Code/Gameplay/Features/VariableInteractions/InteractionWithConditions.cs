using System;
using System.Collections.Generic;
using Code.BlueprintSystem.Attributes;
using Kingmaker.Code.Gameplay.Features.DetectiveClues.View;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Interaction;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.Gameplay.Features.VariableInteractions;

[Serializable]
public class InteractionWithConditions
{
	[Serializable]
	public class ShowReason
	{
		[ValidateNotNull]
		public ConditionsReference Conditions;

		[FormerlySerializedAs("Reason")]
		public LocalizedString ShowHint;
	}

	[field: FormerlySerializedAs("<VariantActor>k__BackingField")]
	[field: SerializeField]
	[field: ValidFieldType(typeof(InteractionSkillCheck))]
	[field: ValidFieldType(typeof(InteractionDetectiveClue))]
	[field: ValidFieldType(typeof(InteractionDetectiveTrace))]
	[field: ValidFieldType(typeof(InteractionAction))]
	public AbstractEntityPartComponent Interaction { get; private set; }

	[field: FormerlySerializedAs("<ShowReasons>k__BackingField")]
	[field: Tooltip("Список условий при выполнении любого из которых этот интеракт будет показан. По наведению на этот вариант в хинте справа будет показана строка \"ShowHint\"")]
	[field: SerializeField]
	public List<ShowReason> ShowConditions { get; private set; }

	[field: SerializeField]
	[field: Tooltip("Эвалюатор условий, при которых интеракт можно будет выбрать в списке. По наведению на этот вариант будет показана строка \"CannotSelectReason\", если вариант недоступен")]
	public ConditionsChecker SelectConditions { get; private set; }

	[field: ShowIf("HasSelectConditions")]
	[field: SerializeField]
	public LocalizedString CannotSelectReason { get; private set; }

	private bool HasSelectConditions => SelectConditions.HasConditions;

	public InteractionWithConditions()
	{
	}

	public InteractionWithConditions(AbstractEntityPartComponent partComponent)
	{
		Interaction = partComponent;
	}

	public IInteractionVariantActor GetVariantActor()
	{
		AbstractEntityPartComponent interaction = Interaction;
		if (!(interaction is InteractionSkillCheck))
		{
			if (!(interaction is InteractionDetectiveClue))
			{
				if (!(interaction is InteractionDetectiveTrace))
				{
					if (interaction is InteractionAction)
					{
						return GetVariantActor<InteractionActionPart>();
					}
					return null;
				}
				return GetVariantActor<InteractionPartDetectiveTrace>();
			}
			return GetVariantActor<InteractionPartDetectiveClue>();
		}
		return GetVariantActor<InteractionSkillCheckPart>();
	}

	public MapObjectEntity GetMapObject()
	{
		AbstractEntityPartComponent interaction = Interaction;
		if (!(interaction is InteractionSkillCheck component))
		{
			if (!(interaction is InteractionDetectiveClue component2))
			{
				if (!(interaction is InteractionDetectiveTrace component3))
				{
					if (interaction is InteractionAction component4)
					{
						return component4.GetEntityPart<InteractionActionPart>().Owner;
					}
					return null;
				}
				return component3.GetEntityPart<InteractionPartDetectiveTrace>().Owner;
			}
			return component2.GetEntityPart<InteractionPartDetectiveClue>().Owner;
		}
		return component.GetEntityPart<InteractionSkillCheckPart>().Owner;
	}

	public IInteractionVariantActor GetVariantActor<TPart>() where TPart : EntityPartWithConfig, new()
	{
		EntityViewBase entityViewBase = Interaction.GetComponent<EntityViewBase>().Or(null);
		object obj;
		if ((object)entityViewBase == null)
		{
			obj = null;
		}
		else
		{
			IEntity data = entityViewBase.Data;
			obj = ((data != null) ? data.ToEntity().GetOrCreate<TPart>() : null);
		}
		return obj as IInteractionVariantActor;
	}

	public InteractionActorWithConditions ToActorWithConditions()
	{
		return new InteractionActorWithConditions(GetVariantActor(), ShowConditions, SelectConditions, CannotSelectReason);
	}
}
