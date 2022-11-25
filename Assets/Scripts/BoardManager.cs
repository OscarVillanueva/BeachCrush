using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager sharedInstance;

    public GameObject currentCandy;
    public int xSize;
    public int ySize;

    // Definir que solo puede ser accedido desde el BoardManager
    public bool IsShifting { get; set; }

    [SerializeField] private List<Sprite> prefabs = new List<Sprite>();
    [SerializeField] AudioClip candyAppearingSound;
    [SerializeField] AudioClip comboSound;

    // Definir una matriz
    private GameObject[,] candies;

    // saber cuantas coincidencias necesitamos para hacer un match, 2 mas el actual
    public const int MinCandiesToMatch = 2;

    // Saber cuales son los candies posibles y sus ids
    private readonly Dictionary<int, Sprite> bookOfCandies = new();

    // Lista con los pesos que va a tener cada sprite
    private readonly List<int> weights = new();

    // Lista de la serie acumulativa
    private readonly List<int> serie = new();

    // Saber si estamos en challenge
    private bool isChallenge;

    // Saber que sprite estamos buscando
    private int lookingForID;

    private int combo = 0;

    private bool alreadyPlayed;

    public bool IsChallenge { get => isChallenge; }
    public int LookingForID { get => lookingForID; }

    public int Combo {
        get => combo;
        set {

            combo = value;

            if (combo == 0)
            {
                alreadyPlayed = false;

                AudioSource[] audios = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];

                foreach (AudioSource audio in audios)
                {
                    if( !audio.CompareTag("MainCamera") )
                    {
                        Destroy(audio.gameObject, 0.2f);
                    }
                }

            }

            if (combo >= 2)
            {
                if (!alreadyPlayed) AudioSource.PlayClipAtPoint(comboSound, transform.position);
                alreadyPlayed = true;
            }

        }
    }

    private void Start()
    {
        if (!sharedInstance) sharedInstance = this;
        else Destroy(gameObject);

        Vector2 offset = currentCandy.GetComponent<BoxCollider2D>().size;

        isChallenge = PlayerPrefs.GetInt("challenge", 0) == 1 ? true : false;

        if (isChallenge)
        {
            SetWeightForSprite();
            CalculateSerie();
        }

        CreateInitialBoard(offset + new Vector2(0.1f, 0.5f));

        if (isChallenge) GUIManager.sharedInstance.SetChallengeIcon(bookOfCandies[lookingForID]);

    }

    // MARK: - Funtions
    private void CreateInitialBoard(Vector2 offset)
    {
        candies = new GameObject[xSize, ySize];

        float startX = transform.position.x;
        float startY = transform.position.y;

        int idx;

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {

                idx = GetIndexForSprite(candies, x, y);

                Sprite sprite = prefabs[idx];

                GameObject newCandy = Instantiate(
                    currentCandy,
                    new Vector3(startX + (offset.x * x), startY + (offset.y * y), 0),
                    currentCandy.transform.rotation
                );

                newCandy.name = string.Format("Candy[{0}][{1}]", x, y);

                newCandy.GetComponent<SpriteRenderer>().sprite = sprite;
                newCandy.GetComponent<CandyController>().id = idx;

                newCandy.transform.parent = transform;

                if (!bookOfCandies.ContainsKey(idx))
                    bookOfCandies.Add(idx, sprite);

                candies[x, y] = newCandy;
            }
        }

    }

    // Esta función me permite sacar un sprite distinto al de la izquierda y abajo
    private int GetIndexForSprite(GameObject[,] candies, int x, int y)
    {
        int index;
        int count = 0;

        do
        {
            // TODO: Si es un juego normal sacamos el random de 0 a lista de prefabs
            // Si no es normal hacemos las series accumulativas
            if (isChallenge)
            {
                index = GetIndexForChallenge();
            }
            else index = Random.Range(0, prefabs.Count);

            count = count + 1;
        }
        while (
            // Si no estamos en la columna 0 y el index es igual al de la derecha
            ((x > 0 && index == candies[x - 1, y].GetComponent<CandyController>().id) ||
            // para vericar con el vecino de abajo
            (y > 0 && index == candies[x, y - 1].GetComponent<CandyController>().id)) &&
            count < 10*10*10*10
        );

        return index;
    }

    private Candy GenerateCandy(int x, int y)
    {
        List<int> possibleKeyCandies = new(bookOfCandies.Keys);
        int keyCandy;

        // si es positiva significa que tenemos un vecino a la izquierda entoces borramos 
        // el sprite que se igual a ese vecino para no sacar un sprite similar
        if (x > 0)
        {
            possibleKeyCandies.Remove(candies[x - 1, y].GetComponent<CandyController>().id);
        }

        // Buscamos si tenemos vecino a la derecha si es así sacamos el sprite para evitar tener repetidos
        if (x < xSize - 1)
        {
            possibleKeyCandies.Remove(candies[x + 1, y].GetComponent<CandyController>().id);
        }

        // si no estamos en la fila de esta abajo
        if (y > 0)
        {
            possibleKeyCandies.Remove(candies[x, y - 1].GetComponent<CandyController>().id);
        }

        keyCandy = possibleKeyCandies[Random.Range(0, possibleKeyCandies.Count)];
        Candy newCandy = new(bookOfCandies[keyCandy], keyCandy);

        return newCandy;
    }

    private void SetWeightForSprite()
    {
        int weight;
        int min = 2;
        int max = 3;

        for (int i = 0; i < prefabs.Count; i++)
        {
            do
            {
                weight = Random.Range(1, prefabs.Count + 5);
            }
            while (weights.Contains(weight));

            if (weight >= min && weight <= max)  lookingForID = i;

            weights.Add(weight);
        }

    }

    private void CalculateSerie()
    {
        int acc = weights[0];
        
        for (int i = 1; i < weights.Count; i++)
        {
            serie.Add(acc);
            acc = acc + weights[i];
        }

        serie.Add(acc);

    }

    private int GetIndexForChallenge()
    {
        bool founded = false;
        int random = Random.Range(0, serie[serie.Count - 1]);
        int index = 0;
        int j = 0;

        while (!founded && j < serie.Count)
        {

            int left = j > 0 ? serie[j-1] : 0;
            
            if (random >= left && random < serie[j]) founded = true;
            else index = index + 1;

            j = j + 1;
        }

        return index;

    }

    // MARK: - Routines
    public IEnumerator FindNullableCandies()
    {
        // TODO: Cambiar el segundo for por un while para evitar el break;
        for(int x = 0; x < xSize; x++)
            for(int y = 0; y < ySize; y++)
            {
                if (candies[x,y].GetComponent<CandyController>().id == -1)
                {
                    yield return StartCoroutine(MakeCandiesFall(x, y));
                    break;
                }
            }

        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
            {
                candies[x, y].GetComponent<CandyController>().FindAllMatches();
            }
    }

    IEnumerator MakeCandiesFall(int x, int ystart, float shiftDelay = 0.05f)
    {
        // TODO: Recuperar el score
        if (IsShifting) yield break;

        IsShifting = true;
        AudioSource.PlayClipAtPoint(candyAppearingSound, transform.position);

        int walks;
        int walksID;

        for(int y = ystart; y < ySize; y++)
        {
            GameObject candy = candies[x, y];
            int id = candy.GetComponent<CandyController>().id;

            if (id == -1)
            {

                GUIManager.sharedInstance.Score += 10;

                walks = y;
                walksID = -1;

                while (walksID == -1 && walks < ySize)
                {
                    GameObject wCandy = candies[x, walks];
                    walksID = wCandy.GetComponent<CandyController>().id;
                    walks = walks + 1;
                }

                walks = walks - 1;

                if (walksID != -1)
                {
                    int bridge = candies[x, walks].GetComponent<CandyController>().id;

                    candies[x, walks].GetComponent<CandyController>().ResetCandy(candies[x, y].GetComponent<CandyController>().id);
                    candies[x, y].GetComponent<CandyController>().ResetCandy(bridge);
                    yield return new WaitForSeconds(shiftDelay);

                }
                else
                {
                    candies[x, y].GetComponent<CandyController>().ResetCandy(GenerateCandy(x, walks - 1).ID);
                }
            }
        }

        IsShifting = false;
    }

}
