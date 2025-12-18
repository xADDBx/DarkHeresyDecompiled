using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ExitLocationWindowBaseView : View<ExitLocationWindowVM>
{
	[SerializeField]
	public TextMeshProUGUI Header;

	[SerializeField]
	public TextMeshProUGUI Description;

	[SerializeField]
	public TextMeshProUGUI AdditionalInformation;

	[SerializeField]
	public TextMeshProUGUI AcceptText;

	[SerializeField]
	public TextMeshProUGUI DeclineText;

	public virtual void Initialize()
	{
		Hide();
	}

	protected override void OnBind()
	{
		Show();
		Header.text = base.ViewModel.Header;
		Description.text = base.ViewModel.Description;
		AdditionalInformation.text = base.ViewModel.AdditionalInformation;
		AcceptText.text = UIStrings.Instance.CommonTexts.Accept;
		DeclineText.text = UIStrings.Instance.CommonTexts.Cancel;
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
