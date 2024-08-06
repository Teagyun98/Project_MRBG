using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceRepository", menuName = "ScriptableObject/ResourceRepository")]
public class ResourceRepository : ScriptableObject
{
    public Dictionary<string, Sprite> SpriteResource;

    public List<Sprite> Sprites;

    public void SetSpriteResource(Dictionary<string, Sprite> dic)
    {
        dic.Clear();
        Sprites = new List<Sprite>();

        SpriteResource = dic;

        foreach(Sprite sprite in dic.Values)
            Sprites.Add(sprite);
    }

    public void ReSetResource()
    {
        SpriteResource.Clear();
    }
}
