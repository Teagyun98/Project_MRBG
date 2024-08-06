using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceRepository", menuName = "ScriptableObject/ResourceRepository")]
public class ResourceRepository : ScriptableObject
{
    public Dictionary<string, Sprite> SpriteResource { get; private set; }

    public void SetSpriteResource(Dictionary<string, Sprite> dic)
    {
        SpriteResource = new Dictionary<string, Sprite>();

        SpriteResource = dic;
    }

    public void ReSetResource()
    {
        SpriteResource.Clear();
    }
}
