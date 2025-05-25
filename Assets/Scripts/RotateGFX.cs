using UnityEngine;

public class RotateGFX : MonoBehaviour
{
    [SerializeField] private bool freezeXZ;

    private void Update()
    {
        if (freezeXZ)
            transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        else
            transform.rotation = Camera.main.transform.rotation;
    }
}