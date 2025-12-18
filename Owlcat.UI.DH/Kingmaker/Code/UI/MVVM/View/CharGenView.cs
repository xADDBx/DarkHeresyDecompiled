using System.Linq;
using Assets.Code.View.UI.MVVM;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.DollRoom;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenView : View<CharGenVM>, ICharGenChangePhaseHandler, ISubscriber, IInitializable
{
	public static readonly string InputLayerContextName = "Chargen";

	[SerializeField]
	protected CanvasGroup m_CanvasGroup;

	[SerializeField]
	protected CharGenRoadmapMenuView RoadmapMenuView;

	[SerializeField]
	protected CharGenPhaseDetailedViewsFactory DetailedViewsFactory;

	[Header("Portrait")]
	[SerializeField]
	private CharGenPortraitView m_PortraitFullView;

	[SerializeField]
	private RectTransform m_PantographPosition;

	[Header("Doll")]
	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	[SerializeField]
	private FadeAnimator m_CharacterDollTexture;

	[SerializeField]
	protected DollRoomTargetController m_ShipController;

	[SerializeField]
	private FadeAnimator m_ShipDollTexture;

	[SerializeField]
	private RectTransform m_DollTransform;

	[SerializeField]
	private DollPosition[] m_DollPositions;

	[SerializeField]
	private PaperHints m_PaperHints;

	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private ChargenProgressionView m_ProgressionView;

	protected InputLayer InputLayer;

	protected GridConsoleNavigationBehaviour Navigation;

	private DollRoomBase m_ActiveRoom;

	protected ICharGenPhaseDetailedView SelectedDetailView;

	protected readonly ReactiveProperty<bool> CanGoBack = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> CanGoNext = new ReactiveProperty<bool>();

	private bool m_DeviceBackWasOpenedBefore;

	private CharGenDollRoom CharacterRoom => UIDollRooms.Instance.Or(null)?.CharGenDollRoom;

	protected bool CurrentPhaseIsFirst => base.ViewModel.CurrentPhaseVM.CurrentValue == base.ViewModel.PhasesCollection.First();

	protected bool CurrentPhaseIsLast => base.ViewModel?.CurrentPhaseVM?.CurrentValue == base.ViewModel?.PhasesCollection?.Last();

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		RoadmapMenuView.Initialize();
		m_CharacterDollTexture.Initialize();
		m_ShipDollTexture.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.gameObject.SetActive(value: true);
		RoadmapMenuView.Bind(base.ViewModel.PhasesSelectionGroupRadioVM);
		base.ViewModel.CurrentPhaseVM.Subscribe(CurrentPhaseChanged).AddTo(this);
		base.ViewModel.PortraitVM.Subscribe(m_PortraitFullView.Bind).AddTo(this);
		m_InfoView.Bind(base.ViewModel.InfoSectionVM);
		m_InfoView.SetActive(state: true);
		m_ProgressionView.Bind(base.ViewModel.ProgressionVM);
		base.ViewModel.CharGenContext.CurrentUnit.Subscribe(delegate
		{
			SetDoll();
		}).AddTo(this);
		base.ViewModel.CurrentPhaseIsCompleted.Subscribe(delegate(bool isCompleted)
		{
			CanGoNext.Value = isCompleted || base.ViewModel.CurrentPhaseCanInterrupt;
		}).AddTo(this);
		base.ViewModel.CharGenContext.IsCustomCharacter.Subscribe(RoadmapMenuView.SetBackgroundFrameState).AddTo(this);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Chargen);
		});
		Game.Instance.RequestPauseUi(isPaused: true);
		EventBus.Subscribe(this).AddTo(this);
		CreateNavigation();
		base.ViewModel.CheckCoopControls.Subscribe(delegate
		{
			DetailedViewsFactory.SetPaperHints(m_PaperHints);
		}).AddTo(this);
		base.ViewModel.IsInCharscreen.Subscribe(HandleCharscreenChanged).AddTo(this);
	}

	protected override void OnUnbind()
	{
		RoadmapMenuView.KillSelectorTween();
		RoadmapMenuView.ShutUpSelector();
		m_InfoView.Unbind();
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
		m_DeviceBackWasOpenedBefore = false;
		base.gameObject.SetActive(value: false);
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
		HideRooms();
		DestroyInputLayer();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Chargen);
		});
		Game.Instance.RequestPauseUi(isPaused: false);
		base.OnUnbind();
	}

	private void HandleSmallPortraitHover(bool hovered)
	{
		SetFullPortraitVisible(hovered);
	}

	protected void SetFullPortraitVisible(bool visible)
	{
		if (visible)
		{
			m_PortraitFullView.Bind(base.ViewModel.PortraitVM.CurrentValue);
		}
		else
		{
			m_PortraitFullView.Unbind();
		}
	}

	public void CurrentPhaseChanged(CharGenPhaseBaseVM viewModel)
	{
		if (viewModel != null)
		{
			CurrentPhaseChangedImpl(viewModel);
		}
	}

	void ICharGenChangePhaseHandler.HandlePhaseChange(CharGenPhaseType phaseType)
	{
		CharGenPhaseBaseVM charGenPhaseBaseVM = base.ViewModel.PhasesCollection.FirstOrDefault((CharGenPhaseBaseVM phase) => phase.PhaseType == phaseType);
		if (!UtilityNet.IsControlMainCharacter())
		{
			base.ViewModel.SetCurrentPhase(charGenPhaseBaseVM);
		}
		CurrentPhaseChangedImpl(charGenPhaseBaseVM);
	}

	public virtual void CurrentPhaseChangedImpl(CharGenPhaseBaseVM viewModel)
	{
		if (viewModel != null)
		{
			if (!m_DeviceBackWasOpenedBefore)
			{
				UISounds.Instance.Sounds.Inventory.PlayOpen();
				m_DeviceBackWasOpenedBefore = true;
			}
			else
			{
				UISounds.Instance.Sounds.Inventory.PlayClose();
				m_DeviceBackWasOpenedBefore = false;
			}
			base.ViewModel.LastPhase?.EndDetailedView();
			SelectedDetailView?.Unbind();
			base.ViewModel.SetLastPhase(viewModel);
			viewModel.BeginDetailedView();
			SelectedDetailView = DetailedViewsFactory.GetDetailedPhaseView(viewModel);
			CanGoBack.Value = !CurrentPhaseIsFirst;
			DelayedUpdate();
			TooltipHelper.HideInfo();
			base.ViewModel.HideVisualSettings();
		}
	}

	private void DelayedUpdate()
	{
		DelayedInvoker.InvokeInFrames(RefreshInput, 3);
	}

	private void CreateNavigation()
	{
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = Navigation.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerContextName
		});
		if (base.ViewModel != null)
		{
			base.ViewModel.SetInputLayer(inputLayer, CreateInputImpl);
		}
		else
		{
			CreateInputImpl(inputLayer, new ReactiveProperty<bool>(UtilityNet.IsControlMainCharacter()));
		}
		return inputLayer;
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer, ReactiveProperty<bool> isMainCharacter)
	{
	}

	private void CreateInputLayer()
	{
		InputLayer = GetInputLayer();
		GamePad.Instance.PushLayer(InputLayer).AddTo(this);
	}

	private void DestroyInputLayer()
	{
		if (InputLayer != null)
		{
			GamePad.Instance.PopLayer(InputLayer);
			InputLayer = null;
		}
	}

	protected virtual void RefreshInput()
	{
	}

	private void HideRooms()
	{
		m_ActiveRoom.Or(null)?.Hide();
	}

	private void UpdateDollRooms(CharGenPhaseBaseVM viewModel)
	{
		if (viewModel != null)
		{
			bool flag = viewModel.DollRoomType == CharGenDollRoomType.Character;
			if (flag)
			{
				SetDoll();
			}
			if (GetActiveDollRoomType() != viewModel.DollRoomType)
			{
				m_ActiveRoom.Or(null)?.Hide();
				CharacterRoom.SetVisibility(flag);
				m_ActiveRoom = CharacterRoom;
			}
			m_CharacterDollTexture.PlayAnimation(flag);
			m_ShipDollTexture.PlayAnimation(!flag);
			DollPosition dollPosition = m_DollPositions.First((DollPosition i) => i.Position == viewModel.DollPosition);
			m_DollTransform.transform.position = dollPosition.Transform.position;
		}
	}

	protected CharGenDollRoomType? GetActiveDollRoomType()
	{
		if (CharacterRoom.IsVisible)
		{
			return CharGenDollRoomType.Character;
		}
		return null;
	}

	private void SetDoll()
	{
		CharacterRoom.BindDollState(base.ViewModel.CharGenContext.Doll);
	}

	protected virtual void CloseCharGen()
	{
		UtilityMessageBox.ShowMessageBox(UtilityNet.IsControlMainCharacter() ? UIStrings.Instance.CharGen.SureWantClose : UIStrings.Instance.CharGen.CloseCoopChargenNotRt, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
		{
			if (base.ViewModel != null && button == DialogMessageBoxButton.Yes)
			{
				base.ViewModel.Close();
			}
		});
	}

	protected virtual void NextPressed()
	{
		if (CanGoNext.Value)
		{
			if (base.ViewModel.CurrentPhaseCanInterrupt && !base.ViewModel.CurrentPhaseIsCompleted.CurrentValue)
			{
				base.ViewModel.CurrentPhaseVM.CurrentValue?.InterruptChargen(InterruptCallback);
				return;
			}
			TooltipHelper.HideInfo();
			GoTeNextPhaseAfterDelay();
		}
	}

	private void InterruptCallback()
	{
		if (base.ViewModel.CurrentPhaseIsCompleted.CurrentValue)
		{
			NextPressed();
		}
	}

	protected virtual void BackPressed()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			GoToPrevPhaseOrClose(first: false);
		}, 1);
	}

	protected virtual void FirstPressed()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			GoToPrevPhaseOrClose(first: true);
		}, 1);
	}

	protected virtual void LastPressed()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			GoToNextPhaseOrComplete(lastValid: true);
		}, 1);
	}

	protected void GoTeNextPhaseAfterDelay()
	{
		_ = CanGoNext.Value;
		DelayedInvoker.InvokeInFrames(delegate
		{
			GoToNextPhaseOrComplete(lastValid: false);
		}, 1);
	}

	protected void GoToNextPhaseOrComplete(bool lastValid)
	{
		if (CurrentPhaseIsLast)
		{
			RoadmapMenuView.ShutUpSelector();
			base.ViewModel.Complete();
		}
		else if (lastValid)
		{
			RoadmapMenuView.SelectLastValidPhase();
		}
		else
		{
			RoadmapMenuView.SelectNextPhase();
		}
	}

	protected void GoToPrevPhaseOrClose(bool first)
	{
		if (first)
		{
			RoadmapMenuView.SelectFirstValidPhase();
		}
		else
		{
			RoadmapMenuView.SelectPrevPhase();
		}
	}

	private void HandleCharscreenChanged(bool windowShowed)
	{
		m_CanvasGroup.alpha = (windowShowed ? 0f : 1f);
		m_CanvasGroup.interactable = !windowShowed;
		m_CanvasGroup.blocksRaycasts = !windowShowed;
		if (!windowShowed)
		{
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Chargen);
			});
		}
	}
}
