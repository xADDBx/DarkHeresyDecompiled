using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.Framework.Interaction;

[Serializable]
public class NewInteractionActionSettings
{
	public UIInteractionType UIType = UIInteractionType.Info;

	public InteractionType InteractionType = InteractionType.Approach;

	public bool NotInCombat = true;

	public int ProximityRadius = 2;

	public bool DisableAfterUse;

	public float OvertipVerticalCorrection;

	[ShowCreator]
	public ConditionsReference Conditions = new ConditionsReference();

	[SerializeReference]
	public List<InteractionModule> Modules = new List<InteractionModule>();
}
