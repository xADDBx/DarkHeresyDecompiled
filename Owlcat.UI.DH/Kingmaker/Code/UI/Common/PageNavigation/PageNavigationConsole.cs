using System.Linq;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.Common.PageNavigation;

public class PageNavigationConsole : PageNavigationBase
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_PreviousHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	private readonly CompositeDisposable m_Disposable = new CompositeDisposable();

	private readonly ReactiveProperty<bool> m_HasPrevious = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasNext = new ReactiveProperty<bool>();

	public new ReadOnlyReactiveProperty<bool> HasPrevious => m_HasPrevious;

	public void AddInput(InputLayer inputLayer, ReadOnlyReactiveProperty<bool> isActive, bool addDpad = false, bool showHints = true)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			OnPreviousClick();
		}, 14, isActive.And(m_HasPrevious).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			OnNextClick();
		}, 15, isActive.And(m_HasNext).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased);
		if (showHints)
		{
			m_Disposable.Add(m_PreviousHint.Bind(inputBindStruct));
			m_Disposable.Add(m_NextHint.Bind(inputBindStruct2));
		}
		else
		{
			m_Disposable.Add(inputBindStruct);
			m_Disposable.Add(inputBindStruct2);
		}
		if (addDpad)
		{
			m_Disposable.Add(inputLayer.AddButton(delegate
			{
				OnPreviousClick();
			}, 4, isActive.And(HasPrevious).ToReadOnlyReactiveProperty(initialValue: false)));
			m_Disposable.Add(inputLayer.AddButton(delegate
			{
				OnNextClick();
			}, 5, isActive.And(m_HasNext).ToReadOnlyReactiveProperty(initialValue: false)));
		}
	}

	public void AddInput()
	{
		if (!m_Disposable.Any())
		{
			InputLayer currentInputLayer = GamePad.Instance.CurrentInputLayer;
			m_Disposable.Add(m_PreviousHint.Bind(currentInputLayer.AddButton(delegate
			{
				OnPreviousClick();
			}, 14)));
			m_Disposable.Add(m_NextHint.Bind(currentInputLayer.AddButton(delegate
			{
				OnNextClick();
			}, 15)));
		}
	}

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
