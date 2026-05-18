using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[RequireComponent(typeof(InteractionLoot))]
[KnowledgeDatabaseID("e543a12d94400d9448819b9e7206cf65")]
public class DroppedLootView : MapObjectView, IResource
{
	public ItemsCollection Loot
	{
		get
		{
			return Data.Loot;
		}
		set
		{
			Data.Loot = value;
		}
	}

	public bool IsSkinningDisabled => Data.DroppedBy.Entity == null;

	public bool IsDroppedByPlayer => Data.IsDroppedByPlayer;

	public EntityRef<Entity> DroppedBy
	{
		get
		{
			return Data.DroppedBy;
		}
		set
		{
			Data.DroppedBy = value;
		}
	}

	public new DroppedLootEntity Data => (DroppedLootEntity)base.Data;

	public override void HandleHoverChange(bool isHover)
	{
		base.HandleHoverChange(isHover);
		MassLootHelper.HighlightLoot(this, isHover);
	}

	protected override MapObjectEntity CreateMapObjectEntityData(bool load)
	{
		return Entity.Initialize(new DroppedLootEntity(this));
	}
}
