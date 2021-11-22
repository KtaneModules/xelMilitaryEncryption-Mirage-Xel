using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using KeepCoding;

public class MilitaryEncryptionTP : TPScript<MilitaryEncryptionScr> {

    public override IEnumerator ForceSolve()
    {
        if (!Module._active)
            Module.Buttons[5].OnInteract();
        yield return YieldUntil(false, () => int.Parse(Module.Timer.text) > 4);
        Debug.Log(Module.ADFGXEncrypt(Module._wordlist[Module._column][Module._allowedIndices[0]]).Select(x => "ADFGX".IndexOf(x)).Join());
        foreach (int i in Module.ADFGXEncrypt(Module._wordlist[Module._column][Module._allowedIndices[0]]).Select(x => "ADFGX".IndexOf(x)).ToArray())
        {
            Module.Buttons[i].OnInteract();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public override IEnumerator Process(string command)
    {
        command = command.ToLowerInvariant().Trim();
        if (!Module._active && command == "activate")
        {
            yield return null;
            Module.Buttons[5].OnInteract();
        }
        if (Module._active && command.All(x => "adfgx".Contains(x)))
        {
            yield return null;
            yield return OnInteractSequence(Module.Buttons, 0.5f, command.Select(x => "adfgx".IndexOf(x)).ToArray());
        }
        else
        yield break;
    }
}
