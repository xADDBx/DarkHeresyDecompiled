using System;
using System.Collections.Generic;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.Code.Gameplay.Features.VariableInteractions;

[Serializable]
public class InteractionVariativeSettings
{
	public UIInteractionType UIType;

	public VariativeType VariativeType;

	[InspectorReadOnly]
	[Obsolete]
	public List<InteractionSkillCheck> Variants;

	public List<InteractionWithConditions> InteractionsWithConditions;

	public bool NotInCombat = true;

	public SharedStringAsset DisplayName;

	public float OvertipVerticalCorrection = 60f;

	public InteractionType InteractionType;

	public float ProximityRadius = 2f;
}
