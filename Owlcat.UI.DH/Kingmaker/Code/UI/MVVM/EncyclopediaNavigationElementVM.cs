using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaNavigationElementVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>();

	public string Title;

	private List<EncyclopediaNavigationElementVM> m_ChildsVM;

	private readonly ReactiveProperty<bool> m_IsAvailablePage = new ReactiveProperty<bool>(value: true);

	private List<IPage> m_Childs = new List<IPage>();

	private List<BlueprintEncyclopediaEntry> m_Entries = new List<BlueprintEncyclopediaEntry>();

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public ReadOnlyReactiveProperty<bool> IsAvailablePage => m_IsAvailablePage;

	public IPage Page { get; }

	public List<EncyclopediaNavigationElementVM> ChildsVM => m_ChildsVM;

	public List<EncyclopediaNavigationElementVM> GetOrCreateChildsVM()
	{
		if (m_ChildsVM == null)
		{
			m_ChildsVM = new List<EncyclopediaNavigationElementVM>();
		}
		if (m_ChildsVM.Count == 0 && Page != null)
		{
			m_Childs = Page.GetChilds();
			foreach (IPage child in m_Childs)
			{
				if (child == null)
				{
					UberDebug.LogError($"{Page} has empty page");
				}
				else if (string.IsNullOrEmpty(child.GetTitle()))
				{
					UberDebug.LogError($"{Page} has element {child} with empty Title");
				}
				else if (!(child is IEncyclopediaPageWithAvailability { IsAvailable: false }))
				{
					EncyclopediaNavigationElementVM item = new EncyclopediaNavigationElementVM(child).AddTo(this);
					m_ChildsVM.Add(item);
				}
			}
		}
		if (m_Entries.Count == 0 && Page is BlueprintEncyclopediaChapter blueprintEncyclopediaChapter)
		{
			foreach (BlueprintEncyclopediaEntryReference encyclopediaEntry in blueprintEncyclopediaChapter.GetEncyclopediaEntries())
			{
				if (encyclopediaEntry == null)
				{
					UberDebug.LogError($"{blueprintEncyclopediaChapter} has empty entry");
				}
				else if (string.IsNullOrEmpty(encyclopediaEntry.Blueprint.Title.Text))
				{
					UberDebug.LogError($"{blueprintEncyclopediaChapter} has element {encyclopediaEntry.Blueprint} with empty Title");
				}
				else if (!(encyclopediaEntry is IEncyclopediaPageWithAvailability { IsAvailable: false }))
				{
					m_Entries.Add(encyclopediaEntry);
					EncyclopediaNavigationElementVM item2 = new EncyclopediaNavigationElementVM(encyclopediaEntry.Blueprint).AddTo(this);
					m_ChildsVM.Add(item2);
				}
			}
		}
		m_ChildsVM.Sort((EncyclopediaNavigationElementVM p, EncyclopediaNavigationElementVM q) => string.Compare(p.Title, q.Title, StringComparison.Ordinal));
		return m_ChildsVM;
	}

	public EncyclopediaNavigationElementVM(IPage page)
	{
		Page = page;
		Title = page.GetTitle();
	}

	public void SelectPage()
	{
		EventBus.RaiseEvent(delegate(IEncyclopediaHandler x)
		{
			x.HandleEncyclopediaPage(Page);
		});
		if (!(Page is GlossaryLetterIndexPage) && Page is BlueprintEncyclopediaChapter)
		{
			m_ChildsVM.FirstOrDefault((EncyclopediaNavigationElementVM p) => p.IsAvailablePage.CurrentValue)?.SelectPage();
		}
	}

	public bool SetSelection(IPage page)
	{
		if (Page is BlueprintEncyclopediaEntry)
		{
			return m_IsSelected.Value = Page == page;
		}
		bool flag2 = Page == page;
		List<EncyclopediaNavigationElementVM> orCreateChildsVM = GetOrCreateChildsVM();
		if (!flag2)
		{
			foreach (EncyclopediaNavigationElementVM item in orCreateChildsVM)
			{
				flag2 = item.SetSelection(page) || flag2;
			}
		}
		m_IsSelected.Value = flag2;
		return flag2;
	}
}
