using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Rule
{
    // 1. Predecessor: O símbolo de origem que vai ser substituído
    public char predecessor; 

    // 2. Sucessor: A lista de novos símbolos que vão substituir o predecessor
    public List<Symbol> successor; 

    // Construtor da classe
    public Rule(char predecessor, List<Symbol> successor)
    {
        this.predecessor = predecessor;
        this.successor = successor;
    }
}
