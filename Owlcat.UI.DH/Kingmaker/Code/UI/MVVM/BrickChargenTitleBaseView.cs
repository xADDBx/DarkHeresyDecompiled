using Dreamteck;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BrickChargenTitleBaseView<T> : BrickBaseView<T> where T : BrickChargenTitleBaseVM
{
	[SerializeField]
	private TMP_Text m_DisplayName;

	[SerializeField]
	private TMP_Text m_Subname;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private RandomPickerBase[] m_RandomPickers;

	protected override void OnBind()
	{
		m_DisplayName.text = base.ViewModel.DisplayName;
		m_Subname.text = base.ViewModel.Subname;
		m_Icon.sprite = base.ViewModel.Icon;
		m_Subname.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.Subname));
		m_Icon.gameObject.SetActive(base.ViewModel.Icon != null);
		m_RandomPickers?.ForEach(delegate(RandomPickerBase rp)
		{
			rp.Randomize(base.ViewModel.DisplayName);
		});
	}
}
