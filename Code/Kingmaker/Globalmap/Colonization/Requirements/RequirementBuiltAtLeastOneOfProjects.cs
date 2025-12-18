using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[TypeId("c36dd42905d24eebb3332d890c9fdf0c")]
public class RequirementBuiltAtLeastOneOfProjects : Requirement
{
	[SerializeField]
	private List<BlueprintColonyProjectReference> m_Projects;

	public List<BlueprintColonyProject> Projects => (from projRef in m_Projects.EmptyIfNull()
		select projRef.Get()).ToList();
}
