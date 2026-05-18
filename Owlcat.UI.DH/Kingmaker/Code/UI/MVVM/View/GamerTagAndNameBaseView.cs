using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class GamerTagAndNameBaseView : View<GamerTagAndNameVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_PLayerName;

	[SerializeField]
	private OwlcatSelectable m_Selectable;

	private readonly ReactiveProperty<bool> m_IsFocused = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		m_PLayerName.text = string.Empty;
		base.ViewModel.Name.Subscribe(delegate(string value)
		{
			m_PLayerName.text = value;
			PFLog.Net.Log("GamerTagAndNameBaseView SET NAME " + value);
		}).AddTo(this);
	}

	public void ShowOrHide(bool state)
	{
		base.gameObject.SetActive(state);
	}

	public void SetFocus(bool value)
	{
		m_IsFocused.Value = value;
		m_Selectable.SetFocus(value);
	}

	public bool IsValid()
	{
		if (base.gameObject.activeSelf)
		{
			return !string.IsNullOrWhiteSpace(base.ViewModel.Name.CurrentValue);
		}
		return false;
	}

	public void AddGamerTagInput()
	{
	}

	public string GetUserId()
	{
		return base.ViewModel.UserId.CurrentValue;
	}
}
