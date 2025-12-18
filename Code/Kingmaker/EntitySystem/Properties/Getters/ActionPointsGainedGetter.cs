using System;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("ed198672ba3d4dd0af6c9d9c17c09b9d")]
public class ActionPointsGainedGetter : IntPropertyGetter
{
	public class ContextData : ContextData<ContextData>
	{
		public int Value { get; private set; }

		public ContextData Setup(int value)
		{
			Value = value;
			return this;
		}

		protected override void Reset()
		{
			Value = 0;
		}
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Action points gained";
	}

	protected override int GetBaseValue()
	{
		return ContextData<ContextData>.Current?.Value ?? 0;
	}
}
