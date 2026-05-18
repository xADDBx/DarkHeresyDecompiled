using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.UI.Transitions;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class SaveLoadBaseView : View<SaveLoadVM>, IInitializable, ITransitable
{
	[Header("Elements")]
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private RectTransform m_WaitingForSaveListUpdatingAnimation;

	[SerializeField]
	private RectTransform m_SavesRect;

	[SerializeField]
	private TextMeshProUGUI m_YouHaveNoSavesLabel;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private TMP_Text m_SaveNameLabel;

	[Header("Screen")]
	[SerializeField]
	private UIServiceWindowPostProcessView m_PostProcessView;

	[Header("Views")]
	[SerializeField]
	private SaveLoadMenuBaseView m_Menu;

	[SerializeField]
	protected SaveSlotBaseView m_NewSaveSlotBaseView;

	[SerializeField]
	protected SaveSlotCollectionVirtualBaseView m_SlotCollectionView;

	[SerializeField]
	private SaveSlotBaseView m_DetailedSaveSlotView;

	[SerializeField]
	private SaveFullScreenshotBaseView m_FullScreenshotBaseView;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_Menu.Initialize();
			m_FadeAnimator.Initialize();
			m_FullScreenshotBaseView.Initialize();
			m_NewSaveSlotBaseView.Initialize();
			m_PostProcessView.Initialize();
			m_YouHaveNoSavesLabel.Or(null)?.transform.parent.gameObject.SetActive(value: false);
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		m_Menu.Bind(base.ViewModel.SaveLoadMenuVM);
		m_SlotCollectionView.Bind(base.ViewModel.SaveSlotCollectionVm);
		m_NewSaveSlotBaseView.Bind(base.ViewModel.NewSaveSlotVM);
		m_YouHaveNoSavesLabel.Or(null)?.SetText(UIStrings.Instance.SaveLoadTexts.EmptySaveListHint);
		m_SaveNameLabel.Or(null)?.SetText(UIStrings.Instance.SaveLoadTexts.SaveNameLabel.Text);
		base.ViewModel.SelectedSaveSlot.Subscribe(delegate(SaveSlotVM value)
		{
			m_DetailedSaveSlotView.Bind(value);
			ScrollToTop();
		}).AddTo(this);
		base.ViewModel.SaveFullScreenshot.Subscribe(m_FullScreenshotBaseView.Bind).AddTo(this);
		SaveListUpdatingAnimation(state: true);
		base.ViewModel.SaveListUpdating.Subscribe(SaveListUpdatingAnimation).AddTo(this);
		m_SlotCollectionView.Saves.ObserveCountChanged().Subscribe(delegate(int count)
		{
			(m_YouHaveNoSavesLabel.Or(null)?.transform.parent.gameObject).Or(null)?.SetActive(count <= 0 && !base.ViewModel.SaveListUpdating.CurrentValue);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(1), delegate
		{
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.SaveLoad);
			});
		}).AddTo(this);
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	protected override void OnUnbind()
	{
		SaveListUpdatingAnimation(state: false);
		Game.Instance.RequestPauseUi(isPaused: false);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.SaveLoad);
		});
	}

	Transition ITransitable.Show()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.ServiceWindowsSounds.PlayOpenSound(ServiceWindowsType.LocalMap);
		return new UIAnimatorShowTransition(m_FadeAnimator).Run(CompleteShowTransition);
	}

	Transition ITransitable.Hide()
	{
		UISounds.Instance.Sounds.ServiceWindowsSounds.PlayCloseSound(ServiceWindowsType.LocalMap);
		m_PostProcessView.Hide(immediate: false);
		return new UIAnimatorHideTransition(m_FadeAnimator).Run(CompleteHideTransition);
	}

	private void CompleteShowTransition()
	{
		m_PostProcessView.ShowFrom(UIPostEffectState.Off);
	}

	private void CompleteHideTransition()
	{
		base.gameObject.SetActive(value: false);
	}

	private void SaveListUpdatingAnimation(bool state)
	{
		m_SavesRect.Or(null)?.gameObject.SetActive(!state);
		m_WaitingForSaveListUpdatingAnimation.Or(null)?.gameObject.SetActive(state);
		m_YouHaveNoSavesLabel.Or(null)?.transform.parent.gameObject.SetActive(!m_SlotCollectionView.Saves.Any() && !state);
	}

	public void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}

	protected void SelectPrev()
	{
		base.ViewModel.SaveLoadMenuVM.SelectionGroup.SelectPrevValidEntity();
		m_SlotCollectionView.ScrollToTop();
	}

	protected void SelectNext()
	{
		base.ViewModel.SaveLoadMenuVM.SelectionGroup.SelectNextValidEntity();
		m_SlotCollectionView.ScrollToTop();
	}
}
