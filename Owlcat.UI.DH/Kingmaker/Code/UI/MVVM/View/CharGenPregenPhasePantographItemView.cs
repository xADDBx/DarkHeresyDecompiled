using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPregenPhasePantographItemView : SelectionGroupEntityView<CharGenPregenSelectorItemVM>
{
	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	private ScrambledTMP m_DisplayName;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DisplayName.SetText(string.Empty, base.ViewModel.CharacterName.CurrentValue);
		m_Portrait.sprite = base.ViewModel.Portrait.CurrentValue;
		m_Portrait.gameObject.SetActive(base.ViewModel.Portrait.CurrentValue != null);
	}
}
