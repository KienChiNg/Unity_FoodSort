using System;

namespace FoodSort
{
	public class SkewerData
	{
		public int[] foods;
	}
	public class StoveData
	{
		public SkewerData skewerData;
	}
	public class LevelData
	{
		public StoveData[] stoveDatas;
	}
}
