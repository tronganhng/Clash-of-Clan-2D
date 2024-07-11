using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxHealth(float maxheal)
    {
        slider.maxValue = maxheal;
        slider.value = maxheal;
    }

    public void SetHealth(float heal)
    {
        slider.value = heal;
    }
}
