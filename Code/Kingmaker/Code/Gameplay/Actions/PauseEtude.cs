using Kingmaker.AreaLogic.Etudes;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("857bd4ab54a54e718d8e20de533585ef")]
public class PauseEtude : PauseEtudeBase, IEtudeReference
{
	public override string GetDescription()
	{
		return "Ставит этюд '" + base.Name + "' на паузу";
	}

	public override string GetCaption()
	{
		return "Pauses etude '" + base.Name + "'";
	}

	protected override void RunAction()
	{
		string source = "action PauseEtude " + base.AssetGuid + " in " + base.Owner.name;
		Game.Instance.EtudesSystem.SetEtudePause(base.Blueprint, isPaused: true, source);
	}

	public EtudeReferenceType GetUsagesFor(BlueprintEtude bpEtude)
	{
		if (base.Blueprint != bpEtude)
		{
			return EtudeReferenceType.None;
		}
		return EtudeReferenceType.Pause;
	}
}
