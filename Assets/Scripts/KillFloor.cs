using UnityEngine;
using System.Collections;

public class KillFloor : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("kill");
        Destroy(other.gameObject);

        GameManager.isKill = true;
    }
}
