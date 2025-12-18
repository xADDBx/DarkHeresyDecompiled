using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintEtude))]
[TypeId("34ed37943b30447a8d15bde729df81ee")]
public class OverrideAreaCR : EntityFactComponentDelegate, IAreaHandler, ISubscriber
{
	[SerializeField]
	private int m_NewCR;

	public int NewCR => m_NewCR;

	public void OnAreaBeginUnloading()
	{
		if (base.OwnerBlueprint is BlueprintEtude blueprintEtude)
		{
			string assetGuid = blueprintEtude.LinkedAreaPart.AssetGuid;
			if (BlueprintAreaHelper.OverridenCR.ContainsKey(assetGuid))
			{
				BlueprintAreaHelper.OverridenCR.Remove(assetGuid);
			}
		}
	}

	public void OnAreaDidLoad()
	{
		if (base.OwnerBlueprint is BlueprintEtude blueprintEtude && !BlueprintAreaHelper.OverridenCR.ContainsKey(blueprintEtude.LinkedAreaPart.AssetGuid))
		{
			BlueprintAreaHelper.OverridenCR[blueprintEtude.LinkedAreaPart.AssetGuid] = NewCR;
		}
	}
}
