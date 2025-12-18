using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.Framework.GameLog;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class CombatLogItemBaseView : VirtualListElementViewBase<CombatLogItemVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[SerializeField]
	private CanvasGroup m_PrefixCanvasGroup;

	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TextMeshProUGUI m_NumberText;

	[Space]
	[SerializeField]
	private VirtualListLayoutElementSettings m_VirtualListSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_VirtualListSettings;

	protected override void OnBind()
	{
		m_Text.text = base.ViewModel.MessageText;
		SetTextColor(base.ViewModel.TextColor);
		SetIcon();
	}

	protected override void OnUnbind()
	{
	}

	private void SetIcon()
	{
		Sprite icon = GameLogUtility.GetIcon((base.ViewModel.ShotNumber > 0) ? PrefixIcon.Empty : base.ViewModel.PrefixIcon);
		if (icon != null)
		{
			m_IconImage.sprite = icon;
		}
		TextMeshProUGUI numberText = m_NumberText;
		int shotNumber = base.ViewModel.ShotNumber;
		numberText.text = shotNumber.ToString();
		m_NumberText.alpha = ((base.ViewModel.ShotNumber > 0) ? 1f : 0f);
		CanvasGroup prefixCanvasGroup = m_PrefixCanvasGroup;
		PrefixIcon prefixIcon = base.ViewModel.PrefixIcon;
		prefixCanvasGroup.alpha = ((prefixIcon == PrefixIcon.None || prefixIcon == PrefixIcon.Invisible) ? 0f : 1f);
	}

	private void SetTextColor(Color color)
	{
		m_Text.color = ((color.a > 0f) ? color : ((Color)GameLogStrings.Instance.DefaultColor));
	}

	public virtual void UpdateTextSize(float multiplier)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}
}
