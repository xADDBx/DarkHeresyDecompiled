using System;
using Kingmaker.Code.UI.Common.PageNavigation;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipConsoleView : MonoBehaviour, IDisposable
{
	[SerializeField]
	private HintView m_Hint;

	[SerializeField]
	private TextMeshProUGUI m_HintLabel;

	[SerializeField]
	private PageNavigationConsole m_PageNavigation;

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	public void SetConfirmPosition(float confirmHintAnchoredY)
	{
		if (!(m_HintLabel == null))
		{
			((RectTransform)m_HintLabel.transform).anchoredPosition = new Vector2(0f, confirmHintAnchoredY);
		}
	}

	public void SetPaginatorPosition(float paginatorAnchoredY)
	{
		((RectTransform)m_PageNavigation.transform).anchoredPosition = new Vector2(0f, paginatorAnchoredY);
	}

	public void SetConfirmHint(ReadOnlyReactiveProperty<bool> isActive, string label)
	{
	}

	public void SetPaginator(bool show, bool isChosen, int surroundingsCount = 0, int surroundingIndex = -1)
	{
	}

	public void Dispose()
	{
		m_Disposable.Clear();
		m_PageNavigation.Hide();
	}
}
