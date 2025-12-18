using System;
using Kingmaker.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("b71eea4eaad04cbf8e9c93dbd38d1b07")]
public class TutorialTriggerPlayerSpaceshipShootsFromAfar : BlueprintComponent
{
	[ValidateNotEmpty]
	[SerializeField]
	private BlueprintUnitFactReference[] m_Facts;

	[SerializeField]
	private int m_CellDistanceToTarget = 2;

	[SerializeField]
	private int m_TimesCanShootFromAfar = 2;

	private int m_TimesShotFromAfar;
}
