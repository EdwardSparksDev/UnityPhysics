using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerDisplay : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    List<Image> lives = new List<Image>();
    #endregion

    #region SerializeField
    [Header("References")]
    [SerializeField] GameObject livesHolder;

    [Space(25), SerializeField] Image pf_Life;

    [Space(25), SerializeField] Sprite lifeFull;
    [SerializeField] Sprite lifeEmpty;
    #endregion

    #endregion


    #region Methods
    /// <summary>
    /// Updates lives diplay based on <currentLives> and <currentMaxLives>
    /// </summary>
    /// <param name="currentLives">The player current lives</param>
    /// <param name="currentMaxLives">The player current max lives</param>
    public void OnUpdateLives(int currentLives, int currentMaxLives)
    {
        if (currentMaxLives > lives.Count)
        {
            int newLives = currentMaxLives - lives.Count;
            for (int i = 0; i < newLives; i++)
            {
                Image life = Instantiate(pf_Life);
                life.transform.SetParent(livesHolder.transform);
                lives.Add(life);
            }
        }

        for (int i = 0; i < currentMaxLives; i++)
        {
            if (i < currentLives)
                lives[i].sprite = lifeFull;
            else
                lives[i].sprite = lifeEmpty;
        }
    }
    #endregion
}
