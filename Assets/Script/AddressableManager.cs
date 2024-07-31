using System;
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
    [SerializeField] private List<string> keys;
    [SerializeField] private Button press;
    [SerializeField] private TextMeshProUGUI downloadText;
    [SerializeField] private Slider downloadPersent;
    [SerializeField] private GameObject warning;

    private bool download;

    private void Start()
    {
        download = false;
    }

    public void Press()
    {
        if(download == false)
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
                string size = string.Concat(opSize.Result, "byte");

                if (opSize.Status == AsyncOperationStatus.Succeeded && opSize.Result > 0)
                {
                    AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(key, true);
                    downloadHandle.Completed += (opDownload) =>
                    {
                        if ((opDownload).Status == AsyncOperationStatus.Succeeded)
                        {
                            // 다운로드 완료
                            endCount++;
                            Debug.Log("다운 완료");
                            UpdateDownloadUI(fileCount, endCount, size, 1.0f);

                            // 다운로드가 끝나면 메모리 해제
                            Addressables.Release(downloadHandle);

                            if (fileCount == endCount)
                            {
                                press.gameObject.SetActive(true);
                                downloadPersent.gameObject.SetActive(false);

                                download = true;
                            }
                        }
                        else
                        {
                            // 다운로드 실패
                            press.gameObject.SetActive(true);
                            downloadPersent.gameObject.SetActive(false);

                            download = false;

                            return;
                        }

                        StartCoroutine(UpdateDownloadProgress(downloadHandle, fileCount, endCount, size));
                    };
                }
                else
                {
                    // 이미 다운로드 완료
                    endCount++;
                    UpdateDownloadUI(fileCount, endCount, size, 1.0f);

                    if (fileCount == endCount)
                    {
                        press.gameObject.SetActive(true);
                        downloadPersent.gameObject.SetActive(false);

                        download = true;
                    }
                }
            };
        }
    }

    public void LoadAssetAsync(object key)
    {
        try
        {
            Addressables.LoadAssetAsync<GameObject>(key).Completed += (op) =>
            {
                if (op.Status != AsyncOperationStatus.Succeeded)
                    return;

                // 로드 완료
            };
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void UpdateDownloadUI(int fileCount, int endCount, string size, float percent)
    {
        downloadText.text = $"{fileCount}/{endCount} : {size}/{percent * 100}%";
        downloadPersent.value = percent;
    }

    private IEnumerator UpdateDownloadProgress(AsyncOperationHandle downloadHandle, int fileCount, int endCount, string size)
    {
        while (!downloadHandle.IsDone)
        {
            float percent = downloadHandle.PercentComplete;
            UpdateDownloadUI(fileCount, endCount, size, percent);
            yield return null;
        }
    }
}
