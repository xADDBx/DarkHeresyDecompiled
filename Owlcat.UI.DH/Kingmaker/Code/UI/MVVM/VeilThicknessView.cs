using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.Common.SmartSliders;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class VeilThicknessView : View<VeilThicknessVM>
{
	[SerializeField]
	protected Image m_TooltipArea;

	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private DelayedSlider m_ValueSlider;

	[SerializeField]
	private Slider m_PredictedValueSlider;

	[SerializeField]
	private Slider m_PhenomenaChanceSlider;

	[SerializeField]
	private OwlcatMultiSelectable[] m_Effects;

	[SerializeField]
	private TextMeshProUGUI m_CurrentValue;

	[SerializeField]
	private TextMeshProUGUI m_PredictValue;

	[SerializeField]
	private FadeAnimator m_PredictAnimator;

	[Header("VeilBuffs")]
	[SerializeField]
	private WidgetList m_BuffsWidgetList;

	[SerializeField]
	private VeilBuffView m_VeilBuffView;

	public void Initialize()
	{
		m_ValueSlider.Initialize();
		m_Animator.Initialize();
		m_Animator.DisappearAnimation();
	}

	protected override void OnBind()
	{
		EventBus.Subscribe(this).AddTo(this);
		int maxVeilDamage = ConfigRoot.Instance.PsykerRoot.MaxVeilDamage;
		m_ValueSlider.SetMaxValue(maxVeilDamage);
		m_PredictedValueSlider.maxValue = maxVeilDamage;
		m_PhenomenaChanceSlider.maxValue = 100f;
		m_PhenomenaChanceSlider.value = 100f - (float)base.ViewModel.Veil.PerilsOfTheWarpChance;
		base.ViewModel.IsTurnBasedActive.CombineLatest(base.ViewModel.IsPlayerTurn, base.ViewModel.IsAppropriateGameMode, (bool isTurnBasedActive, bool isPlayerTurn, bool isAppropriateGameMode) => new { isTurnBasedActive, isPlayerTurn, isAppropriateGameMode }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(value =>
		{
			m_Animator.PlayAnimation(value.isAppropriateGameMode && value.isTurnBasedActive && value.isPlayerTurn);
		})
			.AddTo(this);
		base.ViewModel.Value.Subscribe(delegate(int value)
		{
			m_ValueSlider.SetValue(value);
			m_PhenomenaChanceSlider.value = 100f - (float)base.ViewModel.Veil.PerilsOfTheWarpChance;
			m_CurrentValue.text = value.ToString();
		}).AddTo(this);
		base.ViewModel.PredictedValue.Subscribe(delegate(int value)
		{
			m_PredictedValueSlider.value = value;
		}).AddTo(this);
		base.ViewModel.PredictedDeltaValue.Subscribe(delegate(int value)
		{
			if (value != 0)
			{
				m_PredictValue.text = UIUtilityText.AddSign(value);
				m_PredictAnimator.AppearAnimation();
			}
			else
			{
				m_PredictAnimator.DisappearAnimation();
			}
		}).AddTo(this);
		base.ViewModel.VeilBuffVMs.ObserveCountChanged().Subscribe(delegate
		{
			m_BuffsWidgetList.DrawEntries(base.ViewModel.VeilBuffVMs, m_VeilBuffView);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Animator.DisappearAnimation();
		m_BuffsWidgetList.Clear();
	}
}
