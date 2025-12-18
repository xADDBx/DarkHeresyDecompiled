using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("fa14df2d3ecc4dcbb7bf93dde87525a1")]
public class SwitchChapter : GameAction
{
	public int Chapter;

	public override string GetCaption()
	{
		return $"Switch Chapter ({Chapter})";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.ChangeChapter(Chapter);
	}
}
