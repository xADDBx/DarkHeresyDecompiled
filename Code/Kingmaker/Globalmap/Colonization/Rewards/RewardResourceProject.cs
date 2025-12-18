using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowMultipleComponents]
[TypeId("f1311382123a4b06a9954dae6bc822e3")]
public class RewardResourceProject : Reward
{
	[SerializeField]
	private ResourceData m_Resource;

	public BlueprintResource Resource => m_Resource?.Resource?.Get();

	public int Count => m_Resource?.Count ?? 0;
}
