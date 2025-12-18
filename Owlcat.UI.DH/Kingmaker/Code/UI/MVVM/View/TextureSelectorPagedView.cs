using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSelectorPagedView : TextureSelectorCommonView
{
	[SerializeField]
	private List<GameObject> m_NoSelectionsGroup;

	[SerializeField]
	private List<GameObject> m_HasSelectionsGroup;

	[SerializeField]
	private TextMeshProUGUI m_NoSelectionsWarning;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		base.gameObject.SetActive(value: true);
		m_NoSelectionsWarning.text = base.ViewModel.NoItemsDesc.CurrentValue;
	}

	protected override void OnAvailableStateChange(bool state)
	{
		m_NoSelectionsGroup.ForEach(delegate(GameObject go)
		{
			go.SetActive(!state);
		});
		m_HasSelectionsGroup.ForEach(delegate(GameObject go)
		{
			go.SetActive(state);
		});
	}
}
