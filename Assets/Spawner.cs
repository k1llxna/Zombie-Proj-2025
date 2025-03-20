using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject entity;
    public Transform spawnLocation;
    public bool spawning = false;

    // Use this for initialization
    void Start()
    {
        spawning = true;
        StartCoroutine(SpawnEntity());
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.up, 45 * Time.deltaTime);
    }

    IEnumerator SpawnEntity()
    {
        while (spawning == true)
        {
            GameObject.Instantiate(entity, spawnLocation.position, Quaternion.identity);
            yield return new WaitForSeconds(2.0f);
        }
    }
}
