using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/CheckIronMan")]
[AllowMultipleComponents]
[TypeId("1295367ca8234ccab001ce33070f934b")]
public class CheckIronMan : Condition
{
	[SerializeField]
	private bool m_IsInIronMan;

	public bool IsInIronMan => m_IsInIronMan;

	protected override string GetConditionCaption()
	{
		return $"Check ironman is [{IsInIronMan}]";
	}

	protected override bool CheckCondition()
	{
		return SettingsRoot.Difficulty.OnlyOneSave.GetValue() == m_IsInIronMan;
	}
}
