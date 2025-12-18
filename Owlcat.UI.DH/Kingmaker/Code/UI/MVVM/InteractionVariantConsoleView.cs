using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Pointer;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InteractionVariantConsoleView : InteractionVariantView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	private ConsoleHint m_Hint;

	[SerializeField]
	private TextMeshProUGUI m_ResourcesHint;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		base.OnBind();
		TextMeshProUGUI resourcesHint = m_ResourcesHint;
		int? requiredResourceCount = base.ViewModel.RequiredResourceCount;
		resourcesHint.text = ((requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0) ? GetConsoleResourceHint() : string.Empty);
	}

	public void SetInputLayer(InputLayer inputLayer)
	{
		m_InputLayer = inputLayer;
		m_Hint.gameObject.SetActive(value: false);
		ReadOnlyReactiveProperty<bool> hintIsActive = m_Button.OnFocusAsObservable().And(ConsoleCursor.Instance.IsNotActiveProperty).ToReadOnlyReactiveProperty(initialValue: false)
			.AddTo(this);
		m_Hint.BindCustomAction(8, m_InputLayer, hintIsActive).AddTo(this);
		m_Button.OnFocusAsObservable().Subscribe(delegate
		{
			base.transform.SetAsLastSibling();
		}).AddTo(this);
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
