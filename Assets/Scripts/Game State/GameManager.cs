using System.Collections.Generic;
using UnityEngine;


public enum GAMESTATE { 
    MENU, 
    STAGE_PHASE,
    ENDGAME
}


public class GameManager : MonoBehaviour
{

    [SerializeField] private bool DEBUG_SkipMenu;

    [SerializeField] private List<SectionManager> sectionsList; 


    [HideInInspector] public GAMESTATE currentGameState; 


    private int currentSection;


    static public GameManager Instance;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        currentSection = 0;
        if (DEBUG_SkipMenu)
            UpdateGameState(GAMESTATE.STAGE_PHASE);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateGameState(GAMESTATE newGameState)
    {
        if (newGameState == GAMESTATE.STAGE_PHASE)
        {
            bool isCurrentSectionOver = sectionsList[currentSection].IsSectionOver();

            if (isCurrentSectionOver)
            {
                currentSection++;
            }

            if (currentGameState != GAMESTATE.STAGE_PHASE)
            {
                currentGameState = GAMESTATE.STAGE_PHASE;

                sectionsList[currentSection].InitSection();
            }
        }
    }
}
