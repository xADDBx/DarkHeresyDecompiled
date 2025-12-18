using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Mechanics.Actions;

[Obsolete]
[TypeId("0d122ac7f4ba21c48943faac877cfa63")]
public class ContextActionStarSystemTargetScope : ContextAction
{
	public enum TargetType
	{
		ScannerUnit,
		Starship,
		Party
	}

	public new TargetType Target;

	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions with specific target type that differs from main";
	}

	protected override void RunAction()
	{
	}
}
