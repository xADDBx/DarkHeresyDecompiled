using System.Linq;
using Kingmaker.Code.Framework.Settings.UISettings;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoPagesMenuPCView : View<SelectionGroupRadioVM<CharInfoPagesMenuEntityVM>>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharInfoPagesMenuEntityPCView m_MenuItemViewPrefab;

	[Header("Lens")]
	[SerializeField]
	private Transform m_Lens;

	[SerializeField]
	private float m_LensAnimationDuration = 0.55f;

	private readonly float m_LendDistanceThreshold = 0.01f;

	protected override void OnBind()
	{
		DrawEntities();
		base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.PrevTab.name, SelectPrev).AddTo(this);
		Game.Instance.Keyboard.Bind(uIKeybindGeneralSettings.NextTab.name, SelectNext).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_WidgetList.Clear();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_MenuItemViewPrefab);
		m_WidgetList.Entries.ForEach(delegate(IBindable e)
		{
			(e as CharInfoPagesMenuEntityPCView)?.SetupLens(m_Lens, m_LendDistanceThreshold, m_LensAnimationDuration);
		});
	}

	protected void SelectPrev()
	{
		base.ViewModel.SelectPrevValidEntity();
	}

	protected void SelectNext()
	{
		base.ViewModel.SelectNextValidEntity();
	}
}
