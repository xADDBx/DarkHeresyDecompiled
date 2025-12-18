using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenNameBaseView : CharInfoComponentWithLevelUpView<CharGenNameVM>
{
	[SerializeField]
	private ScrambledTMP m_NameFieldScrambled;

	[SerializeField]
	protected MessageBoxBaseView m_MessageBoxView;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.UnitName.Subscribe(delegate
		{
			SetName();
		}).AddTo(this);
		base.ViewModel.MessageBoxVM.Subscribe(m_MessageBoxView.Bind).AddTo(this);
	}

	private void SetName()
	{
		string currentValue = base.ViewModel.UnitName.CurrentValue;
		if (m_NameFieldScrambled != null && m_NameFieldScrambled.Text != currentValue)
		{
			m_NameFieldScrambled.SetText(string.Empty, currentValue);
		}
	}

	protected void GenerateRandomName()
	{
		base.ViewModel.SetName(base.ViewModel.GetRandomName());
	}
}
