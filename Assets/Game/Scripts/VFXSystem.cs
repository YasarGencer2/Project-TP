using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXSystem : MonoBehaviour
{
    public static VFXSystem Instance;
    [SerializeField] VFXHolder[] vfxHolders;
    [SerializeField] List<(VFXType, GameObject)> vfxPool = new List<(VFXType, GameObject)>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    GameObject GetVFX(VFXType vfxType)
    {
        foreach (var fromPool in vfxPool)
        {
            if (fromPool.Item1 == vfxType)
            {
                vfxPool.Remove(fromPool); 
                return fromPool.Item2;
            }
        }
        foreach (var fromHolder in vfxHolders)
        {
            if (fromHolder.vfxType == vfxType)
            {
                GameObject vfx = Instantiate(fromHolder.vfxPrefab);
                return vfx;
            }
        }
        return null;
    }
    public void PLayVFX(VFXType vfxType, Vector3 position)
    { 
        GameObject vfx = GetVFX(vfxType);
        if (vfx != null)
        {
            vfx.transform.position = position;
            vfx.SetActive(true);
            StartCoroutine(PoolVFX((vfxType, vfx), vfxHolders[(int)vfxType].lifeTime));
        }
    }
    IEnumerator PoolVFX((VFXType, GameObject) vfx, float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        vfx.Item2.SetActive(false);
        vfxPool.Add(vfx);
    } 
}
[System.Serializable]
public class VFXHolder
{
    public VFXType vfxType;
    public GameObject vfxPrefab;
    public float lifeTime;
    public VFXHolder(VFXType vfxType, GameObject vfxPrefab, float lifeTime)
    {
        this.vfxType = vfxType;
        this.vfxPrefab = vfxPrefab;
        this.lifeTime = lifeTime;
    } 
}
public enum VFXType
{
    Jump
}
