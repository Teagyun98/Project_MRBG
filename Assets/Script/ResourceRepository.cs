using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceRepository", menuName = "ScriptableObject/ResourceRepository")]
public class ResourceRepository : ScriptableObject
{
    public Dictionary<string, Sprite> SpriteResource;

    public void SetSpriteResource(Dictionary<string, Sprite> dic)
    {
        SpriteResource = dic;
    }

    public void ReSetResource()
    {
        SpriteResource.Clear();
    }
}
