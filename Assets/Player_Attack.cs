using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player_Attack : MonoBehaviourPunCallbacks
{
    public int Damage=10;
    public float rate;
    public BoxCollider meleeRange;
    public Animator anima;
    void start()
   {
        anima = GetComponentInParent<Animator>();
   }



    public void Attack()
    {
        StopCoroutine("attack");
        StartCoroutine("attack");
    }
       
    public IEnumerator attack()
    {
        yield return new WaitForSeconds(0.5f);
        meleeRange.enabled=true;
        yield return new WaitForSeconds(0.4f);
        meleeRange.enabled = false;
        anima.SetBool("isAttack", false);
        yield return new WaitForSeconds(0.5f);
        yield break;
    }
}
