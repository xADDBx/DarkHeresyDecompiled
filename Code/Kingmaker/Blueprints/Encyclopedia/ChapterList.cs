using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Blueprints.Encyclopedia;

public class ChapterList : ScriptableObject, IEnumerable<BlueprintEncyclopediaChapter>, IEnumerable
{
	private static LogChannel Logger = LogChannelFactory.GetOrCreate("Encyclopedia");

	[SerializeField]
	protected List<BlueprintEncyclopediaChapterReference> m_List = new List<BlueprintEncyclopediaChapterReference>();

	private static Dictionary<string, BlueprintEncyclopediaPage> m_AllPages;

	private static Dictionary<string, BlueprintEncyclopediaEntry> m_AllEntries;

	public BlueprintEncyclopediaChapter this[int i]
	{
		get
		{
			return m_List[i].Get();
		}
		set
		{
			m_List[i] = value.ToReference<BlueprintEncyclopediaChapterReference>();
		}
	}

	public IEnumerator<BlueprintEncyclopediaChapter> GetEnumerator()
	{
		return m_List.Select((BlueprintEncyclopediaChapterReference r) => r.Get()).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_List.GetEnumerator();
	}

	public static BlueprintEncyclopediaPage GetPage(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		if (UIConfig.Instance.ChapterList == null)
		{
			return null;
		}
		UIConfig.Instance.ChapterList.RefreshChapters();
		if (m_AllPages.ContainsKey(key))
		{
			return m_AllPages[key];
		}
		string text = m_AllPages?.FirstOrDefault((KeyValuePair<string, BlueprintEncyclopediaPage> p) => p.Value?.GlossaryEntry?.ToString() == key).Key;
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		if (!m_AllPages.ContainsKey(text))
		{
			return null;
		}
		return m_AllPages[text];
	}

	public static BlueprintEncyclopediaEntry GetEntry(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		if (UIConfig.Instance.ChapterList == null)
		{
			return null;
		}
		UIConfig.Instance.ChapterList.RefreshChapters();
		if (m_AllEntries.ContainsKey(key))
		{
			return m_AllEntries[key];
		}
		return null;
	}

	public void RefreshChapters()
	{
		Initialize();
		InitializeAdditionalChapter(UIConfig.Instance.BookEventsChapter.Get());
		InitializeAdditionalChapter(UIConfig.Instance.AstropathBriefsChapter.Get());
	}

	public void Initialize()
	{
		if (m_AllPages != null && m_AllEntries != null)
		{
			return;
		}
		m_AllPages = new Dictionary<string, BlueprintEncyclopediaPage>();
		foreach (BlueprintEncyclopediaChapterReference item in m_List)
		{
			PrepareNode(item.Get());
		}
		m_AllEntries = new Dictionary<string, BlueprintEncyclopediaEntry>();
		foreach (BlueprintEncyclopediaChapterReference item2 in m_List)
		{
			PrepareEntry(item2.Get());
		}
	}

	private void InitializeAdditionalChapter(BlueprintEncyclopediaChapter chapter)
	{
		if (chapter != null && chapter.ChildPages.Dereference().Any((BlueprintEncyclopediaPage page) => page is IEncyclopediaPageWithAvailability encyclopediaPageWithAvailability && encyclopediaPageWithAvailability.IsAvailable))
		{
			PrepareNode(chapter);
		}
	}

	private static void PrepareNode(BlueprintEncyclopediaNode node)
	{
		if (node == null)
		{
			return;
		}
		if (!m_AllPages.ContainsKey(node.name) && node is BlueprintEncyclopediaPage blueprintEncyclopediaPage)
		{
			m_AllPages.Add(blueprintEncyclopediaPage.name, blueprintEncyclopediaPage);
		}
		foreach (BlueprintEncyclopediaPageReference childPage in node.ChildPages)
		{
			if (childPage != null)
			{
				BlueprintEncyclopediaPage blueprintEncyclopediaPage2 = childPage.Get();
				if (blueprintEncyclopediaPage2 == null)
				{
					Logger.Error("Error: BlueprintEncyclopediaNode [" + node.name + "] has empty links, please delete them");
					continue;
				}
				blueprintEncyclopediaPage2.ParentAsset = node;
				PrepareNode(childPage.Get());
			}
		}
	}

	private static void PrepareEntry(BlueprintEncyclopediaChapter node)
	{
		if (node == null)
		{
			return;
		}
		if (!m_AllEntries.ContainsKey(node.name) && node != null)
		{
			m_AllEntries.Add(node.name, node.MainGlossaryEntry);
		}
		foreach (BlueprintEncyclopediaEntryReference encyclopediaEntry in node.EncyclopediaEntries)
		{
			if (encyclopediaEntry != null)
			{
				BlueprintEncyclopediaEntry blueprintEncyclopediaEntry = encyclopediaEntry.Get();
				if (blueprintEncyclopediaEntry == null)
				{
					Logger.Error("Error: BlueprintEncyclopediaEntry [" + node.name + "] has empty links, please delete them");
				}
				else if (!m_AllEntries.ContainsKey(blueprintEncyclopediaEntry.name))
				{
					m_AllEntries.Add(blueprintEncyclopediaEntry.name, blueprintEncyclopediaEntry);
				}
			}
		}
	}
}
