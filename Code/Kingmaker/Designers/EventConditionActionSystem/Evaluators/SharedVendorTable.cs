using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("424c082e5648c164ebd1000e96ef8e10")]
public class SharedVendorTable : ItemsCollectionEvaluator
{
	[SerializeField]
	[FormerlySerializedAs("SharedVendor")]
	private BlueprintSharedVendorTableReference m_SharedVendor;

	public BlueprintSharedVendorTable SharedVendor => m_SharedVendor?.Get();

	public override string GetDescription()
	{
		return $"Возвращает коллекцию итемов вендора по блюпринту {SharedVendor}";
	}

	protected override ItemsCollection GetValueInternal()
	{
		return Game.Instance.Player.VendorsManager.GetSharedCollection(SharedVendor);
	}

	public override string GetCaption()
	{
		return $"Shared vendor table {SharedVendor}";
	}
}
