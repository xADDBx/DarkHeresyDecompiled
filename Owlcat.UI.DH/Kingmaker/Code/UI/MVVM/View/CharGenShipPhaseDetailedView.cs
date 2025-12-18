using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenShipPhaseDetailedView : CharGenPhaseDetailedView<CharGenShipPhaseVM>
{
	[Header("Ship Name")]
	[SerializeField]
	protected ScrambledTMP m_ShipName;

	[Header("Description")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	[Header("Selector")]
	[SerializeField]
	protected CharGenShipPhaseSelectorView m_CharGenShipPhaseSelectorView;

	protected override void OnBind()
	{
		base.OnBind();
		m_InfoView.Bind(base.ViewModel.InfoVM);
		m_CharGenShipPhaseSelectorView.Bind(base.ViewModel.ShipSelectionGroup);
		base.ViewModel.SelectedShipEntity.Subscribe(HandleSelectedShip).AddTo(this);
		base.ViewModel.ShipName.Subscribe(delegate(string value)
		{
			m_ShipName.SetText(string.Empty, value);
		}).AddTo(this);
		base.ViewModel.InterruptHandler.Subscribe(base.ViewModel.ShowChangeNameMessageBox).AddTo(this);
	}

	protected override void OnUnbind()
	{
		TooltipHelper.HideInfo();
		base.OnUnbind();
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
	}

	protected void GenerateRandomName()
	{
		base.ViewModel.SetName(base.ViewModel.GetRandomName());
	}

	private void HandleSelectedShip(CharGenShipItemVM shipItemVM)
	{
	}
}
