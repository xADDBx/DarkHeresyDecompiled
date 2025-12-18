using Kingmaker.Blueprints.Root;
using Kingmaker.RuleSystem.Rules;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMessageAttackMiss : CombatMessageBase
{
	public AttackResult Result;

	public bool IsCasterCriticallyInjured;

	public Vector3 SourcePosition;

	public Vector3 TargetPosition;

	public override string GetText()
	{
		return ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.CombatTexts.GetAvoidText(Result, IsCasterCriticallyInjured);
	}
}
