using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionBaseView_OBSOLETE : View<TransitionVM_OBSOLETE>
{
	[SerializeField]
	private TMP_Text m_MapName;

	[SerializeField]
	private List<TransitionMapPart_OBSOLETE> m_Parts = new List<TransitionMapPart_OBSOLETE>();

	[SerializeField]
	private TransitionLegendButtonView_OBSOLETE TransitionLegendButtonViewObsoletePrefab;

	protected TransitionMapPart_OBSOLETE m_CurrentPartObsolete;

	private readonly List<Action> m_HoverActions = new List<Action>();

	private readonly List<Action> m_UnHoverActions = new List<Action>();

	private List<TransitionLegendButtonView_OBSOLETE> m_TransitionLegendButtonView;

	protected override void OnBind()
	{
		base.OnBind();
		m_Parts.ForEach(delegate(TransitionMapPart_OBSOLETE p)
		{
			p.Initialize();
		});
		m_CurrentPartObsolete = m_Parts.First((TransitionMapPart_OBSOLETE p) => p.Map == base.ViewModel.Map);
		m_CurrentPartObsolete.MapObject.SetActive(value: true);
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
		m_CurrentPartObsolete.WidgetList.DrawEntries(base.ViewModel.EntryVms, TransitionLegendButtonViewObsoletePrefab);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_CurrentPartObsolete.WidgetList.Clear();
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
		UISounds.Instance.Play(state ? FullScreenSounds.Instance.Dialogue.BookOpen : FullScreenSounds.Instance.Dialogue.BookClose);
	}
}
