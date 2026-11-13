using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{

    public float rotationSpeed = 100.0f; // Velocidade de rotação da câmera´

    public float speed = 10.0f; // Velocidade de movimento da câmera

    public float zoomSpeed = 0.5f; // Velocidade de zoom da câmera
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public Transform alvoRotacao; // O objeto que a câmera vai focar e girar em torno dele
        
    // Update is called once per frame
    void LateUpdate()
    {
        if(Mouse.current == null)return;


        if (Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
        {
            // Cria um raio a partir da posição do rato no ecrã
            Ray raio = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            // Se o raio bater nalgum objeto com um Collider (ex: as tuas esferas dos planetas)
            if (Physics.Raycast(raio, out hit))
            {
                alvoRotacao = hit.transform; // Define esse objeto como o novo centro de rotação
                // Escreve na aba "Console" o nome exato do objeto onde acabaste de clicar
            Debug.Log("O Raycast bateu no objeto: " + hit.transform.name);
            
            // Desenha uma linha vermelha visível na janela "Scene" (não na janela Game)
            Debug.DrawRay(raio.origin, raio.direction * hit.distance, Color.red, 2f);
            }
            else
            {
                alvoRotacao = null; // Se clicares no vazio, volta a focar no centro do sistema
                Debug.Log("O Raycast bateu no vazio. Alvo limpo.");
            }
        }

        float scrollValue = Mouse.current.scroll.ReadValue().y;
        
        if (scrollValue != 0)
        {
            Vector3 zoom = new Vector3(0, 0, scrollValue *zoomSpeed);

            // Move a câmera para frente ou para trás com base no valor do scroll
            transform.Translate(zoom);
        }
        
        if (Mouse.current.rightButton.isPressed)
        {
          Vector2 movimentoRato = Mouse.current.delta.ReadValue();

          Vector3 pontoCentral = (alvoRotacao != null) ? alvoRotacao.position : Vector3.zero;

          transform.RotateAround(pontoCentral, Vector3.up, movimentoRato.x * rotationSpeed * Time.deltaTime);

          transform.RotateAround(pontoCentral, transform.right, -movimentoRato.y * rotationSpeed * Time.deltaTime);
        
        }

        
        // Vai ver se o butão do meio do rato esta a ser perssiondo
        if (Mouse.current.middleButton.isPressed)
        {
            //Retira os valores do rato e poem num vetor2 para termos acesso ao X e ao Y
            Vector2 movimentoRato = Mouse.current.delta.ReadValue();
            
            // Inverte os valores do rato para que a camera se mova na direção correta
            float MovimentoX = -movimentoRato.x;
            float MovimentoY = -movimentoRato.y;
            
            // Transforma os valores do rato em um vetor3 para que possamos usar o método Translate
            Vector3 move = new Vector3(MovimentoX, MovimentoY, 0);

            // Mexe a cena do jogo de acordo com o movimento do rato e a velocidade definida
            transform.Translate(move*speed*Time.deltaTime);
        }
    }
}
