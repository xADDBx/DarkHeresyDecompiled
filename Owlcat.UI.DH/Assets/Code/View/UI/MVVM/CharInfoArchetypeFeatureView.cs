using Kingmaker.Code.UI.MVVM;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoArchetypeFeatureView : View<CharInfoArchetypeFeatureVM>
{
	[SerializeField]
	private TextMeshProUGUI m_SpecializationName;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private GameObject m_BackupImage;

	[SerializeField]
	private OwlcatSelectable m_Selectable;

	protected override void OnBind()
	{
		base.OnBind();
		m_SpecializationName.text = base.ViewModel.Name;
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.gameObject.SetActive(base.ViewModel.Icon != null);
		m_BackupImage?.SetActive(base.ViewModel.Icon == null);
		if (base.ViewModel.Tooltip != null)
		{
			m_Selectable.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		}
	}
}
