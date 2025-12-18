using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerMenuSelectorBaseView : View<SelectionGroupRadioVM<DlcManagerMenuEntityVM>>
{
	[SerializeField]
	private DlcManagerMenuEntityBaseView m_DlcsButton;

	[SerializeField]
	private DlcManagerMenuEntityBaseView m_SwithcOnDlcsButton;

	[SerializeField]
	private DlcManagerMenuEntityBaseView m_ModsButton;

	[SerializeField]
	private GameObject m_Selector;

	[SerializeField]
	private float m_LensSwitchAnimationDuration = 0.55f;

	[SerializeField]
	private RectTransform m_DlcsButtonSpinArrow;

	private bool m_IsInit;

	private bool m_InGame;

	private bool m_IsConsole;

	public void Initialize(bool inGame, bool isConsole)
	{
		if (!m_IsInit)
		{
			m_InGame = inGame;
			m_IsConsole = isConsole;
			m_DlcsButton.gameObject.SetActive(!inGame);
			m_SwithcOnDlcsButton.gameObject.SetActive(inGame);
			m_DlcsButtonSpinArrow.gameObject.SetActive(!isConsole);
			m_ModsButton.gameObject.SetActive(!isConsole);
			if (!inGame)
			{
				m_DlcsButton.Initialize();
			}
			else
			{
				m_SwithcOnDlcsButton.Initialize();
			}
			if (!isConsole)
			{
				m_ModsButton.Initialize();
			}
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		if (!m_InGame)
		{
			m_DlcsButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((DlcManagerMenuEntityVM e) => e.DlcManagerTabVM is DlcManagerTabDlcsVM));
		}
		else
		{
			m_SwithcOnDlcsButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((DlcManagerMenuEntityVM e) => e.DlcManagerTabVM is DlcManagerTabSwitchOnDlcsVM));
		}
		if (!m_IsConsole)
		{
			m_ModsButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((DlcManagerMenuEntityVM e) => e.DlcManagerTabVM is DlcManagerTabModsVM));
		}
		base.ViewModel.SelectedEntity.Skip(1).Subscribe(delegate(DlcManagerMenuEntityVM selectedEntity)
		{
			DlcManagerMenuEntityBaseView dlcManagerMenuEntityBaseView = ((selectedEntity.DlcManagerTabVM is DlcManagerTabDlcsVM && !m_InGame) ? m_DlcsButton : ((selectedEntity.DlcManagerTabVM is DlcManagerTabSwitchOnDlcsVM && m_InGame) ? m_SwithcOnDlcsButton : ((!m_IsConsole) ? m_ModsButton : null)));
			if (!(dlcManagerMenuEntityBaseView == null) && m_Selector.transform.localPosition.x != dlcManagerMenuEntityBaseView.transform.localPosition.x)
			{
				UIUtilityLens.MoveXLensPosition(m_Selector.transform, dlcManagerMenuEntityBaseView.transform.localPosition.x, m_LensSwitchAnimationDuration);
			}
		}).AddTo(this);
		ResetLensPosition();
	}

	protected override void OnUnbind()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStop.Play();
	}

	private void ResetLensPosition()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			UIUtilityLens.MoveLensPosition(m_Selector.transform, (!m_InGame) ? m_DlcsButton.transform.localPosition : m_SwithcOnDlcsButton.transform.localPosition, m_LensSwitchAnimationDuration);
		}, 1);
	}

	public void OnNext()
	{
		base.ViewModel.SelectNextValidEntity();
	}

	public void OnPrev()
	{
		base.ViewModel.SelectPrevValidEntity();
	}
}
