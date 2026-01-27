using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CatSelectionManager : MonoBehaviour
{
    // Player movement, which is sent by the PlayerScript.cs Class
    public Vector2 moveAmount;
    public float clickValue;
    public float submitValue;

    // Other scripts
    private _GameManager gameManager;
    public MenuAnimations anims;

    // UI Objects
    [SerializeField] Button cat1;
    [SerializeField] Button cat2;
    [SerializeField] Button cat3;

    // player movement timer
    bool canMove;
    float moveTimer;

    public enum Selected
    {
        cat1,
        cat2,
        cat3
    }
    public Selected selected;

    private void Awake()
    {
        WaitAFrame();

        gameManager = FindFirstObjectByType<_GameManager>();
        gameManager.state = _GameManager.GameState.CatSelect;
    }

    IEnumerator WaitAFrame()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(cat1.gameObject);
    }

    private void Update()
    {
        if (canMove && submitValue >= 0.1f)
        {
            if (gameManager.whoGetsToPlay == 0)
            {
                anims.scene = "MainMenu";
                anims.PawStB();
            }
            else
            {
                gameManager.whoGetsToPlay = 0;
            }
        }
        switch (selected)
        {
            case Selected.cat1:
                EventSystem.current.SetSelectedGameObject(cat1.gameObject);
                if (canMove && (moveAmount.y < -0.1f || moveAmount.x > 0.1f))
                {
                    selected = Selected.cat2;
                    canMove = false;
                }
                break;

            case Selected.cat2:
                EventSystem.current.SetSelectedGameObject(cat2.gameObject);
                if (clickValue > 0)
                {
                    OnCat2Click();
                }
                if (canMove && (moveAmount.y > 0.1f || moveAmount.x < -0.1f))
                {
                    selected = Selected.cat1;
                    canMove = false;
                }
                if (canMove && (moveAmount.y < -0.1f || moveAmount.x > 0.1f))
                {
                    selected = Selected.cat3;
                    canMove = false;
                }
                break;
            case Selected.cat3:
                EventSystem.current.SetSelectedGameObject(cat3.gameObject);
                if (clickValue > 0)
                {
                    OnCat3Click();
                }
                if (canMove && (moveAmount.y > 0.1f || moveAmount.x < -0.1f))
                {
                    selected = Selected.cat2;
                    canMove = false;
                }
                break;
        }
        if (!canMove)
        {
            if (moveTimer < 0.2f)
            {
                moveTimer += Time.deltaTime;
            }
            else
            {
                canMove = true;
                moveTimer = 0;
            }
        }
    }

    // the first cats click event
    public void OnCat1Click()
    {
        if (gameManager.whoGetsToPlay == 0)
        {
            gameManager.p1.cat = 1;
            if (gameManager.multiplayer == false)
            {
                anims.scene = "StageSelect";
                anims.PawStB();
            }
            else
            {
                gameManager.whoGetsToPlay = 1;
            }
        }
        else if (gameManager.whoGetsToPlay == 1)
        {
            gameManager.p2.cat = 1;
            anims.scene = "StageSelect";
            anims.PawStB();
        }
    }

    // the second cats click event
    public void OnCat2Click()
    {
        if (gameManager.whoGetsToPlay == 0)
        {
            gameManager.p1.cat = 2;
            if (gameManager.multiplayer == false)
            {
                anims.scene = "StageSelect";
                anims.PawStB();
            }
            else
            {
                gameManager.whoGetsToPlay = 1;
            }
        }
        else if (gameManager.whoGetsToPlay == 1)
        {
            gameManager.p2.cat = 2;
            anims.scene = "StageSelect";
            anims.PawStB();
        }
    }

    public void OnCat3Click()
    {
        if (gameManager.whoGetsToPlay == 0)
        {
            gameManager.p1.cat = 3;
            if (gameManager.multiplayer == false)
            {
                anims.scene = "StageSelect";
                anims.PawStB();
            }
            else
            {
                gameManager.whoGetsToPlay = 1;
            }
        }
        else if (gameManager.whoGetsToPlay == 1)
        {
            gameManager.p2.cat = 3;
            anims.scene = "StageSelect";
            anims.PawStB();
        }
    }
}

