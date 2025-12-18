using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class UnitProgressionConsoleView : UnitProgressionCommonView, ICharInfoComponentConsoleView, ICharInfoComponentView, IConsoleNavigationOwner, IConsoleEntity, ICharInfoCanHookDecline, ICharInfoCanHookConfirm
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private ConsoleHintsWidget m_HintsWidget;

	private Coroutine m_AddInputCo;

	private bool m_InputAdded;

	private readonly ReactiveProperty<bool> m_CanHookDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanFuncAdditional = new ReactiveProperty<bool>();

	private CareerPathsListsConsoleView CareerPathsListsConsoleView => m_CareerPathsListsCommonView as CareerPathsListsConsoleView;

	private CareerPathProgressionConsoleView CareerPathProgressionConsoleView => m_CareerPathProgressionCommonView as CareerPathProgressionConsoleView;

	public GridConsoleNavigationBehaviour NavigationBehaviour => m_NavigationBehaviour;

	protected override void OnBind()
	{
		base.OnBind();
		CreateNavigation();
	}

	protected override void OnUnbind()
	{
		if (m_AddInputCo != null)
		{
			StopCoroutine(m_AddInputCo);
		}
		base.OnUnbind();
		m_NavigationBehaviour?.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
		m_AddInputCo = null;
		m_InputAdded = false;
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		m_InputLayer = inputLayer;
		m_HintsWidget = hintsWidget;
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
		if (base.ViewModel.CurrentCareer.CurrentValue != null)
		{
			CareerPathProgressionConsoleView.AddInput(ref inputLayer, ref hintsWidget);
		}
		else
		{
			CareerPathsListsConsoleView.AddInput(ref inputLayer, ref hintsWidget);
		}
		if (!m_InputAdded)
		{
			InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
			{
				OnFuncAdditionalClick();
			}, 17, m_CanFuncAdditional, InputActionEventType.ButtonJustReleased);
			hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CharacterSheet.ToggleFavorites).AddTo(this);
			inputBindStruct.AddTo(this);
			m_InputAdded = true;
		}
	}

	protected override void HandleState(UnitProgressionWindowState state)
	{
		base.HandleState(state);
		TooltipHelper.HideTooltip();
		UpdateNavigation();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		UpdateNavigation();
	}

	protected override void BindPathProgression(CareerPathVM careerPathVM)
	{
		base.BindPathProgression(careerPathVM);
		if (careerPathVM != null)
		{
			m_AddInputCo = StartCoroutine(AddCareerPathWhenHasInput());
		}
		m_CanHookDecline.Value = careerPathVM != null;
	}

	private IEnumerator AddCareerPathWhenHasInput()
	{
		while (m_InputLayer == null)
		{
			yield return null;
		}
		if (CareerPathProgressionConsoleView.ViewModel != null)
		{
			CareerPathProgressionConsoleView.AddInput(ref m_InputLayer, ref m_HintsWidget);
		}
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged).AddTo(this);
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		if (m_NavigationBehaviour != null)
		{
			bool isFocused = m_NavigationBehaviour.IsFocused;
			m_NavigationBehaviour.Clear();
			ConsoleNavigationBehaviour entity = base.ViewModel.State.CurrentValue switch
			{
				UnitProgressionWindowState.CareerPathList => CareerPathsListsConsoleView.GetNavigationBehaviour(this), 
				UnitProgressionWindowState.CareerPathProgression => CareerPathProgressionConsoleView.GetNavigationBehaviour(this), 
				_ => null, 
			};
			m_NavigationBehaviour.AddEntityVertical(entity);
			if (isFocused)
			{
				m_NavigationBehaviour.FocusOnEntityManual(entity);
			}
		}
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		m_CanFuncAdditional.Value = (entity as IFuncAdditionalClickHandler)?.CanFuncAdditionalClick() ?? false;
	}

	private void OnFuncAdditionalClick()
	{
		if (m_NavigationBehaviour.DeepestNestedFocus is IFuncAdditionalClickHandler funcAdditionalClickHandler)
		{
			funcAdditionalClickHandler.OnFuncAdditionalClick();
		}
	}

	public void EntityFocused(IConsoleEntity entity)
	{
		if (entity != null)
		{
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
	}

	public ReadOnlyReactiveProperty<bool> GetCanHookDeclineProperty()
	{
		return m_CanHookDecline;
	}

	public ReadOnlyReactiveProperty<bool> GetCanHookConfirmProperty()
	{
		return CareerPathProgressionConsoleView.GetCanHookConfirmProperty();
	}
}
