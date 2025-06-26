using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatOnScreen : MonoBehaviour
{
    public float timer;
    float time = 0;
    // Start is called before the first frame update
    void Start()
    {
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > timer) Destroy(gameObject);
        Debug.Log("aaaaaaaaaaaaaa");
    }
}
