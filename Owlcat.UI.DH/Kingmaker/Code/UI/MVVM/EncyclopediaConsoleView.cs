using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaConsoleView : EncyclopediaBaseView
{
	[Header("Console")]
	[SerializeField]
	[UsedImplicitly]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly ReactiveProperty<bool> m_IsChaptersNavigation = new ReactiveProperty<bool>(value: true);

	private InputLayer m_GlossaryInputLayer;

	private FloatConsoleNavigationBehaviour m_GlossaryNavigationBehavior;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

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

	private void UpdateNavigation(bool fromGlossary = false)
	{
		m_InputLayer.Unbind();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.SetEntitiesVertical(m_Navigation.GetNavigationEntities(m_IsChaptersNavigation.Value));
		m_InputLayer.Bind();
		if (fromGlossary)
		{
			foreach (IConsoleEntity entity in m_NavigationBehaviour.Entities)
			{
				if (entity is EncyclopediaNavigationElementBaseView encyclopediaNavigationElementBaseView && entity.IsValid() && encyclopediaNavigationElementBaseView.IsSelected)
				{
					m_NavigationBehaviour.FocusOnEntityManual(entity);
					m_NavigationBehaviour.Focus.Value = entity;
					m_Navigation.ScrollMenu(m_NavigationBehaviour.Focus.Value, m_IsChaptersNavigation.Value);
					return;
				}
			}
		}
		foreach (IConsoleEntity entity2 in m_NavigationBehaviour.Entities)
		{
			if (entity2 is EncyclopediaNavigationElementBaseView encyclopediaNavigationElementBaseView2 && entity2.IsValid() && encyclopediaNavigationElementBaseView2.IsSelected)
			{
				m_NavigationBehaviour.FocusOnEntityManual(entity2);
				return;
			}
		}
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void OpenGlossaryLink()
	{
		UpdateNavigation(fromGlossary: true);
		CalculateGlossary();
	}

	private void ShowGlossary(InputActionEventData data)
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		(m_NavigationBehaviour.CurrentEntity as ExpandableElement)?.SetCustomLayer("On");
		m_GlossaryMode.Value = true;
		CalculateGlossary();
		m_GlossaryNavigationBehavior.FocusOnFirstValidEntity();
	}

	private void CloseGlossary()
	{
		TooltipHelper.HideTooltip();
		m_GlossaryNavigationBehavior.UnFocusCurrentEntity();
		GamePad.Instance.PopLayer(m_GlossaryInputLayer);
		m_GlossaryMode.Value = false;
		m_CanOpenGlossaryPage.Value = false;
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	private void CalculateGlossary()
	{
		m_CanOpenGlossaryPage.Value = false;
		m_GlossaryNavigationBehavior.Clear();
		if (m_Page.WidgetList.Entries == null)
		{
			m_HasGlossary.Value = false;
			return;
		}
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		if (m_Page.PageAdditionText != null && !string.IsNullOrWhiteSpace(m_Page.PageAdditionText.text) && m_Page.PageAdditionText.gameObject.activeInHierarchy)
		{
			List<IFloatConsoleNavigationEntity> collection = TMPLinkNavigationGenerator.GenerateEntityList(m_Page.PageAdditionText, m_FirstGlossaryFocus, m_SecondGlossaryFocus, OnClickLink, OnFocusLink, TooltipHelper.GetLinkTooltipTemplate);
			list.AddRange(collection);
		}
		foreach (MonoBehaviour entry in m_Page.WidgetList.Entries)
		{
			if (!entry.gameObject.activeInHierarchy)
			{
				continue;
			}
			List<TextMeshProUGUI> list2 = ((entry is EncyclopediaPageBlockAstropathBriefPCView encyclopediaPageBlockAstropathBriefPCView) ? encyclopediaPageBlockAstropathBriefPCView.GetLinksTexts() : ((entry is EncyclopediaPageBlockBookEventPCView encyclopediaPageBlockBookEventPCView) ? encyclopediaPageBlockBookEventPCView.GetLinksTexts() : ((entry is EncyclopediaPageBlockGlossaryEntryPCView encyclopediaPageBlockGlossaryEntryPCView) ? encyclopediaPageBlockGlossaryEntryPCView.GetLinksTexts() : ((!(entry is EncyclopediaPageBlockTextPCView encyclopediaPageBlockTextPCView)) ? null : encyclopediaPageBlockTextPCView.GetLinksTexts()))));
			List<TextMeshProUGUI> list3 = list2;
			if (list3 == null || !list3.Any())
			{
				continue;
			}
			foreach (TextMeshProUGUI item in list3)
			{
				List<IFloatConsoleNavigationEntity> collection2 = TMPLinkNavigationGenerator.GenerateEntityList(item, m_FirstGlossaryFocus, m_SecondGlossaryFocus, OnClickLink, OnFocusLink, TooltipHelper.GetLinkTooltipTemplate);
				list.AddRange(collection2);
			}
		}
		m_HasGlossary.Value = list.Any();
		if (m_HasGlossary.Value)
		{
			m_GlossaryNavigationBehavior.AddEntities(list);
		}
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
		if (!(GamePad.Instance.CurrentInputLayer.ContextName != "DialogGlossary"))
		{
			m_Key = key;
			DelEnsure();
		}
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

	private void Scroll(InputActionEventData obj, float value)
	{
		m_Page.Scroll(obj, value);
	}

	public void OnFocusedChanged(IConsoleEntity entity)
	{
		bool value = m_IsChaptersNavigation.Value;
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
		if (value != m_IsChaptersNavigation.Value)
		{
			UpdateNavigation();
		}
	}

	private void StepBack()
	{
		m_IsChaptersNavigation.Value = true;
		UpdateNavigation();
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
