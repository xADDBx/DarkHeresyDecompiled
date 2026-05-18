using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureAndColorGroupSelectorView : BaseCharGenAppearancePageComponentView<TextureAndColorGroupSelectorVM>
{
	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private TextureAndColorSelectorItemView m_TextureAndColorSelectorItemPrefab;

	[SerializeField]
	private OwlcatMultiButton m_ButtonAddGroup;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		Show();
		base.ViewModel.Items.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntries();
		}).AddTo(this);
		DrawEntries();
		m_ButtonAddGroup.OnLeftClickAsObservable().Subscribe(base.ViewModel.AddGroup).AddTo(this);
		base.ViewModel.CanAdd.Subscribe(delegate(bool canAdd)
		{
			m_ButtonAddGroup.gameObject.SetActive(canAdd);
		}).AddTo(this);
	}

	private void DrawEntries()
	{
		m_WidgetList.Clear();
		m_WidgetList.DrawEntries(base.ViewModel.Items, m_TextureAndColorSelectorItemPrefab);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_WidgetList.Clear();
		Hide();
	}
}
