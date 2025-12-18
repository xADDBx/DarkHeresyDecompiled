using System;
using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Features.DetectiveClues.View;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Interaction;
using Kingmaker.Localization;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Features.VariableInteractions;

[Serializable]
public class InteractionWithConditions
{
	[Serializable]
	public class ShowReason
	{
		public ConditionsReference Conditions;

		public SharedStringAsset Reason;
	}

	[field: SerializeField]
	public AbstractEntityPartComponent VariantActor { get; private set; }

	[field: SerializeField]
	public List<ShowReason> ShowReasons { get; private set; }

	[field: SerializeField]
	public ConditionsChecker SelectConditions { get; private set; }

	[field: SerializeField]
	public SharedStringAsset CannotSelectReason { get; private set; }

	public InteractionWithConditions()
	{
	}

	public InteractionWithConditions(AbstractEntityPartComponent partComponent)
	{
		VariantActor = partComponent;
	}

	public IInteractionVariantActor GetVariantActor()
	{
		AbstractEntityPartComponent variantActor = VariantActor;
		if (!(variantActor is InteractionSkillCheck))
		{
			if (!(variantActor is InteractionDetectiveClue))
			{
				if (variantActor is InteractionDetectiveTrace)
				{
					return GetVariantActor<InteractionPartDetectiveTrace>();
				}
				return null;
			}
			return GetVariantActor<InteractionPartDetectiveClue>();
		}
		return GetVariantActor<InteractionSkillCheckPart>();
	}

	public MapObjectEntity GetMapObject()
	{
		AbstractEntityPartComponent variantActor = VariantActor;
		if (!(variantActor is InteractionSkillCheck interactionSkillCheck))
		{
			if (!(variantActor is InteractionDetectiveClue interactionDetectiveClue))
			{
				if (variantActor is InteractionDetectiveTrace interactionDetectiveTrace)
				{
					return (interactionDetectiveTrace.GetComponent<EntityViewBase>() as MapObjectView)?.Data;
				}
				return null;
			}
			return (interactionDetectiveClue.GetComponent<EntityViewBase>() as MapObjectView)?.Data;
		}
		return (interactionSkillCheck.GetComponent<EntityViewBase>() as MapObjectView)?.Data;
	}

	public IInteractionVariantActor GetVariantActor<TPart>() where TPart : ViewBasedPart, new()
	{
		EntityViewBase entityViewBase = VariantActor.GetComponent<EntityViewBase>().Or(null);
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
		return new InteractionActorWithConditions(GetVariantActor(), ShowReasons, SelectConditions, CannotSelectReason);
	}
}
