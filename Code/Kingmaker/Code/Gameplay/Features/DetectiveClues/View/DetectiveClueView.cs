using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Particles;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.Gameplay.Features.DetectiveClues.View;

[RequireComponent(typeof(DetectiveClueSignalComponent))]
[RequireComponent(typeof(InteractionDetectiveClue))]
[KnowledgeDatabaseID("c18ac95031d14830a0956e2cbc0ab70a")]
public class DetectiveClueView : MapObjectView, IEntityPositionChangedHandler, ISubscriber<IEntity>, ISubscriber, IScanTargetOverride
{
	public DetectiveClueSignalComponent m_Signal;

	public InteractionDetectiveClue m_Interaction;

	public Transform m_ScanTargetOverride;

	[FormerlySerializedAs("TurnOffAllSignalsInGroupAfterInteraction")]
	public bool TurnOffAllCluesInGroupAfterInteraction;

	public List<DetectiveClueView> TurnOffClueViewsAfterInteraction = new List<DetectiveClueView>();

	public List<DetectiveClueView> PreviousClueViews = new List<DetectiveClueView>();

	public List<DetectiveClueView> NextClueViews = new List<DetectiveClueView>();

	private GameObject m_HighlightFx;

	public DetectiveClueSignalComponent Signal
	{
		get
		{
			if (m_Signal == null)
			{
				m_Signal = GetComponent<DetectiveClueSignalComponent>();
			}
			return m_Signal;
		}
		set
		{
			m_Signal = value;
		}
	}

	public InteractionDetectiveClue Interaction
	{
		get
		{
			if (m_Interaction == null)
			{
				m_Interaction = GetComponent<InteractionDetectiveClue>();
			}
			return m_Interaction;
		}
		set
		{
			m_Interaction = value;
		}
	}

	public Transform ScanTargetOverride => m_ScanTargetOverride;

	public new DetectiveClueEntity Data => (DetectiveClueEntity)base.Data;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new DetectiveClueEntity(UniqueId, base.IsInGameBySettings));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Initialize();
	}

	protected override void OnDrawGizmos()
	{
		if (Signal.Settings.IsJammer)
		{
			Gizmos.DrawIcon(base.transform.position, "jammer.png");
		}
		else
		{
			Gizmos.DrawIcon(base.transform.position, "clue_signal.png");
		}
	}

	public void Initialize()
	{
		Signal = GetComponent<DetectiveClueSignalComponent>();
		Interaction = GetComponent<InteractionDetectiveClue>();
	}

	private void UpdateHighlightFxVisibility()
	{
		bool flag = base.IsInGame && base.IsVisible && AnyPartyCharacterNear();
		if ((flag || !(m_HighlightFx == null)) && !Interaction.Settings.IsVariative)
		{
			if (flag && m_HighlightFx == null)
			{
				DetectiveSystemRoot detectiveSystem = ConfigRoot.Instance.DetectiveSystem;
				m_HighlightFx = FxHelper.SpawnFxOnGameObject(detectiveSystem.ClueHighlightFx.Load(), base.gameObject);
			}
			else if (!flag && m_HighlightFx != null)
			{
				FxHelper.Destroy(m_HighlightFx);
				m_HighlightFx = null;
			}
		}
	}

	private bool AnyPartyCharacterNear()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (Vector3.Distance(item.Position, base.transform.position) <= Interaction.ProximityRadius)
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnVisibilityChanged()
	{
		base.OnVisibilityChanged();
		UpdateHighlightFxVisibility();
	}

	void IEntityPositionChangedHandler.HandleEntityPositionChanged()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != null && mechanicEntity.IsPlayerFaction)
		{
			UpdateHighlightFxVisibility();
		}
	}
}
