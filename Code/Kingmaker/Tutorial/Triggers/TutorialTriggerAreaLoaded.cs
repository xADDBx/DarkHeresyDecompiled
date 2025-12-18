using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("85e64be4d52eb4a40a22cf156f118dd3")]
public class TutorialTriggerAreaLoaded : TutorialTrigger, IAreaActivationHandler, ISubscriber
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintAreaReference m_Area;

	[SerializeField]
	[InfoBox("Always true if empty")]
	private ConditionsChecker m_Condition;

	public void OnAreaActivated()
	{
		if (Game.Instance.CurrentlyLoadedArea == m_Area.Get() && (!m_Condition.HasConditions || m_Condition.Check()))
		{
			TryToTrigger(null);
		}
	}
}
