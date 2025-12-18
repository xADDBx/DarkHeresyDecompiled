using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Pointer;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class UIVisibilityView : View<UIVisibilityVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private UIVisibilityFlags UIVisibilityFlag;

	public void Awake()
	{
		if (m_CanvasGroup == null)
		{
			m_CanvasGroup = GetComponent<CanvasGroup>();
		}
		m_CanvasGroup.alpha = 1f;
	}

	protected override void OnBind()
	{
		if (UIVisibilityFlag == UIVisibilityFlags.None)
		{
			UberDebug.LogError("Error: UIVisibilityFlag is None");
		}
		UIVisibilityState.VisibilityPreset.Subscribe(delegate(UIVisibilityFlags flags)
		{
			m_CanvasGroup.alpha = (flags.HasFlag(UIVisibilityFlag) ? 1f : 0f);
		}).AddTo(this);
	}
}
