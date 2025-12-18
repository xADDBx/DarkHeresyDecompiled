using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatStartWindowConsoleView : CombatStartWindowView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_StartBattleHint;

	protected override void OnBind()
	{
		base.OnBind();
		m_StartBattleHint.Bind(SurfaceCombatInputLayer.Instance.AddButton(delegate
		{
		}, 17)).AddTo(this);
	}
}
