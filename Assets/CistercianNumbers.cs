using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class CistercianNumbers: MonoBehaviour {
    public Sprite[] digits;
    public SpriteRenderer[] numerals;
    public TextMesh operation;
    public TextMesh display;
    public KMSelectable[] numpad;
    public KMSelectable clear;
    public KMSelectable submit;
    int firstNumber;
    int secondNumber;
    int result;
    enum PuzzleType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }
    PuzzleType puzzleType;
    string operators = "+-*/";
    public KMBombModule module;
    public KMAudio sound;
    int moduleId;
    static int moduleIdCounter = 1;
    bool solved;
    // Use this for initialization
    void Awake() {
        moduleId = moduleIdCounter++;
        SetUpPuzzle();
        foreach (KMSelectable i in numpad)
        {
            KMSelectable numpadNum = i;
            numpadNum.OnInteract += delegate { PressNumpad(numpadNum); return false; };
        }
        clear.OnInteract += delegate { PressClear(); return false; };
        submit.OnInteract += delegate { PressSubmit(); return false; };
        module.OnActivate += delegate { StartCoroutine(Generate()); };
	}
	
	
	void SetUpPuzzle () {
        puzzleType = (PuzzleType) rnd.Range(0, 4);
        firstNumber = rnd.Range(1, 10000);
        secondNumber = rnd.Range(1, 10000);
        if ((int)puzzleType % 2 == 1)
        {if (firstNumber < secondNumber)
            {
                int temp = firstNumber;
                firstNumber = secondNumber;
                secondNumber = temp;
            }
        }
        switch (puzzleType)
        {
            case PuzzleType.Addition:
                result = (firstNumber + secondNumber) % 10000;
                break;
            case PuzzleType.Subtraction:
                result = firstNumber - secondNumber;
                break;
            case PuzzleType.Multiplication:
                result = (firstNumber * secondNumber) % 10000;
                break;
            case PuzzleType.Division:
                result = firstNumber / secondNumber;
                break;
        }
        Debug.LogFormat("[Cistercian Numbers #{0}] The operation is {1}.", moduleId, puzzleType.ToString("g"));
        Debug.LogFormat("[Cistercian Numbers #{0}] The numbers are {1} and {2}.", moduleId, firstNumber, secondNumber);
        Debug.LogFormat("[Cistercian Numbers #{0}] The result of the operation, modulo 10000 and ignoring decimals, is {1}.", moduleId, result);
    }

    void SetUpNumber (int number, int startPoint)
    {
        for (int i = startPoint; i < startPoint + 4; i++)
        {
            numerals[i].sprite = digits[number % 10];
            number = number / 10;
        }
    }

    void PressNumpad(KMSelectable button)
    {
        button.AddInteractionPunch(0.5f);
        sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.TypewriterKey, transform);
        if (!solved)
        {
            if (display.text.Length != 4)
            {
                display.text += button.GetComponent<TextMesh>().text;
            }
        }
    }
    void PressClear()
    {
        clear.AddInteractionPunch(0.5f);
        sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.TypewriterKey, transform);
        if (!solved)
        {
            display.text = "";
        }
    }

    void PressSubmit()
    {
        submit.AddInteractionPunch(0.5f);
        sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.TypewriterKey, transform);
        if (!solved)
        {
            Debug.LogFormat("[Cistercian Numbers #{0}] You submitted {1}.", moduleId, display.text);
            if (int.Parse(display.text) == result)
            {
                Debug.LogFormat("[Cistercian Numbers #{0}] That was correct. Module solved.", moduleId);
                sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                module.HandlePass();
                solved = true;
            }
            else
            {
                Debug.LogFormat("[Cistercian Numbers #{0}] That was incorrect. Strike!.", moduleId);
                module.HandleStrike();
                display.text = "";
                SetUpPuzzle();
                StartCoroutine(Generate());
            }
        }
    }

    IEnumerator Generate()
    {
        for (int i = 0; i < 10; i++)
        {
            foreach (SpriteRenderer digit in numerals)
            {
                digit.sprite = digits[rnd.Range(0, 9)];
            }
            operation.text = operators[rnd.Range(0, 4)].ToString();
            yield return new WaitForSeconds(0.08f);
        }
        SetUpNumber(firstNumber, 0);
        SetUpNumber(secondNumber, 4);
        operation.text = operators[(int)puzzleType].ToString();
        StopAllCoroutines();
    }
	string TwitchHelpMessage = " Use '!{0} submit 1234' to sumbit your answer. Only up to four digits are accepted.";
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] commandArray  = command.ToLowerInvariant().Split(' ');
		int number;
		if (commandArray.Length != 2 || commandArray [0] != "submit" || !int.TryParse (commandArray[1], out number) || commandArray[1].Length > 4) {
			yield return "sendtochaterror Invalid command";
			yield break;
		} else 
		{
			List<int> buttons = new List<int> ();
			for (int i = 0; i < commandArray[1].Length; i++)
			{
				buttons.Add(number % 10);
				number = number / 10;
			}
			buttons.Reverse ();
			foreach (int i in buttons) {
				yield return null;
				numpad [i].OnInteract ();
				yield return new WaitForSeconds (0.1f);
			}
			yield return null;
			submit.OnInteract();
			yield break;
		}
	}

    IEnumerator TwitchHandleForcedSolve()
    {
        string curr = display.text;
        string ans = result.ToString();
        bool clrPress = false;
        if (curr.Length > ans.Length)
        {
            clear.OnInteract();
            yield return new WaitForSeconds(0.1f);
            clrPress = true;
        }
        else
        {
            for (int i = 0; i < curr.Length; i++)
            {
                if (i == ans.Length)
                    break;
                if (curr[i] != ans[i])
                {
                    clear.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    clrPress = true;
                    break;
                }
            }
        }
        int start = 0;
        if (!clrPress)
            start = curr.Length;
        for (int j = start; j < ans.Length; j++)
        {
            numpad[int.Parse(ans[j].ToString())].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        submit.OnInteract();
    }
}
