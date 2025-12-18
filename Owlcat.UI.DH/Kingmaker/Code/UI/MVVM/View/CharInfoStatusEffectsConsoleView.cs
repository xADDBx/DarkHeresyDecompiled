using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoStatusEffectsConsoleView : CharInfoStatusEffectsView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void OnBind()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged).AddTo(this);
		base.OnBind();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		if (m_WidgetList.Entries == null)
		{
			return;
		}
		foreach (IBindable entry in m_WidgetList.Entries)
		{
			m_NavigationBehaviour.AddRow<StatusEffectConsoleView>(entry as StatusEffectConsoleView);
		}
	}

	private void OnFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform targetRect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			m_Scroll.EnsureVisibleVertical(targetRect);
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
	}
}
