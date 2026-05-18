using Kingmaker.View.Covers;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipCoverBlockView : View<OvertipCoverBlockVM>
{
	[SerializeField]
	private GameObject m_Root;

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
		m_Root.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.ViewModel.CoverType.Subscribe(HandleCoverTypeChanged).AddTo(this);
	}

	private void HandleCoverTypeChanged(LosCalculations.CoverType? coverType)
	{
		if (!coverType.HasValue || coverType.GetValueOrDefault() == LosCalculations.CoverType.Obstacle)
		{
			m_Root.SetActive(value: false);
			return;
		}
		Image icon = m_Icon;
		icon.sprite = coverType.Value switch
		{
			LosCalculations.CoverType.Cover => m_Cover, 
			LosCalculations.CoverType.LosBlocker => m_Invisible, 
			_ => m_None, 
		};
		m_Root.SetActive(value: true);
	}
}
