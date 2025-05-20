using UnityEngine;
using System.Collections.Generic;

public class ItemPool : MonoBehaviour
{
    private static ItemPool instance;
    public static ItemPool Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ItemPool");
                instance = go.AddComponent<ItemPool>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private Dictionary<string, Queue<Transform>> poolDictionary = new Dictionary<string, Queue<Transform>>();
    private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    public Transform GetItem(string prefabName)
    {
        if (!poolDictionary.ContainsKey(prefabName))
        {
            poolDictionary[prefabName] = new Queue<Transform>();
        }

        if (poolDictionary[prefabName].Count == 0)
        {
            if (!prefabDictionary.ContainsKey(prefabName))
            {
                prefabDictionary[prefabName] = Resources.Load<GameObject>(prefabName);
            }
            GameObject newItem = Instantiate(prefabDictionary[prefabName]);
            newItem.transform.SetParent(transform);
            return newItem.transform;
        }

        Transform item = poolDictionary[prefabName].Dequeue();
        item.gameObject.SetActive(true);
        item.gameObject.transform.localScale = new Vector3(1f, 1f,1f);

        return item;
    }

    public void ReturnItem(string prefabName, Transform item)
    {
        if (!poolDictionary.ContainsKey(prefabName))
        {
            poolDictionary[prefabName] = new Queue<Transform>();
        }

        item.gameObject.SetActive(false);
        item.SetParent(transform);
        poolDictionary[prefabName].Enqueue(item);
    }
} 