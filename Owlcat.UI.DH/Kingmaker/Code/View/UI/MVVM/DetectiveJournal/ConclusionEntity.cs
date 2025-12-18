using DG.Tweening;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionEntity : View<BlueprintConclusion>, IUIHighlighter, ISubscriber
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	private bool m_HighlightAlwaysDisabled;

	[field: SerializeField]
	public LineDirectionData LineFrom { get; private set; }

	[field: SerializeField]
	public LineDirectionData LineTo { get; private set; }

	public RectTransform RectTransform => GetComponent<RectTransform>();

	protected override void OnBind()
	{
		m_Description.text = base.ViewModel.Description.Text;
		EventBus.Subscribe(this).AddTo(this);
		m_StateSelectable.SetActiveLayer(base.ViewModel.IsRefuted() ? 1 : 0);
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
				base.transform.DOKill();
				base.transform.DOScale(endValue, 0.2f).SetUpdate(isIndependentUpdate: true);
			}
		}
	}

	public void StopHighlight(string key)
	{
		if (!m_HighlightAlwaysDisabled && key.StartsWith("ClueScale_"))
		{
			int length = "ClueScale_".Length;
			if (int.TryParse(key.Substring(length, key.Length - length), out var result) && base.transform.GetSiblingIndex() == result)
			{
				base.transform.DOKill();
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
