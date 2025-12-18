using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootSmallUnitCardBaseView : View<StatCheckLootSmallUnitCardVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_UnitNameLabel;

	[SerializeField]
	private TextMeshProUGUI m_StatValueLabel;

	[SerializeField]
	private Image m_UnitPortraitImage;

	[SerializeField]
	private OwlcatToggle m_SelectUnitToggle;

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
		base.ViewModel.StatValue.Subscribe(delegate(int val)
		{
			m_StatValueLabel.text = val.ToString();
		}).AddTo(this);
		base.ViewModel.IsSelected.Subscribe(delegate(bool val)
		{
			m_SelectUnitToggle.Set(val);
		}).AddTo(this);
		m_SelectUnitToggle.IsOn.Subscribe(HandleSelectUnitClick).AddTo(this);
	}

	public void SetToggleGroup(OwlcatToggleGroup toggleGroup)
	{
		m_SelectUnitToggle.Group = toggleGroup;
	}

	private void HandleSelectUnitClick(bool isSelected)
	{
		if (isSelected)
		{
			base.ViewModel.SelectUnit();
		}
	}

	public void SetFocus(bool value)
	{
		m_SelectUnitToggle.Set(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}
}
