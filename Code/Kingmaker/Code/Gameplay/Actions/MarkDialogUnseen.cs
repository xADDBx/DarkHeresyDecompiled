using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Actions;

[TypeId("c67b97591b924584bafa7b49ce75b118")]
public class MarkDialogUnseen : GameAction
{
	[SerializeField]
	private BlueprintDialogReference m_Dialogue;

	public override string GetCaption()
	{
		return "Mark Dialog " + m_Dialogue?.Get()?.name + " unseen";
	}

	protected override void RunAction()
	{
		BlueprintDialog blueprintDialog = m_Dialogue.Get();
		if ((bool)blueprintDialog)
		{
			Game.Instance.DialogState.ShownDialogsRemove(blueprintDialog);
		}
	}
}
