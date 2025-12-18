using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LootCollectorConsoleView : LootCollectorView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_CollectAllHint;

	[SerializeField]
	private ConsoleHint m_CollectAllLongHint;

	[SerializeField]
	private ConsoleHint m_CloseHint;

	[SerializeField]
	private ConsoleHint[] m_ChangeViewHints;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly ReactiveProperty<bool> m_IsFocused = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		m_CollectAllHint.gameObject.SetActive(value: true);
	}

	public void AddInput(InputLayer inputLayer)
	{
		m_CollectAllHint.Bind(inputLayer.AddButton(Collect, 10, base.ViewModel.NoLoot.Not().And(base.ViewModel.ExtendedView.Not()).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased)).AddTo(this);
		m_CollectAllHint.SetLabel(UIStrings.Instance.LootWindow.CollectAll);
		m_CollectAllLongHint.Bind(inputLayer.AddButton(CollectAllAndScrollToTop, 10, base.ViewModel.NoLoot.Not().And(base.ViewModel.ExtendedView).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed)).AddTo(this);
		m_CollectAllLongHint.SetLabel(UIStrings.Instance.LootWindow.CollectAll);
		inputLayer.AddButton(delegate
		{
			ChangeView();
		}, 18).AddTo(this);
		ConsoleHint[] changeViewHints = m_ChangeViewHints;
		for (int i = 0; i < changeViewHints.Length; i++)
		{
			changeViewHints[i].Bind(inputLayer.AddButton(delegate
			{
			}, 18)).AddTo(this);
		}
		m_CloseHint.Bind(inputLayer.AddButton(delegate
		{
		}, 9, base.ViewModel.NoLoot)).AddTo(this);
		m_CloseHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(ForceScrollToObj).AddTo(this);
	}

	public ConsoleNavigationBehaviour GetNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		return m_NavigationBehaviour;
	}

	public IConsoleEntity GetCurrentFocus()
	{
		return m_NavigationBehaviour?.DeepestNestedFocus;
	}

	protected override void Hide()
	{
		m_NavigationBehaviour?.Clear();
		base.Hide();
	}

	private void ForceScrollToObj(IConsoleEntity entity)
	{
		LootSlotConsoleView lootSlotConsoleView = entity as LootSlotConsoleView;
		m_IsFocused.Value = lootSlotConsoleView != null;
		if (m_IsFocused.Value)
		{
			RectTransform targetRect = lootSlotConsoleView.transform as RectTransform;
			m_ScrollRect.EnsureVisibleVertical(targetRect);
		}
	}

	private void CollectAllAndScrollToTop(InputActionEventData data)
	{
		m_ScrollRect.ScrollToTop();
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		Collect(data);
	}

	private void Collect(InputActionEventData data)
	{
		CollectAll();
		UISounds.Instance.Sounds.Buttons.LootCollectAllButtonClick.Play();
	}
}
