using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;
using KeepCoding;

public class MilitaryEncryptionScr : ModuleScript {
    public TextMesh Timer;
    public TextMesh EncryptedWord;
    public SpriteRenderer Rank;
    public Sprite[] RankOptions;
    public MeshRenderer[] TimerLEDs;
    public Material[] LEDMats;
    public KMSelectable[] Buttons;

    internal bool _active;
    private int _numSoftResets = 0;
    private int _numHardResets = 0;
    private int _keysquareIndex;
    internal int _column;
    internal List<int> _allowedIndices =  new List<int>();
    internal List<List<string>> _wordlist = new List<List<string>>(){
        new List<string>(){ "ACID", "HOWL", "ORCA", "TECH"},
        new List<string>(){ "ANKH", "ISLE", "PAST", "TRIO"},
        new List<string>(){ "BOAR", "KNOT", "QUIZ", "USER"},
        new List<string>(){ "COPY", "LADY", "RAID", "VAST"},
        new List<string>(){ "DAWN", "LION", "SCAN", "VICE"},
        new List<string>(){ "FANG", "MIKE", "SILO", "WARD"},
        new List<string>(){ "GERM", "NEON", "SMOG", "YETI"},
        new List<string>(){ "HEIR", "OKAY", "TANK", "ZINC"}
    };
    private List<List<string>> _keysquares = new List<List<string>>(){
        new List<string>(){"DTHFR","AGVNC","LUXQI","ESBKW","PYOMZ"},
        new List<string>(){"VMRBX","QOPLD","NCSGH","FEIWZ","TAYUK"},
        new List<string>(){"STHGR","IYWND","CVZUO","EFMLX","KAPQB"},
        new List<string>(){"BOGEF","HPYTZ","WVXNI","DCRAL","QSKUM"},
        new List<string>(){"UDEIL","CKVZQ","XGYPO","NTMWR","HABFS"},
        new List<string>(){"ZDSHI","YQUBP","LTCEW","ARKGO","VNMXF"},
        new List<string>(){"XZWNY","OFICQ","GEKTH","BSPAU","VDLMR"},
        new List<string>(){"DXVGZ","ORUYS","CKQIE","ATWHF","MNPBL"},
        new List<string>(){"VYQSA","BZWDE","CKNOP","FMGIX","RUHTL"},
        new List<string>(){"AVZKS","PYUHD","GWOCQ","MNITB","EXFLR"},
        new List<string>(){"OIRAN","ZTSGX","PMVKU","CYWEL","BHFQD"},
        new List<string>(){"RSLMH","EFBWZ","OTPIV","ADKUG","CQXYN"},       
        };
    private List<int> _transpositionKey = new List<int>();

    private void Start () {
        Buttons.Assign(onInteract: x => OnButtonPress(x));
        Buttons[5].Assign(onInteract: () => OnExclamationPress());
        _keysquareIndex = Rnd.Range(0, _keysquares.Count);
        Rank.sprite = RankOptions[_keysquareIndex];
        Log("The chosen keysquare is keysquare {0}.", _keysquareIndex + 1);
    }
    
    private void OnButtonPress (KMSelectable button) {
        ButtonEffect(button, 1, KMSoundOverride.SoundEffect.ButtonPress);

        if (!IsSolved && _active)
        {
            if (EncryptedWord.text.Length == 8)
                EncryptedWord.text = "";
            EncryptedWord.text += button.GetComponentInChildren<TextMesh>().text;
            if (EncryptedWord.text.Length == 8)
            {
                Log("You submitted {0}.", EncryptedWord.text);
                _active = false;
                StopAllCoroutines();
                Timer.text = "--";
                if (_wordlist[_column].Any(x => _allowedIndices.Contains(_wordlist[_column].IndexOf(x)) && ADFGXEncrypt(x).Equals(EncryptedWord.text))) {
                    Log("That was correct. Module solved.");
                    PlaySound(KMSoundOverride.SoundEffect.CorrectChime);
                    EncryptedWord.text = "WELL DONE";                  
                    Solve();
                }
                else
                {
                    Log("That was incorrect. Strike!");
                    EncryptedWord.text = "";
                    Strike();
                }
            }
        }    
	}

    private void OnExclamationPress() {
        ButtonEffect(Buttons[5], 1, KMSoundOverride.SoundEffect.ButtonPress);
        if (!IsSolved) {
            if (!_active) {
                _active = true;
                StartCoroutine(RunTimer());
                OnHardReset();
                OnSoftReset();
            }

            else
                EncryptedWord.text = "";           
        }
    }

    internal string ADFGXEncrypt(string plaintext) {
        string ciphertextAlphabet = "ADFGX";
        List<char> preTranspositionCiphertext = new List<char>();
        List<string> keysquare = _keysquares[_keysquareIndex];
        foreach (char i in plaintext)
        {
            string foundRow = keysquare.First(x => x.Contains(i));
            preTranspositionCiphertext.Add(ciphertextAlphabet[keysquare.IndexOf(foundRow)]);
            preTranspositionCiphertext.Add(ciphertextAlphabet[foundRow.IndexOf(i)]);
        }
        string[] columns = new string[4];
        string encrypted = "";

        for (int bb = 0; bb < 8; bb++)
            columns[bb % 4] = columns[bb % 4] + "" + preTranspositionCiphertext.Join("")[bb];
        for (int cc = 0; cc < 4; cc++)
        {
            string find = (cc).ToString();
            encrypted += columns[_transpositionKey.Join("").IndexOf(find[0])];
        }
        return encrypted;
    }

    private void OnSoftReset() {
        Log(_numSoftResets > 0 ? string.Format("Soft Reset #{0}:", _numSoftResets) : "Inital state:");
        int newWordIndex = _allowedIndices[Rnd.Range(0, _allowedIndices.Count)];
        _allowedIndices.Remove(newWordIndex);
        EncryptedWord.text = ADFGXEncrypt(_wordlist[_column][newWordIndex]);
        Log("The chosen word is {0}, which encrypts to {1}.", _wordlist[_column][newWordIndex], EncryptedWord.text);
        Log("Vaid submissions are {0}.", _allowedIndices.Select(x => ADFGXEncrypt(_wordlist[_column][x])).Reverse().Zip(new[] { "", " and " }.Concat(Enumerable.Repeat(", ", int.MaxValue)),(x, y) => x + y).Reverse().Join(""));
        _numSoftResets++;
    }

    private void OnHardReset() {
        _numSoftResets = 0;
        Log(_numHardResets > 0 ? string.Format("Hard Reset #{0}:", _numHardResets) : "Inital state:");
        _transpositionKey = Enumerable.Range(0, 4).ToList().Shuffle();
        _column = Rnd.Range(0, _wordlist.Count);
        _allowedIndices = Enumerable.Range(0, _wordlist[_column].Count).ToList();
        Log("The chosen transpoistion key is {0}.", _transpositionKey.Select(x => "ABCD"[x]).Join(""));
        Log("The chosen column is {0}.", _column + 1);
        _numHardResets++;
    }

    private IEnumerator RunTimer() {
        while (_active) {
            int currentTime = 90;
            while (currentTime > 0)
            {
                Timer.text = currentTime.ToString();
                Material flashMat = _numSoftResets == 2 ? LEDMats[2] : LEDMats[1];
                if (currentTime <= 10)
                    TimerLEDs.ForEach(x => x.material = (currentTime % 2 == 0 ? flashMat : LEDMats[0]));
                yield return new WaitForSeconds(1f);
                currentTime--;
            }
            if (_numSoftResets == 3)
                OnHardReset();
            OnSoftReset();
        }
        yield break;
    }
}
