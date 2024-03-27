using System;
using System.Collections.Generic;

namespace Beamable.Theta.Common.Api.Inventory
{
	[Serializable]
	public class SaveBinaryRequest
	{
		public List<BinaryDefinition> binary;
	}
}