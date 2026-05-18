using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickHomeworldInfoView : BrickBaseView<BrickHomeworldInfoVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_PlanetName;

	[SerializeField]
	private TMP_Text m_PlanetDescription;

	[SerializeField]
	private Image m_PlanetIcon;

	protected override void OnBind()
	{
		string text = base.ViewModel.PlanetDescription ?? string.Empty;
		int num = text.IndexOf('\n');
		if (num >= 0)
		{
			m_PlanetName.text = text.Substring(0, num);
			m_PlanetDescription.text = text.Substring(num + 1);
		}
		else
		{
			m_PlanetName.text = text;
			m_PlanetDescription.text = string.Empty;
		}
		m_PlanetIcon.sprite = base.ViewModel.Picture;
		m_PlanetIcon.gameObject.gameObject.SetActive(base.ViewModel.Picture != null);
	}
}
