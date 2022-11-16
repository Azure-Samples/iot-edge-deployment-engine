using System.Collections.Generic;

namespace IoTEdgeDeploymentEngine.Tools
{
	public class KeyValuePairDictionaryEqualComparer : IEqualityComparer<KeyValuePair<string, IDictionary<string, object>>>
	{
		public bool Equals(KeyValuePair<string, IDictionary<string, object>> x, KeyValuePair<string, IDictionary<string, object>> y)
		{
			if (x.Key != y.Key)
				return false;

			return true;
		}

		public int GetHashCode(KeyValuePair<string, IDictionary<string, object>> obj)
		{
			return obj.Key.GetHashCode();
		}
	}
}

