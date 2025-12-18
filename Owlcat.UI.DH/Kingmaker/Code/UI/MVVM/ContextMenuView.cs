using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuView : View<ContextMenuVM>
{
	[SerializeField]
	private ContextMenuEntitiesView m_Config;

	[SerializeField]
	private RectTransform m_Container;

	protected readonly List<MonoBehaviour> m_Entities = new List<MonoBehaviour>();

	protected override void OnBind()
	{
		foreach (ContextMenuEntityVM entity in base.ViewModel.Entities)
		{
			MonoBehaviour entityView = ContextMenuEngine.GetEntityView(m_Config, entity);
			entityView.transform.SetParent(m_Container, worldPositionStays: false);
			m_Entities.Add(entityView);
		}
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(0.1f.Seconds(), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
		{
			base.gameObject.SetActive(value: true);
			UIUtilityRect.SetPopupWindowPosition((RectTransform)base.transform, base.ViewModel.Owner, Vector2.zero);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Entities.ForEach(WidgetFactory.DisposeWidget);
		m_Entities.Clear();
		base.gameObject.SetActive(value: false);
	}
}
