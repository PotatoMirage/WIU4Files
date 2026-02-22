using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerUps", menuName = "Scriptable Objects/PowerUps")]
public class PowerUps : ScriptableObject
{
    [SerializeField] private float Duration = 0f; //for duration of power-up
    [SerializeField] private int effect = 0; //for amount from effect
    [SerializeField] private string buff = "test"; //for affecting which kind of stat
    [SerializeField] private bool pernemant = false; //for pernemant buffs and non pernemant ones


    //getters for the values
    public float durationgettier()
    {
        return Duration;
    }

    public int effectgetter()
    {
        return effect;
    }

    public string buffgetter()
    {
        return buff;
    }

    public bool pernemantgetter()
    {
        return pernemant;
    }
}
