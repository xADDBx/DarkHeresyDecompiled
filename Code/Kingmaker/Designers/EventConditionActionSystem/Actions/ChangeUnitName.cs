using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("7cc58858100b48d2afccde1925f97993")]
public class ChangeUnitName : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[HideIf("ReturnTheOldName")]
	public LocalizedString NewName;

	public bool ReturnTheOldName;

	public override string GetDescription()
	{
		return $"Меняет имя юниту {Unit} на новое {NewName}";
	}

	public override string GetCaption()
	{
		return $"Change {Unit} unit name to {NewName}";
	}

	protected override void RunAction()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			LocalizedString obj = (ReturnTheOldName ? null : NewName);
			baseUnitEntity.Description.SetCustomNameLocalizedString(obj?.Key);
			EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity, (Action<IUnitNameHandler>)delegate(IUnitNameHandler h)
			{
				h.OnUnitNameChanged();
			}, isCheckRuntime: true);
		}
	}
}
