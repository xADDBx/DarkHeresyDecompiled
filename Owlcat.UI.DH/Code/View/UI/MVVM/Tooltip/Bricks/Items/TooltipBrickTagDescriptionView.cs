using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class TooltipBrickTagDescriptionView : View<TooltipBrickTagDescriptionVM>
{
	[Header("Elements")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private TMP_Text m_Description;

	[Header("Values")]
	[SerializeField]
	private float m_StartWidth;

	protected override void OnBind()
	{
		m_Icon.sprite = base.ViewModel.Icon;
		string text = UIUtilityText.WrapWithWeight(base.ViewModel.TagName, TextFontWeight.SemiBold) + ": " + base.ViewModel.TagDescription;
		m_Description.text = text;
		Color bgrColor = base.ViewModel.BgrColor;
		bgrColor.a = 0.35f;
		m_Background.color = bgrColor;
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			float nameWidth = GetNameWidth(base.ViewModel.TagName.Length + 1);
			RectTransform component = m_Background.GetComponent<RectTransform>();
			component.sizeDelta = new Vector2(nameWidth + m_StartWidth, component.sizeDelta.y);
		}).AddTo(this);
	}

	private float GetNameWidth(int nameLength)
	{
		m_Description.ForceMeshUpdate();
		TMP_TextInfo textInfo = m_Description.textInfo;
		int num = Mathf.Min(nameLength - 1, textInfo.characterCount - 1);
		TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[0];
		return textInfo.characterInfo[num].xAdvance - tMP_CharacterInfo.origin;
	}
}
