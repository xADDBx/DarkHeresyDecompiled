using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Controllers.Combat;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUD.PreciseAttack;

public class PreciseAttackBodyPartView : MonoBehaviour
{
	public enum BodyPartState
	{
		Available,
		Unavailable,
		Hidden
	}

	private const int AvailableButtonLayer = 0;

	private const int UnavailableButtonLayer = 1;

	[SerializeField]
	private Image m_VitalIcon;

	[SerializeField]
	private MonoBehaviour m_SpecialEffectIcon;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_ChanceLabel;

	[SerializeField]
	private TMP_Text m_IndexLabel;

	[SerializeField]
	private RectTransform m_LineStartRect;

	[SerializeField]
	private GameObject m_HiddenByCoverMarker;

	[SerializeField]
	private MonoBehaviour m_HiddenByCoverHintSource;

	private IDisposable m_VitalHintDisposable;

	private IDisposable m_CoverHintDisposable;

	private PreciseAttackController.BodyPartUIData m_BodyPartData;

	public OwlcatMultiButton Button => m_Button;

	public RectTransform LineStartRect => m_LineStartRect;

	public PreciseAttackController.BodyPartUIData BodyPartData => m_BodyPartData;

	public BodyPartState State { get; private set; }

	public void Initialize(PreciseAttackController.BodyPartUIData bodyPartData, string index, string label, string chance, bool isVital)
	{
		m_BodyPartData = bodyPartData;
		SetLabel(label);
		SetChance(chance);
		SetIndex(index);
		SetVitalIconActive(isVital);
		SetVitalHint(isVital);
		SetCoverIconActive(bodyPartData.IsHiddenByCover);
		SetCoverHint(bodyPartData.IsHiddenByCover);
		int num;
		if (!isVital)
		{
			LocalizedString description = bodyPartData.BodyPart.Description;
			num = ((description != null && !description.Empty) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		m_SpecialEffectIcon.gameObject.SetActive(flag);
		if (flag)
		{
			m_SpecialEffectIcon.SetHint(bodyPartData.BodyPart.Description).AddTo(this);
		}
	}

	public void SetState(BodyPartState bodyPartState)
	{
		State = bodyPartState;
		bool flag = bodyPartState != BodyPartState.Hidden;
		base.gameObject.SetActive(flag);
		if (flag)
		{
			int activeLayer = ((bodyPartState != 0) ? 1 : 0);
			m_Button.SetActiveLayer(activeLayer);
		}
	}

	public void SetFocused(bool isFocused)
	{
		m_Button.SetFocus(isFocused);
	}

	public void Clear()
	{
		SetState(BodyPartState.Hidden);
		SetLabel(null);
		SetChance(null);
		SetVitalIconActive(isActive: false);
		SetIndex(null);
		m_VitalHintDisposable?.Dispose();
	}

	private void SetLabel(string label)
	{
		m_Label.text = label;
	}

	private void SetChance(string label)
	{
		m_ChanceLabel.text = label;
	}

	private void SetIndex(string index)
	{
		m_IndexLabel.SetText(index);
	}

	private void SetVitalIconActive(bool isActive)
	{
		m_VitalIcon.gameObject.SetActive(isActive);
	}

	private void SetVitalHint(bool isVital)
	{
		m_VitalHintDisposable?.Dispose();
		if (isVital)
		{
			m_VitalHintDisposable = m_VitalIcon.SetHint(UIStrings.Instance.PreciseAttack.VitalDamageHint);
		}
	}

	private void SetCoverIconActive(bool isHiddenByCover)
	{
		m_ChanceLabel.gameObject.SetActive(!isHiddenByCover);
		m_HiddenByCoverMarker.gameObject.SetActive(isHiddenByCover);
	}

	private void SetCoverHint(bool isHiddenByCover)
	{
		m_CoverHintDisposable?.Dispose();
		if (isHiddenByCover)
		{
			m_CoverHintDisposable = m_HiddenByCoverHintSource.SetHint(UIStrings.Instance.PreciseAttack.HiddenByCoverHint);
		}
	}

	private void OnDestroy()
	{
		m_VitalHintDisposable?.Dispose();
	}
}
