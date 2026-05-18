using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootUnitCardBaseView : View<StatCheckLootUnitCardVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_UnitNameLabel;

	[SerializeField]
	private TextMeshProUGUI m_StatNameLabel;

	[SerializeField]
	private TextMeshProUGUI m_StatValueLabel;

	[SerializeField]
	private Image m_UnitPortraitImage;

	[SerializeField]
	private OwlcatSelectable m_Selectable;

	public void Initialize()
	{
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void OnBind()
	{
		base.ViewModel.UnitName.Subscribe(delegate(string val)
		{
			m_UnitNameLabel.text = val;
		}).AddTo(this);
		base.ViewModel.UnitPortrait.Subscribe(delegate(Sprite val)
		{
			m_UnitPortraitImage.sprite = val;
		}).AddTo(this);
		base.ViewModel.StatName.Subscribe(delegate(string val)
		{
			m_StatNameLabel.text = val;
		}).AddTo(this);
		base.ViewModel.StatValue.Subscribe(delegate(int val)
		{
			m_StatValueLabel.text = val.ToString();
		}).AddTo(this);
	}

	public void SetFocus(bool value)
	{
		m_Selectable.SetFocus(value);
		base.ViewModel.SetSelected(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	protected void OnCheckStat()
	{
		base.ViewModel.CheckStat();
	}

	protected void OnSwitchUnit()
	{
		base.ViewModel.SwitchUnit();
	}
}
