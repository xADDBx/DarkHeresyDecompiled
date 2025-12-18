using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

[Obsolete]
[AllowedOn(typeof(BlueprintStarSystemObject))]
[TypeId("fbf24398eea844fab0cfa40166a129c2")]
public class ScanStarSystemObjectActions : BlueprintComponent
{
	[SerializeField]
	public ConditionsChecker Conditions;

	[SerializeField]
	public ActionList AdditionalActions;
}
