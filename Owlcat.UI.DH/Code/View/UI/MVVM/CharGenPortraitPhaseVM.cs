using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Selections.Portrait;
using Kingmaker.Visual.Sound;
using R3;

namespace Code.View.UI.MVVM;

public class CharGenPortraitPhaseVM : CharGenPhaseBaseVM, ICharGenPortraitSelectorHoverHandler, ISubscriber, ICharGenAppearancePageComponentHandler
{
	private readonly ReactiveProperty<PortraitVM> m_HoverPortrait = new ReactiveProperty<PortraitVM>();

	private SelectionStatePortrait m_SelectionStatePortrait;

	public CharGenPortraitsSelectorVM PortraitSelectorVM;

	public ReadOnlyReactiveProperty<PortraitVM> HoverPortrait => m_HoverPortrait;

	public override MusicStateHandler.MusicChargenState ChargenMusicState => MusicStateHandler.MusicChargenState.None;

	public CharGenPortraitPhaseVM(CharGenContext charGenContext, SelectionStatePortrait selectionStatePortrait, BlueprintPortraitSelection blueprint)
		: base(charGenContext, CharGenPhaseType.Portrait)
	{
		PortraitSelectorVM = new CharGenPortraitsSelectorVM(charGenContext, selectionStatePortrait);
		base.DisplayMode = CharGenDisplayMode.PortraitOnly;
		base.HasSmallPortrait = true;
		EventBus.Subscribe(this).AddTo(this);
		base.BlueprintSelectionWithUI = blueprint;
		SetPhaseHint(base.BlueprintSelectionWithUI?.CallToAction?.Text ?? string.Empty);
		m_SelectionStatePortrait = selectionStatePortrait;
	}

	protected override bool CheckIsCompleted()
	{
		SelectionStatePortrait selectionStatePortrait = m_SelectionStatePortrait;
		if (selectionStatePortrait != null && selectionStatePortrait.IsMade)
		{
			return selectionStatePortrait.IsValid;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
	}

	public void HandleComponentChanged(CharGenAppearancePageComponent pageComponent)
	{
		if (pageComponent == CharGenAppearancePageComponent.Portraits)
		{
			UpdateIsCompleted();
		}
	}

	public void HandleHoverStart(PortraitData portrait)
	{
		m_HoverPortrait.Value?.Dispose();
		m_HoverPortrait.Value = new PortraitVM(portrait);
	}

	public void HandleHoverStop()
	{
		m_HoverPortrait.Value?.Dispose();
		m_HoverPortrait.Value = new PortraitVM(PortraitSelectorVM.PortraitVM.CurrentValue?.PortraitData);
	}
}
