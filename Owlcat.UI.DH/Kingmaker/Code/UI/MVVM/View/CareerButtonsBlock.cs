using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerButtonsBlock : MonoBehaviour
{
	[SerializeField]
	public OwlcatButton NextButton;

	[SerializeField]
	public TextMeshProUGUI NextButtonLabel;

	[SerializeField]
	public OwlcatButton BackButton;

	[SerializeField]
	public TextMeshProUGUI BackButtonLabel;

	[SerializeField]
	public OwlcatButton FinishButton;

	[SerializeField]
	public TextMeshProUGUI FinishButtonLabel;

	public void SetActive(bool state)
	{
		base.gameObject.SetActive(state);
	}
}
