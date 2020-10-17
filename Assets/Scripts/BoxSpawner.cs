using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    public GameObject box;

    Vector3 worldPosition;

    private void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane;
        worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject go = Instantiate(
                box, new Vector3(
                    worldPosition.x,
                    worldPosition.y,
                    0),
                Quaternion.identity) as GameObject;
            go.transform.localScale = new Vector2(
                Random.Range(.5f, 1f),
                Random.Range(.5f, 1f));
            Destroy(go, 3f);
        }
    }
}

