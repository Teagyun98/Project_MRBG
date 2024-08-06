using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AddressableManager : MonoBehaviour
{
    [Header("ResourceKeys")]
    [SerializeField] private List<string> spriteKeys;

    [Header("Info")]
    [SerializeField] private List<string> keys;
    [SerializeField] private Button press;
    [SerializeField] private TextMeshProUGUI downloadText;
    [SerializeField] private Slider downloadPersent;
    [SerializeField] private GameObject warning;

    [Header("Repository")]
    [SerializeField] private ResourceRepository repository;

    private bool download;

    private void Start()
    {
        download = false;
    }

    public void Press()
    {
        if (download == false)
        {
            press.gameObject.SetActive(false);

            DownLoadDependenciesAsync();
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    public void DownLoadDependenciesAsync()
    {
        downloadPersent.gameObject.SetActive(true);
        int fileCount = keys.Count;
        int endCount = 0;

        foreach (string key in keys)
        {
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
                                LoadSprite();
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
                        LoadSprite();
                }
            };
        }
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

    // 미리 사용할 모든 리소스를 로드한다.
    public void LoadSprite()
    {
        Dictionary<string, Sprite> dicSprite = new Dictionary<string, Sprite>();

        foreach(string key in spriteKeys)
        {
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(key);

            handle.Completed += (sp) => 
            {
                if (sp.Status == AsyncOperationStatus.Succeeded)
                {
                    dicSprite.Add(key, sp.Result);

                    UpdateLoadUI("Image", spriteKeys.Count, dicSprite.Count);

                    if (dicSprite.Count == spriteKeys.Count)
                    {
                        // 로드한 리소스 전달
                        repository.SetSpriteResource(dicSprite);

                        press.gameObject.SetActive(true);
                        downloadPersent.gameObject.SetActive(false);

                        download = true;
                    }

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
        }
    }

    private void UpdateLoadUI(string loadState, int fileCount, int nowLoad)
    {
        downloadText.text = $"{loadState} Load : {nowLoad}/{fileCount}%";
        downloadPersent.value = (float)nowLoad / fileCount;
    }
}
