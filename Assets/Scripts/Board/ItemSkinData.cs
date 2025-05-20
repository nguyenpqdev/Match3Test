using UnityEngine;

[CreateAssetMenu(fileName = "ItemSkinData", menuName = "Match3/Item Skin Data")]
public class ItemSkinData : ScriptableObject
{
    [System.Serializable]
    public class ItemSkinInfo
    {
        public NormalItem.eNormalType itemType;
        public Sprite itemSprite;
    }

    public ItemSkinInfo[] itemSkins;

    public Sprite GetSpriteForType(NormalItem.eNormalType type)
    {
        foreach (var skin in itemSkins)
        {
            if (skin.itemType == type)
            {
                return skin.itemSprite;
            }
        }
        return null;
    }
}
