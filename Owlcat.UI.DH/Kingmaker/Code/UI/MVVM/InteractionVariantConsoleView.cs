using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InteractionVariantConsoleView : InteractionVariantView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	private HintView m_Hint;

	[SerializeField]
	private TextMeshProUGUI m_ResourcesHint;

	protected override void OnBind()
	{
		base.OnBind();
		TextMeshProUGUI resourcesHint = m_ResourcesHint;
		int? requiredResourceCount = base.ViewModel.RequiredResourceCount;
		resourcesHint.text = ((requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0) ? GetConsoleResourceHint() : string.Empty);
	}

	public void SetInputLayer()
	{
	}

	private string GetConsoleResourceHint()
	{
		UIOvertips overtips = UIStrings.Instance.Overtips;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"{overtips.HasResourceCount.Text}: {base.ViewModel.ResourceCount}\n");
		stringBuilder.Append($"{overtips.RequiredResourceCount.Text}: {base.ViewModel.RequiredResourceCount}\n");
		return stringBuilder.ToString();
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel != null;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.Interact();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
