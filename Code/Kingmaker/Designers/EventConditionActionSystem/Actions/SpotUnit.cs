using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("ce78ce77b3ca12645af87aac7696ad3a")]
public class SpotUnit : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	[SerializeReference]
	public AbstractUnitEvaluator Spotter;

	public override string GetCaption()
	{
		if (Spotter == null)
		{
			return $"Main character spot unit {Target}";
		}
		return $"{Spotter} spot unit {Target}";
	}

	protected override void RunAction()
	{
		BaseUnitEntity spotter = Game.Instance.Player.MainCharacterEntity;
		if (Spotter != null)
		{
			if (!(Spotter.GetValue() is BaseUnitEntity baseUnitEntity))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Spotter} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return;
			}
			spotter = baseUnitEntity;
		}
		if (!(Target.GetValue() is BaseUnitEntity baseUnitEntity2))
		{
			string message2 = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Target} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message2))
			{
				UberDebug.LogError(message2);
			}
		}
		else if (baseUnitEntity2.Stealth.AddSpottedBy(spotter))
		{
			EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity2, (Action<IUnitSpottedHandler>)delegate(IUnitSpottedHandler h)
			{
				h.HandleUnitSpotted(spotter);
			}, isCheckRuntime: true);
		}
	}
}
