using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ShowCombatText")]
[AllowMultipleComponents]
[TypeId("48a1f355d7f9401ebade9815a803574a")]
public class ShowCombatText : GameAction
{
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Action)]
	public LocalizedString CombatText;

	[SerializeReference]
	public AbstractUnitEvaluator TargetUnit;

	protected override void RunAction()
	{
		EventBus.RaiseEvent((IMechanicEntity)TargetUnit.GetValue(), (Action<IUnitCustomCombatText>)delegate(IUnitCustomCombatText h)
		{
			h.HandleCustomCombatText(CombatText);
		}, isCheckRuntime: true);
	}

	public override string GetCaption()
	{
		return $"Show Combat text (on {TargetUnit})";
	}
}
