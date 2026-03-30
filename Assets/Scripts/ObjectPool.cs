using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [Header("Prefabs")]
    public GameObject ironOrePrefab;
    public GameObject ironIngotPrefab;
    public GameObject handcuffPrefab;

    [Header("Prisoner Prefabs")]
    public GameObject normalPrisonerPrefab;
    public GameObject jailbirdPrefab;

    [Header("Money Prefab")]
    public GameObject moneyPrefab;

    public int poolSize = 100;
    public int prisonerPoolSize = 15;

    List<GameObject> ironOrePool = new List<GameObject>();
    List<GameObject> ironIngotPool = new List<GameObject>();
    List<GameObject> handcuffPool = new List<GameObject>();

    List<GameObject> normalPrisonerPool = new List<GameObject>();
    List<GameObject> jailbirdPool = new List<GameObject>();

    List<GameObject> moneyPool = new List<GameObject>();

    private Color ironIngotNaturalColor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        MeshRenderer ingotRenderer = ironIngotPrefab.GetComponent<MeshRenderer>();
        if (ingotRenderer != null) ironIngotNaturalColor = ingotRenderer.sharedMaterial.color;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject ore = Instantiate(ironOrePrefab);
            ore.SetActive(false);
            ironOrePool.Add(ore);

            GameObject ingot = Instantiate(ironIngotPrefab);
            ingot.SetActive(false);
            ironIngotPool.Add(ingot);

            GameObject handcuff = Instantiate(handcuffPrefab);
            handcuff.SetActive(false);
            handcuffPool.Add(handcuff);

            GameObject money = Instantiate(moneyPrefab);
            money.SetActive(false);
            moneyPool.Add(money);
        }

        for (int i = 0; i < prisonerPoolSize; i++)
        {
            GameObject p1 = Instantiate(normalPrisonerPrefab);
            p1.SetActive(false);
            normalPrisonerPool.Add(p1);

            GameObject p2 = Instantiate(jailbirdPrefab);
            p2.SetActive(false);
            jailbirdPool.Add(p2);
        }
    }
    private void SetColorOfObjectMeshRenderer(GameObject obj, Color color) { 
        MeshRenderer mr = obj.GetComponent<MeshRenderer>(); 
        if (mr != null) mr.material.color = color; 
        else { 
            MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>(); 
            foreach (MeshRenderer meshRenderer in meshRenderers) meshRenderer.material.color = color; 
        } 
    }

    public GameObject GetIronOre()
    {
        foreach (GameObject obj in ironOrePool) if (!obj.activeInHierarchy) return obj;

        GameObject newObj = Instantiate(ironOrePrefab, transform);
        newObj.SetActive(false); ironOrePool.Add(newObj); return newObj;
    }

    public GameObject GetIronIngot()
    {
        foreach (GameObject obj in ironIngotPool)
        {
            if (!obj.activeInHierarchy) { SetColorOfObjectMeshRenderer(obj, ironIngotNaturalColor); return obj; }
        }
        GameObject newObj = Instantiate(ironIngotPrefab, transform);
        newObj.SetActive(false); ironIngotPool.Add(newObj);
        SetColorOfObjectMeshRenderer(newObj, ironIngotNaturalColor); return newObj;
    }

    public GameObject GetHandcuff()
    {
        foreach (GameObject obj in handcuffPool) if (!obj.activeInHierarchy) return obj;

        GameObject newObj = Instantiate(handcuffPrefab, transform);
        newObj.SetActive(false); handcuffPool.Add(newObj); return newObj;
    }

    public GameObject GetMoney()
    {
        foreach (GameObject obj in moneyPool) if (!obj.activeInHierarchy) return obj;

        GameObject newObj = Instantiate(moneyPrefab, transform);
        newObj.SetActive(false); moneyPool.Add(newObj); return newObj;
    }

    public GameObject GetNormalPrisoner()
    {
        foreach (GameObject obj in normalPrisonerPool) if (!obj.activeInHierarchy) return obj;

        GameObject newObj = Instantiate(normalPrisonerPrefab, transform);
        newObj.SetActive(false); normalPrisonerPool.Add(newObj); return newObj;
    }

    public GameObject GetJailbird()
    {
        foreach (GameObject obj in jailbirdPool) if (!obj.activeInHierarchy) return obj;

        GameObject newObj = Instantiate(jailbirdPrefab, transform);
        newObj.SetActive(false); jailbirdPool.Add(newObj); return newObj;
    }
}