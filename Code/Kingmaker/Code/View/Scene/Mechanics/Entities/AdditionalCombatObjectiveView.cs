using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.Code.View.Scene.Mechanics.Entities;

[Obsolete]
[KnowledgeDatabaseID("1a028169b4444892a77aa9e6ffbb9e9b")]
public class AdditionalCombatObjectiveView : MapObjectView
{
	protected override bool HasHighlight => true;

	protected override bool CheckHighlightConditions()
	{
		if (base.Data != null && base.Data.IsRevealed)
		{
			if (!CanBeAttackedDirectly)
			{
				return base.Data.Parts.GetAll<AbstractInteractionPart>().Any(ShouldHighlightInteraction);
			}
			return true;
		}
		return false;
		static bool ShouldHighlightInteraction(AbstractInteractionPart i)
		{
			InteractionType type = i.Type;
			if (type == InteractionType.Approach || type == InteractionType.Direct)
			{
				if (i.ShowOvertip)
				{
					return i.ShowHighlight;
				}
				return true;
			}
			return false;
		}
	}

	protected override Color GetHighlightColor()
	{
		ViewHighlightingColors viewHighlightingColors = MapObjectView.UIConfig.ViewHighlightingColors;
		if (!base.MouseHoverHighlighting)
		{
			return viewHighlightingColors.AdditionalCombatObjective.HighlightColor;
		}
		return viewHighlightingColors.AdditionalCombatObjective.HoverColor;
	}
}
