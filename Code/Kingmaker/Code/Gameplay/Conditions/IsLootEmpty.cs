using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Conditions;

[TypeId("a58ccd493ec743f58341ceca22fc639a")]
public class IsLootEmpty : Condition
{
	[AllowedEntityType(typeof(MapObjectView))]
	public EntityReference? LootObject;

	protected override string GetConditionCaption()
	{
		string text = LootObject?.EntityNameInEditor;
		return "Is " + (string.IsNullOrEmpty(text) ? "<no loot>" : text) + " empty";
	}

	protected override bool CheckCondition()
	{
		EntityReference? lootObject = LootObject;
		bool? obj;
		if (lootObject == null)
		{
			obj = null;
		}
		else
		{
			IEntity entity = lootObject.FindData();
			if (entity == null)
			{
				obj = null;
			}
			else
			{
				InteractionLootPart optional = entity.ToEntity().GetOptional<InteractionLootPart>();
				obj = ((optional != null) ? new bool?(optional.Loot.Items.Any()) : null);
			}
		}
		bool? flag = obj;
		return !flag.GetValueOrDefault();
	}
}
