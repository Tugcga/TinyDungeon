using UnityEngine;
using UnityEditor;

namespace TD
{
	public class YourClassAsset
	{
		[MenuItem("Assets/Create/NavmeshData")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<NavmeshData>();
		}
	}
}
