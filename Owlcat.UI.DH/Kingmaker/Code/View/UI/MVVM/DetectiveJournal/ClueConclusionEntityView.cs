using DG.Tweening;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClueConclusionEntityView : DetectiveJournalClueView, IUIHighlighter, ISubscriber
{
	[SerializeField]
	private bool m_HighlightAlwaysDisabled;

	[field: SerializeField]
	public LineDirectionData LineFrom { get; private set; }

	protected override void OnBind()
	{
		base.OnBind();
		m_StateSelectable.SetActiveLayer("Default");
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_HighlightAlwaysDisabled = false;
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
