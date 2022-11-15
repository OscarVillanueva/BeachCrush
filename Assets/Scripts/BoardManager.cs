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

    // Definir una matriz
    private GameObject[,] candies;

    // Saber cual candy fue seleccionado
    private CandyController selectedCandy;

    // saber cuantas coincidencias necesitamos para hacer un match, 2 mas el actual
    public const int MinCandiesToMatch = 2;

    // Saber cuales son los candies posibles y sus ids
    private Dictionary<int, Sprite> bookOfCandies = new();

    private void Start()
    {
        if (!sharedInstance) sharedInstance = this;
        else Destroy(gameObject);

        Vector2 offset = currentCandy.GetComponent<BoxCollider2D>().size;
        CreateInitialBoard(offset + new Vector2(0.1f, 0.5f));
    }

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

        do
        {
            index = Random.Range(0, prefabs.Count);
        }
        while (
            // Si no estamos en la columna 0 y el index es igual al de la derecha
            (x > 0 && index == candies[x - 1, y].GetComponent<CandyController>().id) ||
            // para vericar con el vecino de abajo
            (y > 0 && index == candies[x, y - 1].GetComponent<CandyController>().id)
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
            possibleKeyCandies.Remove(candies[x, y-1].GetComponent<CandyController>().id);
        }

        keyCandy = possibleKeyCandies[Random.Range(0, possibleKeyCandies.Count)];
        Candy newCandy = new(bookOfCandies[keyCandy], keyCandy);

        return newCandy;
    }

    public IEnumerator FindNullableCandies()
    {
        // TODO: Cambiar el segundo for por un while para evitar el break;
        for(int x = 0; x < xSize; x++)
            for(int y = 0; y < ySize; y++)
            {
                if (!candies[x,y].GetComponent<SpriteRenderer>().sprite)
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
        IsShifting = true;

        List<GameObject> renderers = new List<GameObject>();
        int nullCandies = 0;

        for(int y = ystart; y < ySize; y++)
        {
            GameObject candy = candies[x, y];
            SpriteRenderer spriteRenderer = candy.GetComponent<SpriteRenderer>();

            if (!spriteRenderer.sprite) nullCandies = nullCandies + 1;

            renderers.Add(candy);
        }

        for (int i = 0; i < nullCandies; i++)
        {
            yield return new WaitForSeconds(shiftDelay);

            for (int j = 0; j < renderers.Count - 1; j++)
            {
                Candy newCandy = GenerateCandy(x, ySize - 1);

                renderers[j].GetComponent<CandyController>().id = renderers[j + 1].GetComponent<CandyController>().id;
                renderers[j + 1].GetComponent<CandyController>().id = newCandy.ID;

                renderers[j].GetComponent<SpriteRenderer>().sprite = renderers[j + 1].GetComponent<SpriteRenderer>().sprite;
                renderers[j + 1].GetComponent<SpriteRenderer>().sprite = newCandy.SpriteCandy;
            }
        }

        IsShifting = false;
    }

}
