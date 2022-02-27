using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Token : MonoBehaviour {
    /// <summary>
    /// 相碰的两个相同的Token，t1的y轴高度大于t2
    /// </summary>
    public static UnityEvent<Token, Token> TokenMatched = new UnityEvent<Token, Token>();
    /// <summary>
    /// 第一个参数为SpecialToken，第二个参数为与其相碰的Token
    /// </summary>
    public static UnityEvent<Token, Token> SpecicalTokenContacted = new UnityEvent<Token, Token>();
    /// <summary>
    /// 该Token的OnMatch事件被引发时调用
    /// </summary>
    public static UnityEvent<Token> Matched = new UnityEvent<Token>();
    /// <summary>
    /// 当Token碰到触发位时触发
    /// </summary>
    public static UnityEvent<Token, RedLine> RedLineTouched = new UnityEvent<Token, RedLine>();
    public static float AllowedRedLineStayTime = 2.0f;
    public int TokenID {
        get {
            return _tokenID;
        }
        set {
            _tokenID = value;
        }
    }
    public float ScaleRatio {
        get {
            return _scaleRatio;
        }
        set {
            _scaleRatio = value;
            _radius = transform.Find("AnimationHandler").Find("SpriteHandler").Find("Sprite").GetComponent<SpriteRenderer>().size.x * _scaleRatio / 2;
            transform.localScale = new Vector3(_scaleRatio, _scaleRatio);
        }
    }
    public int Score {
        get {
            int score = (int)Mathf.Pow(TokenID, 2);
            if (transform.localScale.x > _scaleRatio) {
                score = (int)(score * (1 + (transform.localScale.x - _scaleRatio)));
            }
            return score;
        }
    }
    public float Radius {
        get {
            return _radius;
        }
    }
    public TokenType TokenType {
        get {
            return _tokenType;
        }
        set {
            _tokenType = value;
        }
    }

    private int _tokenID;
    private float _scaleRatio;
    private float _radius;
    private bool _hasTouchedGround;
    private bool _isMatched;
    private bool _actived;
    private bool _canTriggerRedLine;
    private TokenType _tokenType;
    private Animator _animator;
    Coroutine _redLineTicker;

    void Awake() {
        _isMatched = false;
        _actived = false;
        _canTriggerRedLine = false;
        _hasTouchedGround = false;
        _animator = gameObject.transform.Find("AnimationHandler").GetComponent<Animator>();
        transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
    }

    public void OnTrans() {
        StartCoroutine(ScaleHelper(0, () => {
            Matched?.Invoke(this);
        }));
    }
    public void OnMatched() {
        Matched?.Invoke(this);
        Destroy(gameObject);
    }
    public void OnCreated() {
        StartCoroutine(OnCreateHelper());
    }
    public void Active() {
        _actived = true;
        transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }
    public void Deactive() {
        _actived = false;
        transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
    }
    public void Scale(float size) {
        StartCoroutine(ScaleHelper(size, null));
    }
    public void OnCollisionEnter2D(Collision2D collision) {
        if (collision.transform.CompareTag("Token")) {
            var otherToken = collision.gameObject.GetComponent<Token>();
            // 如果未启用，不进行判定
            if (!_actived || !otherToken._actived) {
                return;
            }
            // 传递边界触碰检测
            if (otherToken._canTriggerRedLine) {
                _canTriggerRedLine = true;
            }
            // 如果为两个Token
            if (TokenType == TokenType.Normal && otherToken.TokenType == TokenType.Normal) {
                if (otherToken.TokenID == TokenID) {
                    if (otherToken._isMatched || _isMatched) {
                        return;
                    }
                    _isMatched = true;
                    otherToken._isMatched = true;
                    SoundManager.Instance.PlayOneShot(SoundManager.Instance.MergedSound);
                    // 保证t1的位置在t2上方
                    if (transform.position.y > otherToken.transform.position.y) {
                        TokenMatched?.Invoke(this, otherToken);
                    }
                    else {
                        TokenMatched?.Invoke(otherToken, this);
                    }
                }
            }
            // 若都是SpecialToken，不执行操作
            else if (TokenType == TokenType.Special && otherToken.TokenType == TokenType.Special) {
                return;
            }
            // 如果自己为SpecialToken
            else if (TokenType == TokenType.Special && otherToken.TokenType == TokenType.Normal) {
                if (_isMatched) {
                    return;
                }
                _isMatched = true;
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.MergedSound);
                SpecicalTokenContacted?.Invoke(this, otherToken);
            }
            // 如果被触碰的为SpecialToken
            else if (TokenType == TokenType.Normal && otherToken.TokenType == TokenType.Special) {
                if (otherToken._isMatched) {
                    return;
                }
                otherToken._isMatched = true;
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.MergedSound);
                SpecicalTokenContacted?.Invoke(otherToken, this);
            }
        }
        else if (collision.transform.CompareTag("Ground")) {
            if (!_hasTouchedGround) {
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.TouchGroundSound);
            }
            _hasTouchedGround = true;
            _canTriggerRedLine = true;
        }
    }
    public void OnTriggerStay2D(Collider2D collision) {
        if (collision.transform.CompareTag("RedLine")) {
            if (_canTriggerRedLine) {
                collision.transform.GetComponent<RedLine>().Tip();
                if (_redLineTicker != null) {
                    return;
                }
                _redLineTicker = StartCoroutine(RedLineTouchCountDown(collision.transform.GetComponent<RedLine>()));
            }
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        if (collision.transform.CompareTag("RedLine")) {
            if (_redLineTicker != null) {
                StopCoroutine(_redLineTicker);
            }
        }
    }

    IEnumerator RedLineTouchCountDown(RedLine redLine) {
        yield return new WaitForSeconds(AllowedRedLineStayTime);
        RedLineTouched?.Invoke(this, redLine);
    }
    IEnumerator OnCreateHelper() {
        float originGravityScale = transform.GetComponent<Rigidbody2D>().gravityScale;
        _animator.SetTrigger("created");
        transform.GetComponent<Rigidbody2D>().gravityScale = 0;
        yield return null;
        while (_animator.GetCurrentAnimatorStateInfo(0).IsName("token_created")) {
            yield return null;
        }
        transform.GetComponent<Rigidbody2D>().gravityScale = originGravityScale;
    }
    IEnumerator ScaleHelper(float scaleSize, System.Action scaleFinishedCallback) {
        float newSize = scaleSize * transform.localScale.x;
        float step = (newSize - transform.localScale.x) / 15;
        for (int i = 0; i < 15; i++) {
            gameObject.transform.localScale = gameObject.transform.localScale + new Vector3(step, step, 0);
            yield return null;
        }
        gameObject.transform.localScale = new Vector3(newSize, newSize, newSize);
        scaleFinishedCallback?.Invoke();
    }
}
