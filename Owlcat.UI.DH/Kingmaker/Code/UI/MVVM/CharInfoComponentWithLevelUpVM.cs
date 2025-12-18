using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Photon.Realtime;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoComponentWithLevelUpVM : CharInfoComponentVM, INetLobbyPlayersHandler, ISubscriber
{
	private readonly ReactiveProperty<BaseUnitEntity> m_PreviewUnit = new ReactiveProperty<BaseUnitEntity>();

	protected readonly ReadOnlyReactiveProperty<LevelUpManager> LevelUpManager;

	private readonly ReactiveCommand<bool> m_CheckCoopControls = new ReactiveCommand<bool>();

	private readonly ReactiveProperty<bool> m_IsMainCharacter = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<BaseUnitEntity> PreviewUnit => m_PreviewUnit;

	public Observable<bool> CheckCoopControls => m_CheckCoopControls;

	public ReadOnlyReactiveProperty<bool> IsMainCharacter => m_IsMainCharacter;

	protected CharInfoComponentWithLevelUpVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager)
		: base(unit)
	{
		LevelUpManager = levelUpManager;
		m_IsMainCharacter.Value = UtilityNet.IsControlMainCharacter();
		LevelUpManager?.Subscribe(UpdatePreviewUnit).AddTo(this);
	}

	private void UpdatePreviewUnit(LevelUpManager levelUpManager)
	{
		if (levelUpManager != null && Unit.CurrentValue == levelUpManager.TargetUnit)
		{
			m_PreviewUnit.Value = levelUpManager.PreviewUnit;
		}
		else
		{
			m_PreviewUnit.Value = Unit.CurrentValue;
		}
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdatePreviewUnit(LevelUpManager?.CurrentValue);
	}

	public override void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public override void HandleDestroyLevelUpManager()
	{
	}

	public override void HandleUISelectCareerPath()
	{
		UpdatePreviewUnit(LevelUpManager?.CurrentValue);
	}

	public override void HandleUICommitChanges()
	{
	}

	public override void HandleUISelectionChanged()
	{
		UpdatePreviewUnit(LevelUpManager?.CurrentValue);
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		m_IsMainCharacter.Value = UtilityNet.IsControlMainCharacter();
		m_CheckCoopControls.Execute(UtilityNet.IsControlMainCharacter());
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}
}
