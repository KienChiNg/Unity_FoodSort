using UnityEngine;

namespace FoodSort
{
	[CreateAssetMenu(fileName = "FoodSO", menuName = "ScriptableObjects/FoodSO", order = 1)]
	public class FoodSO : ScriptableObject
	{
		public int typeFood;
		public string foodName;
		public float scale;
		public Sprite foodSprite;
	}
}
