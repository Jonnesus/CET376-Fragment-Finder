using System.Collections;
using UnityEngine;

public class DeleteObject : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ObjectLifetime());
    }

    IEnumerator ObjectLifetime()
    {
        yield return new WaitForSeconds(1.25f);
        Destroy(gameObject);
    }
}