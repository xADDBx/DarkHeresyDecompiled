using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatStartWindowConsoleView : CombatStartWindowView
{
	[Header("Console")]
	[SerializeField]
	private HintView m_StartBattleHint;

	protected override void OnBind()
	{
		base.OnBind();
	}
}
