using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HitPointManager : MonoBehaviour
{
    public float hp;
    int multiplier;
    public List<float> diffMultiplierList;
    public float diffMultiplier;
    float timer;
    public float maxTimer = 2;
    public string difficulty;

    public Image mask;
    public TextMeshProUGUI text;
    public float fillamount = 0.5f;


    public enum State
    {
        easy,
        normal,
        hard,
        difficult
    }
    public State state;

    public void Update()
    {
        fillamount = hp / 100;
        switch (state)
        {
            case State.easy:
                multiplier = 1;
                mask.color = new Color(0.53f, 1, 0.5f);
                if (hp > 90)
                {
                    if (timer < maxTimer)
                    {
                        timer += Time.deltaTime;
                    }
                    else
                    {
                        timer = 0;
                        state = State.normal;
                    }
                }
                else
                {
                    timer = 0;
                }
                break;
            case State.normal:
                multiplier = 2;
                mask.color = new Color(0.93f, 1, 0.5f);
                if (hp == 0)
                {
                    state = State.easy;
                }
                break;
            case State.hard:
                multiplier = 3;
                mask.color = new Color(1, 0.52f, 0.5f);
                if (hp == 0)
                {
                    state = State.normal;
                }
                break;
            case State.difficult:
                multiplier = 5;
                mask.color = new Color(0.85f, 0.5f, 1);
                if (hp == 0)
                {
                    state = State.hard;
                }
                break;
        }
    }

    public void HPChange(string hitType)
    {
        if (difficulty.ToLower() == "beginner")
        {
            diffMultiplier = diffMultiplierList[0];
        }
        else if (difficulty.ToLower() == "medium")
        {
            diffMultiplier = diffMultiplierList[1];
        }
        else if (difficulty.ToLower() == "hard")
        {
            diffMultiplier = diffMultiplierList[2];
        }
        else if (difficulty.ToLower() == "challenge")
        {
            diffMultiplier = diffMultiplierList[3];
        }
        if (hitType == "MARVELOUS") { hp += 2 * diffMultiplier; }
        else if (hitType == "PERFECT") { hp += 1 * diffMultiplier; }
        else if (hitType == "GREAT") { }
        else if (hitType == "GOOD") { if (hp != 0) { hp -= (1 * multiplier) * diffMultiplier; } }
        else if (hitType == "BAD") { if (hp != 0) { hp -= (3 * multiplier) * diffMultiplier; if (hp < 0) { hp = 0; } } }
        else if (hitType == "MISS") { if (hp != 0) { hp -= (4 * multiplier) * diffMultiplier; if (hp < 0) { hp = 0; } } }
    }
}
