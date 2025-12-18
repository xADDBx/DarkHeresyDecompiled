using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Code.View.UI.Components.Camera;
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
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected InputLayer m_InputLayer;

	public const string InputLayerContextName = "SaveLoad";

	[FormerlySerializedAs("UIPostProcessMember")]
	[Header("Screen")]
	[SerializeField]
	private UIPostProcessMember m_UIPostProcessMember;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_Menu.Initialize();
			m_FadeAnimator.Initialize();
			m_FullScreenshotBaseView.Initialize();
			m_NewSaveSlotBaseView.Initialize();
			m_YouHaveNoSavesLabel.Or(null)?.transform.parent.gameObject.SetActive(value: false);
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		m_UIPostProcessMember.Bind();
		m_Menu.Bind(base.ViewModel.SaveLoadMenuVM);
		m_SlotCollectionView.Bind(base.ViewModel.SaveSlotCollectionVm);
		m_NewSaveSlotBaseView.Bind(base.ViewModel.NewSaveSlotVM);
		m_YouHaveNoSavesLabel.text = UIStrings.Instance.SaveLoadTexts.EmptySaveListHint;
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
		CreateInput();
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
		UISounds.Instance.Sounds.LocalMap.PlayOpen();
		return new UIAnimatorShowTransition(m_FadeAnimator).Run(CompleteShowTransition);
	}

	Transition ITransitable.Hide()
	{
		UISounds.Instance.Sounds.LocalMap.PlayClose();
		UIPostProcessingAnimator.Instance.Or(null)?.PlayState(UIPostEffectState.Off);
		return new UIAnimatorHideTransition(m_FadeAnimator).Run(CompleteHideTransition);
	}

	private void CompleteShowTransition()
	{
		UIPostProcessingAnimator.Instance.Or(null)?.PlayState(UIPostEffectState.Default);
	}

	private void CompleteHideTransition()
	{
		base.gameObject.SetActive(value: false);
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_UIPostProcessMember?.Dispose();
		}).AddTo(this);
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_NavigationBehaviour.AddEntityVertical(m_NewSaveSlotBaseView);
		m_NavigationBehaviour.AddEntityVertical(m_SlotCollectionView.NavigationBehaviour);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "SaveLoad"
		}, null, leftStick: true, rightStick: false, null, new List<NavigationInputEventTypeConfig>
		{
			new NavigationInputEventTypeConfig
			{
				Action = 10,
				InputActionEventType = InputActionEventType.ButtonJustReleased
			}
		});
		m_SlotCollectionView.AttachedFirstValidView.Subscribe(FocusOnFirstValidSaveSlot).AddTo(this);
		base.ViewModel.Mode.Subscribe(delegate
		{
			FocusOnFirstValidSaveSlot();
		}).AddTo(this);
		base.ViewModel.SaveListUpdating.Subscribe(delegate(bool value)
		{
			if (!value)
			{
				if (!m_NavigationBehaviour.Entities.Any())
				{
					m_NavigationBehaviour.AddEntityVertical(m_NewSaveSlotBaseView);
					m_NavigationBehaviour.AddEntityVertical(m_SlotCollectionView.NavigationBehaviour);
				}
				FocusOnFirstValidSaveSlot();
			}
			else
			{
				m_SlotCollectionView.Or(null)?.NavigationBehaviour?.UnFocusCurrentEntity();
				m_NavigationBehaviour?.UnFocusCurrentEntity();
				m_NavigationBehaviour?.Clear();
			}
		}).AddTo(this);
		CreateInputImpl(m_InputLayer);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
	}

	private void FocusOnFirstValidSaveSlot()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(1), delegate
		{
			if (base.ViewModel.Mode.CurrentValue == SaveLoadMode.Save)
			{
				m_NavigationBehaviour.FocusOnEntityManual(m_NewSaveSlotBaseView);
				m_SlotCollectionView.NavigationBehaviour.ResetCurrentEntity();
			}
			else
			{
				foreach (IConsoleEntity entity in m_SlotCollectionView.NavigationBehaviour.Entities)
				{
					if (entity is VirtualListElement { View: SaveSlotBaseView view } && view.IsValid())
					{
						m_SlotCollectionView.NavigationBehaviour.FocusOnEntityManual(entity);
						m_NavigationBehaviour.FocusOnEntityManual(m_SlotCollectionView.NavigationBehaviour);
						return;
					}
				}
				m_SlotCollectionView.NavigationBehaviour.FocusOnFirstValidEntity();
				m_NavigationBehaviour.FocusOnEntityManual(m_SlotCollectionView.NavigationBehaviour);
			}
		}).AddTo(this);
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
