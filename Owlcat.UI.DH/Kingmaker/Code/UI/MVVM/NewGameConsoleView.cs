using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGameConsoleView : NewGameBaseView
{
	[Header("Views")]
	[SerializeField]
	private NewGamePhaseStoryConsoleView m_NewGamePhaseStoryConsoleView;

	[SerializeField]
	private NewGamePhaseDifficultyConsoleView m_NewGamePhaseDifficultyConsoleView;

	[SerializeField]
	private HintView m_ConfirmHint;

	[SerializeField]
	private HintView m_DeclineHint;

	[SerializeField]
	private HintView m_PrevHint;

	[SerializeField]
	private HintView m_NextHint;

	[SerializeField]
	private HintView m_SwitchOnOffDlcHint;

	[SerializeField]
	private HintView m_PurchaseHint;

	[SerializeField]
	private HintView m_InstallDlcHint;

	[SerializeField]
	private HintView m_DeleteDlcHint;

	[SerializeField]
	private HintView m_PlayPauseVideoHint;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_NewGamePhaseDifficultyConsoleView.Initialize();
		m_Selector.Initialize();
		m_NewGamePhaseStoryConsoleView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_NewGamePhaseDifficultyConsoleView.Bind(base.ViewModel.DifficultyVM);
	}
}
