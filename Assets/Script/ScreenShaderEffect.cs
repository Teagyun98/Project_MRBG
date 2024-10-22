using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenShaderEffect : MonoBehaviour
{
    [SerializeField] private Material mat;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(mat != null)
            Graphics.Blit(source, destination, mat);
        else
            Graphics.Blit(source, destination);
    }
}
