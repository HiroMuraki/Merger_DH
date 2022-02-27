using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GameMain : MonoBehaviour {
    public static readonly int InGameTokensCount = 11;
    public static readonly int InGameGeneratableCount = 4;
    public static GameMain Instance {
        get {
            return _instance;
        }
    }
    public static UnityEvent<GameMain> GameRestarted = new UnityEvent<GameMain>();
    public static UnityEvent<GameMain> GameCompleted = new UnityEvent<GameMain>();
    public float HalfScreenWidth {
        get {
            return _halfScreenWidth;
        }
        set {
            _halfScreenWidth = value;
        }
    }
    public float TokenRadiusMultiplier {
        get {
            return _tokenRadiusMulptier;
        }
        set {
            _tokenRadiusMulptier = value;
        }
    }
    public GameObject[] Tokens;
    public GameObject[] SpecialTokens;
    public Transform NextTokenPosition;

    float _tokenRadiusMulptier = 1.0f;
    float _nextTokenGenerateDelay = 0.5f;
    List<GameObject> _selectedTokens = new List<GameObject>();
    GameStatistics _gameStatistics;
    static GameMain _instance;
    Token _nextToken;
    GameObject _tokensHandler;
    bool _isGameCompleted;
    Coroutine _tokenGenerator;
    float _halfScreenWidth;

    void Awake() {
        _instance = this;
        _gameStatistics = gameObject.GetComponent<GameStatistics>();
    }

    // Start is called before the first frame update
    void Start() {
        Token.TokenMatched.AddListener((t1, t2) => {
            int nextId = t1.TokenID + 1;
            if (nextId >= _selectedTokens.Count) {
                return;
            }
            var token = _selectedTokens[nextId];
            var pos = t2.transform.position;
            t1.OnMatched();
            t2.OnMatched();
            var created = GenerateToken(token, pos);
            created.Active();
            if (nextId == _selectedTokens.Count - 1) {
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.MeowSound);
            }
        });
        Token.SpecicalTokenContacted.AddListener((specialToken, token) => {
            // 升级一次Token
            if (specialToken.TokenID == 0) {
                specialToken.OnMatched();
                int nextId = token.TokenID + 1;
                if (nextId >= _selectedTokens.Count) {
                    return;
                }
                var pos = token.transform.position;
                token.OnMatched();
                var created = GenerateToken(_selectedTokens[token.TokenID + 1], pos);
                created.Active();
                if (nextId == _selectedTokens.Count - 1) {
                    SoundManager.Instance.PlayOneShot(SoundManager.Instance.MeowSound);
                }
            }
            // 降级一次Token
            else if (specialToken.TokenID == 1) {
                specialToken.OnMatched();
                if (token.TokenID == 0) {
                    return;
                }
                else {
                    var pos = token.transform.position;
                    Destroy(token.gameObject);
                    var created = GenerateToken(_selectedTokens[token.TokenID - 1], pos);
                    created.Active();
                }
            }
            // 将Token变大一点
            else if (specialToken.TokenID == 2) {
                specialToken.OnMatched();
                token.Scale(1.1f);
            }
            // 将Token变小一点
            else if (specialToken.TokenID == 3) {
                specialToken.OnMatched();
                token.Scale(0.75f);
            }
            // 将所有的Token缩小一点
            else if (specialToken.TokenID == 4) {
                specialToken.OnMatched();
                for (int i = 0; i < _tokensHandler.transform.childCount; i++) {
                    var t = _tokensHandler.transform.GetChild(i).GetComponent<Token>();
                    if (t != _nextToken) {
                        t.Scale(0.95f);
                    }
                }
            }
        });
        Token.Matched.AddListener((t) => {
            if (t.TokenType != TokenType.Special) {
                _gameStatistics.IncreaseScore(t.Score);
            }
        });
        Token.RedLineTouched.AddListener((token, touchedRedLine) => {
            _isGameCompleted = true;
            touchedRedLine.Flash();
            for (int i = 0; i < _tokensHandler.transform.childCount; i++) {
                _tokensHandler.transform.GetChild(i).GetComponent<Token>().Deactive();
            }
            GameCompleted?.Invoke(this);
        });
    }

    // Update is called once per frame
    void Update() {
        if (_isGameCompleted) {
            return;
        }
        if (_nextToken == null) {
            return;
        }
#if PLATFORM_ANDROID
        if (Input.touchCount < 1) {
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId)) {
            return;
        }
        if (Input.touches[0].phase == TouchPhase.Moved || Input.touches[0].phase == TouchPhase.Began) {
            var pos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            // 边界检查
            if (pos.x < -_halfScreenWidth + _nextToken.Radius) {
                pos.x = -_halfScreenWidth + _nextToken.Radius;
            }
            else if (pos.x > _halfScreenWidth - _nextToken.Radius) {
                pos.x = _halfScreenWidth - _nextToken.Radius;
            }
            _nextToken.transform.position = new Vector3(pos.x, NextTokenPosition.position.y, 0);
        }
        if (Input.touches[0].phase == TouchPhase.Ended) {
            _nextToken.GetComponent<Token>().Active();
            GetNextToken();
        }
