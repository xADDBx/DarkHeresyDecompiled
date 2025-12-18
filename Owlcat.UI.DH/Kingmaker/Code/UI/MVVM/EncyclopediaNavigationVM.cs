using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaNavigationVM : ViewModel
{
	public readonly List<EncyclopediaNavigationElementVM> NavigationChapters = new List<EncyclopediaNavigationElementVM>();

	private readonly ReactiveProperty<EncyclopediaNavigationElementVM> m_SelectedChapter = new ReactiveProperty<EncyclopediaNavigationElementVM>();

	private readonly Dictionary<INode, EncyclopediaNavigationElementVM> m_Chapters = new Dictionary<INode, EncyclopediaNavigationElementVM>();

	public ReadOnlyReactiveProperty<EncyclopediaNavigationElementVM> SelectedChapter => m_SelectedChapter;

	public EncyclopediaNavigationVM()
	{
		UIConfig.Instance.ChapterList.RefreshChapters();
		foreach (BlueprintEncyclopediaChapter item in UIConfig.Instance.ChapterList.Where((BlueprintEncyclopediaChapter c) => c != null && !c.HiddenInEncyclopedia))
		{
			EncyclopediaNavigationElementVM encyclopediaNavigationElementVM = new EncyclopediaNavigationElementVM(item).AddTo(this);
			NavigationChapters.Add(encyclopediaNavigationElementVM);
			m_Chapters[item] = encyclopediaNavigationElementVM;
		}
		CheckAdditionalChapter(UIConfig.Instance.BookEventsChapter.Get());
		CheckAdditionalChapter(UIConfig.Instance.AstropathBriefsChapter.Get());
	}

	protected override void OnDispose()
	{
		NavigationChapters.ForEach(delegate(EncyclopediaNavigationElementVM slot)
		{
			slot.Dispose();
		});
		NavigationChapters.Clear();
		m_Chapters.Clear();
	}

	private void CheckAdditionalChapter(BlueprintEncyclopediaChapter chapter)
	{
		if (chapter != null && chapter.ChildPages.Dereference().Any((BlueprintEncyclopediaPage page) => page is IEncyclopediaPageWithAvailability encyclopediaPageWithAvailability && encyclopediaPageWithAvailability.IsAvailable))
		{
			EncyclopediaNavigationElementVM encyclopediaNavigationElementVM = new EncyclopediaNavigationElementVM(chapter).AddTo(this);
			NavigationChapters.Add(encyclopediaNavigationElementVM);
			m_Chapters[chapter] = encyclopediaNavigationElementVM;
		}
	}

	public void HandleEncyclopediaPage(INode page)
	{
		IPage page2 = page as IPage;
		BlueprintEncyclopediaChapter blueprintEncyclopediaChapter = page as BlueprintEncyclopediaChapter;
		if (blueprintEncyclopediaChapter == null)
		{
			blueprintEncyclopediaChapter = page2?.Parent as BlueprintEncyclopediaChapter;
		}
		if (blueprintEncyclopediaChapter != null && m_Chapters.TryGetValue(blueprintEncyclopediaChapter, out var value))
		{
			m_SelectedChapter.Value = value;
		}
		SetSelection(page2);
	}

	private void SetSelection(IPage page)
	{
		foreach (EncyclopediaNavigationElementVM value in m_Chapters.Values)
		{
			value.SetSelection(page);
		}
	}
}
