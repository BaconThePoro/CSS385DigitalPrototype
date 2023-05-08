using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditScript : MonoBehaviour
{
    // Start is called before the first frame update
    
    // Update is called once per frame
    private float speed = 10f;

    public GameObject KobeMoveUp;
    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }
}
