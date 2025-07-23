using UnityEngine;
using System.Collections; // Necesario para las Corutinas

public class GameManager : MonoBehaviour
{
    // VARIABLES PÚBLICAS (Para conectar en el editor de Unity)
    [Header("Configuración de Juego")]
    [SerializeField] private GameObject blockPrefab;    // El prefab del bloque que creamos.
    [SerializeField] private Transform lastBlock;      // El último bloque de la torre, para saber dónde crear el siguiente.
    
    [Header("Parámetros de Dificultad")]
    [SerializeField] private float minMoveSpeed = 1.5f; // La velocidad mínima a la que se moverá un bloque.
    [SerializeField] private float maxMoveSpeed = 4.0f; // La velocidad máxima.

    // NUEVO: Variables para la cámara y el sonido
    [Header("Configuración de Cámara y Sonido")]
    [SerializeField] private Camera mainCamera; // La cámara principal de la escena.
    [SerializeField] private float cameraFollowSpeed = 5f;

       // NUEVO: Reorganizamos los sonidos para más claridad
    [Header("Clips de Audio")]
    [SerializeField] private AudioClip musicLoop;      // Música de fondo
    [SerializeField] private AudioClip placeSound;     // Sonido de colocación normal
    [SerializeField] private AudioClip perfectSound;   // Sonido de colocación perfecta
    [SerializeField] private AudioClip gameOverSound; // Sonido de fin de juego

    // VARIABLES PRIVADAS (Para uso interno del script)
    private GameObject currentBlock;    // El bloque que el jugador está controlando actualmente.
    private bool blockIsMoving = false; // Flag para saber si un bloque está en movimiento.
    private bool gameOver = false;      // Flag para detener el juego.
    private float currentMoveSpeed;     // La velocidad del bloque actual.
    private float moveLimit = 4.5f;     // Hasta dónde se moverá el bloque en el eje X.
    
        // NUEVO: Variables para la lógica de sonido y cámara
    private AudioSource audioSource;
    private Vector3 cameraTargetPosition;

    // Puntuación (Según el GDD)
    private int score = 0;
    
    // NUEVO: Una variable para recordar cuál fue el primer bloque (el suelo).
    private Transform firstBlock;

    // Se ejecuta una sola vez cuando el juego empieza.
        void Start()
    {

        // NUEVO: Guardamos una referencia al suelo como el primer bloque de la torre.
        firstBlock = lastBlock;

        audioSource = GetComponent<AudioSource>();
        
        // NUEVO: Configurar y reproducir la música de fondo
        if (musicLoop != null)
        {
            audioSource.clip = musicLoop; // Asignamos el clip de música
            audioSource.loop = true;      // Nos aseguramos de que esté en modo loop
            audioSource.Play();           // ¡La reproducimos!
        }

        if (mainCamera != null)
        {
            cameraTargetPosition = mainCamera.transform.position;
        }
        
        SpawnBlock();
    }

