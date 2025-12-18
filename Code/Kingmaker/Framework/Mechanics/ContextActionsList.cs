using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.Mechanics;

[Serializable]
[TypeId("18d023e160d0436ba5ecfb67eca00f40")]
public sealed class ContextActionsList : BlueprintScriptableObject
{
	public ActionList Actions = new ActionList();

	public override bool AllowContextActionsOnly => true;

	public bool HasActions => Actions.HasActions;

	public void Run()
	{
		Actions.Run();
	}
}
