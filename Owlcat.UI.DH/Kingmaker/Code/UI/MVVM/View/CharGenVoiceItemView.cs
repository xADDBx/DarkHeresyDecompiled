using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
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
		PlayBarkIfSelected();
	}

	public string GetFunc01ClickHint()
	{
		return UIStrings.Instance.CharGen.PlayVoicePreview;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_VoiceName.text = base.ViewModel.DisplayName;
		PlayBarkIfSelected();
	}

	protected override void OnClick()
	{
		if (base.ViewModel.IsSelected.Value)
		{
			PlayBarkIfSelected();
			return;
		}
		if (UtilityNet.IsControlMainCharacter())
		{
			base.ViewModel.SetSelectedFromView(!base.ViewModel.IsSelected.Value);
		}
		PlayBarkIfSelected();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value && !base.ViewModel.IsSelected.Value)
		{
			base.ViewModel.Voice.PlayPreview();
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
			return;
		}
		base.ViewModel.Voice.PlayPreview();
		foreach (Animation audioAnimation in m_AudioAnimations)
		{
			audioAnimation.gameObject.SetActive(value: true);
			audioAnimation.Stop();
			audioAnimation.Play();
		}
	}
}
