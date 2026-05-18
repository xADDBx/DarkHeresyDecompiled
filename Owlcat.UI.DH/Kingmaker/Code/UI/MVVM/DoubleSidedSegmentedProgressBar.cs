using Kingmaker.Code.UI.Common;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DoubleSidedSegmentedProgressBar : MonoBehaviour
{
	[SerializeField]
	private BaseProgressBar<int> m_ProgressPositive;

	[SerializeField]
	private BaseProgressBar<int> m_ProgressNegative;

	public void SetValue(int min, int max, int current)
	{
		m_ProgressPositive.SetLimits(0, max);
		m_ProgressNegative.SetLimits(0, Mathf.Abs(min));
		bool flag = current >= 0;
		m_ProgressPositive.SetValue(flag ? current : 0);
		m_ProgressNegative.SetValue((!flag) ? Mathf.Abs(current) : 0);
	}
}
