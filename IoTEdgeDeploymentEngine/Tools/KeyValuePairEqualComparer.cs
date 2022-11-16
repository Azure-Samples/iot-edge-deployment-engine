using System.Collections.Generic;

namespace IoTEdgeDeploymentEngine.Tools
{
	public class KeyValuePairEqualComparer : IEqualityComparer<KeyValuePair<string, object>>
	{
		public bool Equals(KeyValuePair<string, object> x, KeyValuePair<string, object> y)
		{
			if (x.Key != y.Key)
				return false;

			return true;
		}

		public int GetHashCode(KeyValuePair<string, object> obj)
		{
			return obj.Key.GetHashCode();
		}
	}
}

