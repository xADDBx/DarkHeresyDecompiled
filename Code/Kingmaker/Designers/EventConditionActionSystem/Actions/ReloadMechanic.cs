using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[TypeId("54433c6dbef335648b44073fc3f0f06e")]
public class ReloadMechanic : GameAction
{
	public string Desc = "Empty action";

	public bool ClearFx = true;

	protected override void RunAction()
	{
		Game.ReloadAreaMechanic(ClearFx, needNavMeshRescam: false);
	}

	public override string GetCaption()
	{
		return "Reload mechanic scenes";
	}
}
