using UnityEngine;
using System.Collections;

public enum ZombieType {
    Zombie1,
    Zombie2,
    FlagZombie,
    ConeheadZombie,
    BucketheadZombie,
    NewspaperZombie,
    PoleVaultingZombie
}

[System.Serializable]
public struct Wave {
    [System.Serializable]
    public struct Data {
        public ZombieType zombieType;
        public uint count;
    }

    public bool isLargeWave;
    [Range(0f, 1f)]
    public float percentage;
    public Data[] zombieData;
}

public class GameController : MonoBehaviour {

    [Space(10)]
    public AudioClip bgmMusic;

    [Space(10)]
    public AudioClip loseMusic;
    public AudioClip winMusic;
    public AudioClip readySound;
    public AudioClip zombieComing;
    public AudioClip hugeWaveSound;
    public AudioClip finalWaveSound;  

    [Space(10)]
    public GameObject Zombie1;
    public GameObject Zombie2;
    public GameObject FlagZombie;
    public GameObject ConeheadZombie;
    public GameObject BucketheadZombie;
    public GameObject NewspaperZombie;
    public GameObject PoleVaultingZombie;

    [Space(10)]
    public GameObject gameLabel;
    public GameObject progressBar;
    public GameObject stageLabel;
    public GameObject cardDialog;
    public GameObject sunLabel;
    public GameObject shovelBg;

    [Space(10)]
    public GameObject sunPrefab;
    public float sunInterval;

    [Space(10)]
    public int initSun = 150;
    public bool inDay = true;
    public string nextStage;

    [Space(10)]
    public Wave[] waves;

    [Space(10)]
    public float readyTime;
    public float playTime;

    private GameModel model;
    private float elapsedTime = 0;
    private bool hasLostGame = false;

    void Awake() {
        model = GameModel.GetInstance();
    }

    void Start() {
        model.Clear();
        model.sun = initSun;
        model.inDay = inDay;

        ArrayList flags = new ArrayList();
        for (int i = 0; i < waves.Length; ++i) {
            if (waves[i].isLargeWave) {
                flags.Add(waves[i].percentage);
            }
        }      
        progressBar.GetComponent<ProgressBar>().InitWithFlag((float[])flags.ToArray(typeof(float)));
        progressBar.SetActive(false);

        stageLabel.SetActive(false);
        cardDialog.SetActive(false);
        sunLabel.SetActive(false);
        shovelBg.SetActive(false);
        GetComponent<HandlerForPlant>().enabled = false;
        GetComponent<HandlerForShovel>().enabled = false;

        StartCoroutine(GameReady());
        AudioManager.GetInstance().PlayMusic(bgmMusic);
    }

	void Update () {
        if (Input.GetKeyDown(KeyCode.M)) {
            model.sun += 50;
        }

        if (!hasLostGame) {
            for (int row = 0; row < model.zombieList.Length; ++row) {
                foreach (GameObject zombie in model.zombieList[row]) {
                    if (zombie.transform.position.x < (StageMap.GRID_LEFT - 0.4f)) {
                        LoseGame();
                        hasLostGame = true;
                        return;
                    }
                }
            }
        } else if (Input.GetMouseButtonDown(0)) {
            GameObject.Find("btn_menu")
                .GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        }
	}

    IEnumerator GameReady() {
        yield return new WaitForSeconds(0.5f);
        MoveBy move = Camera.main.gameObject.AddComponent<MoveBy>();
        move.offset = new Vector3(3.9f, 0, 0);
        move.time = 1f;
        move.Begin();
        yield return new WaitForSeconds(1.5f);
        sunLabel.SetActive(true);
        cardDialog.SetActive(true);
    }

    public void AfterSelectCard() {
        Destroy(cardDialog);
        shovelBg.SetActive(true);
        GetComponent<HandlerForPlant>().enabled = true;
        GetComponent<HandlerForShovel>().enabled = true;

        Camera.main.transform.position = new Vector3(1.1f, 0, -1f);
        StartCoroutine(Workflow());
        if (inDay) {
            InvokeRepeating("ProduceSun", sunInterval, sunInterval);
        }
    }

