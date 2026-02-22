using System;
using UnityEngine;

public class CollisionCheck : MonoBehaviour
{
    [SerializeField] private PowerUps[] powerUps;
    private string naming;


    //for storing the values (temp)
    private int effect;
    private float duration;
    private bool pernemant;

    void OnCollisionEnter(Collision collision)
    {
        //get the name of the collision target
        naming = collision.gameObject.name;

        //reset the values
        effect = 0;
        duration = 0;
        pernemant = false;

        if (collision.gameObject.layer == 12)
        { //check for layer
            //get the name of the item
            for (int i = 0; i < powerUps.Length; i++)
            {
                if (powerUps[i].buffgetter() == naming)
                {  //get the info (temp)
                    effect = powerUps[i].effectgetter();
                    duration = powerUps[i].durationgettier();
                    pernemant = powerUps[i].pernemantgetter();

                    //change stat here and send the duration over
                    
                    if (!pernemant)
                    {
                        //count the duration if its not pernamanet
                    }

                    //remove the gameobject
                    Destroy(collision.transform.parent.gameObject);
                }
            }
        }
    }
}
