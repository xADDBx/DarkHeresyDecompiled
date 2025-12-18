using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("546c217ec8c177f4cb25b4b4b6ca35e8")]
internal class StartEtudeForced : PlayerUpgraderOnlyAction
{
	public BlueprintEtudeReference Etude;

	protected override void RunActionOverride()
	{
		BlueprintEtude blueprintEtude = Etude.Get();
		if ((bool)blueprintEtude && Game.Instance.EtudesSystem.EtudeIsNotStarted(blueprintEtude))
		{
			Game.Instance.EtudesSystem.StartEtude(blueprintEtude, "player upgrader " + base.AssetGuid);
		}
	}

	public override string GetCaption()
	{
		return "Start " + Etude;
	}

	public override string GetDescription()
	{
		return "Starts " + Etude.NameSafe() + " even if it does not allow action start";
	}
}
