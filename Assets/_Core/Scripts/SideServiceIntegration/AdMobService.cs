using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class AdMobService : IDisposable
{
    public static readonly string TestRewardAdID = "ca-app-pub-3940256099942544/5224354917";

    private RewardedAd rewardedAd;

    public AdMobService()
    {
        MobileAds.Initialize(_ => LoadNewRewardedAd());        
    }

    private void LoadNewRewardedAd(object _ = null, EventArgs __ = null)
    {
        DisposePreviousRewardedAd();

        rewardedAd = new RewardedAd(TestRewardAdID);
        AdRequest adRequest = new AdRequest.Builder().Build();
        rewardedAd.LoadAd(adRequest);

        rewardedAd.OnUserEarnedReward += RewardTheUser;
        rewardedAd.OnAdClosed += LoadNewRewardedAd;
    }

    private void DisposePreviousRewardedAd()
    {
        if (rewardedAd == null)
            return;

        rewardedAd.OnUserEarnedReward -= RewardTheUser;
        rewardedAd.OnAdClosed -= LoadNewRewardedAd;
        rewardedAd.Destroy();
    }

    private void RewardTheUser(object _, Reward __)
    {
        Debug.Log($"You've been rewarded for watching ad");
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd.IsLoaded())
            rewardedAd.Show();
    }

    public void Dispose()
    {
        DisposePreviousRewardedAd();
    }
}
