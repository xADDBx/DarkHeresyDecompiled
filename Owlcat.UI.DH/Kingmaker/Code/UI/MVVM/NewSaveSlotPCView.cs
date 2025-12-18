using Kingmaker.Blueprints.Root.Strings;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewSaveSlotPCView : SaveSlotPCView
{
	[Header("New save name")]
	[SerializeField]
	private PCInputField m_PCInputField;

	public override void DoInitialize()
	{
		UISaveLoadTexts saveLoadTexts = UIStrings.Instance.SaveLoadTexts;
		m_PCInputField.Initialize(saveLoadTexts.ClickToEdit, saveLoadTexts.SaveDefaultName);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(base.gameObject.SetActive));
		AddDisposable(m_PCInputField.Bind(base.ViewModel.SaveName.CurrentValue, base.ViewModel.TrySetSaveName));
		m_PCInputField.OnStateChanged.Subscribe(delegate(bool value)
		{
			if (value)
			{
				base.ViewModel.SetSelected(state: true);
			}
		}).AddTo(this);
	}

	public override bool IsValid()
	{
		return base.ViewModel.IsAvailable.CurrentValue;
	}
}
