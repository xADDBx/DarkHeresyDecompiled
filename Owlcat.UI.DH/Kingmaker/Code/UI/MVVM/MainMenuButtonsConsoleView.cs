using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuButtonsConsoleView : MainMenuButtonsView<ContextMenuEntityConsoleView>, ISavesUpdatedHandler, ISubscriber
{
	[Space]
	[SerializeField]
	private HintView m_LicenseHint;

	[SerializeField]
	private OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	private OwlcatMultiButton m_SecondGlossaryFocus;

	[Header("XBox")]
	[SerializeField]
	protected GameObject m_XBoxGamerGroup;

	[SerializeField]
	protected TextMeshProUGUI m_XBoxGamerTagText;

	[SerializeField]
	protected RawImage m_XBoxGamerRawImage;

	private readonly ReactiveProperty<bool> m_InputEnabled = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasLinks = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsLinkMode = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		m_XBoxGamerGroup.gameObject.SetActive(value: false);
	}

	protected override void OnUnbind()
	{
	}

	public void OnSaveListUpdated()
	{
	}

	private void GetInputLayer()
	{
	}

	private async void OnStreamSaves()
	{
		await base.ViewModel.OnStreamSaves();
	}

	private void OnClickLink(string key)
	{
		Application.OpenURL(key);
	}

	private void OnFocusLink(string key)
	{
	}

	private void EnterLinks()
	{
		if (m_InputEnabled.Value)
		{
			m_IsLinkMode.Value = true;
		}
	}

	private void ExitLinks()
	{
		m_IsLinkMode.Value = false;
	}
}
