using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using TMPro;


public class GameController : MonoBehaviour
{
    public CanvasGroup buttonPanel;
    public CanvasGroup pausePanel;
    public CanvasGroup endGamePanel;
    public Button button, buttonPause;
    public Character[] playerCharacter;
    public Character[] enemyCharacter;
    Character currentTarget;
    bool waitingForInput;
    public TextMeshProUGUI view;
    
    

    Character FirstAliveCharacter(Character[] characters)
    {
        // LINQ: return enemyCharacter.FirstOrDefault(x => !x.IsDead());
        foreach (var character in characters) {
            if (!character.IsDead())
                return character;
        }
        return null;
    }

    void PlayerWon()
    {
        Utility.SetCanvasGroupEnabled(buttonPanel, false);
        Utility.SetCanvasGroupEnabled(pausePanel, false);
        Utility.SetCanvasGroupEnabled(endGamePanel, true);
        
        view = endGamePanel.GetComponentInChildren<TextMeshProUGUI>();
        view.text = "Player won.";
    }

    void PlayerLost()
    {
        Utility.SetCanvasGroupEnabled(buttonPanel, false);
        Utility.SetCanvasGroupEnabled(pausePanel, false);
        Utility.SetCanvasGroupEnabled(endGamePanel, true);

        view = endGamePanel.GetComponentInChildren<TextMeshProUGUI>();
        view.text = "Player lost.";
    }

    bool CheckEndGame()
    {
        if (FirstAliveCharacter(playerCharacter) == null) {
            PlayerLost();
            return true;
        }

        if (FirstAliveCharacter(enemyCharacter) == null) {
            PlayerWon();
            return true;
        }

        return false;
    }

    void PlayerAttack()
    {
        waitingForInput = false;
    }

    public void NextTarget()
    {
        int index = Array.IndexOf(enemyCharacter, currentTarget);
        for (int i = 1; i < enemyCharacter.Length; i++) {
            int next = (index + i) % enemyCharacter.Length;
            if (!enemyCharacter[next].IsDead()) {
                currentTarget.targetIndicator.gameObject.SetActive(false);
                currentTarget = enemyCharacter[next];
                currentTarget.targetIndicator.gameObject.SetActive(true);
                return;
            }
        }
    }

    public void Pause()
    {
        Utility.SetCanvasGroupEnabled(buttonPanel, false);
        Utility.SetCanvasGroupEnabled(pausePanel, true);
    }

    public void Continue()
    {
        Utility.SetCanvasGroupEnabled(buttonPanel, true);
        Utility.SetCanvasGroupEnabled(pausePanel, false);
    }


    public void Restart()
    {
        string curentScene = SceneManager.GetActiveScene().name;
        LoadingScreen.instance.LoadScene(curentScene);
    }


    public void MainMenu()
    {
        LoadingScreen.instance.LoadScene("MainMenu");
    }


    IEnumerator GameLoop()
    {
        yield return null;
        while (!CheckEndGame()) {
            foreach (var player in playerCharacter) {
                if (!player.IsDead()) 
                {
                    if (currentTarget == null || currentTarget.IsDead())
                        currentTarget = FirstAliveCharacter(enemyCharacter);
                    
                    if (currentTarget == null)
                        break;

                    currentTarget.targetIndicator.gameObject.SetActive(true);
                    Utility.SetCanvasGroupEnabled(buttonPanel, true);

                    waitingForInput = true;
                    while (waitingForInput)
                        yield return null;

                    Utility.SetCanvasGroupEnabled(buttonPanel, false);
                    currentTarget.targetIndicator.gameObject.SetActive(false);

                    player.target = currentTarget.transform;
                    player.AttackEnemy();

                    while (!player.IsIdle())
                        yield return null;

                    break;
                }
            }

            foreach (var enemy in enemyCharacter) {
                if (!enemy.IsDead()) {
                    Character target = FirstAliveCharacter(playerCharacter);
                    if (target == null)
                        break;

                    enemy.target = target.transform;
                    enemy.AttackEnemy();

                    while (!enemy.IsIdle())
                        yield return null;

                    break;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(PlayerAttack);
        
        Utility.SetCanvasGroupEnabled(buttonPanel, false);
        Utility.SetCanvasGroupEnabled(pausePanel, false);
        StartCoroutine(GameLoop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
