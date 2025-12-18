using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class PersonInfoEntity : View<DetectivePersonInfo>
{
	[SerializeField]
	private GameObject m_ParentContainer;

	[SerializeField]
	private TMP_Text m_Label;

	[SerializeField]
	private TMP_Text m_Value;

	[Header("Values")]
	[SerializeField]
	private string m_DefaultValue = "???";

	[field: Header("Elements")]
	[field: SerializeField]
	public PersonInfoType Type { get; private set; }

	protected override void OnBind()
	{
		string valueStr;
		bool value = GetValue(out valueStr);
		m_ParentContainer.SetActive(value);
		UpdateLabel();
		if (value)
		{
			m_Value.text = valueStr;
		}
	}

	protected override void OnUnbind()
	{
		UpdateLabel();
		m_Value.text = m_DefaultValue;
		m_ParentContainer.SetActive(value: true);
	}

	private void UpdateLabel()
	{
		m_Label.text = UIStrings.Instance.DetectiveDecor.GetPersonInfoLabel(Type);
	}

	private bool GetValue(out string valueStr)
	{
		switch (Type)
		{
		case PersonInfoType.Height:
			valueStr = base.ViewModel.Height.ToString();
			return base.ViewModel.Height > 0;
		case PersonInfoType.Weight:
			valueStr = base.ViewModel.Weight.ToString();
			return base.ViewModel.Weight > 0;
		case PersonInfoType.Age:
			valueStr = base.ViewModel.Age.ToString();
			return base.ViewModel.Age > 0;
		case PersonInfoType.HairColor:
			valueStr = base.ViewModel.HairColor.Text;
			return base.ViewModel.HairColor.IsSet();
		case PersonInfoType.EyeColor:
			valueStr = base.ViewModel.EyesColor.Text;
			return base.ViewModel.EyesColor.IsSet();
		default:
			valueStr = null;
			return false;
		}
	}
}
