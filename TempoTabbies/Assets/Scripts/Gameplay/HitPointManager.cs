using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitPointManager : MonoBehaviour
{
    public float hp;
    public float hpMax;
    int multiplier;
    public List<int> diffMultiplierList;
    public float diffMultiplier;
    float timer;
    public float maxTimer = 2;
    public string difficulty;

    public Image mask;
    public float fillamount = 0.5f;

    public enum ClearType
    {
        Failed,
        EasyClear,
        NormalClear,
        HardClear,
        DifficultClear,
        FullCombo,
        PerfectFullCombo
    }

    public enum State
    {
        easy,
        normal,
        hard,
        difficult
    }
    public State state;

    private void Awake()
    {
        hp = hpMax;
        state = State.difficult;
    }

    public void Update()
    {
        if (hp > hpMax)
        {
            hp = hpMax;
        }
        fillamount = hp / hpMax;
        mask.fillAmount = fillamount;
        switch (state)
        {
            case State.easy:
                multiplier = 1;
                mask.color = new Color(0.53f, 1, 0.5f);
                if (hp > hpMax - (hpMax * 0.1f))
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
                    hp = hpMax * 0.7f;
                }
                break;
            case State.hard:
                multiplier = 3;
                mask.color = new Color(1, 0.52f, 0.5f);
                if (hp == 0)
                {
                    state = State.normal;
                    hpMax *= 3;
                    hp = hpMax * 0.7f;

                }
                break;
            case State.difficult:
                multiplier = 5;
                mask.color = new Color(0.85f, 0.5f, 1);
                if (hp == 0)
                {

                    state = State.hard;
                    hpMax *= 2;
                    hp = hpMax * 0.7f;
                }
                break;
        }
    }

    public ClearType GetClearType(ScoreManager scoreManager)
    {
        // Failed if HP ended at 0
        if (hp <= 0)
            return ClearType.Failed;

        bool isFullCombo =
            scoreManager.missCount == 0 &&
            scoreManager.badCount == 0;

        bool isPerfectFullCombo =
            isFullCombo &&
            scoreManager.greatCount == 0 &&
            scoreManager.goodCount == 0;

        if (isPerfectFullCombo)
            return ClearType.PerfectFullCombo;

        if (isFullCombo)
            return ClearType.FullCombo;

        // Otherwise use HP state tier
        switch (state)
        {
            case State.easy:
                return ClearType.EasyClear;

            case State.normal:
                return ClearType.NormalClear;

            case State.hard:
                return ClearType.HardClear;

            case State.difficult:
                return ClearType.DifficultClear;
        }

        return ClearType.NormalClear;
    }


    public void HPChange(string hitType)
    {
        if (diffMultiplierList.Count > 0)
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
        }
        else
        {
            diffMultiplier = 5;
        }
        if (diffMultiplier <= 0)
        {
            diffMultiplier = 5;
        }
        Debug.Log("HP:" + hp);
        if (hitType == "MARVELOUS") { hp += 0.5f * diffMultiplier; }
        else if (hitType == "PERFECT") { }
        else if (hitType == "GREAT") { if (hp != 0) { hp -= (1 * multiplier) * diffMultiplier; } }
        else if (hitType == "GOOD") { if (hp != 0) { hp -= (2 * multiplier) * diffMultiplier; } }
        else if (hitType == "BAD") { if (hp != 0) { hp -= (3 * multiplier) * diffMultiplier; if (hp < 0) { hp = 0; } } }
        else if (hitType == "MISS") { if (hp != 0) { hp -= (4 * multiplier) * diffMultiplier; if (hp < 0) { hp = 0; } } }
        else { return; }
        Debug.Log("HP after change:" + hp);
    }
}
