using Kingmaker.UI.Common;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.Common.PageNavigation;

public class PageNavigationConsole : PageNavigationBase
{
	[Header("Console")]
	[SerializeField]
	private HintView m_PreviousHint;

	[SerializeField]
	private HintView m_NextHint;

	private readonly CompositeDisposable m_Disposable = new CompositeDisposable();

	private readonly ReactiveProperty<bool> m_HasPrevious = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasNext = new ReactiveProperty<bool>();

	public new ReadOnlyReactiveProperty<bool> HasPrevious => m_HasPrevious;

	public void ClearInput()
	{
		m_Disposable.Clear();
	}

	protected override void OnCurrentIndexChanged(int index)
	{
		base.OnCurrentIndexChanged(index);
		m_HasPrevious.Value = base.HasPrevious;
		m_HasNext.Value = base.HasNext;
	}

	public override void Dispose()
	{
		base.Dispose();
		m_Disposable.Clear();
	}
}
