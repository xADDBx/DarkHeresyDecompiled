using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CharGenPhaseBaseVM : SelectionGroupEntityVM
{
	private readonly ReactiveProperty<string> m_PhaseNextHint = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_ShowVisualSettings = new ReactiveProperty<bool>();

	protected readonly CharGenContext CharGenContext;

	public InfoSectionVM InfoVM;

	private readonly ReactiveProperty<bool> m_IsCompleted = new ReactiveProperty<bool>(value: true);

	public readonly ReadOnlyReactiveProperty<bool> IsCompletedAndAvailable;

	private bool m_DetailedViewBinded;

	private readonly ReactiveProperty<bool> m_IsInDetailedView = new ReactiveProperty<bool>(value: false);

	[CanBeNull]
	private CharGenPhaseBaseVM m_NextPhase;

	protected readonly ReactiveProperty<string> m_OverrideConfirmHintLabel = new ReactiveProperty<string>(string.Empty);

	protected readonly ReactiveProperty<string> m_PhaseName = new ReactiveProperty<string>(string.Empty);

	public readonly CharGenPhaseType PhaseType;

	public InfoSectionVM SecondaryInfoVM;

	public ReadOnlyReactiveProperty<bool> IsCompleted => m_IsCompleted;

	public ReadOnlyReactiveProperty<string> OverrideConfirmHintLabel => m_OverrideConfirmHintLabel;

	public ReadOnlyReactiveProperty<string> PhaseName => m_PhaseName;

	public int OrderPriority { get; }

	public bool HasPantograph { get; protected set; } = true;


	public bool HasPortrait { get; protected set; } = true;


	public bool CanInterruptChargen { get; protected set; }

	public CharacterDollPosition DollPosition { get; protected set; }

	public CharGenDollRoomType DollRoomType { get; protected set; }

	public int Rank { get; protected set; }

	public ReadOnlyReactiveProperty<bool> ShowVisualSettings => m_ShowVisualSettings;

	public ReadOnlyReactiveProperty<string> PhaseNextHint => m_PhaseNextHint;

	public virtual TooltipBaseTemplate NotCompletedReasonTooltip
	{
		get
		{
			if (!IsCompletedAndAvailable.CurrentValue)
			{
				return new TooltipTemplateSimple(UIStrings.Instance.CharGen.PhaseNotCompleted);
			}
			return null;
		}
	}

	public ReadOnlyReactiveProperty<bool> IsInDetailedView => m_IsInDetailedView;

	protected CharGenPhaseBaseVM(CharGenContext charGenContext, CharGenPhaseType type)
		: base(allowSwitchOff: false)
	{
		PhaseType = type;
		CharGenContext = charGenContext;
		OrderPriority = (int)type * 1000;
		m_PhaseName.Value = UIStrings.Instance.CharGen.GetPhaseName(type);
		IsCompletedAndAvailable = base.IsAvailable.And(IsCompleted).ToReadOnlyReactiveProperty(initialValue: false);
		AddDisposable(IsCompletedAndAvailable.Subscribe(delegate(bool completed)
		{
			m_NextPhase?.UpdateAvailableState(completed);
			if (completed)
			{
				m_PhaseNextHint.Value = string.Empty;
			}
		}));
	}

	protected abstract bool CheckIsCompleted();

	public void UpdateAvailableState(bool previousIsCompleted)
	{
		SetAvailableState(previousIsCompleted);
		UpdateIsCompleted();
	}

	protected void UpdateIsCompleted()
	{
		m_IsCompleted.Value = CheckIsCompleted();
	}

	public void BeginDetailedView()
	{
		m_DetailedViewBinded = true;
		m_IsInDetailedView.Value = true;
		OnBeginDetailedView();
		UpdateIsCompleted();
	}

	public void EndDetailedView()
	{
		m_DetailedViewBinded = false;
	}

	public void ResetDetailedViewState()
	{
		m_IsInDetailedView.Value = m_DetailedViewBinded;
	}

	public void SetNextPhase(CharGenPhaseBaseVM phase)
	{
		if (phase != null)
		{
			m_NextPhase = phase;
			m_NextPhase.UpdateAvailableState(IsCompleted.CurrentValue);
		}
	}

	public virtual void InterruptChargen(Action onComplete)
	{
	}

	protected void SetPhaseHint(string hint)
	{
		m_PhaseNextHint.Value = hint;
	}

	protected void SetShowVisualSettings(bool show)
	{
		m_ShowVisualSettings.Value = show;
	}

	protected abstract void OnBeginDetailedView();

	protected override void DoSelectMe()
	{
	}
}
