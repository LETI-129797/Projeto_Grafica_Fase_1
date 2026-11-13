using UnityEngine;
using System.Collections.Generic;


public class LSystemEngine : MonoBehaviour
{
    private List<Symbol> currentWord = new List<Symbol>();
    private List<List<Symbol>> history = new List<List<Symbol>>();
    private List<Rule> rules = new List<Rule>();

    void Start()
    {
        LoadRules();
        SetAxiom('n');
    }

    void LoadRules()
    {
        // P1: n -> p d3
        rules.Add(new Rule('n', CreateSymbolList("p d3")));
        
        // P2: p -> e
        rules.Add(new Rule('p', CreateSymbolList("e")));

        // P3: d3 -> D d2
        rules.Add(new Rule('d', CreateSymbolList("D d2"))); // 'd' representa disco parametrizado
        
        // P4: d2 -> D d1
        rules.Add(new Rule('d', CreateSymbolList("D d1")));

        // P5: d1 -> D
        rules.Add(new Rule('d', CreateSymbolList("D")));

        // P6: D -> m
        rules.Add(new Rule('D', CreateSymbolList("m")));

        // P7: m -> N
        rules.Add(new Rule('m', CreateSymbolList("N")));

        // P8: N -> P[1][1]
        rules.Add(new Rule('N', CreateSymbolList("P [ 1 ] [ 1 ]")));
    }

    public void SetAxiom(char axiomChar)
    {
        currentWord.Clear();
        history.Clear();
        
        currentWord.Add(new Symbol(axiomChar));
        SaveHistory();
    }

    public void StepForward()
    {
        List<Symbol> nextWord = new List<Symbol>();

        foreach (Symbol sym in currentWord)
        {
            Rule matchedRule = FindMatchingRule(sym);

            if (matchedRule != null)
            {
                // Substitui pelo sucessor da regra
                foreach (Symbol succ in matchedRule.successor)
                {
                    nextWord.Add(succ.Clone());
                }
            }
            else
            {
                // Produção identidade: copia sem alterações
                nextWord.Add(sym.Clone());
            }
        }

        // Verifica se a evolução terminou
        if (IsWordEqual(currentWord, nextWord))
        {
            Debug.Log("A evolução do sistema terminou!");
            return;
        }

        currentWord = nextWord;
        SaveHistory();
        PrintCurrentWord();
    }

    private Rule FindMatchingRule(Symbol sym)
    {
        foreach (Rule r in rules)
        {
            if (r.predecessor == sym.character)
                return r;
        }
        return null; // Aplica regra de identidade se não encontrar
    }

    private void SaveHistory()
    {
        List<Symbol> copy = new List<Symbol>();
        foreach (Symbol s in currentWord) copy.Add(s.Clone());
        history.Add(copy);
    }

    private bool IsWordEqual(List<Symbol> w1, List<Symbol> w2)
    {
        if (w1.Count != w2.Count) return false;
        for (int i = 0; i < w1.Count; i++)
        {
            if (w1[i].character != w2[i].character) return false;
        }
        return true;
    }

    private List<Symbol> CreateSymbolList(string input)
    {
        List<Symbol> list = new List<Symbol>();
        string[] tokens = input.Split(' ');
        foreach (string t in tokens)
        {
            if (!string.IsNullOrEmpty(t))
                list.Add(new Symbol(t[0]));
        }
        return list;
    }

    private void PrintCurrentWord()
    {
        string wordStr = "";
        foreach (Symbol s in currentWord) wordStr += s.character;
        Debug.Log("Passo " + (history.Count - 1) + ": " + wordStr);
    }
}
