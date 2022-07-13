using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollCubeMovement : MonoBehaviour
{
    public BothClient UDPC;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


        {
            this.gameObject.transform.position = UDPC.newPos;
        }
    }
}
