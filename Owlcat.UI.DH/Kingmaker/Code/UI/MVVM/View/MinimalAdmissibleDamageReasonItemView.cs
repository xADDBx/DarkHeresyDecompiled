using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class MinimalAdmissibleDamageReasonItemView : ViewBase<MinimalAdmissibleDamageReasonItemVM>, IWidgetView
{
	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_IconImage.color = Color.black;
		m_Text.text = base.ViewModel.Text;
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as MinimalAdmissibleDamageReasonItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is MinimalAdmissibleDamageReasonItemVM;
	}
}
