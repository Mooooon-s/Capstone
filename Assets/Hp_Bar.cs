using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hp_Bar : MonoBehaviour
{
    public Image hpbar;
    public Text HpText;
    // Start is called before the first frame update
    void Start()
    {
        
        hpbar = GetComponentInChildren<Image>();
        HpText = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        player_HP_Bar();
    }

    public void player_HP_Bar()
    {
        float Hp = GetComponentInParent<PlayerMovementTutorial>().currentHelth;
        hpbar.fillAmount = Hp / 100f;
        HpText.text = string.Format("HP {0}/100", Hp);
    }
}
