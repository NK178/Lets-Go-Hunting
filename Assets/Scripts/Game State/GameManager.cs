using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//based on current state player is in 
public enum GAMESTATE { 
    MENU, 
    STAGE_PHASE,
    ENDGAME
}

//a bit wonky, abit out of data so idk what da hell is going on 19/4 
public class GameManager : MonoBehaviour
{

    [SerializeField] private bool DEBUG_SkipMenu;

    private List<SectionManager> sectionsInSceneList;
    [SerializeField] private float loadSectionWaitTime; 
    [HideInInspector] public GAMESTATE currentGameState;

    //For now, I guess I will keep a linear strucutre, maybe in the future I will develop branching paths 
    //Since i not doing some complicated nonsense, strings will surfice for now, I can steal other methods in the future 
    [SerializeField] private List<string> sceneOrderList;
    private int currentSceneIndex;


    private int currentSection;


    private bool isSectionActive; 

    static public GameManager Instance = null;


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


        isSectionActive = false;
        currentSection = 0;

        sectionsInSceneList = new List<SectionManager>();


        if (DEBUG_SkipMenu)
        {
            SceneManager.LoadScene("MainScene");
        }

            //UpdateGameState(GAMESTATE.STAGE_PHASE);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //void UpdateGameState(GAMESTATE newGameState)
    //{
    //    if (newGameState == GAMESTATE.STAGE_PHASE)
    //    {
    //        bool isCurrentSectionOver = sectionsInSceneList[currentSection].IsSectionOver();

    //        if (isCurrentSectionOver)
    //        {
    //            currentSection++;
    //        }

    //        if (currentGameState != GAMESTATE.STAGE_PHASE)
    //        {
    //            currentGameState = GAMESTATE.STAGE_PHASE;

    //            sectionsInSceneList[currentSection].InitSection();
    //        }
    //    }                             
    //}


    public void RegisterSection(SectionManager section)
    {
        foreach (SectionManager sect in sectionsInSceneList)
        {
            if (section == sect)
                return; 
        }   

        Debug.Log("INITALIZED: " + section.gameObject.name);
        sectionsInSceneList.Add(section);
    }


    public SectionManager GetCurrentSectionManager()
    {
        if (currentSection >= sectionsInSceneList.Count)
            return null;
        else
            return sectionsInSceneList[currentSection];
    }

    //need to load sections in this way 
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this != Instance) return;
        Debug.Log("Entered Scene: " + scene.name);

        //if (scene.name != "MainMenu")
        //{
        //    StartCoroutine(LoadSectionCoroutine());
        //}
    }
     

    
    //does not mark the end of a level, but of a scene 
    public void RecieveEndSceneSignal(GAMESIGNAL gameSignal)
    {
        if (!gameSignal.ToString().Contains("END"))
            return;

        Debug.Log("END GAME");

        //safety check, shld not happen tho 
        foreach (SectionManager section in sectionsInSceneList)
        {
            if (!section.IsSectionOver())
            {
                Debug.Log("SECTION: " + section.gameObject.name + " NOT OVER");
                return; 
            }
        }

        currentSceneIndex++;
        if (currentSceneIndex == sceneOrderList.Count)
        {
            Debug.Log("SCENE INDEX OUT OF RANGE AT " + currentSceneIndex);
            return; 
        }
        string nextScene = sceneOrderList[currentSceneIndex];

        //just debug loading 
        SceneManager.LoadScene(nextScene);

    }

    //private IEnumerator LoadSectionCoroutine()
    //{
    //    yield return new WaitForSeconds(loadSectionWaitTime);

    //    StartCoroutine(DEBUG_WaitToStartSection());

    //    //SectionManager[] tempArray = new SectionManager[sectionsInSceneList.Count]; 
    //    //for (int i = 0; i < sectionsInSceneList.Count; i++)
    //    //{
    //    //    tempArray[i] = sectionsInSceneList[i];  
    //    //}
    //    //MergeSort(tempArray, 0, tempArray.Length - 1);

    //    //for (int i = 0; i < tempArray.Length; i++)
    //    //{
    //    //    Debug.Log("SECTION INDEX: " + tempArray[i].GetSectionIndex());  
    //    //    sectionsInSceneList[i] = tempArray[i]; 
    //    //}

    //    //isSectionActive = true;
    //    //UpdateGameState(GAMESTATE.STAGE_PHASE);
    //}


    //private IEnumerator DEBUG_WaitToStartSection()
    //{
    //    yield return new WaitForSeconds(1f);
    //    //order the sections 
    //    SectionManager[] tempArray = new SectionManager[sectionsInSceneList.Count];
    //    for (int i = 0; i < sectionsInSceneList.Count; i++)
    //    {
    //        tempArray[i] = sectionsInSceneList[i];
    //    }
    //    MergeSort(tempArray, 0, tempArray.Length - 1);
    //    for (int i = 0; i < tempArray.Length; i++)
    //    {
    //        Debug.Log("SECTION INDEX: " + tempArray[i].GetSectionIndex());
    //        sectionsInSceneList[i] = tempArray[i];
    //    }

    //    isSectionActive = true;
    //    UpdateGameState(GAMESTATE.STAGE_PHASE);
    //}

    private void MergeSort(SectionManager[] array, int left, int right)
    {
        if (left < right)
        {
            int mid = left + (right - left) / 2;

            MergeSort(array, left, mid);
            MergeSort(array, mid + 1, right);
            Merge(array, left, mid, right);
        }
    }

    private void Merge(SectionManager[] array, int left, int mid, int right)
    {
        int n1 = mid - left + 1;
        int n2 = right - mid;

        SectionManager[] list1 = new SectionManager[n1];
        SectionManager[] list2 = new SectionManager[n2];
        int i = 0;
        int j = 0;

        for (i = 0; i < n1; i++)
        {
            list1[i] = array[left + i]; 
        }
        for (j = 0; j < n2; j++)
        {
            list2[j] = array[mid + 1 + j];
        }

        int k = mid;
        while (i < n1 && j < n2)
        {
            int lIndex = list1[i].GetSectionIndex(); 
            int rIndex = list2[i].GetSectionIndex();

            if (lIndex <= rIndex)
            {
                array[k] = list1[i];
                i++;
            }
            else 
            {
                array[k] = list2[j];
                j++; 
            }
            k++;
        }

        while (i < n1)
        {
            array[k] = list1[i];
            i++;
            k++;
        }
        while (j < n2)
        {
            array[k] = list2[j];
            j++;
            k++;
        }
    }




}
