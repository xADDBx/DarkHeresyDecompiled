using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class JournalOrderProfitFactorVM : ViewModel
{
	private readonly ReactiveProperty<Sprite> m_Icon = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<float> m_Count = new ReactiveProperty<float>();

	private readonly ReactiveProperty<int> m_CountAdditional = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_ArrowDirection = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_IsNegative = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<Sprite> Icon => m_Icon;

	public ReadOnlyReactiveProperty<float> Count => m_Count;

	public ReadOnlyReactiveProperty<int> CountAdditional => m_CountAdditional;

	public ReadOnlyReactiveProperty<int> ArrowDirection => m_ArrowDirection;

	public ReadOnlyReactiveProperty<bool> IsNegative => m_IsNegative;

	public JournalOrderProfitFactorVM(int arrowDirection = 0)
	{
	}

	public void UpdateCount(float count)
	{
		m_Count.Value = count;
	}

	public void SetCountAdditional(int value)
	{
		m_CountAdditional.Value = value;
	}

	public void UpdateArrowDirection(int arrowDirection = 0)
	{
		m_ArrowDirection.Value = arrowDirection;
	}

	private void OnUpdateHandler()
	{
	}
}
