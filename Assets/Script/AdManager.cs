using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class AdManager : MonoBehaviour
{
    private GameManager gm;
    private UserDataManager udm;

    [Inject]
    public void Construct(GameManager _gameManager) => gm = _gameManager;
    [Inject]
    public void Construct(UserDataManager _userDataManager) => udm = _userDataManager;

    // RewardAd_Id : ca-app-pub-4040170286662606/8651314585
    private string rewardAdId = "ca-app-pub-3940256099942544/5224354917"; // 테스트 리워드 광고 ID

    private RewardedAd ad;

    private void Start()
    {
        MobileAds.Initialize((initStatus) =>
        {
            LoadRewardedAd();
        });
    }

    private void LoadRewardedAd()
    {
        if (ad != null)
        {
            ad.Destroy();
            ad = null;
        }

        AdRequest adRequest = new AdRequest();

        RewardedAd.Load(rewardAdId, adRequest, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                CustomDebug.SendLog("광고 로드 실패 : " + error);
                return;
            }

            CustomDebug.SendLog("광고 로드 성공");

            this.ad = ad;

            // 광고 로그 보내기
            RegisterEventHandlers(this.ad);
        });
    }

    public bool ViewAd(UnityAction action = null)
    {
        if (ad != null && ad.CanShowAd())
        {
            ad.Show((reward) =>
            {
                if (reward != null)
                {
                    // 보상
                    udm.GetData().AddBettingPoint(1000);
                    udm.SaveFirebaseDatabase((complete) =>
                    {
                        if (complete == false)
                        {
                            gm.ActiveKickPanel();
                        }
                    });
                    udm.SaveRanking((complete) =>
                    {
                        if (complete == false)
                        {
                            gm.ActiveKickPanel();
                        }
                    });

                    action?.Invoke();
                }
            });

            return true;
        }
        else
        {
            LoadRewardedAd();
            gm.Warning("Please try again later");
            return false;
        }
    }


    // 광고 로그 및 로드
    private void RegisterEventHandlers(RewardedAd ad)
    {
        // 광고 수익이 발생한 것으로 추정 되었을 때
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(string.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // 광고에 대한 노출
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // 광고 클릭이 기록되면
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // 광고가 전체 화면 콘텐츠를 열 때
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // 광고에 대한 전체 화면 콘텐츠를 닫을 때
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewardedAd();
        };
        // 광고가 전체 화면 콘텐츠를 열지 못했을 때
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
            LoadRewardedAd();
        };
    }
}
