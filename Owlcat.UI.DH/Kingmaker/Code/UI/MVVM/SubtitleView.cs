using DG.Tweening;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SubtitleView : View<SubtitleVM>
{
	[SerializeField]
	private TextMeshProUGUI m_SubtitleText;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	public virtual void Awake()
	{
		base.gameObject.SetActive(value: true);
		m_CanvasGroup.alpha = 0f;
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.BarkText.Subscribe(delegate(string value)
		{
			m_CanvasGroup.DOFade((value != string.Empty) ? 1 : 0, 0.2f).SetUpdate(isIndependentUpdate: true);
			m_SubtitleText.text = value;
		}).AddTo(this);
	}
}
