using System;
using System.Threading.Tasks;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Framework.Interaction;

[Serializable]
public sealed class PlayCutsceneInteractionModule : InteractionModule
{
	public BpRef<BlueprintCutscene> Cutscene = new BpRef<BlueprintCutscene>();

	public override string GetCaption()
	{
		return "Play cutscene";
	}

	public override Task Execute(BaseUnitEntity initiator, MapObjectEntity target)
	{
		BlueprintCutscene maybeBlueprint = Cutscene.MaybeBlueprint;
		if (maybeBlueprint != null)
		{
			CutscenePlayerView.Play(maybeBlueprint);
		}
		return Task.CompletedTask;
	}
}
