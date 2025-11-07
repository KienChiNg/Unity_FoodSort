using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FoodSort
{
    [System.Serializable]
    public class UnlockLevelFoodSO
    {
        public int lvUnlock;
        public FoodSO foodSO;
    }
    [CreateAssetMenu(fileName = "UnlockFoodSO", menuName = "ScriptableObjects/UnlockFoodSO", order = 2)]
    public class UnlockFoodSO : ScriptableObject
    {
        public List<UnlockLevelFoodSO> unlockLevelFoodSOs;
    }
}
