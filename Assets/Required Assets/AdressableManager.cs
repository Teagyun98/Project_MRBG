using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AdressableManager : MonoBehaviour
{
    private AssetReference spawnablePrefab;
    [SerializeField] private List<string> keys;

    public void DownLoadDependenciesAsync()
    {
        foreach(string key in keys)
        {
            Addressables.GetDownloadSizeAsync(key).Completed += (opSize) =>
            {
                // 번들의 크기
                string size = string.Concat(opSize.Result, "byte");

                if (opSize.Status == AsyncOperationStatus.Succeeded && opSize.Result > 0)
                {
                    Addressables.DownloadDependenciesAsync(key, true).Completed += (opDownload) =>
                    {
                        // 다운로드 퍼센트
                        float percent = opDownload.PercentComplete;

                        if ((opDownload).Status != AsyncOperationStatus.Succeeded)
                            return;

                        // 다운로드 완료

                        // 다운로드가 끝나면 메모리 해제
                        Addressables.Release(key);
                    };
                }
                else
                {
                    // 이미 다운로드 완료
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

    public void CreatePrefab()
    {
        List<AsyncOperationHandle<GameObject>> handles = new List<AsyncOperationHandle<GameObject>>();

        AsyncOperationHandle<GameObject> handle = spawnablePrefab.InstantiateAsync();
        handles.Add(handle);
    }
}
