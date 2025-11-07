using UnityEngine;

namespace FoodSort
{
	public class UIBuyBoosterExtra : UIBuyBooster
	{
		protected override void UseAdsBooster()
		{
			MaxMediationController.instance.ShowRewardedAd("_booster_extra", () =>
		 {
			 BoosterManager.Instance.StateExtraBooster(true);
			 BoosterManager.Instance.ExtraStoveBooster();
			 CloseUI();
		 });
		}

		protected override void UseCoinBooster()
		{
			if (GameManager.Instance.Coin >= 900)
			{
				GameManager.Instance.StateCoin(false, 900);
				BoosterManager.Instance.StateExtraBooster(true);
				BoosterManager.Instance.ExtraStoveBooster();
				CloseUI();
			}
		}
	}
}
