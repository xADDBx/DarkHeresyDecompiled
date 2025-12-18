using System;
using System.Collections.Generic;

namespace Kingmaker.Code.Framework.VO;

[Serializable]
public class MismatchMapObjects
{
	public List<MismatchEntitiesEntry> Entries = new List<MismatchEntitiesEntry>();

	public List<MismatchActionEntry> Actions = new List<MismatchActionEntry>();
}
