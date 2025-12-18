using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[AllowMultipleComponents]
[TypeId("479ebb2441f4430fb4ff044a89ffb9ff")]
public abstract class AddStringToFact : EntityFactComponentDelegate
{
	[SerializeField]
	private LocalizedString m_AdditionalString;

	public StringJunctionType JunctionType;

	public virtual string AdditionalString => m_AdditionalString;

	public string NewString(string baseString)
	{
		return JunctionType switch
		{
			StringJunctionType.AfterSpace => baseString + " " + AdditionalString, 
			StringJunctionType.AfterNewString => baseString + "\n" + AdditionalString, 
			StringJunctionType.AfterFirstSquareBrackets => "[" + baseString + "] " + AdditionalString, 
			StringJunctionType.AfterSecondSquareBrackets => baseString + " [" + AdditionalString + "]", 
			StringJunctionType.AfterFirstRoundBrackets => "(" + baseString + ") " + AdditionalString, 
			StringJunctionType.AfterSecondRoundBrackets => baseString + " (" + AdditionalString + ")", 
			StringJunctionType.BeforeSpace => AdditionalString + " " + baseString, 
			StringJunctionType.BeforeNewString => AdditionalString + "\n" + baseString, 
			StringJunctionType.BeforeFirstSquareBrackets => "[" + AdditionalString + "] " + baseString, 
			StringJunctionType.BeforeSecondSquareBrackets => AdditionalString + " [" + baseString + "]", 
			StringJunctionType.BeforeFirstRoundBrackets => "(" + AdditionalString + ") " + baseString, 
			StringJunctionType.BeforeSecondRoundBrackets => AdditionalString + " (" + baseString + ")", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
