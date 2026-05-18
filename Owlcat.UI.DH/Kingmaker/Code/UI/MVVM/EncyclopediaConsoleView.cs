using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaConsoleView : EncyclopediaBaseView
{
	[Header("Console")]
	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	private readonly ReactiveProperty<bool> m_IsChaptersNavigation = new ReactiveProperty<bool>(value: true);

	[SerializeField]
	private OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	private OwlcatMultiButton m_SecondGlossaryFocus;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	private readonly ReactiveProperty<bool> m_GlossaryMode = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasGlossary = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanOpenGlossaryPage = new ReactiveProperty<bool>();

	private string m_Key;

	private const string GlossaryInputLayerContextName = "DialogGlossary";

	private void OpenGlossaryLink()
	{
		CalculateGlossary();
	}

	private void ShowGlossary()
	{
	}

	private void CloseGlossary()
	{
		TooltipHelper.HideTooltip();
		m_GlossaryMode.Value = false;
		m_CanOpenGlossaryPage.Value = false;
	}

	private void CalculateGlossary()
	{
		m_CanOpenGlossaryPage.Value = false;
		if (m_Page.WidgetList.Entries == null)
		{
			m_HasGlossary.Value = false;
			return;
		}
		List<IFloatConsoleNavigationEntity> source = new List<IFloatConsoleNavigationEntity>();
		foreach (MonoBehaviour entry in m_Page.WidgetList.Entries)
		{
			if (!entry.gameObject.activeInHierarchy)
			{
				continue;
			}
			List<TextMeshProUGUI> list = ((entry is EncyclopediaPageBlockAstropathBriefPCView encyclopediaPageBlockAstropathBriefPCView) ? encyclopediaPageBlockAstropathBriefPCView.GetLinksTexts() : ((entry is EncyclopediaPageBlockBookEventPCView encyclopediaPageBlockBookEventPCView) ? encyclopediaPageBlockBookEventPCView.GetLinksTexts() : ((entry is EncyclopediaPageBlockGlossaryEntryPCView encyclopediaPageBlockGlossaryEntryPCView) ? encyclopediaPageBlockGlossaryEntryPCView.GetLinksTexts() : ((!(entry is EncyclopediaPageBlockTextPCView encyclopediaPageBlockTextPCView)) ? null : encyclopediaPageBlockTextPCView.GetLinksTexts()))));
			List<TextMeshProUGUI> list2 = list;
			if (list2 == null || !list2.Any())
			{
				continue;
			}
			foreach (TextMeshProUGUI item in list2)
			{
				_ = item;
			}
		}
		m_HasGlossary.Value = source.Any();
		if (m_GlossaryMode.Value)
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void OnClickLink(string key)
	{
		string[] glossaryEntry = UtilityLink.GetKeysFromLink(key);
		if (glossaryEntry.Length > 1)
		{
			EventBus.RaiseEvent(delegate(IEncyclopediaHandler x)
			{
				x.HandleEncyclopediaPage(glossaryEntry[1]);
			});
			CloseGlossary();
		}
	}

	protected virtual void OnFocusLink(string key)
	{
		m_Key = key;
		DelEnsure();
	}

	private void DelEnsure()
	{
		DelayedInvoker.InvokeInFrames(EnsureVisible, 1);
	}

	private void EnsureVisible()
	{
		m_Page.ScrollRect.EnsureVisibleVertical(m_FirstGlossaryFocus.transform as RectTransform);
		Focus();
	}

	protected virtual void Focus()
	{
		if (m_GlossaryMode.Value)
		{
			m_CanOpenGlossaryPage.Value = true;
			m_FirstGlossaryFocus.ShowTooltip(TooltipHelper.GetLinkTooltipTemplate(m_Key), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace));
		}
	}

	private void Scroll(float value)
	{
		m_Page.Scroll(value);
	}

	public void OnFocusedChanged(IConsoleEntity entity)
	{
		_ = m_IsChaptersNavigation.Value;
		if (entity is EncyclopediaNavigationElementBaseView encyclopediaNavigationElementBaseView)
		{
			encyclopediaNavigationElementBaseView.SelectPage();
			if (m_IsChaptersNavigation.Value)
			{
				m_IsChaptersNavigation.Value = false;
			}
			if (!m_IsChaptersNavigation.Value)
			{
				CalculateGlossary();
			}
		}
	}

	private void StepBack()
	{
		m_IsChaptersNavigation.Value = true;
	}

	private void CloseWindow()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}

	protected override void OnCloseGlossaryMode()
	{
		base.OnCloseGlossaryMode();
		CloseGlossary();
		DelayedInvoker.InvokeInFrames(OpenGlossaryLink, 1);
	}
}
