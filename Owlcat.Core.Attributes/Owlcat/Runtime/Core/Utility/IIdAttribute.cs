using System;

namespace Owlcat.Runtime.Core.Utility;

public interface IIdAttribute
{
	string GuidString { get; }

	Guid Guid { get; }
}
