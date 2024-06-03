using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField]
    private Text _scoreText;
    public int _score;

    public Transform _rivalAgent;

    [SerializeField]
    private Text _rivalScoreText;
    public int _rivalScore;

    // Start is called before the first frame update
    void Start()
    {
        _scoreText = GetComponent<Text>();
        _score = 0;

        //_rivalScoreText = transform.GetComponent<Text>();
        _rivalScore = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _scoreText.text = "Score: " + _score;
        //_rivalScoreText.text = "Rival score: " + _rivalScore;
    }
}
