using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public GameObject Destroy_obj;
    public CanvasGroup canvasGroup;
    public HealBar healbar;
    public float MaxHeal, currentHeal, hurt_cooldown = 2.5f, healbar_duration = 1.3f;
    public float delayDestroy = 1.35f;
    float cnt_time;
    public bool in_hurt;
    Animator ani;

    private void Start() 
    {
        
        cnt_time = hurt_cooldown;
        canvasGroup.alpha = 0;
        //StartCoroutine(SetCurrentHealth());
        currentHeal = MaxHeal;
        healbar.SetMaxHealth(currentHeal);
        ani = transform.parent.GetComponent<Animator>();
    }

    void Update()
    {
        if(in_hurt) // cập nhật trạng thái đang bị tấn công
        {
            canvasGroup.alpha = 1f;
            cnt_time -= Time.deltaTime;
            if(cnt_time <= 0) in_hurt = false;
        }
        else
        {
            // biến mất thanh máu nếu ko bị atk
            canvasGroup.alpha -= (Time.deltaTime/healbar_duration);
            if (canvasGroup.alpha <= 0) canvasGroup.alpha = 0;
        }
    }

    IEnumerator SetCurrentHealth()
    {
        yield return null;
        currentHeal = MaxHeal;
        healbar.SetMaxHealth(currentHeal);
    }    
    public void TakeDame(float dame)
    {
        currentHeal -= dame;
        healbar.SetHealth(currentHeal); 
        canvasGroup.alpha = 1;
        in_hurt = true;
        cnt_time = hurt_cooldown;
        if(currentHeal <= 0f){
            Die();
        }
        else ani.SetTrigger("hurt");
    }

    void Die()
    {
        ani.SetTrigger("dead");  // ani die
        if(Destroy_obj) Destroy(Destroy_obj, delayDestroy);
        else Destroy(transform.parent.gameObject, delayDestroy);
    }
}
