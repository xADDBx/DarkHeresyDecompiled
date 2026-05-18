using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[Serializable]
[AllowedOn(typeof(BlueprintItem))]
[TypeId("aefd066e685a4a6e8dafe361a9242ccd")]
public sealed class ItemUIInteraction : BlueprintComponent
{
	[SerializeField]
	private ConditionsChecker _conditions = new ConditionsChecker();

	[SerializeField]
	private ItemUIInteractionType _type;

	[SerializeField]
	[InfoBox("Юнит, взаимодейтвующий с предметом, доступен через эвалюатор InteractingUnit")]
	private ActionList _actions = new ActionList();

	public ItemUIInteractionType Type => _type;

	public bool CanInteract([NotNull] BaseUnitEntity user)
	{
		using (ContextData<InteractingUnitData>.Request().Setup(user))
		{
			return _conditions.Check();
		}
	}

	public void Interact([NotNull] ItemEntity item, [NotNull] BaseUnitEntity user)
	{
		using (ContextData<InteractingUnitData>.Request().Setup(user))
		{
			_actions.Run();
		}
	}
}
