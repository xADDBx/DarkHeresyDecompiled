using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTitleView : BrickBaseView<BrickTitleVM>
{
	[SerializeField]
	private List<TitleElement> m_Titles;

	private void Awake()
	{
		foreach (TitleElement title in m_Titles)
		{
			title.Container.SetActive(value: false);
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		int type = (int)base.ViewModel.Type;
		for (int i = 0; i < m_Titles.Count; i++)
		{
			if (m_Titles[i].Container != null)
			{
				m_Titles[i].Container.SetActive(type == i);
			}
		}
		if (m_Titles.Count > type)
		{
			TitleElement titleElement = m_Titles[type];
			titleElement.Text.Bind(base.ViewModel.Title);
			if (titleElement.LayoutGroup != null)
			{
				titleElement.LayoutGroup.childAlignment = base.ViewModel.TextAnchor;
			}
		}
	}
}
