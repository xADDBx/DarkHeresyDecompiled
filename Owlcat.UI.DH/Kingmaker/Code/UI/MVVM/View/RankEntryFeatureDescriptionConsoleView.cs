using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankEntryFeatureDescriptionConsoleView : BaseCareerPathSelectionTabConsoleView<RankEntryFeatureItemVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private ConsoleHint m_ScrollHint;

	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	private readonly ReactiveProperty<bool> m_ScrollActive = new ReactiveProperty<bool>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly ReactiveProperty<bool> m_HasNextHint = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasScroll = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		SetHeader(UIStrings.Instance.CharacterSheet.HeaderFeatureDescriptionTab);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.ViewModel.CareerPathVM.CanCommit.Subscribe(delegate(bool canCommit)
		{
			bool flag = canCommit && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel;
			SetNextButtonLabel(flag ? UIStrings.Instance.CharacterSheet.ToSummaryTab : UIStrings.Instance.CharGen.Next);
		}).AddTo(this);
		base.ViewModel.CareerPathVM.ReadOnly.Subscribe(delegate(bool ro)
		{
			ButtonActive.Value = !ro;
		}).AddTo(this);
		IsTabActiveProp.Subscribe(delegate(bool value)
		{
			m_ScrollActive.Value = value && m_InfoView.IsScrollActive;
		}).AddTo(this);
		m_NavigationBehaviour = m_InfoView.GetNavigationBehaviour().AddTo(this);
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(UpdateFocus).AddTo(this);
	}

	public override void UpdateState()
	{
	}

	protected override void HandleClickNext()
	{
		if (base.ViewModel != null)
		{
			if (base.ViewModel.CareerPathVM.CanCommit.CurrentValue && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel)
			{
				base.ViewModel.CareerPathVM.SetRankEntry(null);
				return;
			}
			base.ViewModel.CareerPathVM.SelectNextItem();
			UISounds.Instance.Sounds.Buttons.DoctrineNextButtonClick.Play();
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.CareerPathVM.SelectPreviousItem();
	}

	public override void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		if (InputAdded)
		{
			return;
		}
		InputBindStruct inputBindStruct = inputLayer.AddAxis(delegate(InputActionEventData _, float f)
		{
			m_InfoView.Scroll(f);
		}, 3, m_HasScroll);
		m_ScrollHint.Bind(inputBindStruct).AddTo(this);
		inputBindStruct.AddTo(this);
		if ((bool)m_ConfirmHint)
		{
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				HandleClickNext();
			}, 8, m_HasNextHint.ToReadOnlyReactiveProperty());
			m_ConfirmHint.SetLabel(UIStrings.Instance.CharGen.Next);
			m_ConfirmHint.Bind(inputBindStruct2).AddTo(this);
			inputBindStruct2.AddTo(this);
		}
		InputAdded = true;
	}

	private void UpdateFocus(IConsoleEntity entity)
	{
		m_HasNextHint.Value = entity != null && (base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.CurrentValue?.CanSelect() ?? false);
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_HasScroll.Value = entity == null && m_InfoView.HasScroll;
		}, 1);
		if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			TooltipBaseTemplate entryTooltip = hasTooltipTemplate.TooltipTemplate();
			if (hasTooltipTemplate.TooltipTemplate() is TooltipTemplateGlossary tooltipTemplateGlossary)
			{
				entryTooltip = new TooltipTemplateGlossary(tooltipTemplateGlossary.GlossaryEntry);
			}
			EventBus.RaiseEvent(delegate(ISetTooltipHandler h)
			{
				h.SetTooltip(entryTooltip);
			});
		}
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		return m_NavigationBehaviour;
	}
}
