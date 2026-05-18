using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.Visual.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CharGenPhaseBaseVM : SelectionGroupEntityVM
{
	private readonly ReactiveProperty<string> m_PhaseNextHint = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_ShowVisualSettings = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsCompleted = new ReactiveProperty<bool>(value: true);

	protected readonly ReactiveProperty<string> m_PhaseName = new ReactiveProperty<string>(string.Empty);

	protected readonly ReactiveProperty<string> m_OverrideConfirmHintLabel = new ReactiveProperty<string>(string.Empty);

	public readonly ReadOnlyReactiveProperty<bool> IsCompletedAndAvailable;

	private readonly ReactiveProperty<bool> m_IsInDetailedView = new ReactiveProperty<bool>(value: false);

	[CanBeNull]
	private CharGenPhaseBaseVM m_NextPhase;

	private bool m_DetailedViewBinded;

	protected readonly CharGenContext m_CharGenContext;

	public readonly CharGenPhaseType PhaseType;

	public InfoSectionVM InfoVM;

	public InfoSectionVM SecondaryInfoVM;

	public CharGenDisplayMode DisplayMode { get; protected set; } = CharGenDisplayMode.DollOnly;


	public bool ShowPortrait
	{
		get
		{
			CharGenDisplayMode displayMode = DisplayMode;
			return displayMode == CharGenDisplayMode.PortraitOnly || displayMode == CharGenDisplayMode.Both;
		}
	}

	public bool ShowDoll
	{
		get
		{
			CharGenDisplayMode displayMode = DisplayMode;
			return displayMode == CharGenDisplayMode.DollOnly || displayMode == CharGenDisplayMode.Both;
		}
	}

	public bool HasSmallPortrait { get; protected set; } = true;


	public bool CanInterruptChargen { get; protected set; }

	public CharacterDollPosition DollPosition { get; protected set; }

	public CharGenDollRoomType DollRoomType { get; protected set; }

	public int Rank { get; protected set; }

	public BlueprintSelectionWithUI BlueprintSelectionWithUI { get; protected set; }

	public virtual MusicStateHandler.MusicChargenState ChargenMusicState => MusicStateHandler.MusicChargenState.Declamation;

	public ReadOnlyReactiveProperty<bool> IsCompleted => m_IsCompleted;

	public ReadOnlyReactiveProperty<string> OverrideConfirmHintLabel => m_OverrideConfirmHintLabel;

	public ReadOnlyReactiveProperty<string> PhaseName => m_PhaseName;

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

	protected CharGenPhaseBaseVM(CharGenContext charGenContext, CharGenPhaseType type, bool allowSwitchOff = false)
		: base(allowSwitchOff)
	{
		PhaseType = type;
		m_CharGenContext = charGenContext;
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
		OnEndDetailedView();
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

	protected virtual void OnEndDetailedView()
	{
	}

	protected override void DoSelectMe()
	{
	}
}
