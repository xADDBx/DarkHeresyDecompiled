using Kingmaker.UI.DollRoom;
using Owlcat.Runtime.Core.Utility;
using R3;
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
	private HintView m_ConfirmHint;

	[SerializeField]
	private HintView m_NextPhaseHint;

	[Space]
	[SerializeField]
	private HintView m_DeclineHint;

	[SerializeField]
	private HintView m_PreviousPhaseHint;

	[Space]
	[SerializeField]
	private HintView m_VisualSettingsHint;

	[SerializeField]
	private CharacterVisualSettingsConsoleView m_VisualSettingsConsoleView;

	[SerializeField]
	private HintView m_FullPortraitHint;

	[Space]
	private readonly ReactiveProperty<bool> m_NextEnabled = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_CanGoNextOnConfirm = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_CanGoNextInMenu = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<bool> m_CanGoBackOnDecline = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<string> m_ConfirmLabel = new ReactiveProperty<string>(string.Empty);

	private CompositeDisposable m_PhaseCanGoSubscription;

	public static bool ShowTooltip = true;

	private readonly ReactiveProperty<bool> m_DollZoomEnabled = new ReactiveProperty<bool>();

	private DollRoomTargetController RoomTargetController => m_CharacterController;

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
		m_DollZoomEnabled.Where((bool v) => !v).Subscribe(delegate
		{
			RoomTargetController.ZoomMax();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_PhaseCanGoSubscription?.Dispose();
		m_PhaseCanGoSubscription = null;
	}

	protected void CreateInputImpl()
	{
	}

	private void RotateDoll(float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			RoomTargetController.Or(null)?.Rotate((0f - x) * m_RotateFactor);
		}
	}

	private void ZoomDoll(float x)
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
		if (m_SelectedDetailView != null)
		{
			m_PhaseCanGoSubscription.Add(m_SelectedDetailView.GetCanGoNextOnConfirmProperty().Subscribe(delegate(bool value)
			{
				m_CanGoNextOnConfirm.Value = value;
			}));
			m_PhaseCanGoSubscription.Add(m_SelectedDetailView.GetCanGoBackOnDeclineProperty().Subscribe(delegate(bool value)
			{
				m_CanGoBackOnDecline.Value = value;
			}));
			m_PhaseCanGoSubscription.Add(m_SelectedDetailView.CanGoNextInMenuProperty().Subscribe(delegate(bool value)
			{
				m_CanGoNextInMenu.Value = value;
			}));
		}
	}
}
