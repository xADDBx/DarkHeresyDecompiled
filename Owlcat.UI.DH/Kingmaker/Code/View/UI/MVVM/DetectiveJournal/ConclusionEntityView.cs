using DG.Tweening;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionEntityView : View<ClueConclusionEntityVM>, IUIHighlighter, ISubscriber
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Name;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private Image m_Icon;

	[Header("Values")]
	[SerializeField]
	private Sprite m_DefaultIcon;

	[SerializeField]
	private bool m_HighlightAlwaysDisabled;

	public const string HighlightPrefix = "ClueScale_";

	[field: SerializeField]
	public RectTransform RectTransform { get; private set; }

	[field: SerializeField]
	public LineDirectionData LineFrom { get; private set; }

	protected override void OnBind()
	{
		m_Name.text = base.ViewModel.Name.Text;
		m_Icon.sprite = base.ViewModel.Icon ?? m_DefaultIcon;
		m_Description.text = base.ViewModel.Description.Text;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void SetHighlightAlwaysDisabled()
	{
		m_HighlightAlwaysDisabled = true;
	}

	public void StartHighlight(string key)
	{
		if (!m_HighlightAlwaysDisabled && key.StartsWith("ClueScale_"))
		{
			int length = "ClueScale_".Length;
			if (int.TryParse(key.Substring(length, key.Length - length), out var result))
			{
				float endValue = ((base.transform.GetSiblingIndex() == result) ? 1.02f : 1f);
				DOTween.Kill(base.transform);
				base.transform.DOScale(endValue, 0.2f).SetUpdate(isIndependentUpdate: true);
			}
		}
	}

	public void StopHighlight(string key)
	{
		if (!m_HighlightAlwaysDisabled && key.StartsWith("ClueScale_"))
		{
			int length = "ClueScale_".Length;
			if (int.TryParse(key.Substring(length, key.Length - length), out var _))
			{
				DOTween.Kill(base.transform);
				base.transform.DOScale(1f, 0.2f).SetUpdate(isIndependentUpdate: true);
			}
		}
	}

	public void Highlight(string key)
	{
	}

	public void HighlightOnce(string key)
	{
	}
}
