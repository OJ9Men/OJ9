using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Racket : MonoBehaviour
{
    public void Shooting(Vector3 inputVector)
    {
        Debug.Log("[Name] : " + gameObject.name + " [Aim] : " + inputVector);

        var forceVector =
            new Vector3(-inputVector.x, -inputVector.y, 0.0f);


    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
