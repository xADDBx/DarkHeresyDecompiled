using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.UI.DollRoom;
using Kingmaker.Code.View.Bridge.Enums;
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
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenView : View<CharGenVM>, ICharGenChangePhaseHandler, ISubscriber
{
	private const string InputLayerContextName = "Chargen";

	[SerializeField]
	protected CanvasGroup m_CanvasGroup;

	[FormerlySerializedAs("RoadmapMenuView")]
	[SerializeField]
	protected CharGenRoadmapMenuView m_RoadmapMenuView;

	[FormerlySerializedAs("DetailedViewsFactory")]
	[SerializeField]
	protected CharGenPhaseDetailedViewsFactory m_DetailedViewsFactory;

	[Header("Portrait")]
	[SerializeField]
	private CharGenPortraitView m_PortraitHalfView;

	[SerializeField]
	private CharGenPortraitView m_PortraitFullView;

	[SerializeField]
	private CharGenPortraitView m_LevelUpPortraitFullView;

	[SerializeField]
	private CharInfoExperiencePCView m_ExperiencePCView;

	[Header("Doll")]
	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	[SerializeField]
	private FadeAnimator m_CharacterDollTexture;

	[SerializeField]
	private RectTransform m_DollTransform;

	[SerializeField]
	private DollPosition[] m_DollPositions;

	[Header("Ruler")]
	[SerializeField]
	private InventoryRuler m_Ruler;

	[SerializeField]
	private OwlcatButton m_ResetRulerTargetButton;

	[SerializeField]
	private PaperHints m_PaperHints;

	[SerializeField]
	private InfoSectionView m_LevelUpInfoView;

	[SerializeField]
	private InfoSectionView m_ChargenInfoView;

	[SerializeField]
	private ChargenProgressionView m_ProgressionView;

	[SerializeField]
	private OwlcatMultiSelectable m_LevelupSelectable;

	[Header("Custom Phase Info Views")]
	[SerializeField]
	private PartyStatsOverviewView m_PartyStatsOverviewView;

	protected ICharGenPhaseDetailedView m_SelectedDetailView;

	protected readonly ReactiveProperty<bool> m_CanGoBack = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> m_CanGoNext = new ReactiveProperty<bool>();

	private CharGenDollRoom CharacterRoom => UIDollRooms.Instance.Or(null)?.CharGenDollRoom;

	protected bool CurrentPhaseIsFirst => base.ViewModel.CurrentPhaseVM.CurrentValue == base.ViewModel.PhasesCollection.First();

	protected bool CurrentPhaseIsLast => base.ViewModel?.CurrentPhaseVM?.CurrentValue == base.ViewModel?.PhasesCollection?.Last();

	public void Awake()
	{
		Initialize();
	}

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_DetailedViewsFactory.Initialize();
		m_DetailedViewsFactory.SetPaperHints(m_PaperHints);
		m_PortraitHalfView.Initialize(HandleSmallPortraitHover);
		m_CharacterDollTexture.Initialize();
		m_ExperiencePCView.Initialize();
	}

	protected override void OnBind()
	{
		PFLog.UI.Log($"[{Time.frameCount}] | {Time.realtimeSinceStartup} Start creating CharGenView");
		base.OnBind();
		base.gameObject.SetActive(value: true);
		CharacterRoom.Initialize(m_CharacterController);
		bool num = base.ViewModel.CharGenContext.CharGenConfig.Mode == CharGenMode.LevelUp;
		m_RoadmapMenuView.Bind(base.ViewModel.PhasesSelectionGroupRadioVM);
		base.ViewModel.CurrentPhaseVM.Subscribe(CurrentPhaseChanged).AddTo(this);
		base.ViewModel.PortraitVM.Subscribe(delegate(PortraitVM vm)
		{
			m_PortraitFullView.Bind(vm);
			m_LevelUpPortraitFullView.Bind(vm);
			m_PortraitHalfView.Bind(vm);
		}).AddTo(this);
		m_PortraitFullView.SetVisibility(base.ViewModel.CurrentPhaseVM.CurrentValue.ShowPortrait);
		base.ViewModel.PartyStatsOverviewVM.IsActive.Subscribe(delegate(bool active)
		{
			m_LevelUpPortraitFullView.SetVisibility(!active);
		}).AddTo(this);
		if (num)
		{
			m_ExperiencePCView.Bind(base.ViewModel.Experience);
		}
		m_LevelUpInfoView.Bind(base.ViewModel.LevelUpInfoSectionVM);
		m_LevelUpInfoView.SetActive(state: true);
		m_ChargenInfoView.Bind(base.ViewModel.ChargenInfoSectionVM);
		m_ChargenInfoView.SetActive(state: true);
		if (m_PartyStatsOverviewView != null)
		{
			m_PartyStatsOverviewView.Bind(base.ViewModel.PartyStatsOverviewVM);
		}
		m_ProgressionView.Bind(base.ViewModel.ProgressionVM);
		base.ViewModel.CharGenContext.CurrentUnit.Subscribe(delegate
		{
			CharGenPhaseBaseVM currentValue = base.ViewModel.CurrentPhaseVM.CurrentValue;
			if (currentValue != null && currentValue.ShowDoll)
			{
				SetDoll();
			}
		}).AddTo(this);
		m_Ruler.SetHint(UIStrings.Instance.InventoryScreen.RulerHint);
		m_CharacterController.CurrentZoomNormalized.Subscribe(m_Ruler.SetZoom).AddTo(this);
		m_CharacterController.CurrentRotationAngle.Subscribe(m_Ruler.SetRotation).AddTo(this);
		m_CharacterController.IsHoveredOver.Subscribe(m_Ruler.SetHighlight).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ResetRulerTargetButton.OnLeftClickAsObservable(), delegate
		{
			m_CharacterController.ResetTargetView();
			ServiceWindowsSounds.Instance.Inventory.ResetDollPosition.Play();
		}).AddTo(this);
		base.ViewModel.CurrentPhaseIsCompleted.Subscribe(delegate(bool isCompleted)
		{
			m_CanGoNext.Value = isCompleted || base.ViewModel.CurrentPhaseCanInterrupt;
		}).AddTo(this);
		base.ViewModel.CharGenContext.IsCustomCharacter.Subscribe(m_RoadmapMenuView.SetBackgroundFrameState).AddTo(this);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Chargen);
		});
		Game.Instance.RequestPauseUi(isPaused: true);
		EventBus.Subscribe(this).AddTo(this);
		CreateNavigation();
		base.ViewModel.CheckCoopControls.Subscribe(delegate
		{
			m_DetailedViewsFactory.SetPaperHints(m_PaperHints);
		}).AddTo(this);
		base.ViewModel.IsInCharscreen.Subscribe(HandleCharscreenChanged).AddTo(this);
		PFLog.UI.Log($"[{Time.frameCount}] | {Time.realtimeSinceStartup} Finished creating CharGenView");
	}

	protected override void OnUnbind()
	{
		m_RoadmapMenuView.KillSelectorTween();
		m_RoadmapMenuView.ShutUpSelector();
		m_LevelUpInfoView.Unbind();
		m_ChargenInfoView.Unbind();
		if (m_PartyStatsOverviewView != null)
		{
			m_PartyStatsOverviewView.Unbind();
		}
		UISounds.Instance.Play(SystemSounds.Instance.Selector.LoopStop, isButton: false, playAnyway: true);
		HideRooms();
		base.gameObject.SetActive(value: false);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Chargen);
		});
		Game.Instance.RequestPauseUi(isPaused: false);
		base.OnUnbind();
	}

	private void HandleSmallPortraitHover(bool hovered)
	{
		SetFullPortraitVisible(hovered || base.ViewModel.CurrentPhaseVM.CurrentValue.ShowPortrait);
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
			FullScreenSounds.Instance.Chargen.ChargenTabSwitch.Play();
			base.ViewModel.LastPhase?.EndDetailedView();
			m_SelectedDetailView?.Unbind();
			base.ViewModel.SetLastPhase(viewModel);
			m_PortraitFullView.SetVisibility(viewModel.ShowPortrait);
			m_CharacterDollTexture.PlayAnimation(viewModel.ShowDoll);
			UpdateDollRooms(viewModel);
			viewModel.BeginDetailedView();
			m_SelectedDetailView = m_DetailedViewsFactory.GetDetailedPhaseView(viewModel);
			m_CanGoBack.Value = true;
			string activeLayer = (viewModel.HasSmallPortrait ? "Default" : "Levelup");
			m_LevelupSelectable.SetActiveLayer(activeLayer);
			TooltipHelper.HideInfo();
			base.ViewModel.HideVisualSettings();
			m_ChargenInfoView?.ResetPosition();
			m_LevelUpInfoView?.ResetPosition();
		}
	}

	private void CreateNavigation()
	{
	}

	private void HideRooms()
	{
		CharacterRoom.Or(null)?.Hide();
	}

	private void UpdateDollRooms(CharGenPhaseBaseVM viewModel)
	{
		if (viewModel != null)
		{
			bool showDoll = viewModel.ShowDoll;
			if (showDoll)
			{
				SetDoll();
			}
			CharacterRoom.SetVisibility(showDoll);
			m_CharacterDollTexture.PlayAnimation(showDoll);
			DollPosition dollPosition = m_DollPositions.First((DollPosition i) => i.Position == viewModel.DollPosition);
			m_DollTransform.transform.position = dollPosition.Transform.position;
		}
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
				Metrics.Chargen.Close(base.ViewModel.CurrentPhaseVM.CurrentValue.PhaseType.ToString()).Send();
				base.ViewModel.Close();
			}
		});
	}

	protected virtual void NextPressed()
	{
		if (m_CanGoNext.Value)
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
		if (CurrentPhaseIsFirst)
		{
			CloseCharGen();
			return;
		}
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
		_ = m_CanGoNext.Value;
		DelayedInvoker.InvokeInFrames(delegate
		{
			GoToNextPhaseOrComplete(lastValid: false);
		}, 1);
	}

	protected void GoToNextPhaseOrComplete(bool lastValid)
	{
		if (CurrentPhaseIsLast)
		{
			m_RoadmapMenuView.ShutUpSelector();
			base.ViewModel.Complete();
		}
		else if (lastValid)
		{
			m_RoadmapMenuView.SelectLastValidPhase();
		}
		else
		{
			m_RoadmapMenuView.SelectNextPhase();
		}
	}

	protected void GoToPrevPhaseOrClose(bool first)
	{
		if (first)
		{
			m_RoadmapMenuView.SelectFirstValidPhase();
		}
		else
		{
			m_RoadmapMenuView.SelectPrevPhase();
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
