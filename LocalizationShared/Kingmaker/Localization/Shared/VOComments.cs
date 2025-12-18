using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Localization.Shared;

[Serializable]
public class VOComments
{
	[Multiline]
	[TextArea(1, 6)]
	[JsonProperty("enGB")]
	public string CommentEn;

	[Multiline]
	[TextArea(1, 6)]
	[JsonProperty("ruRU")]
	public string CommentRu;
}
