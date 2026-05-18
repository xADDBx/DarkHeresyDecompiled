using System;
using System.Threading.Tasks;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Framework.Interaction;

[Serializable]
public sealed class StartDialogInteractionModule : InteractionModule
{
	public BpRef<BlueprintDialog> Dialog = new BpRef<BlueprintDialog>();

	public override string GetCaption()
	{
		return "Start dialog";
	}

	public override Task Execute(BaseUnitEntity initiator, MapObjectEntity target)
	{
		BlueprintDialog maybeBlueprint = Dialog.MaybeBlueprint;
		if (maybeBlueprint != null)
		{
			DialogData data = DialogController.SetupDialogWithMapObject(maybeBlueprint, target, null, initiator);
			Game.Instance.Controllers.DialogController.StartDialog(data);
		}
		return Task.CompletedTask;
	}
}
