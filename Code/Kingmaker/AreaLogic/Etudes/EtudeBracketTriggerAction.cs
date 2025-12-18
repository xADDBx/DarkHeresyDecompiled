using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("fae54e860aa242b6995ca39ee03ad184")]
public class EtudeBracketTriggerAction : EtudeBracketTrigger
{
	public ActionList OnActivated;

	public ActionList OnDeactivated;

	protected override void OnExit()
	{
		OnDeactivated.Run();
	}

	protected override void OnEnter()
	{
		OnActivated.Run();
	}
}
