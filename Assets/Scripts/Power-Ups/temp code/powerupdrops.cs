using System.Security.Cryptography;
using UnityEngine;

public class powerupdrops : MonoBehaviour
{
    [SerializeField] private GameObject[] powerup;
    private string naming;

    public void spawn()
    {
        int randomInt = Random.Range(1, 101); //random range

        //loop through so that it doesnt matter what order it is in
        for (int i = 0; i < powerup.Length; i++)
        {
            naming = powerup[i].name; //get the name of the current powerup

            //randomising for spawning
            if (randomInt < 30 && naming == "Health")
            {
                //spawn gameobject
                Instantiate(powerup[i], new Vector3(0, 1, 0), Quaternion.identity);
                break;
            }
            else if (randomInt < 45 && naming == "Damage")
            {
                //spawn gameobject
                Instantiate(powerup[1], new Vector3(0, 1, 0), Quaternion.identity);
                break;
            }
        }
    }
}
