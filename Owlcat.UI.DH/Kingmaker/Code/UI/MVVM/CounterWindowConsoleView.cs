using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CounterWindowConsoleView : CounterWindowPCView
{
	[Header("ConsoleInput")]
	[SerializeField]
	private HintView m_AcceptHint;

	[SerializeField]
	private HintView m_CloseHint;

	private void CreateInput()
	{
	}

	private void OnLeftStickX(float x)
	{
		float num = ((Mathf.Abs(x) > 0.1f) ? Mathf.Sign(x) : 0f);
		ChangeValue((int)num);
	}

	private void ChangeValue(int value)
	{
		m_CountSlider.value += value;
	}

	private int GetMediumShiftAmount()
	{
		return Mathf.Min((base.ViewModel.MaxValue - 1) / 2, 10);
	}
}
