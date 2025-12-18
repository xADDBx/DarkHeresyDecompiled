using Kingmaker.Blueprints.Root.Strings;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipUnitConsoleView : OvertipUnitView
{
	[Header("Console")]
	[SerializeField]
	private OvertipConsoleView m_OvertipConsoleView;

	protected override void OnBind()
	{
		base.OnBind();
		m_OvertipConsoleView.SetConfirmHint(base.ViewModel.MechanicEntityUIState.NeedConsoleHint, UIStrings.Instance.ActionTexts.Talk);
		base.ViewModel.HasSurrounding.CombineLatest(base.ViewModel.IsChosen, (bool hasSur, bool chosen) => new { hasSur, chosen }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			if (!Game.Instance.Player.IsInCombat)
			{
				m_OvertipConsoleView.SetPaginator(value.hasSur, value.chosen);
			}
		})
			.AddTo(this);
		m_OvertipConsoleView.AddTo(this);
	}
}
