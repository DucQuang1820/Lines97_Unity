using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : Singleton<GameManager>, ISaveable
{
    [SerializeField] private BallColorConfig[] ballColorConfigs = null;
    [SerializeField] private GameplayConfig gameplayConfig = null;

    [SerializeField] private GameObject pausePanel = null;
    [SerializeField] private GameObject gameOverPanel = null;

    private Queue<int> colorIndexQueue = null;
    private uint currentScore = 0;
    private uint highScore = 0;
    private Stack<Vector2> explodeKeys = null;

    [HideInInspector]
    public TileNode.BaseNode SelectedNode = null;
    [HideInInspector]
    public TileNode.BaseNode TargetNode = null;

    public void EmptySelectedNode() => SelectedNode = null;
    public void EmptyTargetNode()
    {
        TargetNode = null; 
    }

    private List<Vector2> waypoints = null;
    private BoardManager boardManager = null;
    private UIManager uiManager = null;
    private Ball ball = null;
    private MovingBall movingBall = null;
    public GameObject moveMusic;
    public GameObject destroyMusic;
    public GameObject cantmoveMusic;
    
    public GameObject moveMusictmp;
    public GameObject destroyMusictmp;
    public GameObject cantmoveMusictmp;
    [SerializeField] private GameObject muteButton = null;
    [SerializeField] private GameObject unmuteButton = null;
    [SerializeField] private GameObject InformationPanel = null;
    
    public bool BlockInput = false;


    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        moveMusictmp = moveMusic;
        destroyMusictmp = destroyMusic;
        cantmoveMusictmp = cantmoveMusic;

        boardManager = BoardManager.Instance;
        uiManager = UIManager.Instance;

        boardManager.Init();
        explodeKeys = new Stack<Vector2>();

        movingBall = MovingBall.Instance;
        movingBall.Init();
        movingBall.OnEndEvent += GrowTarget;

        highScore = 0;
        EmptySelectedNode();
        EmptyTargetNode();
        BlockInput = false;
        Timer.Reset();

            NewGame();
        
    }

    public void NewGame()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        Timer.Reset();
        ClearResources();
        boardManager.CreateNewBoard(gameplayConfig.QueuedCount, gameplayConfig.InitGrowUpCount);
        InitScoreboard();
        Resume();
    }

    public void ShowRule()
    {
        InformationPanel.SetActive(true);
        BlockInput = true;
    }

    public void CloseRulePanel()
    {
        InformationPanel.SetActive(false);
        BlockInput = false;
    }
    public void Mute()
    {
        //Debug.Log("Mute");
        muteButton.SetActive(false);
        unmuteButton.SetActive(true);
        moveMusictmp = null;
        destroyMusictmp = null;
        cantmoveMusictmp = null;
    }

    public void Unmute()
    {
        //Debug.Log("Unmute");
        unmuteButton.SetActive(false);
        muteButton.SetActive(true);
        moveMusictmp = moveMusic;
        destroyMusictmp = destroyMusic;
        cantmoveMusictmp = cantmoveMusic;
        
    }

    public void GameOver()
    {
        boardManager.ClearResources();
        Timer.Stop();
        Timer.Reset();
        SetHighScore();
        ClearResources();
        Time.timeScale = 0f;
        BlockInput = true;
        SaveSystem.SaveJSonData();
        gameOverPanel.SetActive(true);
        
    }

    public void Quit()
    {
        SetHighScore();
        Timer.Stop();
        SaveSystem.SaveJSonData();
        Application.Quit();
    }

    public void Pause()
    {
        Timer.Stop();
        BlockInput = true;
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        BlockInput = false;
        Timer.Resume();
        pausePanel.SetActive(false);
    }

    private void ClearResources()
    {
        currentScore = 0;
        colorIndexQueue = new Queue<int>();
    }

    private void Update()
    {
        Timer.Update(Time.deltaTime);
        uiManager.SetTimerText(Timer.Format());
    }

    private void InitScoreboard()
    {
        Timer.Reset();
        var count = gameplayConfig.QueuedCount;
        Sprite[] initNextQueueSprites = new Sprite[count];
        for (int index = 0; index < count; index++)
        {
            int rdmIndex = GetRandomColorIndex();
            colorIndexQueue.Enqueue(rdmIndex);
            initNextQueueSprites[index] = ballColorConfigs[rdmIndex].Sprite;
        }
        uiManager.SetCurrentScoreText(currentScore);
        uiManager.SetHighScoreText(highScore);
    }

    private void AttachQueueBallsToBoard()
    {
        var count = gameplayConfig.QueuedCount;
        for (int index = 0; index < count; index++)
        {
            if (boardManager.OutOfActiveNode())
            {
                GameOver();
                return;
            }

            int configIndex = colorIndexQueue.Dequeue();
            boardManager.AttachQueueBallToRandomActiveNode(ballColorConfigs[configIndex]);
        }

        if (boardManager.OutOfActiveNode())
        {
            GameOver();
            return;
        }
    }

    private void MakeNewColorIndexQueue()
    {
        var count = gameplayConfig.QueuedCount;
        while (colorIndexQueue.Count < count)
        {
            int newRandomColorIndex = GetRandomColorIndex();
            colorIndexQueue.Enqueue(newRandomColorIndex);
        }
    }

    private int GetRandomColorIndex() => Random.Range(0, ballColorConfigs.Length - 1);

    private void MoveToTarget()
    {
        boardManager.ReleaseGrowUpNode(SelectedNode.Pos);
        SelectedNode.BackToDefault();

        BlockInput = true;
        movingBall.Move(SelectedNode.Ball.GetSprite(), waypoints);
        Instantiate(moveMusictmp,transform.position,Quaternion.identity);
    }

    private void GrowTarget()
    {
        boardManager.AddGrowUpNode(TargetNode.Pos);
        TargetNode.AttachGrowUp(SelectedNode.Ball.ColorID, SelectedNode.Ball.GetSprite());
        
        TargetNode.CheckChainExplode();
        BlockInput = false;

        EmptyTargetNode();
        EmptySelectedNode();
    }

    public BallColorConfig GetRandomColorConfig() => ballColorConfigs[GetRandomColorIndex()];

    public Sprite GetSprite(int colorID)
    {
        foreach (var config in ballColorConfigs)
        {
            if (config.ColorID == colorID)
                return config.Sprite;
        }
        return null;
    }

    public void Score()
    {
        if (currentScore == gameplayConfig.MaxScore)
            return;

        currentScore += gameplayConfig.ScorePerExploded;
        Instantiate(destroyMusictmp,transform.position,Quaternion.identity);
        uiManager.SetCurrentScoreText(currentScore);
    }

    private void SetHighScore()
    {
        highScore = currentScore;
        uiManager.SetHighScoreText(highScore);
    }

    public void EndTurn()
    {
        MoveToTarget();
        boardManager.GrowQueueBalls();
        AttachQueueBallsToBoard();
        MakeNewColorIndexQueue();
    }

    public void FindPath()
    {
        waypoints = Pathfinding.FindPath(SelectedNode, TargetNode, boardManager.TotalNodeCount);
        
        if (waypoints == null)
        {
            BlockInput = false;
            Instantiate(cantmoveMusictmp,transform.position,Quaternion.identity);
            EmptyTargetNode(); 
            
        } 
        
        var count = waypoints.Count;
        
    }

    public void AddExplodeKey(Vector2 key)
    {
        if (!explodeKeys.Contains(key))
            explodeKeys.Push(key);
    }

    public bool CheckExplode()
    {
        bool explode = explodeKeys.Count >= gameplayConfig.ExplodeCount;
        if (explode)
            boardManager.ChainExplode(explodeKeys);
        return explode;
    }

    public void  ClearExplodeKeys() => explodeKeys.Clear();


    #region Save Load
    public void PopulateSaveData(SaveData saveData)
    {
        saveData.HighScore = highScore;
        saveData.CurrentScore = currentScore;
        saveData.PlayTime = Timer.GetTime();

        while (colorIndexQueue.Count != 0)
        {
            saveData.ColorIndexQueue.Add(colorIndexQueue.Dequeue());
        }
        boardManager.PopulateSaveData(saveData);
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        ClearResources();
        if (saveData.ColorIndexQueue.Count == 0)
        {
            NewGame();
        }
        else
        {
            List<Sprite> temptList = new List<Sprite>();
            foreach (var data in saveData.ColorIndexQueue)
            {
                colorIndexQueue.Enqueue(data);
                temptList.Add(ballColorConfigs[data].Sprite);
            }
            Sprite[] initNextQueueSprites = temptList.ToArray();
        }

        highScore = saveData.HighScore;
        currentScore = saveData.CurrentScore;
        Timer.SetTime(saveData.PlayTime);

        uiManager.SetCurrentScoreText(currentScore);
        uiManager.SetHighScoreText(highScore);
        boardManager.LoadFromSaveData(saveData);
    }
    #endregion
}
