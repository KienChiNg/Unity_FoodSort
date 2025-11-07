using FoodSort;
using UnityEngine;

namespace FoodSort
{
	public class UIBuyBoosterBackstep : UIBuyBooster
	{
		protected override void UseAdsBooster()
		{
			MaxMediationController.instance.ShowRewardedAd("_booster_hint", () =>
			{
				BoosterManager.Instance.StateUndoBooster(true);
				CloseUI();
			});
		}

		protected override void UseCoinBooster()
		{
			if (GameManager.Instance.Coin >= 900)
			{
				GameManager.Instance.StateCoin(false, 900);
				BoosterManager.Instance.StateUndoBooster(true);
				CloseUI();
			}
		}
	}
}
