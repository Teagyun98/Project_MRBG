using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Zenject;

public class AddressableManager : MonoBehaviour
{
    private UserDataManager udm;
    [Inject]
    public void Construct(UserDataManager _userDataManager) => udm = _userDataManager;

    [Header("ResourceKeys")]
    [SerializeField] private string sceneKey;

    [Header("Info")]
    [SerializeField] private string key;
    [SerializeField] private Button press;
    [SerializeField] private TextMeshProUGUI pressText;
    [SerializeField] private TextMeshProUGUI downloadText;
    [SerializeField] private Slider downloadPersent;
    [SerializeField] private GameObject warning;

    private bool download;
    private bool login;

    public Image test;
    public TextMeshProUGUI text;
    public bool testPlay;

    private void Start()
    {
        download = false;
        login = false;

        pressText.text = "Press Anywhere";
    }

    public void Press()
    {
        if (download == false)
        {
            press.gameObject.SetActive(false);

            DownLoadDependenciesAsync();
        }
        else if (login == false)
        {
            // 테스트 모드
            if(testPlay == true)
            {
                udm.SetTestData();
                login = true;
                pressText.text = "Play Game";
                press.gameObject.SetActive(true);
            }
            else
            {
                press.gameObject.SetActive(false);
                LoginAsync();
            }
        }
        else
        {
            LoadScene();
        }
    }

    // 번들 삭제 함수
    public void ClearBundle()
    {
        Addressables.ClearDependencyCacheAsync(key);

        Caching.ClearCache();

        download = false;
    }

    // 번들 안에 라벨로 묶여 있는 리소스 다운로드
    public void DownLoadDependenciesAsync()
    {
        downloadPersent.gameObject.SetActive(true);
        int fileCount = 1;
        int endCount = 0;

        Addressables.GetDownloadSizeAsync(key).Completed += (opSize) =>
        {
            // 번들의 크기
            string size = string.Concat(float.Parse((opSize.Result / Mathf.Pow(1024, 2)).ToString("N1")), "mb");

            if (opSize.Status == AsyncOperationStatus.Succeeded && opSize.Result > 0)
            {
                AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(key, true);
                downloadHandle.Completed += (opDownload) =>
                {
                    if ((opDownload).Status == AsyncOperationStatus.Succeeded)
                    {
                        // 다운로드 완료
                        endCount++;
                        UpdateDownloadUI(size, 1.0f);

                        // 다운로드가 끝나면 메모리 해제
                        Addressables.Release(downloadHandle);

                        if (fileCount == endCount)
                        {
                            pressText.text = "Login";
                            press.gameObject.SetActive(true);
                            downloadPersent.gameObject.SetActive(false);

                            download = true;

                            // Test
                            Test();
                        }
                    }
                    else
                    {
                        // 다운로드 실패
                        press.gameObject.SetActive(true);
                        downloadPersent.gameObject.SetActive(false);

                        warning.SetActive(true);

                        return;
                    }
                };

                StartCoroutine(UpdateDownloadProgress(downloadHandle, size));
            }
            else
            {
                // 이미 다운로드 완료
                endCount++;
                UpdateDownloadUI(size, 1.0f);

                if (fileCount == endCount)
                {
                    pressText.text = "Login";
                    press.gameObject.SetActive(true);
                    downloadPersent.gameObject.SetActive(false);

                    download = true;

                    // Test
                    Test();
                }
            }
        };
    }

    private void UpdateDownloadUI(string size, float percent)
    {
        float _percent = float.Parse((percent * 100f).ToString("N1"));
        downloadText.text = $"DownLoad : {size}/{_percent}%";
        downloadPersent.value = percent;
    }

    private IEnumerator UpdateDownloadProgress(AsyncOperationHandle downloadHandle, string size)
    {
        while (!downloadHandle.IsDone)
        {
            float percent = downloadHandle.PercentComplete;
            UpdateDownloadUI(size, percent);
            yield return null;
        }
    }

    // 게임 씬 로드 
    public void LoadScene()
    {
        AsyncOperationHandle handle = Addressables.LoadSceneAsync(sceneKey);

        handle.Completed += (sc) =>
        {
            if (sc.Status == AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);
            }
            else
            {
                press.gameObject.SetActive(true);
                downloadPersent.gameObject.SetActive(false);

                warning.SetActive(true);

                download = false;
            }
        };

        StartCoroutine(UpdateLoadProgress(handle));
    }

    private void UpdateLoadUI(string loadState, float percent)
    {
        downloadText.text = $"{loadState} Load : {percent}%";
        downloadPersent.value = percent / 100f;
    }

    private IEnumerator UpdateLoadProgress(AsyncOperationHandle loadHandle)
    {
        while (!loadHandle.IsDone)
        {
            float percent = loadHandle.PercentComplete;
            UpdateLoadUI("Scene", percent);
            yield return null;
        }
    }

    private void LoginAsync()
    {
        udm.SignInGPGSFirebase(task =>
        {
            if (task == true)
            {
                login = true;
                pressText.text = "Play Game";
                press.gameObject.SetActive(true);
            }
            else
            {
                udm.SetTestData();
                login = true;
                pressText.text = "Play Test Game";
                press.gameObject.SetActive(true);
            }
        });
    }

    public void Test()
    {
        // 테스트 안하면 게임 씬 넘어갈 때 리소스들이 정상적으로 로드되지 않아 Missing나는 현상 발생

        Addressables.LoadAssetAsync<Sprite>("Assets/Image/BankIcon.png").Completed += (sp) =>
        {
            if (sp.Status == AsyncOperationStatus.Succeeded)
                test.sprite = sp.Result;
        };

        Addressables.LoadAssetAsync<Material>("Assets/Material/default.mat").Completed += (ma) =>
        {
            if (ma.Status == AsyncOperationStatus.Succeeded)
                test.material = ma.Result;
        };

        Addressables.LoadAssetAsync<TMP_FontAsset>("Assets/Font/DungGeunMo SDF.asset").Completed += (ft) =>
        {
            if (ft.Status == AsyncOperationStatus.Succeeded)
                text.font = ft.Result;
        };

        //test.gameObject.SetActive(true);
    }
}
