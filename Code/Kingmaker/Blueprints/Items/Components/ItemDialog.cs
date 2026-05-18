using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintItem))]
[TypeId("30e1c6dc037bcba48bf8ac2d70793262")]
public class ItemDialog : BlueprintComponent
{
	[SerializeField]
	private LocalizedString m_ItemName;

	[SerializeField]
	private BlueprintDialogReference m_DialogReference;

	public BlueprintDialog Dialog => m_DialogReference?.Get();

	public LocalizedString ItemName => m_ItemName;

	public void StartDialog()
	{
		BlueprintDialog blueprintDialog = m_DialogReference.Get();
		if (blueprintDialog != null)
		{
			DialogData data = DialogController.SetupDialogWithoutTarget(blueprintDialog, m_ItemName, Game.Instance.Player.MainCharacterEntity);
			Game.Instance.Controllers.DialogController.StartDialog(data);
		}
	}
}
