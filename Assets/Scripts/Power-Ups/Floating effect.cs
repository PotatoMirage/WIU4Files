using System;
using Unity.VisualScripting;
using UnityEngine;

public class Floatingeffect : MonoBehaviour
{
    [SerializeField] private ConstantForce force;
    [SerializeField] private float timer = 10f;
    [SerializeField] private float upamount = 9.9f;
    [SerializeField] private float downamount = 9.7f;
    private float temptimer = 0f;
    private bool updir = false;

    void Update()
    {
        if (force != null)
        {
            if (temptimer > timer) //swap when the timer has reached 
            {
                if (updir)
                {
                    force.force = new Vector3(0, upamount, 0);

                    //reset the values and swap to down
                    temptimer = 0f;
                    updir = false;
                }
                else
                {
                    force.force = new Vector3(0, downamount, 0);

                    //reset the values and swap to up
                    temptimer = 0f;
                    updir = true;
                }
            }
            else
            { //increment the timer
                temptimer += Time.deltaTime;
            }
        }
    }
}
