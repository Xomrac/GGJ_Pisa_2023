using System;
using System.Collections;
using Jam.General;
using Rewired;
using Unity.Mathematics;
using UnityEngine;

public class MovementAroundPlanet : MonoBehaviour
{
    private float radius;
    public float vel;
    private Vector3 currCenterPlanet;
    private Player player;
    public Transform piedi;
    [Range(0, 6.28f)] private float angle;
    public PlanetStats firstPlanet;
    public PlanetStats second;
    public PlanetStats curr;
    public GameObject basePrefab;
    public GameObject interact;
    public GameObject move;
    public GameObject drop;
    public GameObject iddle;
    public bool canMove;


    private void Start()
    {
        
    }

    public void OnLand(PlanetStats planet)
    {
        player = ReInput.players.GetPlayer(0);
        // player.controllers.maps.SetMapsEnabled(false, RewiredConsts.Category.OnPlanet);
        curr = planet;
        switch (planet.fert)
        {
            case Fertility.Fertile:
                AudioManaegr.Instance.playmusic("OstMain");
                break;
            case Fertility.NotSoFertile:
                AudioManaegr.Instance.playmusic("OstMain");
                break;
            case Fertility.Barren:
                AudioManaegr.Instance.playmusic("ostSterileBarren");
                break;
            case Fertility.Unfertile:
                AudioManaegr.Instance.playmusic("ostSterileBarren");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if (curr.wasVisited==false)
        {
            curr.SpawnPonds();
        }
        curr.wasVisited = true;
        transform.position = new Vector3(planet.landingPoint.position.x,
            planet.landingPoint.position.y+ (GetComponent<Collider>().bounds.extents.y ),
            planet.landingPoint.position.z);
        currCenterPlanet = planet.transform.position;
        // player.controllers.maps.SetMapsEnabled(true, RewiredConsts.Category.OnPlanet);
        radius = Vector3.Distance(transform.position, planet.gameObject.transform.position);
        planet.lastVisitedTime = 0;
        Debug.DrawLine(transform.position, currCenterPlanet, Color.red, 10f);
        foreach (var instancePlanet in PlanetsManager.Instance.planets)
        {
            if (instancePlanet != planet)
            {
                foreach (var VARIABLE in instancePlanet.Roots)
                {
                    VARIABLE.wasUsedInThisVisit = false;
                }
            }
        }
    }

    public void Move()
    {
        Vector3 newPos;
        angle += vel * Input.GetAxis("Horizontal") * Time.deltaTime;
        float angledegrees = angle * Mathf.Rad2Deg;
        newPos.x = currCenterPlanet.x + (radius * math.sin(angle));
        newPos.y = currCenterPlanet.y + (radius * math.cos(angle));
        newPos.z = transform.position.z;
        Quaternion rot = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y,-angledegrees);
        transform.position = newPos;
        transform.rotation = rot;
    }

    private void Update()
    {

        if ((player.GetAxis(RewiredConsts.Action.Move) != 0) && canMove)
        { 
            Move();
           move.SetActive(true);
           iddle.SetActive(false);
        }
        else if (canMove)
        {
            move.SetActive(false);
            iddle.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (curr.Roots.Count <= 2)
            {
                canMove = false;
                drop.SetActive(true);
                move.SetActive(false);
                iddle.SetActive(false);
                var temp = Instantiate(basePrefab,new Vector3(piedi.position.x,piedi.position.y,piedi.position.z+70f), transform.rotation);
                temp.GetComponentInChildren<MeshRenderer>().material.SetFloat("_Height", 0);
                temp.GetComponentInChildren<Root>().wasUsedInThisVisit = true;
                temp.GetComponentInChildren<MeshRenderer>().material.SetFloat("_Radius", radius-(GetComponent<Collider>().bounds.extents.y ));
                temp.GetComponentInChildren<Root>().maxRadius = Vector3.Distance(piedi.position, curr.transform.position);
                StartCoroutine(temp.GetComponentInChildren<Root>().RootAnimation(40));
                temp.GetComponentInChildren<Root>().planetWhereIsPlanted = curr;
                temp.GetComponentInChildren<Root>().setHeights();
                curr.Roots.Add(temp.GetComponentInChildren<Root>());
                StartCoroutine(waitForSecondsMove(2));
                foreach (var instancePlanet in PlanetsManager.Instance.planets)
                {
                    if (instancePlanet != curr)
                    {
                        foreach (var VARIABLE in instancePlanet.Roots)
                        {
                            VARIABLE.Decrease();
                        }
                        //instancePlanet.lastVisitedTime++;
                    }
                }
            }
        }
    }

    public IEnumerator waitForSecondsMove(float sec)
    {
        yield return new WaitForSeconds(sec);
        canMove = true;
        interact.SetActive(false);
        drop.SetActive(false);
        iddle.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.GetComponent<Root>())
        {
            var temp = other.GetComponent<Root>();
            if (!temp.wasUsedInThisVisit)
            {
                canMove = false;
                move.SetActive(false);
                iddle.SetActive(false);
                interact.SetActive(true);
                StartCoroutine(waitForSecondsMove(2));
                StartCoroutine(temp.RootAnimation(Mathf.Clamp(temp.currGrowth+temp.howMuchGrows,0,temp.maxRadius)));
            }
        }
    }
}