    IEnumerator Workflow() {
        gameLabel.GetComponent<GameTips>().ShowStartTip();
        AudioManager.GetInstance().PlaySound(readySound);
        yield return new WaitForSeconds(readyTime);

        ShowProgressBar();
        AudioManager.GetInstance().PlaySound(zombieComing);

        for (int i = 0; i < waves.Length; ++i) {
            yield return StartCoroutine(WaitForWavePercentage(waves[i].percentage));

            if (waves[i].isLargeWave) {
                StopCoroutine("UpdateProgress");
                yield return StartCoroutine(WaitForZombieClear());
                yield return new WaitForSeconds(3.0f);

                gameLabel.GetComponent<GameTips>().ShowApproachingTip();
                AudioManager.GetInstance().PlaySound(hugeWaveSound);

                yield return new WaitForSeconds(3.0f);
                StartCoroutine("UpdateProgress");
            }
            if (i + 1 == waves.Length) {
                gameLabel.GetComponent<GameTips>().ShowFinalTip();
                AudioManager.GetInstance().PlaySound(finalWaveSound);
            }

            CreateZombies(ref waves[i]);
        }

        yield return StartCoroutine(WaitForZombieClear());
        yield return new WaitForSeconds(3.0f);
        WinGame();
    }

    void ShowProgressBar() {
        stageLabel.SetActive(true);
        progressBar.SetActive(true);
        StartCoroutine("UpdateProgress");
    }

    IEnumerator UpdateProgress() {
        while (true) {
            elapsedTime += Time.deltaTime;
            progressBar.GetComponent<ProgressBar>().SetProgress(elapsedTime / playTime);
            yield return 0;
        }
    }

    void CreateZombies(ref Wave wave) {
        foreach (Wave.Data data in wave.zombieData) {
            for (int i = 0; i < data.count; ++i) {
                CreateOneZombie(data.zombieType);
            }
        }
    }

    void CreateOneZombie(ZombieType type) {
        GameObject zombie;
        switch (type) {
            case ZombieType.Zombie1:
                zombie = Instantiate(Zombie1);
                break;
            case ZombieType.Zombie2:
                zombie = Instantiate(Zombie2);
                break;
            case ZombieType.FlagZombie:
                zombie = Instantiate(FlagZombie);
                break;
            case ZombieType.ConeheadZombie:
                zombie = Instantiate(ConeheadZombie);
                break;
            case ZombieType.BucketheadZombie:
                zombie = Instantiate(BucketheadZombie);
                break;
            case ZombieType.PoleVaultingZombie:
                zombie = Instantiate(PoleVaultingZombie);
                break;
            case ZombieType.NewspaperZombie:
                zombie = Instantiate(NewspaperZombie);
                break;
            default:
                throw new System.Exception("Wrong zombie type");
        }
        int row = Random.Range(0, StageMap.ROW_MAX - 1);
        zombie.transform.position = StageMap.GetZombiePos(row);
        zombie.GetComponent<ZombieMove>().row = row;
        zombie.GetComponent<SpriteDisplay>().SetOrderByRow(row);
        model.zombieList[row].Add(zombie);
    }

    IEnumerator WaitForZombieClear() {
        while (true) {
            bool hasZombie = false;
            for (int row = 0; row < StageMap.ROW_MAX; ++row) {
                if (model.zombieList[row].Count != 0) {
                    hasZombie = true;
                    break;
                }
            }
            if (hasZombie) {
                yield return new WaitForSeconds(0.1f);
            } else {
                break;
            }
        }           
    }

    IEnumerator WaitForWavePercentage(float percentage) {
        while (true) {
            if ((elapsedTime / playTime) >= percentage) {
                break;
            } else {
                yield return 0;
            }
        }
    }

    void ProduceSun() {
        float x = Random.Range(StageMap.GRID_LEFT, StageMap.GRID_RIGHT);
        float y = Random.Range(StageMap.GRID_BOTTOM, StageMap.GRID_TOP);
        float startY = StageMap.GRID_TOP + 1.5f;
        GameObject sun = Instantiate(sunPrefab);
        sun.transform.position = new Vector3(x, startY, 0);
        MoveBy move = sun.AddComponent<MoveBy>();
        move.offset = new Vector3(0, y - startY, 0);
        move.time = (startY - y) / 1.0f;
        move.Begin();
    }

    void LoseGame() {
        gameLabel.GetComponent<GameTips>().ShowLostTip();
        GetComponent<HandlerForPlant>().enabled = false;
        GetComponent<HandlerForShovel>().enabled = false;
        CancelInvoke("ProduceSun");
        AudioManager.GetInstance().PlayMusic(loseMusic, false);
    }

    void WinGame() {
        CancelInvoke("ProduceSun");
        AudioManager.GetInstance().PlayMusic(winMusic, false);
        Invoke("GoToNextStage", 5.0f);
    }

    void GoToNextStage() {
        Application.LoadLevel(nextStage);
    }
}
