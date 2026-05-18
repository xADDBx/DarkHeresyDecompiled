using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[ComponentName("Command/CommandStartDialog")]
[TypeId("dd8667f2c38e5804fba853885780b39a")]
public class CommandStartDialog : CommandBase, IDialogReference
{
	private class Finisher : IDialogFinishHandler, ISubscriber
	{
		private BlueprintDialog m_Dialog;

		private BlueprintCutscene m_Cutscene;

		public bool Finished { get; set; }

		public void Subscribe(BlueprintDialog dialog, BlueprintCutscene cutscene)
		{
			m_Dialog = dialog;
			m_Cutscene = cutscene;
			Finished = false;
			EventBus.Subscribe(this);
		}

		void IDialogFinishHandler.HandleDialogFinished(BlueprintDialog dialog, bool success)
		{
			Finished = true;
			if (!success)
			{
				PFLog.Default.Error($"Dialog {m_Dialog} failed to start in cutscene {m_Cutscene}");
				QAModeExceptionReporter.MaybeShowError($"Dialog {m_Dialog} failed to start in cutscene {m_Cutscene}");
			}
			EventBus.Unsubscribe(this);
		}
	}

	[SerializeReference]
	public AbstractUnitEvaluator Speaker;

	[SerializeField]
	[FormerlySerializedAs("Dialog")]
	private BlueprintDialogReference m_Dialog;

	[SerializeReference]
	public BlueprintEvaluator DialogEvaluator;

	public LocalizedString SpeakerName;

	public BlueprintDialog Dialog => m_Dialog?.Get();

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		BlueprintDialog blueprintDialog = Dialog;
		if (blueprintDialog == null && DialogEvaluator != null && DialogEvaluator.TryGetValue(out var value) && value is BlueprintDialog blueprintDialog2)
		{
			blueprintDialog = blueprintDialog2;
		}
		if (blueprintDialog == null)
		{
			OnStop(player);
			player.GetCommandData<Finisher>(this).Finished = true;
			return CommandResult.FailWithReport($"Cutscene command {this} in {player.Cutscene} unable to start dialog: no dialog found");
		}
		if (Speaker != null)
		{
			if (!Speaker.TryGetValue(out var value2) || !(value2 is BaseUnitEntity unit))
			{
				return CommandResult.FailWithReport("Dialog speaker is set to invalid Unit");
			}
			DialogData data = DialogController.SetupDialogWithUnit(blueprintDialog, unit);
			Game.Instance.Controllers.DialogController.StartDialog(data);
		}
		else
		{
			DialogData data2 = DialogController.SetupDialogWithoutTarget(blueprintDialog, SpeakerName);
			Game.Instance.Controllers.DialogController.StartDialog(data2);
		}
		player.GetCommandData<Finisher>(this).Subscribe(blueprintDialog, player.Cutscene);
		return CommandResult.Success;
	}

	public override bool TrySkip(CutscenePlayerData player)
	{
		return false;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Finisher>(this).Finished;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "<b>Dialog</b> " + (Dialog ? Dialog.name : "???");
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
