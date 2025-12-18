using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("740349afc5a347d088228fbba1ea243e")]
public class MarkDialogSeen : PlayerUpgraderOnlyAction
{
	public BlueprintDialogReference Dialog;

	protected override void RunActionOverride()
	{
		BlueprintDialog blueprintDialog = Dialog.Get();
		if ((bool)blueprintDialog)
		{
			Game.Instance.DialogState.ShownDialogsAdd(blueprintDialog);
		}
	}

	public override string GetCaption()
	{
		return $"Mark dialog {Dialog} seen";
	}

	public override string GetDescription()
	{
		return $"Mark dialog {Dialog} seen";
	}
}
