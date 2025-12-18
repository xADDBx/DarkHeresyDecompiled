using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCharInfoSkillView : CharInfoSkillPCView
{
	[SerializeField]
	private GameObject m_RecommendedMark;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.HighlightedBySource.Subscribe(delegate(bool value)
		{
			string activeLayer = (value ? "Highlighted" : "Normal");
			m_Selectable.SetActiveLayer(activeLayer);
		}).AddTo(this);
		if ((bool)m_RecommendedMark)
		{
			base.ViewModel.IsRecommended.Subscribe(delegate(bool value)
			{
				m_RecommendedMark.SetActive(value);
			}).AddTo(this);
		}
	}

	protected override void SetValues(int statValue, int previewValue, int bonus)
	{
		base.SetValues(previewValue, previewValue, bonus);
	}
}
