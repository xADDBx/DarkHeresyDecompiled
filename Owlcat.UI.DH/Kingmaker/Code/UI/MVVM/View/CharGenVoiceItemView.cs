using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenVoiceItemView : SelectionGroupEntityView<CharGenVoiceItemVM>, IFunc01ClickHandler, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_VoiceName;

	[SerializeField]
	private List<Animation> m_AudioAnimations = new List<Animation>();

	public bool CanFunc01Click()
	{
		return true;
	}

	public void OnFunc01Click()
	{
	}

	public string GetFunc01ClickHint()
	{
		return UIStrings.Instance.CharGen.PlayVoicePreview;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_VoiceName.text = base.ViewModel.DisplayName;
		base.ViewModel.IsSelected.Subscribe(delegate
		{
			PlayBarkIfSelected();
		}).AddTo(this);
	}

	protected override void OnClick()
	{
		if (UtilityNet.IsControlMainCharacter())
		{
			base.ViewModel.OnClicked?.Invoke();
		}
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value && !base.ViewModel.IsSelected.Value)
		{
			base.ViewModel.Asks.PlayPreview();
			if (UtilityNet.IsControlMainCharacter())
			{
				base.ViewModel.SetSelectedFromView(state: true);
			}
		}
	}

	private void PlayBarkIfSelected()
	{
		if (!base.ViewModel.IsSelected.Value)
		{
			foreach (Animation audioAnimation in m_AudioAnimations)
			{
				audioAnimation.gameObject.SetActive(value: false);
				audioAnimation.Stop();
			}
			return;
		}
		base.ViewModel.Asks.PlayPreview();
		foreach (Animation audioAnimation2 in m_AudioAnimations)
		{
			audioAnimation2.gameObject.SetActive(value: true);
			audioAnimation2.Stop();
			audioAnimation2.Play();
		}
	}
}
