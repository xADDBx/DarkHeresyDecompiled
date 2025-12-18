using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathDescriptionConsoleView : BaseCareerPathSelectionTabConsoleView<CareerPathVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_ScrollHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly ReactiveProperty<bool> m_Unfocused = new ReactiveProperty<bool>();

	public override void Initialize()
	{
		base.Initialize();
		m_InfoView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		SetHeader(null);
		m_InfoView.Bind(base.ViewModel.TabInfoSectionVM);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_NavigationBehaviour.Clear();
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		m_NavigationBehaviour.Dispose();
		m_NavigationBehaviour = m_InfoView.GetNavigationBehaviour();
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChange).AddTo(this);
		return m_NavigationBehaviour;
	}

	public override void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		if (!InputAdded)
		{
			InputBindStruct inputBindStruct = inputLayer.AddAxis(delegate(InputActionEventData _, float f)
			{
				m_InfoView.Scroll(f);
			}, 3, m_Unfocused);
			m_ScrollHint.Bind(inputBindStruct).AddTo(this);
			inputBindStruct.AddTo(this);
			inputLayer.AddButton(delegate
			{
				OnDecline();
			}, 9).AddTo(this);
			InputAdded = true;
		}
	}

	private void OnFocusChange(IConsoleEntity entity)
	{
		m_Unfocused.Value = entity == null;
		if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			TooltipBaseTemplate entryTooltip = hasTooltipTemplate.TooltipTemplate();
			if (entryTooltip is TooltipTemplateGlossary tooltipTemplateGlossary)
			{
				entryTooltip = new TooltipTemplateGlossary(tooltipTemplateGlossary.GlossaryEntry);
			}
			EventBus.RaiseEvent(delegate(ISetTooltipHandler h)
			{
				h.SetTooltip(entryTooltip);
			});
		}
	}

	private void OnDecline()
	{
		if (m_NavigationBehaviour.IsFocused)
		{
			EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
			{
				h.SetFocusOn(null);
			});
		}
	}
}
