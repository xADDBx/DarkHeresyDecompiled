using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageBlockImagePCView : EncyclopediaPageBlockPCView<EncyclopediaPageBlockImageVM>
{
	[SerializeField]
	private Image m_Image;

	protected override void OnBind()
	{
		base.OnBind();
		if (base.ViewModel.Image == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		m_Image.sprite = base.ViewModel.Image;
		m_Image.color = Color.white;
	}

	protected override void OnUnbind()
	{
		m_Image.sprite = null;
		m_Image.color = new Color(0f, 0f, 0f, 0f);
		base.OnUnbind();
	}

	public override List<TextMeshProUGUI> GetLinksTexts()
	{
		return null;
	}
}
