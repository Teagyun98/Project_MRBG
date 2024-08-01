using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;

public class AddressTest : MonoBehaviour
{
    private Image img;

    void Start()
    {
        img = GetComponent<Image>();

        Addressables.LoadAssetAsync<Sprite>("Assets/Image/Addressable.png").Completed += AssetLoad;
    }

    void AssetLoad(AsyncOperationHandle<Sprite> aSp)
    {
        if(aSp.Status == AsyncOperationStatus.Succeeded)
        {
            img.sprite = aSp.Result;
            Debug.Log(aSp.Result.name);
        }
    }
}
