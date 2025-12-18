using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("0398116090b795145a3f75322111e36f")]
public class SavesFixerFactReplacer : UnitFactComponentDelegate
{
	[SerializeField]
	[FormerlySerializedAs("OldFacts")]
	private BlueprintUnitFactReference[] m_OldFacts;

	[SerializeField]
	[FormerlySerializedAs("NewFacts")]
	private BlueprintUnitFactReference[] m_NewFacts;

	public ReferenceArrayProxy<BlueprintUnitFact> OldFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] oldFacts = m_OldFacts;
			return oldFacts;
		}
	}

	public ReferenceArrayProxy<BlueprintUnitFact> NewFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] newFacts = m_NewFacts;
			return newFacts;
		}
	}

	protected override void OnActivate()
	{
		foreach (BlueprintUnitFact oldFact in OldFacts)
		{
			base.Owner.Facts.Remove(oldFact);
		}
		foreach (BlueprintUnitFact newFact in NewFacts)
		{
			base.Owner.AddFact(newFact)?.AddSource(base.Fact);
		}
	}
}
