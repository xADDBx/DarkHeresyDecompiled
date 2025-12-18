using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class AlignmentMarkRewardVM : ViewModel, INetLobbyRequest, ISubscriber
{
	public readonly string FeatureName;

	public readonly Sprite FeatureIcon;

	public readonly TooltipBaseTemplate Tooltip;

	private readonly Action m_OnClose;

	public AlignmentMarkRewardVM(AlignmentAxis direction, int rankAdded, Action onClose)
	{
		m_OnClose = onClose;
		AlignmentAxisStaticInfo alignmentInfo = ConfigRoot.Instance.AlignmentMarksRoot.GetAlignmentInfo(direction);
		FeatureName = alignmentInfo.Name;
		FeatureIcon = alignmentInfo.Icon;
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		Tooltip = new TooltipTemplateSoulMarkFeature(mainCharacterEntity, direction, rankAdded, null);
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
	}

	public void HandleNetLobbyClose()
	{
	}

	public void OnAcceptPressed()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleOpenCharacterInfo();
		});
		m_OnClose?.Invoke();
	}

	public void OnDeclinePressed()
	{
		m_OnClose?.Invoke();
	}
}