#else
        if (EventSystem.current.IsPointerOverGameObject()) {
            return;
        };
        if (Input.GetMouseButton(0)) {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // 边界检查
            if (pos.x < -_halfScreenWidth + _nextToken.Radius) {
                pos.x = -_halfScreenWidth + _nextToken.Radius;
            }
            else if (pos.x > _halfScreenWidth - _nextToken.Radius) {
                pos.x = _halfScreenWidth - _nextToken.Radius;
            }
            _nextToken.transform.position = new Vector3(pos.x, NextTokenPosition.position.y, 0);
        }
        if (Input.GetMouseButtonUp(0)) {
            _nextToken.GetComponent<Token>().Active();
            GetNextToken();
        }
#endif
    }

    public void StartNewGame() {
        if (_tokensHandler != null) {
            Destroy(_tokensHandler);
        }
        _tokensHandler = new GameObject("TokensHandler");
        // 从Tokens中随机选择InGameTokensCount(11)个Token作为本局游戏Token
        var tokens = new List<GameObject>(Tokens);
        _selectedTokens.Clear();
        for (int i = 0; i < InGameTokensCount; i++) {
            int selectedIndex = Random.Range(0, tokens.Count);
            _selectedTokens.Add(tokens[selectedIndex]);
            tokens.RemoveAt(selectedIndex);
            //// 设置ID与ScaleRatio
            _selectedTokens[i].GetComponent<Token>().TokenID = i;
            _selectedTokens[i].GetComponent<Token>().ScaleRatio = (i * 0.065f + 0.10f) * _tokenRadiusMulptier;
        }
        // 设置SpecialToken大小
        for (int i = 0; i < SpecialTokens.Length; i++) {
            SpecialTokens[i].GetComponent<Token>().TokenType = TokenType.Special;
            SpecialTokens[i].GetComponent<Token>().ScaleRatio = 0.2f * _tokenRadiusMulptier;
            SpecialTokens[i].GetComponent<Token>().TokenID = i;
        }
        // 重置状态
        _isGameCompleted = false;
        _gameStatistics.ResetStatistics();
        GetNextToken();
        GameRestarted?.Invoke(this);
    }
    public void RestartGame() {
        if (_tokensHandler != null) {
            Destroy(_tokensHandler);
        }
        _tokensHandler = new GameObject("TokensHandler");
        // 重置状态
        _isGameCompleted = false;
        _gameStatistics.ResetStatistics();
        GetNextToken();
        GameRestarted?.Invoke(this);
    }

    void GetNextToken() {
        if (_tokenGenerator != null) {
            StopCoroutine(_tokenGenerator);
        }
        _tokenGenerator = StartCoroutine(GetNextTokenHelper());
    }
    Token GenerateToken(GameObject tokenPrefab, Vector3 pos) {
        var created = Instantiate(tokenPrefab, pos, Quaternion.identity, _tokensHandler.transform);
        created.GetComponent<Token>().ScaleRatio = tokenPrefab.GetComponent<Token>().ScaleRatio;
        created.GetComponent<Token>().TokenID = tokenPrefab.GetComponent<Token>().TokenID;
        created.GetComponent<Token>().TokenType = tokenPrefab.GetComponent<Token>().TokenType;
        created.GetComponent<Token>().OnCreated();
        return created.GetComponent<Token>();
    }
    IEnumerator GetNextTokenHelper() {
        _nextToken = null;
        yield return new WaitForSeconds(_nextTokenGenerateDelay);
        GameObject tokenPrefab;
        // 5%的可能出SpecialToken
        if (Random.Range(0, 20) == 0) {
            tokenPrefab = SpecialTokens[Random.Range(0, SpecialTokens.Length)];
        }
        else {
            tokenPrefab = _selectedTokens[Random.Range(0, InGameGeneratableCount)];
        }
        _nextToken = GenerateToken(tokenPrefab, NextTokenPosition.position);
        _nextToken.GetComponent<Token>().Deactive();
    }
}
