using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class LootCollectorView : View<LootCollectorVM>
{
	[Header("Loot Objects")]
	[SerializeField]
	public LootObjectView m_LootToInventory;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	public virtual void Initialize()
	{
		m_LootToInventory.Initialize();
		Hide();
	}

	protected override void OnBind()
	{
		Show();
		m_LootToInventory.Bind(base.ViewModel.ContextLoot[0]);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.Loot.LootUpdated, delegate
		{
			m_ScrollRect.ScrollToTop();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	protected virtual void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void ChangeView()
	{
		base.ViewModel.ChangeView();
	}

	public void CollectAll()
	{
		base.ViewModel.CollectAll();
	}
}
