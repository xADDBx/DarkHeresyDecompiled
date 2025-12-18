using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RandomGameObjectPicker : RandomPickerBase
{
	[SerializeField]
	private List<GameObject> m_Objects = new List<GameObject>();

	[SerializeField]
	private int m_ChooseCount = 1;

	public override void Randomize(string seed)
	{
		Reset();
		if (m_ChooseCount > 0 && m_Objects.Count != 0)
		{
			GetRandomIds(seed, m_ChooseCount, m_Objects.Count).ForEach(delegate(int id)
			{
				m_Objects[id].SetActive(value: true);
			});
		}
	}

	public override void Reset()
	{
		m_Objects?.ForEach(delegate(GameObject o)
		{
			o.SetActive(value: false);
		});
	}
}
