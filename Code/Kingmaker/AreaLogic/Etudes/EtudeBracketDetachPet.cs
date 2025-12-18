using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("802c3a06580733f46842f89747a399aa")]
public class EtudeBracketDetachPet : EtudeBracketTrigger
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Master;

	public PetType PetType;

	protected override void OnEnter()
	{
	}

	protected override void OnExit()
	{
	}
}
