using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.View.Scene.Mechanics.Entities;

namespace Kingmaker.Code.View.Mechanics.Entities.Covers;

[KnowledgeDatabaseID("17adbf3bb70ce17468fd498b8f888bcc")]
[KDB("Used when a door needs to be destructible. Combines destructability of Cover and non-combat highlighting of a common MapObject")]
public class DestructibleDoorEntityView : AbstractDestructibleEntityView
{
	protected override bool HasHighlight => true;

	public override bool CanBeAttackedDirectly => true;

	protected override bool CheckHighlightConditions()
	{
		if (GlobalHighlighting && base.IsInFogOfWar)
		{
			return false;
		}
		if (m_CurrentDestructionStage == DestructionStage.Destroyed)
		{
			return false;
		}
		if (base.IsRevealed && base.InteractionConditions)
		{
			return base.IsAwarenessCheckPassed;
		}
		return false;
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new CoverEntity(UniqueId, base.IsInGameBySettings, base.Blueprint));
	}
}
