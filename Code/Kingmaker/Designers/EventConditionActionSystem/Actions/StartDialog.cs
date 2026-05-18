using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Localization;
using Kingmaker.QA;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/StartDialog")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("0e0c1fe99d7862d4492bcd1535939a9a")]
public class StartDialog : GameAction, IDialogReference
{
	[HideIf("IsDialogOnMapObject")]
	[Tooltip("Unit with BlueprintDialog. If unit have no BlueprintDialog or Null - Dialog from field 'Dialog' will be used.")]
	[SerializeReference]
	public AbstractUnitEvaluator DialogueOwner;

	public bool IsDialogOnMapObject;

	[ShowIf("IsDialogOnMapObject")]
	[Tooltip("Sets MapObject as dialogOwner")]
	[SerializeReference]
	public MapObjectEvaluator MapObjectDialogueOwner;

	[Tooltip("This dialog overrides dialog in 'Dialogue Owner' if it exists")]
	[SerializeField]
	[FormerlySerializedAs("Dialogue")]
	private BlueprintDialogReference m_Dialogue;

	[Tooltip("Evaluator. Works if Dialogue is null")]
	[SerializeReference]
	public BlueprintEvaluator DialogEvaluator;

	[Tooltip("Interlocutor name. Uses only if 'Dialogue Owner' is Null")]
	public LocalizedString SpeakerName;

	public BlueprintDialog Dialogue => m_Dialogue?.Get();

	protected override void RunAction()
	{
		BlueprintDialog blueprintDialog = (Dialogue ? Dialogue : (DialogEvaluator ? ((BlueprintDialog)DialogEvaluator.GetValue()) : null));
		if (DialogueOwner != null && !IsDialogOnMapObject)
		{
			if (!(DialogueOwner.GetValue() is BaseUnitEntity baseUnitEntity))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {DialogueOwner} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return;
			}
			StartDialog component = baseUnitEntity.Blueprint.GetComponent<StartDialog>();
			BlueprintDialog blueprintDialog2 = ((component != null) ? component.Dialogue : blueprintDialog);
			if (blueprintDialog2 != null)
			{
				DialogData data = DialogController.SetupDialogWithUnit(blueprintDialog2, baseUnitEntity);
				Game.Instance.Controllers.DialogController.StartDialog(data);
			}
		}
		else if (MapObjectDialogueOwner != null && IsDialogOnMapObject)
		{
			if (blueprintDialog != null)
			{
				DialogData data2 = DialogController.SetupDialogWithMapObject(blueprintDialog, MapObjectDialogueOwner.GetValue(), null);
				Game.Instance.Controllers.DialogController.StartDialog(data2);
			}
		}
		else if (blueprintDialog != null)
		{
			DialogData data3 = DialogController.SetupDialogWithoutTarget(blueprintDialog, SpeakerName);
			Game.Instance.Controllers.DialogController.StartDialog(data3);
		}
	}

	public override string GetCaption()
	{
		return string.Format("Start Dialog ({0})", Dialogue ? Dialogue.NameSafe() : (DialogEvaluator ? DialogEvaluator.GetCaption() : (IsDialogOnMapObject ? MapObjectDialogueOwner.GetCaption() : (DialogueOwner ? DialogueOwner.GetCaption() : "??"))));
	}

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (dialog != Dialogue)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}
}
