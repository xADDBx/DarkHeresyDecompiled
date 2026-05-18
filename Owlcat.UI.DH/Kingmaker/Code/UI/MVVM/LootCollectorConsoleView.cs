using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LootCollectorConsoleView : LootCollectorView
{
	[Header("Console")]
	[SerializeField]
	private HintView m_CollectAllHint;

	[SerializeField]
	private HintView m_CollectAllLongHint;

	[SerializeField]
	private HintView m_CloseHint;

	[SerializeField]
	private HintView[] m_ChangeViewHints;

	private readonly ReactiveProperty<bool> m_IsFocused = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		m_CollectAllHint.gameObject.SetActive(value: true);
	}

	public void AddInput()
	{
	}

	private void ForceScrollToObj(IConsoleEntity entity)
	{
		LootSlotConsoleView lootSlotConsoleView = entity as LootSlotConsoleView;
		m_IsFocused.Value = lootSlotConsoleView != null;
		if (!(lootSlotConsoleView == null))
		{
			RectTransform targetRect = lootSlotConsoleView.transform as RectTransform;
			m_ScrollRect.EnsureVisibleVertical(targetRect);
		}
	}

	private void CollectAllAndScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
		Collect();
	}

	private void Collect()
	{
		CollectAll();
		ButtonsSounds.Instance.LootCollectAllButton.Click.Play();
	}
}
