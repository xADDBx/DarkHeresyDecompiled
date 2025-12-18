using System.Linq;
using Kingmaker.Code.Gameplay.Features.DetectiveClues.View;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public static class OvertipUtils
{
	public static void PrewarmOvertips<T>(T prefab, int count, Transform targetContainer) where T : MonoBehaviour
	{
		WidgetFactory.InstantiateWidget(prefab, count, targetContainer);
	}

	public static bool CheckNeedMapObjectOvertip(MapObjectEntity mapObject)
	{
		InteractionSkillCheckSettings interactionSkillCheckSettings = mapObject.GetOptional<InteractionSkillCheckPart>()?.Settings;
		if (interactionSkillCheckSettings != null && !interactionSkillCheckSettings.ShowOvertip && interactionSkillCheckSettings.DisplayName == null && interactionSkillCheckSettings.DisplayNameAfterUse == null && interactionSkillCheckSettings.CheckFailedActions == null && interactionSkillCheckSettings.CheckPassedActions == null && interactionSkillCheckSettings.CheckFailBark == null && interactionSkillCheckSettings.CheckPassedBark == null)
		{
			return false;
		}
		InteractionPartDetectiveClue optional = mapObject.GetOptional<InteractionPartDetectiveClue>();
		if (optional == null || optional.Type != InteractionType.Variant)
		{
			InteractionPartDetectiveTrace optional2 = mapObject.GetOptional<InteractionPartDetectiveTrace>();
			if (optional2 == null || optional2.Type != InteractionType.Variant)
			{
				if (mapObject.View.GetComponent<AreaTransition>() != null)
				{
					return false;
				}
				return !(mapObject is DestructibleEntity);
			}
		}
		return false;
	}

	public static bool IsAdditionalCombatOvertip(MapObjectEntity mapObject, out InteractionSkillCheckPart skillCheck)
	{
		skillCheck = mapObject.GetOptional<InteractionSkillCheckPart>();
		return skillCheck?.Settings.AdditionalCombatObjective != null;
	}

	public static bool IsAdditionalCombatOvertip(MapObjectEntity mapObject)
	{
		InteractionVariativePart optional = mapObject.GetOptional<InteractionVariativePart>();
		if (optional != null)
		{
			return optional.GetAvailableInteractions()?.Any((InteractionWithConditions iwc) => iwc.GetVariantActor().CombatObjective != null) ?? false;
		}
		return mapObject.GetOptional<InteractionSkillCheckPart>()?.Settings.AdditionalCombatObjective != null;
	}

	public static bool IsVisited(AbstractInteractionPart interaction)
	{
		if (interaction is InteractionVariativePart interactionVariativePart)
		{
			return interactionVariativePart.AlreadyVisited;
		}
		if (interaction is InteractionSkillCheckPart interactionSkillCheckPart && interactionSkillCheckPart?.Settings.AdditionalCombatObjective != null)
		{
			return interactionSkillCheckPart.AlreadyUsed;
		}
		return interaction.AlreadyVisited;
	}

	public static bool IsDetectiveInteract(UIInteractionType uiInteractionType)
	{
		return uiInteractionType switch
		{
			UIInteractionType.None => false, 
			UIInteractionType.Action => false, 
			UIInteractionType.Move => false, 
			UIInteractionType.Info => false, 
			UIInteractionType.Credits => false, 
			UIInteractionType.DetectiveTrace => true, 
			UIInteractionType.DetectiveClue => true, 
			_ => false, 
		};
	}
}
