using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void setHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1.0f);
    }
}
