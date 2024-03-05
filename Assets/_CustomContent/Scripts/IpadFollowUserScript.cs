using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IpadFollowUserScript : MonoBehaviour
{
    public Transform head;

    public float Snappiness = 1f;
    public float SpeedMove = 1f;
    public float MinDistance = 0.01f;
    public float offsetPos;

    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = this.head.position + this.head.forward * offsetPos;
        this.transform.LookAt(head, -head.transform.right);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(this.transform.position, this.head.position + this.head.forward * offsetPos)>MinDistance){
            this.transform.position = Vector3.Lerp(this.transform.position, this.head.position+ this.head.forward*offsetPos, SpeedMove*Time.deltaTime);
        }else{
            this.transform.position = this.head.position+ this.head.forward*offsetPos;
        }
        this.transform.LookAt(head, -head.transform.right);
    }
}
