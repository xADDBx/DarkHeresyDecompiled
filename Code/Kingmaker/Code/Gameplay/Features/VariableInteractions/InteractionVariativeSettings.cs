using System;
using System.Collections.Generic;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.Gameplay.Features.VariableInteractions;

[Serializable]
public class InteractionVariativeSettings
{
	[Header("General")]
	public UIInteractionType UIType;

	public VariativeType VariativeType;

	public bool NotInCombat = true;

	public float OvertipVerticalCorrection = 60f;

	public InteractionType InteractionType;

	public float ProximityRadius = 2f;

	public LocalizedString DisplayName;

	[FormerlySerializedAs("InteractionsWithConditions")]
	[Header("Interactions")]
	public List<InteractionWithConditions> Interactions;
}
