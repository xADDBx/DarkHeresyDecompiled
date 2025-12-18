using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EpilogCueBaseView : View<CueVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	private DialogCueColors m_DialogCueColors;

	private bool m_IsInit;

	[Header("First letter")]
	[SerializeField]
	private TMP_FontAsset m_FirstLetterFont;

	[SerializeField]
	private Material m_FirstLetterFontMaterial;

	[SerializeField]
	private Color m_FirstLetterColor = Color.black;

	[SerializeField]
	private int m_FirstLetterSize = 170;

	[SerializeField]
	private int m_FirstLetterVOffset;

	public void Initialize(DialogCueColors dialogCueColors)
	{
		if (!m_IsInit)
		{
			m_DialogCueColors = dialogCueColors;
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		Show();
		m_Text.text = UIUtilityText.GetBookFormat(base.ViewModel.RawText, m_FirstLetterFont, m_FirstLetterColor, m_FirstLetterSize, m_FirstLetterVOffset, m_FirstLetterFontMaterial);
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
