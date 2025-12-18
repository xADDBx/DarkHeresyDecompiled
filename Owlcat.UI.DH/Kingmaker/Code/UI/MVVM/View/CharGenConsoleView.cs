using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.UI.DollRoom;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenConsoleView : CharGenView
{
	[Header("Customization Values")]
	[SerializeField]
	private float m_RotateFactor = 1f;

	[SerializeField]
	private float m_ZoomFactor = 1f;

	[SerializeField]
	private float m_ZoomThresholdValue = 0.01f;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	[SerializeField]
	private ConsoleHint m_NextPhaseHint;

	[Space]
	[SerializeField]
	private ConsoleHint m_DeclineHint;

	[SerializeField]
	private ConsoleHint m_PreviousPhaseHint;

	[Space]
	[SerializeField]
	private ConsoleHint m_VisualSettingsHint;

	[SerializeField]
	private CharacterVisualSettingsConsoleView m_VisualSettingsConsoleView;

	[SerializeField]
	private ConsoleHint m_FullPortraitHint;

	[Space]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private readonly ReactiveProperty<bool> m_NextEnabled = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_CanGoNextOnConfirm = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_CanGoNextInMenu = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_CanGoBackOnDecline = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<string> m_ConfirmLabel = new ReactiveProperty<string>(string.Empty);

	private CompositeDisposable m_PhaseCanGoSubscription;

	public static bool ShowTooltip = true;

	private InputBindStruct m_VisualSettingsBind;

	private readonly ReactiveProperty<bool> m_DollZoomEnabled = new ReactiveProperty<bool>();

	private DollRoomTargetController RoomTargetController
	{
		get
		{
			if (GetActiveDollRoomType() != CharGenDollRoomType.Character)
			{
				return m_ShipController;
			}
			return m_CharacterController;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		m_VisualSettingsConsoleView.Initialize();
		m_VisualSettingsConsoleView.SetDollRoomController(m_CharacterController, m_RotateFactor, m_ZoomFactor, m_ZoomThresholdValue);
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.VisualSettingsVM.Subscribe(m_VisualSettingsConsoleView.Bind).AddTo(this);
		m_ConfirmLabel.Subscribe(delegate(string value)
		{
			m_ConfirmHint.SetLabel(value);
		}).AddTo(this);
		m_DollZoomEnabled.Where((bool v) => !v).Subscribe(delegate
		{
			RoomTargetController.ZoomMax();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_HintsWidget.Dispose();
		m_PhaseCanGoSubscription?.Dispose();
		m_PhaseCanGoSubscription = null;
	}

	protected override void CreateInputImpl(InputLayer inputLayer, ReactiveProperty<bool> isMainCharacter)
	{
		InputActionEventType eventType = (base.CurrentPhaseIsLast ? InputActionEventType.ButtonJustLongPressed : InputActionEventType.ButtonJustPressed);
		UpdateConfirmLabel();
		string label = (base.CurrentPhaseIsFirst ? UIStrings.Instance.CommonTexts.CloseWindow : UIStrings.Instance.CharGen.Back);
		if (base.CurrentPhaseIsLast)
		{
			m_NextEnabled.Value = false;
			DelayedInvoker.InvokeInTime(delegate
			{
				m_NextEnabled.Value = true;
			}, 0.5f);
		}
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			NextPressed();
		}, 15, m_NextEnabled.And(m_CanGoNextInMenu).And(isMainCharacter).ToReadOnlyReactiveProperty(initialValue: false), eventType);
		m_NextPhaseHint.Bind(inputBindStruct).AddTo(this);
		inputBindStruct.AddTo(this);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			ConfirmPressed();
		}, 8, m_CanGoNextOnConfirm.And(isMainCharacter).ToReadOnlyReactiveProperty(initialValue: false), eventType);
		m_ConfirmHint.Bind(inputBindStruct2).AddTo(this);
		inputBindStruct2.AddTo(this);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			BackPressed();
		}, 14, CanGoBack.And(isMainCharacter).ToReadOnlyReactiveProperty(initialValue: false));
		m_PreviousPhaseHint.Bind(inputBindStruct3).AddTo(this);
		inputBindStruct3.AddTo(this);
		InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
		{
			DeclinePressed();
		}, 9, m_CanGoBackOnDecline.And(isMainCharacter).ToReadOnlyReactiveProperty(initialValue: false));
		m_DeclineHint.Bind(inputBindStruct4).AddTo(this);
		inputBindStruct4.AddTo(this);
		m_DeclineHint.SetLabel(label);
		m_VisualSettingsBind = inputLayer.AddButton(delegate
		{
			base.ViewModel.SwitchVisualSettings();
		}, 16, base.ViewModel.ShouldShowVisualSettings.And(m_CanGoBackOnDecline).And(isMainCharacter).ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
		m_VisualSettingsHint.SetLabel(UIStrings.Instance.CharGen.ShowVisualSettings);
		base.ViewModel.VisualSettingsVM.Subscribe(delegate(CharacterVisualSettingsVM value)
		{
			if (value == null)
			{
				m_VisualSettingsHint.Bind(m_VisualSettingsBind);
			}
		}).AddTo(this);
		inputLayer.AddButton(delegate
		{
			CloseCharGen();
		}, 9, base.ViewModel.IsMainCharacter.Not().And(m_CanGoBackOnDecline).ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
		inputLayer.AddAxis(RotateDoll, 2).AddTo(this);
		inputLayer.AddAxis(ZoomDoll, 3, m_DollZoomEnabled).AddTo(this);
		InputBindStruct inputBindStruct5 = inputLayer.AddButton(delegate
		{
			SetFullPortraitVisible(visible: true);
		}, 17, m_CanGoBackOnDecline);
		inputBindStruct5.AddTo(this);
		inputLayer.AddButton(delegate
		{
			SetFullPortraitVisible(visible: false);
		}, 17, InputActionEventType.ButtonJustReleased).AddTo(this);
		inputLayer.AddButton(delegate
		{
			SetFullPortraitVisible(visible: false);
		}, 17, InputActionEventType.ButtonLongPressJustReleased).AddTo(this);
		m_FullPortraitHint.Bind(inputBindStruct5).AddTo(this);
	}

	protected override void RefreshInput()
	{
		try
		{
			base.RefreshInput();
			m_HintsWidget.Dispose();
			SelectedDetailView?.AddInput(ref InputLayer, ref Navigation, m_HintsWidget, base.ViewModel.IsMainCharacter);
			ReactiveProperty<bool> dollZoomEnabled = m_DollZoomEnabled;
			ICharGenPhaseDetailedView selectedDetailView = SelectedDetailView;
			dollZoomEnabled.Value = selectedDetailView != null && !selectedDetailView.HasYScrollBind;
		}
		catch (Exception)
		{
			Debug.Log("Error!");
		}
	}

	private void RotateDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			RoomTargetController.Or(null)?.Rotate((0f - x) * m_RotateFactor);
		}
	}

	private void ZoomDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			RoomTargetController.Or(null)?.Zoom(x * m_ZoomFactor);
		}
	}

	public override void CurrentPhaseChangedImpl(CharGenPhaseBaseVM viewModel)
	{
		base.CurrentPhaseChangedImpl(viewModel);
		m_PhaseCanGoSubscription?.Dispose();
		m_PhaseCanGoSubscription = new CompositeDisposable();
		if (SelectedDetailView != null)
		{
			m_PhaseCanGoSubscription.Add(SelectedDetailView.GetCanGoNextOnConfirmProperty().Subscribe(delegate(bool value)
			{
				m_CanGoNextOnConfirm.Value = value;
			}));
			m_PhaseCanGoSubscription.Add(SelectedDetailView.GetCanGoBackOnDeclineProperty().Subscribe(delegate(bool value)
			{
				m_CanGoBackOnDecline.Value = value;
			}));
			m_PhaseCanGoSubscription.Add(SelectedDetailView.CanGoNextInMenuProperty().Subscribe(delegate(bool value)
			{
				m_CanGoNextInMenu.Value = value;
			}));
		}
	}

	private void DeclinePressed()
	{
		if (SelectedDetailView == null || SelectedDetailView.PressDeclineOnPhase())
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				GoToPrevPhaseOrClose(first: false);
			}, 1);
		}
	}

	private void ConfirmPressed()
	{
		if (SelectedDetailView.PressConfirmOnPhase())
		{
			if (!CanGoNext.Value)
			{
				m_ConfirmHint.ShowTooltip(base.ViewModel.CurrentPhaseVM.CurrentValue.NotCompletedReasonTooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.None));
			}
			else if (base.ViewModel.CurrentPhaseCanInterrupt && !base.ViewModel.CurrentPhaseIsCompleted.CurrentValue)
			{
				base.ViewModel.CurrentPhaseVM.CurrentValue?.InterruptChargen(InterruptCallback);
			}
			else
			{
				GoTeNextPhaseAfterDelay();
			}
		}
	}

	private void InterruptCallback()
	{
		if (base.ViewModel.CurrentPhaseIsCompleted.CurrentValue)
		{
			ConfirmPressed();
		}
	}

	private void UpdateConfirmLabel()
	{
		if (base.CurrentPhaseIsLast)
		{
			m_ConfirmLabel.Value = UIStrings.Instance.CharGen.Complete;
			return;
		}
		CharGenPhaseBaseVM currentValue = base.ViewModel.CurrentPhaseVM.CurrentValue;
		if (currentValue != null && currentValue.CanInterruptChargen && !currentValue.IsCompletedAndAvailable.CurrentValue && !string.IsNullOrEmpty(base.ViewModel.CurrentPhaseVM.CurrentValue.OverrideConfirmHintLabel.CurrentValue))
		{
			m_PhaseCanGoSubscription.Add(base.ViewModel.CurrentPhaseVM.CurrentValue.OverrideConfirmHintLabel.Subscribe(delegate(string value)
			{
				m_ConfirmLabel.Value = value;
			}));
		}
		else
		{
			m_ConfirmLabel.Value = UIStrings.Instance.CharGen.Next;
		}
	}
}
