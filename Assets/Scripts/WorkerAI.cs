using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkerAI : MonoBehaviour
{
    [Header("설정")]
    public float moveSpeed = 3f;
    public float turnSpeed = 10f;

    [HideInInspector] public Transform ironDropZone;
    [HideInInspector] public Transform waypoint;

    private Animator anim;
    private GameObject targetOre;
    private bool hasIron = false;
    private bool isWorking = false;

    private bool hasPassedWaypoint = false;

    private static List<GameObject> targetedOres = new List<GameObject>();

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (isWorking) return;

        if (!hasIron)
        {
            if (targetOre == null || !targetOre.activeInHierarchy || !targetOre.GetComponent<Collider>().enabled)
            {
                FindNextOre();
            }

            if (targetOre != null)
            {
                if (waypoint != null && !hasPassedWaypoint)
                {
                    MoveTowards(waypoint.position);
                    if (Vector3.Distance(transform.position, waypoint.position) < 1.0f)
                    {
                        hasPassedWaypoint = true; 
                    }
                }
                else
                {
                    MoveTowards(targetOre.transform.position);
                    if (Vector3.Distance(transform.position, targetOre.transform.position) < 1.0f)
                    {
                        StartCoroutine(MineOreRoutine());
                    }
                }
            }
            else
            {
                if (anim != null) anim.SetBool("Walk", false);
            }
        }
        else
        {
            if (ironDropZone != null)
            {
                if (waypoint != null && !hasPassedWaypoint)
                {
                    MoveTowards(waypoint.position);
                    if (Vector3.Distance(transform.position, waypoint.position) < 1.0f)
                    {
                        hasPassedWaypoint = true;
                    }
                }
                else
                {
                    MoveTowards(ironDropZone.position);
                    if (Vector3.Distance(transform.position, ironDropZone.position) < 1.2f)
                    {
                        if (ironDropZone.childCount < 100)
                        {
                            DropIron();
                        }
                        else
                        {
                            if (anim != null) anim.SetBool("Walk", false);
                        }
                    }
                }
            }
        }
    }

    void MoveTowards(Vector3 targetPos)
    {
        Vector3 moveDir = targetPos - transform.position;
        moveDir.y = 0;

        if (moveDir.magnitude > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x, transform.position.y, targetPos.z), moveSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            if (anim != null) anim.SetBool("Walk", true);
        }
    }

    void FindNextOre()
    {
        GameObject[] allOres = GameObject.FindGameObjectsWithTag("Ironstone");

        var validOres = allOres.Where(o => o.activeInHierarchy &&
                                           o.GetComponent<Collider>() != null &&
                                           o.GetComponent<Collider>().enabled &&
                                           !targetedOres.Contains(o)).ToList();

        validOres = validOres.OrderByDescending(o => o.transform.position.x + o.transform.position.z).ToList();

        if (validOres.Count > 0)
        {
            targetOre = validOres[0];
            targetedOres.Add(targetOre);

            hasPassedWaypoint = false;
        }
    }

    IEnumerator MineOreRoutine()
    {
        isWorking = true;
        if (anim != null) anim.SetBool("Walk", false);
        if (anim != null) anim.SetBool("Digging_Pickaxe", true);

        yield return new WaitForSeconds(1.5f);

        if (targetOre != null)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.pickaxeHitSound, 0.7f);
            }

            foreach (MeshRenderer mr in targetOre.GetComponentsInChildren<MeshRenderer>()) mr.enabled = false;
            foreach (Collider col in targetOre.GetComponentsInChildren<Collider>()) col.enabled = false;

            targetedOres.Remove(targetOre);
            StartCoroutine(RespawnOreRoutine(targetOre, 5f));
            hasIron = true;

            hasPassedWaypoint = false;
        }

        if (anim != null) anim.SetBool("Digging_Pickaxe", false);
        isWorking = false;
    }

    IEnumerator RespawnOreRoutine(GameObject ore, float respawnTime)
    {
        yield return new WaitForSeconds(respawnTime);
        if (ore != null)
        {
            foreach (MeshRenderer mr in ore.GetComponentsInChildren<MeshRenderer>()) mr.enabled = true;
            foreach (Collider col in ore.GetComponentsInChildren<Collider>()) col.enabled = true;
        }
    }

    void DropIron()
    {
        hasIron = false;
        hasPassedWaypoint = false;

        GameObject newIron = ObjectPool.Instance.GetIronOre();
        if (newIron != null && ironDropZone != null)
        {
            newIron.transform.SetParent(ironDropZone);
            int count = ironDropZone.childCount - 1;
            newIron.transform.localPosition = new Vector3(0, 0.05f, count * -1f);
            newIron.transform.localRotation = Quaternion.identity;
            newIron.SetActive(true);

            MachineController machine = ironDropZone.GetComponentInParent<MachineController>();
            if (machine == null) machine = FindObjectOfType<MachineController>();

            if (machine != null)
            {
                machine.StartProduction(1, ironDropZone.gameObject);
            }
        }
    }

    private void OnDisable()
    {
        if (targetOre != null && targetedOres.Contains(targetOre))
        {
            targetedOres.Remove(targetOre);
        }
    }
}