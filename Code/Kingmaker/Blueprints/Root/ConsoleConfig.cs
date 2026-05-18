using System;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class ConsoleConfig
{
	[field: SerializeField]
	public HintGamepadIconProvider HintGamepadIconProvider { get; private set; }
}
