using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asdf : MonoBehaviour
{
    public float speed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed,GetComponent<Rigidbody2D>().velocity.y);
    }
}
