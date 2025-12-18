using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsMenuEntityBaseView : SelectionGroupEntityView<SettingsMenuEntityVM>
{
	[SerializeField]
	protected TMP_Text m_Title;

	[Header("Scale Settings")]
	[SerializeField]
	protected float m_MaxWidth = 225f;

	[SerializeField]
	protected float m_SetScaleWidth = 0.9f;

	[SerializeField]
	protected float m_SetCharacterSpacing = -25f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Title.Subscribe(SetText));
	}

	private void SetText(string text)
	{
		m_Title.text = text;
	}
}
