using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cast : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        float distance = 100;
        Gizmos.color = Color.red;

        //if(Physics.RayCast(transform.position,transform.forward,out RaycastHit hit,distance))
        //if(Physics.BoxCast(transform.position,transform.lossyScale/2,transform.forward,out RaycastHit hit,transform.rotation,distance))
        if(Physics.SphereCast(transform.position,transform.lossyScale.x/2,transform.forward,out RaycastHit hit,distance))
        {
            Gizmos.DrawRay(transform.position, transform.forward * hit.distance);
            Gizmos.DrawWireCube(transform.position + transform.forward * hit.distance, transform.lossyScale);
            //Gizmos.DrawWireSphere(transform.position + transform.forward * hit.distance, transform.lossyScale.x/2);
        }
        else
        { 
            Gizmos.DrawRay(transform.position, transform.forward * distance);
        }
    }
}
