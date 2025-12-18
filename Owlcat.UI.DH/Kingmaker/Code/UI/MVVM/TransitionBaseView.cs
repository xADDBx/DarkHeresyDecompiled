using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionBaseView : View<TransitionVM>
{
	[SerializeField]
	private TextMeshProUGUI m_MapName;

	[SerializeField]
	private List<TransitionMapPart> m_Parts = new List<TransitionMapPart>();

	[SerializeField]
	private PantographView m_PantographView;

	[SerializeField]
	private float m_DefaultPantographMaxY = -100f;

	[SerializeField]
	private TransitionLegendButtonView m_TransitionLegendButtonViewPrefab;

	protected TransitionMapPart m_CurrentPart;

	private readonly List<Action> m_HoverActions = new List<Action>();

	private readonly List<Action> m_UnHoverActions = new List<Action>();

	private List<TransitionLegendButtonView> m_TransitionLegendButtonView;

	protected override void OnBind()
	{
		base.OnBind();
		m_PantographView.Show().AddTo(this);
		m_Parts.ForEach(delegate(TransitionMapPart p)
		{
			p.Initialize();
		});
		m_CurrentPart = m_Parts.First((TransitionMapPart p) => p.Map == base.ViewModel.Map);
		m_CurrentPart.MapObject.SetActive(value: true);
		m_PantographView.SetCustomMaxY(m_CurrentPart.CustomPantographMaxY ? m_CurrentPart.CustomPantographMaxYValue : m_DefaultPantographMaxY);
		if (m_MapName != null)
		{
			m_MapName.text = base.ViewModel.Name;
		}
		SetVisible(state: true);
		DrawObjects();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.TransitionMap);
		});
	}

	private void DrawObjects()
	{
		m_CurrentPart.WidgetList.DrawEntries(base.ViewModel.EntryVms, m_TransitionLegendButtonViewPrefab);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_PantographView.Unbind();
		m_CurrentPart.WidgetList.Clear();
		m_HoverActions.Clear();
		m_UnHoverActions.Clear();
		SetVisible(state: false);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.TransitionMap);
		});
	}

	private void SetVisible(bool state)
	{
		base.gameObject.SetActive(state);
		UISounds.Instance.Play(state ? UISounds.Instance.Sounds.Dialogue.BookOpen : UISounds.Instance.Sounds.Dialogue.BookClose);
	}
}
