using System;

namespace Kingmaker.Code.Framework.VO;

[Serializable]
public class VoIdField
{
	public string Guid;

	public bool Empty => string.IsNullOrEmpty(Guid);
}
