using Kingmaker.View.Covers;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipCoverBlockView : View<OvertipCoverBlockVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private Image m_Icon;

	[Header("State sprites")]
	[SerializeField]
	private Sprite m_None;

	[SerializeField]
	private Sprite m_Cover;

	[SerializeField]
	private Sprite m_Invisible;

	public void HideInstant()
	{
		m_CanvasGroup.alpha = 0f;
	}

	protected override void OnBind()
	{
		base.ViewModel.IsVisibleTrigger.CombineLatest(base.ViewModel.CoverType, (bool visible, LosCalculations.CoverType coverType) => new { visible, coverType }).Subscribe(value =>
		{
			bool flag = value.visible && value.coverType != LosCalculations.CoverType.Obstacle;
			m_CanvasGroup.alpha = (flag ? 1f : 0f);
		}).AddTo(this);
		base.ViewModel.CoverType.Subscribe(delegate(LosCalculations.CoverType value)
		{
			Image icon = m_Icon;
			icon.sprite = value switch
			{
				LosCalculations.CoverType.Obstacle => m_None, 
				LosCalculations.CoverType.Cover => m_Cover, 
				LosCalculations.CoverType.LosBlocker => m_Invisible, 
				_ => m_Icon.sprite, 
			};
		}).AddTo(this);
	}
}
