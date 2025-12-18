using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("7f5d3fc38ea64094e895aba7f3a9100d")]
public class EtudeBracketMarkUnitEssential : EtudeBracketTrigger
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	protected override void OnEnter()
	{
		Target.GetValue().GetOrCreate<UnitPartEssential>().Retain();
	}

	protected override void OnExit()
	{
		Target.GetValue().GetOptional<UnitPartEssential>()?.Release();
	}
}
