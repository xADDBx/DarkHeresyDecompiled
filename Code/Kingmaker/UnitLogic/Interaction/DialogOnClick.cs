using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Interaction;

[Obsolete]
[ComponentName("Dialog/Start On Click")]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("e8816ba746a7cbf419ced2b0e9156560")]
public class DialogOnClick : UnitInteractionComponent, IDialogReference
{
	[JsonProperty]
	[SerializeField]
	[FormerlySerializedAs("Dialog")]
	private BlueprintDialogReference m_Dialog;

	[JsonProperty]
	public ActionList NoDialogActions;

	public BlueprintDialog Dialog => m_Dialog?.Get();

	public override bool IsDialog => true;

	[JsonConstructor]
	protected DialogOnClick()
	{
	}

	public override bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target)
	{
		if (!base.IsAvailable(initiator, target))
		{
			return false;
		}
		if (Dialog == null || Dialog.FirstCue.Cues.Count <= 0)
		{
			return NoDialogActions.HasActions;
		}
		return true;
	}

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		if (!(target is BaseUnitEntity unit))
		{
			return AbstractUnitCommand.ResultType.Fail;
		}
		if (Dialog == null)
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				NoDialogActions.Run();
			}
			return AbstractUnitCommand.ResultType.Fail;
		}
		DialogData data = DialogController.SetupDialogWithUnit(Dialog, unit, user);
		Game.Instance.Controllers.DialogController.StartDialog(data);
		return AbstractUnitCommand.ResultType.Success;
	}

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (dialog != Dialog)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}
}
