using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InitiativeTrackerUnitHitChanceView : View<OvertipHitChanceBlockVM>
{
	[SerializeField]
	private bool m_HasAttackParams;

	[SerializeField]
	private GameObject m_AttacksContainer;

	[SerializeField]
	private bool m_HasDefenceParams = true;

	[SerializeField]
	private GameObject m_DefenceContainer;

	[Header("Sum Hit Chance")]
	[SerializeField]
	[ShowIf("m_HasAttackParams")]
	private GameObject m_SumChanceBlock;

	[SerializeField]
	[ShowIf("m_HasAttackParams")]
	private TextMeshProUGUI m_SumChanceLabel;

	[Header("Burst")]
	[ShowIf("m_HasAttackParams")]
	[SerializeField]
	private GameObject m_BurstBlock;

	[SerializeField]
	[ShowIf("m_HasAttackParams")]
	private TextMeshProUGUI m_BurstIndexLabel;

	[Header("Hit")]
	[ShowIf("m_HasAttackParams")]
	[SerializeField]
	private GameObject m_HitBlock;

	[SerializeField]
	[ShowIf("m_HasAttackParams")]
	private TextMeshProUGUI m_HitLabel;

	[Header("Dodge")]
	[SerializeField]
	[ShowIf("m_HasDefenceParams")]
	private GameObject m_DodgeBlock;

	[SerializeField]
	[ShowIf("m_HasDefenceParams")]
	private TextMeshProUGUI m_DodgeLabel;

	[SerializeField]
	[ShowIf("m_HasDefenceParams")]
	private TextMeshProUGUI m_ParryLabel;

	[Header("Cover")]
	[SerializeField]
	[ShowIf("m_HasDefenceParams")]
	private GameObject m_CoverBlock;

	[SerializeField]
	[ShowIf("m_HasDefenceParams")]
	private TextMeshProUGUI m_CoverLabel;

	protected override void OnBind()
	{
		m_HasAttackParams = false;
		m_AttacksContainer.Or(null)?.SetActive(m_HasAttackParams);
		m_DefenceContainer.Or(null)?.SetActive(m_HasDefenceParams);
		base.ViewModel.IsVisible.CombineLatest(base.ViewModel.CanTarget, base.ViewModel.IsCaster, (bool isVisible, bool canTarget, bool isCaster) => isVisible && (canTarget || isCaster)).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(base.gameObject.SetActive)
			.AddTo(this);
		if (m_HasAttackParams)
		{
			base.ViewModel.HitChance.Subscribe(delegate(float value)
			{
				m_SumChanceBlock.gameObject.SetActive(base.ViewModel.IsCaster.CurrentValue && !base.ViewModel.HitAlways.CurrentValue);
				m_SumChanceLabel.text = UIUtilityText.GetPercentString(value);
			}).AddTo(this);
			base.ViewModel.BurstIndex.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(int val)
			{
				m_BurstBlock.gameObject.SetActive(val > 1 && base.ViewModel.IsCaster.CurrentValue);
				m_BurstIndexLabel.text = $"{base.ViewModel.BurstIndex.CurrentValue}";
			}).AddTo(this);
			base.ViewModel.InitialHitChance.Subscribe(delegate(float value)
			{
				m_HitBlock.gameObject.SetActive(base.ViewModel.IsCaster.CurrentValue && !base.ViewModel.HitAlways.CurrentValue);
				m_HitLabel.text = UIUtilityText.GetPercentString(value);
			}).AddTo(this);
		}
		if (m_HasDefenceParams)
		{
			base.ViewModel.DefenceChance.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(float val)
			{
				m_DodgeBlock.gameObject.SetActive(val > 0f);
				m_DodgeLabel.text = UIUtilityText.GetPercentString(base.ViewModel.DefenceChance.CurrentValue);
			}).AddTo(this);
			base.ViewModel.CoverChance.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate(float val)
			{
				m_CoverBlock.gameObject.SetActive(val > 0f);
				m_CoverLabel.text = UIUtilityText.GetPercentString(base.ViewModel.CoverChance.CurrentValue);
			}).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
