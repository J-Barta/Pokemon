using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;
    [SerializeField] Text healthNum;
    [SerializeField] Text totalHealth;

    public void SetHP(float hpNormalized, float currentHealth, float maxHealth)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1.0f);
        healthNum.text = System.Math.Round(currentHealth).ToString();
        totalHealth.text = maxHealth.ToString();
    }

    public void ResetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1.0f);
    }

    public IEnumerator SetHPSmooth(float newHp, float currentHealth, float maxHealth)
    {
        float curHp = health.transform.localScale.x;
        float changeAmt = curHp - newHp;

        while (curHp - newHp > Mathf.Epsilon)
        {
            curHp -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(curHp, 1f);
            healthNum.text = System.Math.Round(curHp * maxHealth).ToString();
            totalHealth.text = maxHealth.ToString();
            yield return null;
        }

        health.transform.localScale = new Vector3(newHp, 1.0f);

        healthNum.text = System.Math.Round(currentHealth).ToString();
        totalHealth.text = maxHealth.ToString();
    }
}
