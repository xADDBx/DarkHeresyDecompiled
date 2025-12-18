using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Etude/HideFactsWhileEtudePlaying")]
[TypeId("8e007ced396cfc843810259aa527cdb9")]
public class HideFactsWhileEtudePlaying : UnitFactComponentDelegate, IHiddenFacts, IEtudesUpdateHandler, ISubscriber
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool Enabled;
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintEtudeReference m_Etude;

	[SerializeField]
	private BlueprintUnitFactReference[] m_Facts;

	public BlueprintEtude Etude => m_Etude;

	public IEnumerable<BlueprintFact> Facts => m_Facts.Select((BlueprintUnitFactReference f) => f?.Get()).NotNull();

	protected override void OnActivateOrPostLoad()
	{
		Update();
	}

	protected override void OnDeactivate()
	{
		RequestTransientData<ComponentData>().Enabled = false;
		base.Owner.GetOptional<PartHiddenFacts>()?.Remove(base.Fact, this);
	}

	public void OnEtudesUpdate()
	{
		Update();
	}

	private void Update()
	{
		bool flag = Game.Instance.EtudesSystem.Etudes.Get(Etude)?.IsPlaying ?? false;
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (flag && !componentData.Enabled)
		{
			componentData.Enabled = true;
			base.Owner.GetOrCreate<PartHiddenFacts>().Add(base.Fact, this);
		}
		else if (!flag && componentData.Enabled)
		{
			componentData.Enabled = false;
			base.Owner.GetOptional<PartHiddenFacts>()?.Remove(base.Fact, this);
		}
	}
}
