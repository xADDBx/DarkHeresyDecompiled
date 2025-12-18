using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class HeightFenceView : View<int>
{
	[Header("Elements")]
	[SerializeField]
	private WidgetList m_FenceContainer;

	[SerializeField]
	private GameObject m_DefaultView;

	[Header("Views")]
	[SerializeField]
	private HeightLineView m_HeightLinePrefab;

	[Header("Values")]
	[SerializeField]
	private int m_LinesCount = 11;

	protected override void OnBind()
	{
		int num = Mathf.CeilToInt((float)base.ViewModel / 5f) * 5;
		int[] array = new int[m_LinesCount];
		for (int i = 0; i < m_LinesCount; i++)
		{
			array[i] = num - 5 * i;
		}
		m_FenceContainer.DrawEntries(array, m_HeightLinePrefab).AddTo(this);
		base.gameObject.SetActive(value: true);
		m_DefaultView.SetActive(value: false);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_DefaultView.SetActive(value: true);
	}
}
