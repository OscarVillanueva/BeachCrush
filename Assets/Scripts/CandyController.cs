using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyController : MonoBehaviour
{

    // Todos los que sean CandyController van a poder acceder al color y previous
    // Compartiendo el mismo valor
    private static Color selectedColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    private static CandyController previousSelected = null;

    private SpriteRenderer spriteRenderer; 
    private bool isSelected = false;

    private bool wasMatch = false;

    private int candiesToDestroy = 0;

    private Vector2[] adjacentDirections = new Vector2[] {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    public int id;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void SelectCandy()
    {
        isSelected = true;
        spriteRenderer.color = selectedColor;
        previousSelected = gameObject.GetComponent<CandyController>();
    }

    private void DeselectCandy()
    {
        isSelected = false;
        spriteRenderer.color = Color.white;
        previousSelected = null;
    }

    // Este se ejecuta cuando se da clic sobre un collider en este caso el que se le colloco 
    // al prefab de candy
    private void OnMouseDown()
    {
        if (spriteRenderer.sprite == null || BoardManager.sharedInstance.IsShifting) return;

        BoardManager.sharedInstance.Combo = 0;

        // si yo como candy estoy seleccionado
        if (isSelected) DeselectCandy();
        else
        {
            // si no hay un candy seleccionado, me selecciono yo
            if (!previousSelected) SelectCandy();
            else
            {

                if (CanSwipe())
                {
                    SwapSprite(previousSelected);
                
                    // primero verificamos si el previo que estaba seleccionado provoca un 3 in line
                    previousSelected.FindAllMatches();
                    previousSelected.DeselectCandy();

                    // luego verificamos que el nuevo seleccioando provoca un 3 in line
                    FindAllMatches();

                    // si challenge damos un movimiento extra
                    if (
                        BoardManager.sharedInstance.IsChallenge 
                        && BoardManager.sharedInstance.LookingForID == id 
                        && wasMatch
                    )
                    {
                        GUIManager.sharedInstance.MoveCounter += 1;
                    }
                    else
                    {
                        // Restamos el movimiento
                        GUIManager.sharedInstance.MoveCounter--;
                    }
                }
                else
                {
                    previousSelected.DeselectCandy();
                    SelectCandy();
                }

            }
        }

    }

    public void SwapSprite(CandyController newCandy)
    {
        if (spriteRenderer.sprite == newCandy.GetComponent<SpriteRenderer>().sprite) return;

        Sprite oldCandy = newCandy.spriteRenderer.sprite;
        newCandy.spriteRenderer.sprite = this.spriteRenderer.sprite;
        this.spriteRenderer.sprite = oldCandy;

        int oldID = newCandy.id;
        newCandy.id = this.id;
        this.id = oldID;
    }

    private GameObject GetNeighboar(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction);

        if (hit.collider) return hit.collider.gameObject;

        return null;
    }

    private List<GameObject> GetAllNeighboars()
    {
        List<GameObject> neighbors = new List<GameObject>();

        foreach(Vector2 direction in adjacentDirections)
        {
            neighbors.Add(GetNeighboar(direction));
        }

        return neighbors;
    }

    private bool CanSwipe()
    {
        return GetAllNeighboars().Contains(previousSelected.gameObject);
    }

    // AddRange para agregar una lista en otra
    private List<GameObject> FindMatch(Vector2 direction)
    {
        List<GameObject> matchingCandies = new List<GameObject>();

        // consulta de los vecinos en la dirección del parámetro
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, direction);

        // mientras haya una colision y el id sea igual
        while(hit.collider && hit.collider.GetComponent<CandyController>().id == this.id)
        {
            // si hay una colisión y es igual a mi ahora el vecino pregunta
            matchingCandies.Add(hit.collider.gameObject);
            hit = Physics2D.Raycast(hit.collider.transform.position, direction);
        }

        return matchingCandies;

    }

    private bool ClearMatch(Vector2[] directions)
    {
        List<GameObject> matchingCandies = new List<GameObject>();

        foreach (Vector2 direction in directions)
        {
            matchingCandies.AddRange(FindMatch(direction));
        }

        candiesToDestroy += matchingCandies.Count;

        if (matchingCandies.Count >= BoardManager.MinCandiesToMatch)
        {
            foreach(GameObject candy in matchingCandies)
            {
                candy.GetComponent<SpriteRenderer>().sprite = null;
            }

            return true;
        }

        return false;

    }

    public void FindAllMatches()
    {
        if (!spriteRenderer.sprite) return;

        bool hMatch = ClearMatch(new Vector2[2]
        {
            Vector2.left,
            Vector2.right
        });

        bool vMatch = ClearMatch(new Vector2[2]
        {
            Vector2.up,
            Vector2.down
        });

        // El clearMatch Borra los matches pero ahora hay que borrarnos a nostros mismos
        if (hMatch || vMatch)
        {
            spriteRenderer.sprite = null;

            wasMatch = true;

            candiesToDestroy = candiesToDestroy + 1;

            if (
                BoardManager.sharedInstance.IsChallenge
                && BoardManager.sharedInstance.LookingForID == id
            )
            {
                GUIManager.sharedInstance.SetChallengeValue(candiesToDestroy);
            }

            BoardManager.sharedInstance.Combo += 1;

            // llevamos los espacios vacios
            //BoardManager.sharedInstance.StopCandiesRainRoutine();
            StopCoroutine(BoardManager.sharedInstance.FindNullableCandies());
            StartCoroutine(BoardManager.sharedInstance.FindNullableCandies());
        }
        else
        {
            wasMatch = false;
        }

        candiesToDestroy = 0;
    }


}
