using System;
using System.Collections.Generic;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFoldableSectionVM : BricksGroupBaseVM, IFoldableSectionStateChanged, ISubscriber
{
	public readonly string Header;

	public readonly string SectionKey;

	private readonly ReactiveProperty<bool> m_IsExpanded;

	private readonly Action<bool> m_OnExpandedChanged;

	public ReadOnlyReactiveProperty<bool> IsExpanded => m_IsExpanded;

	public BrickFoldableSectionVM(IReadOnlyList<TooltipBrickVM> children, string sectionKey, string header, bool initialExpanded, Action<bool> onExpandedChanged)
		: base(children)
	{
		SectionKey = sectionKey;
		Header = header;
		m_IsExpanded = new ReactiveProperty<bool>(initialExpanded);
		m_OnExpandedChanged = onExpandedChanged;
		if (!string.IsNullOrEmpty(SectionKey))
		{
			EventBus.Subscribe(this).AddTo(this);
		}
	}

	public void Toggle()
	{
		bool next = !m_IsExpanded.Value;
		m_IsExpanded.Value = next;
		m_OnExpandedChanged?.Invoke(next);
		if (!string.IsNullOrEmpty(SectionKey))
		{
			EventBus.RaiseEvent(delegate(IFoldableSectionStateChanged h)
			{
				h.HandleFoldableSectionStateChanged(SectionKey, next);
			});
		}
	}

	public void HandleFoldableSectionStateChanged(string sectionKey, bool isExpanded)
	{
		if (!(sectionKey != SectionKey) && m_IsExpanded.Value != isExpanded)
		{
			m_IsExpanded.Value = isExpanded;
		}
	}
}
