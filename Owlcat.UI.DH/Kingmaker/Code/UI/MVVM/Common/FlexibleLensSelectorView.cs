using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.Common;

public class FlexibleLensSelectorView : View<LensSelectorVM>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton[] m_Buttons;

	[SerializeField]
	private RectTransform m_Lens;

	[Header("Values")]
	[SerializeField]
	private float m_LensSwitchAnimationDuration = 0.75f;

	[SerializeField]
	private float m_Offset;

	[Header("Fake double buttons")]
	[SerializeField]
	private bool m_HasFakeButtons;

	[SerializeField]
	[ShowIf("m_HasFakeButtons")]
	private OwlcatButton m_FakeButton;

	[SerializeField]
	[ShowIf("m_HasFakeButtons")]
	private OwlcatMultiButton m_FakeSelectingButton;

	public int CurrentTabIndex { get; private set; }

	protected override void OnBind()
	{
		if (m_Buttons.Length != 0 && !(m_Lens == null))
		{
			TryAddLensPositions();
			if (base.ViewModel.NeedToResetPosition)
			{
				ResetSelectorPosition();
			}
		}
	}

	protected override void OnUnbind()
	{
		SystemSounds.Instance.Selector.Stop.Play();
		SystemSounds.Instance.Selector.LoopStop.Play();
		CurrentTabIndex = 0;
	}

	public void ForceFocus(RectTransform buttonTransform)
	{
		SetLensPosition(buttonTransform);
	}

	private void ResetSelectorPosition()
	{
		OwlcatMultiButton[] buttons = m_Buttons;
		if (buttons != null && buttons.Length > 0)
		{
			SetLensPosition(m_Buttons[0].transform as RectTransform, withSound: false);
		}
	}

	private void TryAddLensPositions()
	{
		OwlcatMultiButton[] buttons = m_Buttons;
		if (buttons != null && buttons.Length > 0)
		{
			m_Buttons.ForEach(delegate(OwlcatMultiButton b)
			{
				ObservableSubscribeExtensions.Subscribe(b.OnLeftClickAsObservable(), delegate
				{
					SetLensPosition(b.transform as RectTransform);
				}).AddTo(this);
			});
		}
		if (m_HasFakeButtons && (bool)m_FakeButton && (bool)m_FakeSelectingButton)
		{
			ObservableSubscribeExtensions.Subscribe(m_FakeButton.OnLeftClickAsObservable(), delegate
			{
				SetLensPosition(m_FakeSelectingButton.transform as RectTransform);
			}).AddTo(this);
		}
	}

	private void SetLensPosition(RectTransform buttonTransform, bool withSound = true)
	{
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			if (!(buttonTransform == null) && !(m_Lens == null))
			{
				Vector3 localPosition = buttonTransform.localPosition;
				UIUtilityLens.MoveLensPosition(target: new Vector3(localPosition.x - m_Offset, localPosition.y, localPosition.z), lens: m_Lens, duration: m_LensSwitchAnimationDuration, withSound: withSound);
				OwlcatMultiButton[] buttons = m_Buttons;
				if (buttons != null && buttons.Length > 0)
				{
					buttons = m_Buttons;
					foreach (OwlcatMultiButton owlcatMultiButton in buttons)
					{
						if (!(owlcatMultiButton.transform != buttonTransform))
						{
							CurrentTabIndex = m_Buttons.IndexOf(owlcatMultiButton);
							break;
						}
					}
				}
			}
		}).AddTo(this);
	}

	public void ChangeTab(int index, bool withSound = true)
	{
		OwlcatMultiButton[] buttons = m_Buttons;
		if (buttons != null && buttons.Length > 0 && index <= m_Buttons.Length - 1 && index >= 0)
		{
			RectTransform buttonTransform = m_Buttons[index].transform as RectTransform;
			SetLensPosition(buttonTransform, withSound);
		}
	}

	public void SetNextTab()
	{
		OwlcatMultiButton[] buttons = m_Buttons;
		if (buttons != null && buttons.Length > 0)
		{
			int num = ((CurrentTabIndex + 1 < m_Buttons.Length) ? (CurrentTabIndex + 1) : 0);
			RectTransform buttonTransform = m_Buttons[num].transform as RectTransform;
			SetLensPosition(buttonTransform);
		}
	}

	public void SetPrevTab()
	{
		OwlcatMultiButton[] buttons = m_Buttons;
		if (buttons != null && buttons.Length > 0)
		{
			int num = ((CurrentTabIndex == 0) ? (m_Buttons.Length - 1) : (CurrentTabIndex - 1));
			RectTransform buttonTransform = m_Buttons[num].transform as RectTransform;
			SetLensPosition(buttonTransform);
		}
	}
}
