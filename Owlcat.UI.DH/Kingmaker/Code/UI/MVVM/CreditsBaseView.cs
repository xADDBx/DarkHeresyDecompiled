using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints.Credits;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CreditsBaseView : View<CreditsVM>, ICreditsView
{
	[Header("Main Page")]
	[SerializeField]
	private TextMeshProUGUI m_TitleLabel;

	[SerializeField]
	protected TextMeshProUGUI m_HeadLabel;

	[SerializeField]
	private Transform m_Content;

	[SerializeField]
	protected PageGenerator m_PageGenerator = new PageGenerator();

	[Header("Menu")]
	[SerializeField]
	protected CreditsMenuSelectorBaseView m_MenuSelector;

	[SerializeField]
	protected FlexibleLensSelectorView m_SelectorView;

	[Header("Right Page")]
	[SerializeField]
	protected TextMeshProUGUI m_MessageLabel;

	[SerializeField]
	protected Image m_Logo;

	[Header("Bottom Panel")]
	[SerializeField]
	protected TextMeshProUGUI m_PageCounterText;

	[SerializeField]
	protected OwlcatMultiButton m_ButtonLeft;

	[SerializeField]
	protected OwlcatMultiButton m_ButtonRight;

	[SerializeField]
	protected OwlcatMultiButton m_PlayMultiButton;

	[SerializeField]
	protected TMP_InputField m_SearchField;

	[SerializeField]
	protected OwlcatMultiButton m_SearchButton;

	[SerializeField]
	protected TextMeshProUGUI m_SearchButtonText;

	[Header("Prefabs and Misc")]
	[SerializeField]
	protected CreditsOneColumnPage m_OneColumnPrefab;

	[SerializeField]
	protected CreditsTwoColumnsPage m_TwoColumnsPrefab;

	[SerializeField]
	private float m_PageDurationTime = 5f;

	[SerializeField]
	private float m_PingDelay = 0.2f;

	private BlueprintCreditsGroup m_CurrentGroup;

	private ICreditsBlock m_CurrentBlock;

	private int m_CurrentPage;

	private List<string> m_Pages;

	private string m_Filter;

	private LinkedListNode<PageGenerator.SearchResult> m_ResultSearchView;

	private bool m_PlayPagesCoroutineIsPlaying;

	private bool m_IsInitialized;

	protected readonly LinkedList<PageGenerator.SearchResult> ResultSearch = new LinkedList<PageGenerator.SearchResult>();

	public Transform Content => m_Content;

	protected override void OnBind()
	{
		if (!m_IsInitialized)
		{
			m_MenuSelector.Initialize();
			m_IsInitialized = true;
		}
		SetupTexts();
		base.ViewModel.Pause.Subscribe(delegate(bool value)
		{
			if (value)
			{
				if (m_PlayPagesCoroutineIsPlaying)
				{
					StopAllCoroutines();
					m_PlayPagesCoroutineIsPlaying = false;
				}
			}
			else if (!m_PlayPagesCoroutineIsPlaying && base.gameObject.activeSelf)
			{
				StartCreditsCoroutine(m_CurrentGroup, m_CurrentPage, -1);
				m_PlayPagesCoroutineIsPlaying = true;
			}
			m_PlayMultiButton.SetActiveLayer((!value) ? 1 : 0);
		}).AddTo(this);
		base.ViewModel.OnSelectGroup.Subscribe(SelectCreditsGroup).AddTo(this);
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.ServiceWindowsSounds.PlayOpenSound(ServiceWindowsType.LocalMap);
		m_MenuSelector.Bind(base.ViewModel.SelectionGroup);
		m_SelectorView.Bind(base.ViewModel.Selector);
		StartCreditsCoroutine(base.ViewModel.Groups?.FirstOrDefault(), 0, -1, withSound: false);
		m_PlayPagesCoroutineIsPlaying = true;
		Observable.EveryValueChanged(m_SearchField, (TMP_InputField f) => f.text).Subscribe(delegate(string t)
		{
			base.ViewModel.CheckInputFieldAnySymbols(t);
		}).AddTo(this);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Credits);
		});
	}

	protected override void OnUnbind()
	{
		if (m_CurrentBlock != null)
		{
			StopAllCoroutines();
			m_CurrentBlock.Hide();
			m_CurrentBlock = null;
		}
		m_PlayPagesCoroutineIsPlaying = false;
		UISounds.Instance.Sounds.ServiceWindowsSounds.PlayCloseSound(ServiceWindowsType.LocalMap);
		base.gameObject.SetActive(value: false);
		m_MenuSelector.Unbind();
		m_SelectorView.Unbind();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Credits);
		});
	}

	private void SelectCreditsGroup(BlueprintCreditsGroup group)
	{
		if (m_CurrentGroup != group)
		{
			base.ViewModel.SetPause(isPause: true);
			StopAllCoroutines();
			m_PlayPagesCoroutineIsPlaying = false;
			StartCreditsCoroutine(group, 0, -1);
			m_PlayPagesCoroutineIsPlaying = true;
		}
	}

	private void StartCreditsCoroutine(BlueprintCreditsGroup group, int numberPage, int pingRow, bool withSound = true)
	{
		try
		{
			StartCoroutine(DoCreditsGroup(group, numberPage, pingRow, withSound));
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
			throw;
		}
	}

	public void OnNextPage(Action nextChapterAction = null)
	{
		if (m_CurrentPage != m_Pages.Count - 1)
		{
			DoPage(1);
		}
		else
		{
			nextChapterAction?.Invoke();
		}
	}

	public void OnPrevPage(Action prevChapterAction = null)
	{
		if (m_CurrentPage != 0)
		{
			DoPage(-1);
		}
		else
		{
			prevChapterAction?.Invoke();
		}
	}

	public void DoPage(int direction)
	{
		base.ViewModel.SetPauseState(state: true);
		StopAllCoroutines();
		m_PlayPagesCoroutineIsPlaying = false;
		StartCreditsCoroutine(m_CurrentGroup, m_CurrentPage + direction, -1);
		m_PlayPagesCoroutineIsPlaying = true;
	}

	private IEnumerator DoCreditsGroup(BlueprintCreditsGroup group, int numberPage, int pingRow, bool withSound = true)
	{
		yield return null;
		if (!group)
		{
			PFLog.UI.Log("Element m_CurrentGroup is null ");
			yield break;
		}
		if (m_CurrentGroup != group)
		{
			m_Pages = m_PageGenerator.GeneratePages(group);
		}
		int num = 0;
		if (numberPage >= 0 && numberPage < m_Pages?.Count)
		{
			num = numberPage;
		}
		m_CurrentGroup = group;
		for (int i = num; i < m_Pages?.Count; i++)
		{
			if (m_CurrentBlock != null)
			{
				m_CurrentBlock.Hide();
				m_CurrentBlock = null;
			}
			m_CurrentPage = i;
			string s = m_Pages[m_CurrentPage];
			ICreditsBlock creditsBlock;
			if (group.TeamsData == null)
			{
				ICreditsBlock instance = m_TwoColumnsPrefab.GetInstance<CreditsTwoColumnsPage>();
				creditsBlock = instance;
			}
			else
			{
				ICreditsBlock instance = m_OneColumnPrefab.GetInstance<CreditsOneColumnPage>();
				creditsBlock = instance;
			}
			ICreditsBlock creditsBlock2 = creditsBlock;
			creditsBlock2.Initialize(this);
			m_CurrentBlock = creditsBlock2;
			using (StringReader stringReader = new StringReader(s))
			{
				string row;
				while (!string.IsNullOrEmpty(row = stringReader.ReadLine()))
				{
					m_CurrentBlock.Append(row, group);
				}
			}
			m_MessageLabel.text = group.PageText;
			m_HeadLabel.text = group.HeaderText;
			m_Logo.sprite = group.PageImage;
			base.ViewModel.SetSelectedGroup(group);
			m_SelectorView.ChangeTab(base.ViewModel.SelectedMenuIndex, withSound);
			m_CurrentBlock.Show();
			m_PageCounterText.text = m_CurrentPage + 1 + " / " + m_Pages.Count;
			m_ButtonLeft.Interactable = m_CurrentPage != 0;
			m_ButtonRight.Interactable = m_CurrentPage != m_Pages.Count - 1;
			if (pingRow >= 0)
			{
				yield return new WaitForSecondsRealtime(m_PingDelay);
				m_CurrentBlock.Ping(pingRow);
			}
			while (base.ViewModel.Pause.CurrentValue)
			{
				yield return null;
				pingRow = -1;
			}
			if (!base.ViewModel.Pause.CurrentValue)
			{
				yield return new WaitForSecondsRealtime(m_PageDurationTime);
			}
		}
		while (base.ViewModel.Pause.CurrentValue)
		{
			yield return null;
		}
		int num2 = base.ViewModel.Groups.FindIndex((BlueprintCreditsGroup x) => x.Equals(group)) + 1;
		if (num2 < base.ViewModel.Groups.Count)
		{
			yield return StartCoroutine(DoCreditsGroup(base.ViewModel.Groups[num2], 0, -1));
			m_PlayPagesCoroutineIsPlaying = true;
		}
	}

	public bool SearchPerson(string personName)
	{
		base.ViewModel.SetPauseState(state: true);
		StopAllCoroutines();
		if (string.IsNullOrEmpty(personName))
		{
			ResultSearch.Clear();
			m_Filter = string.Empty;
			return false;
		}
		if (m_Filter != personName)
		{
			m_Filter = personName;
			ResultSearch.Clear();
			foreach (BlueprintCreditsGroup group in base.ViewModel.Groups)
			{
				foreach (PageGenerator.SearchResult item in m_PageGenerator.GenerateSearch(group, m_Filter))
				{
					ResultSearch.AddLast(item);
				}
			}
			m_ResultSearchView = ResultSearch.First;
		}
		LinkedListNode<PageGenerator.SearchResult> resultSearchView = m_ResultSearchView;
		if (resultSearchView != null)
		{
			_ = resultSearchView.Value;
			if (0 == 0)
			{
				StartCreditsCoroutine(m_ResultSearchView.Value.Group, m_ResultSearchView.Value.Page, m_ResultSearchView.Value.Row);
				m_PlayPagesCoroutineIsPlaying = true;
				m_ResultSearchView = m_ResultSearchView.Next ?? ResultSearch.First;
				return true;
			}
		}
		return false;
	}

	public void OnFind()
	{
		if (!SearchPerson(m_SearchField.text))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.Credits.PersonNotFound, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
	}

	private void SetupTexts()
	{
		m_TitleLabel.text = UIStrings.Instance.MainMenu.Credits;
		m_SearchButtonText.text = UIStrings.Instance.CommonTexts.Search;
		m_SearchField.placeholder.GetComponent<TextMeshProUGUI>().text = UIStrings.Instance.Credits.EnterSearchNameHere;
		m_Filter = string.Empty;
	}

	protected void ChangeTab(bool direction)
	{
		if (direction)
		{
			m_MenuSelector.OnNext();
		}
		else
		{
			m_MenuSelector.OnPrev();
		}
		m_SelectorView.ChangeTab(base.ViewModel.SelectedMenuIndex);
	}
}
