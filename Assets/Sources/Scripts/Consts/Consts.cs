using UnityEngine;

namespace FoodSort
{
	public static class Consts
	{
		public const string GAME_NAME = "FoodSort";
		public const string SCENE_HOME = "Home";
		public const string SCENE_GAMEPLAY = "Gameplay";
		public const float SCALE_IMAGE = 4f;

		#region UI
		public const float SLIDER_WIDTH_MAX = 450f;
		public const int COIN_AFTER_WIN = 50;
		#endregion

		#region LEVEL
		public const int LEVEL_HIDDEN_START = 5;
		public const int LEVEL_HIDDEN_SPACE = 3;
		public const int LEVEL_SPECIAL_SPACE = 5;
		public const int LEVEL_RATE_US = 8;
		#endregion

		#region GRID
		public const float SPACING_STOVE_X = 0.8f;
		public const float SPACING_STOVE_Y = 1.8f;
		public const float SCALE_STOVE_DELTA = 0.1f;
		public const float SCALE_STOVE_DELTA_SPECIAL = 0.3f;
		public const int MAX_PER_ROW = 5;
		#endregion

		#region LAYER
		public const string LAYER_FOOD = "Food";
		public const string LAYER_SKEWER = "Skewer";
		public const string LAYER_STOVE = "Stove";
		#endregion

		#region FOOD
		public const int NORMAL_SKEWER_FOOD_COUNT = 4;
		public const int SPECIAL_SKEWER_FOOD_COUNT = 8;
		public const int STATUS_ONTOP = 0;
		public const int STATUS_BACK = 1;
		public const int STATUS_FLY = 2;
		public const int STATUS_UNDO = 3;
		public const int STATUS_UNDO_FLY_PLATE = 4;
		public const int MAX_FOOD_SPRITE = 29;
		#endregion

		#region SKEWER
		public const float INIT_SKEWER_LENGTH = 2f;
		public const float EXTRA_SKEWER_LENGTH = 1.15f;
		public const float FOOD_SPACING_ON_SKEWER = 0.28f;
		public const float FOOD_SPACING_ROOT = 0.3f;
		#endregion

		#region STOVE
		public const int STOVE_MAX = 15;
		public const int STOVE_MAX_SPECIAL = 15;
		#endregion

		#region PLAYERPREFS
		public const string LEVEL_START = "LevelSave_start_fs";
		public const string LEVEL_SAVE = "LevelSave_fs";
		public const string LEVEL_SAVE_SPECIAL = "LevelSaveSpecial_fs";
		public const string LEVEL_NEW_FOOD = "LevelNewFood_fs";
		public const string MUSIC_FOODSORT = "Music_fs";
		public const string SOUND_FOODSORT = "Sound_fs";
		public const string HAPTIC_FOODSORT = "Haptic_fs";
		public const string UNDO_FOODSORT = "Undo_fs";
		public const string EXTRA_FOODSORT = "Extra_fs";
		public const string COIN_FOODSORT = "Coin_fs";
		public const string LEVEL_SPECIAL = "Level_Special_fs";
		#endregion
	}
}