    // Se ejecuta en cada frame.
    void Update()
    {
        // Si el juego ha terminado, no hacemos nada.
        if (gameOver)
        {
            return;
        }

        // Si hay un bloque en movimiento, lo movemos.
        if (blockIsMoving)
        {
            MoveBlock();
        }

        // Si el jugador hace clic (o toca la pantalla).
        if (Input.GetMouseButtonDown(0))
        {
            PlaceBlock();
        }
         // NUEVO: En cada frame, movemos suavemente la cámara hacia su posición objetivo.
        // La cámara sigue al jugador solo si el juego NO ha terminado.
        if (mainCamera != null && !gameOver)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, cameraTargetPosition, Time.deltaTime * cameraFollowSpeed);
        }
    }

    void SpawnBlock()
    {
        // Creamos una nueva instancia del bloque usando el prefab.
        currentBlock = Instantiate(blockPrefab);

        // Lo posicionamos un nivel por encima del último bloque.
        Vector3 newPos = lastBlock.position;
        newPos.y += lastBlock.localScale.y;
        currentBlock.transform.position = newPos;

        // Le damos el mismo tamaño que el último bloque.
        currentBlock.transform.localScale = lastBlock.transform.localScale;
        // NUEVO: Desactivamos el collider para que no pueda chocar.
        currentBlock.GetComponent<Collider2D>().enabled = false;
        // Le asignamos una velocidad aleatoria para esta ronda.
        currentMoveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

        // Activamos el movimiento.
        blockIsMoving = true;
    }

    void MoveBlock()
    {
        // Usamos una función de seno para un movimiento suave de vaivén.
        float xPos = Mathf.PingPong(Time.time * currentMoveSpeed, moveLimit * 2) - moveLimit;
        
        Vector3 newPos = currentBlock.transform.position;
        newPos.x = xPos;
        currentBlock.transform.position = newPos;
    }

    void PlaceBlock()
    {
        if (currentBlock == null) return;

        // Detenemos el movimiento.
        blockIsMoving = false;

        // NUEVO: Reactivamos el collider justo antes de soltarlo.
        currentBlock.GetComponent<Collider2D>().enabled = true;

        // Calculamos la diferencia de posición entre el bloque actual y el anterior.
        float offset = currentBlock.transform.position.x - lastBlock.position.x;
        float absoluteOffset = Mathf.Abs(offset);

        // --- LÓGICA DE CORTE ---
        if (absoluteOffset >= lastBlock.localScale.x)
        {
            // El jugador falló completamente.
            EndGame();
            Destroy(currentBlock); // Destruimos el bloque que cayó.
            return;
        }

        // Creamos la pieza que se cae (el sobrante).
        if (absoluteOffset > 0.01f) // Solo si no es una colocación casi perfecta.
        {
            GameObject fallingPiece = Instantiate(blockPrefab);
            fallingPiece.transform.localScale = new Vector3(absoluteOffset, currentBlock.transform.localScale.y, 1);
            
            float fallingPieceX = currentBlock.transform.position.x + (currentBlock.transform.localScale.x / 2 * Mathf.Sign(offset)) - (absoluteOffset / 2 * Mathf.Sign(offset));
            fallingPiece.transform.position = new Vector3(fallingPieceX, currentBlock.transform.position.y, 0);
            
            // Hacemos que la pieza sobrante caiga.
            fallingPiece.GetComponent<Rigidbody2D>().isKinematic = false;
            Destroy(fallingPiece, 20f); // La destruimos después de 2 segundos.
        }

        // Ajustamos el bloque actual para que sea la nueva base.
        float newWidth = currentBlock.transform.localScale.x - absoluteOffset;
        currentBlock.transform.localScale = new Vector3(newWidth, currentBlock.transform.localScale.y, 1);

        float newX = lastBlock.position.x + (offset / 2);
        currentBlock.transform.position = new Vector3(newX, currentBlock.transform.position.y, 0);

        // --- PUNTUACIÓN ---
        // Definimos "perfecto" como un offset muy pequeño.
        // --- PUNTUACIÓN Y SONIDO ---
        if (absoluteOffset < 0.05f)
        {
            score += 5;
            Debug.Log("¡Perfecto! Puntuación: " + score);
            // NUEVO: Reproducimos el sonido de colocación perfecta.
            if (perfectSound != null) audioSource.PlayOneShot(perfectSound);
        }
        else
        {
            score += 1;
            Debug.Log("Puntuación: " + score);
            // NUEVO: Reproducimos el sonido de colocación normal.
            if (placeSound != null) audioSource.PlayOneShot(placeSound);
        }
        
        // El bloque actual se convierte en el "último bloque" para la siguiente ronda.
        lastBlock = currentBlock.transform;

        // AHORA: Convertimos el bloque en un objeto estático. No se moverá por nada.
        currentBlock.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        cameraTargetPosition.y = lastBlock.position.y + 3f; // El '3f' es un buen offset, puedes ajustarlo.

        // Creamos el siguiente bloque.
        StartCoroutine(WaitAndSpawn());
    }
    
    // Pequeña espera para que el jugador vea el resultado antes de que aparezca el siguiente bloque.
    IEnumerator WaitAndSpawn()
    {
        yield return new WaitForSeconds(0.2f);
        SpawnBlock();
    }

    void EndGame()
    {
        if (gameOver) return; // Evita que se ejecute múltiples veces

        gameOver = true;
        Debug.Log("¡GAME OVER! Puntuación Final: " + score);

        // NUEVO: Reproducir el sonido de Game Over
        if (gameOverSound != null)
        {
            audioSource.Stop(); // Detenemos la música de fondo
            audioSource.PlayOneShot(gameOverSound); // Reproducimos el sonido de fin de juego
        }
        // NUEVO: Iniciamos la animación de la cámara.
        StartCoroutine(ZoomOutOnGameOver());
    }


    // NUEVO: Corutina completa para animar la cámara al final del juego.
    IEnumerator ZoomOutOnGameOver()
    {
        float duration = 2.5f; // Duración de la animación de zoom en segundos.
        float elapsedTime = 0f;

        Vector3 startCameraPos = mainCamera.transform.position;
        float startOrthoSize = mainCamera.orthographicSize;

        // --- Calculamos el encuadre final ---
        // 1. Altura de la torre
        float towerHeight = lastBlock.position.y - firstBlock.position.y;
        
        // 2. Posición Y central de la torre
        float targetCameraY = firstBlock.position.y + (towerHeight / 2);
        Vector3 targetCameraPos = new Vector3(mainCamera.transform.position.x, targetCameraY, mainCamera.transform.position.z);
        
        // 3. Tamaño de zoom necesario para que quepa la torre + un poco de margen (padding)
        float targetOrthoSize = (towerHeight / 2f) + 2f; // El +2f es el margen

        // --- Animamos durante el tiempo definido ---
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / duration); // Usamos SmoothStep para una animación más suave.

            // Movemos y hacemos zoom a la cámara gradualmente
            mainCamera.transform.position = Vector3.Lerp(startCameraPos, targetCameraPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(startOrthoSize, targetOrthoSize, t);

            yield return null; // Espera al siguiente frame
        }

        // Aseguramos que la cámara termine exactamente en la posición y zoom final.
        mainCamera.transform.position = targetCameraPos;
        mainCamera.orthographicSize = targetOrthoSize;
    }

}