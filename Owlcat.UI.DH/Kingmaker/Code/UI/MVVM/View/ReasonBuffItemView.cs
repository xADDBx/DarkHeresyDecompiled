using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class ReasonBuffItemView : ViewBase<ReasonBuffItemVM>, IWidgetView
{
	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_IconImage.sprite = base.ViewModel.Icon;
		m_IconImage.gameObject.SetActive(base.ViewModel.Icon != null);
		m_Text.text = base.ViewModel.Name;
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ReasonBuffItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ReasonBuffItemVM;
	}
}
