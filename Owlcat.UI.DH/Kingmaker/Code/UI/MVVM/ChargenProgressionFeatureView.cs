using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChargenProgressionFeatureView : View<ChargenProgressionFeatureVM>
{
	[SerializeField]
	private TextMeshProUGUI m_TitleLeft;

	[SerializeField]
	private TextMeshProUGUI m_TitleRight;

	[SerializeField]
	private List<ChargenProgressionFeatureLevelView> m_LevelViews;

	[SerializeField]
	private TMP_FontAsset m_DefaultFont;

	[SerializeField]
	private TMP_FontAsset m_SelectedFont;

	[SerializeField]
	private Color m_DefaultColor;

	[SerializeField]
	private Color m_SelectedColor;

	protected override void OnBind()
	{
		base.OnBind();
		m_TitleLeft.text = base.ViewModel.Title;
		m_TitleRight.text = base.ViewModel.Title;
		for (int i = 1; i <= m_LevelViews.Count; i++)
		{
			BindLevelView(i);
		}
		base.ViewModel.IsHovered.Subscribe(OnHovered).AddTo(this);
		base.ViewModel.LevelUpdated.Subscribe(BindLevelView).AddTo(this);
	}

	private void OnHovered(bool value)
	{
		TextMeshProUGUI titleRight = m_TitleRight;
		TMP_FontAsset font = (m_TitleLeft.font = (value ? m_SelectedFont : m_DefaultFont));
		titleRight.font = font;
		TextMeshProUGUI titleRight2 = m_TitleRight;
		Color color2 = (m_TitleRight.color = (value ? m_SelectedColor : m_DefaultColor));
		titleRight2.color = color2;
	}

	private void BindLevelView(int index)
	{
		if (index <= m_LevelViews.Count && m_LevelViews.Count > 0 && base.ViewModel.LevelVMs.TryGetValue(index, out var value))
		{
			m_LevelViews[index - 1].gameObject.SetActive(value != null);
			m_LevelViews[index - 1].Bind(value);
		}
	}
}
