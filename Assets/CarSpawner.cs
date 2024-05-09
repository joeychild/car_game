using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using PathCreation;
using System;
public class CarSpawner : MonoBehaviour
{
    [Header ("Paths")]
    public string directoryPath;
    public string palettePath;
    string[] palettes;
    public Material[] mats;

    [Header ("Parameters")]
    public int roadWidth;
    public int lanes;
    public int length;
    public int spawnDistance;
    public float spawnProbability;
    public int minSpeed;
    public int maxSpeed;

    [Header ("References")]
    public driving playerCar;
    public PathCreator pathCreator;
    public daynight dayNight;
    string[] prefabs;
    GameObject prefab;
    npcdriving script;
    [HideInInspector]
    public GameObject[,] npcs;
    
    void Start()
    {
        prefabs = Directory.GetFiles(directoryPath, "*.prefab", SearchOption.AllDirectories);
        StartCoroutine(Spawner());
        npcs = new GameObject[length, lanes];
        // StartSpawn();
        palettes = Directory.GetFiles(palettePath, "*.png", SearchOption.AllDirectories);
        // foreach(var palette in palettes)
        // {
        //     Debug.Log(palette);
        // }
    }

    IEnumerator Spawner()
    {
        while (true)
        {
            prefab = Instantiate(AssetDatabase.LoadAssetAtPath(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], typeof(GameObject)) as GameObject, new Vector3(0,0,0), Quaternion.identity);
            script = prefab.GetComponent<npcdriving>();
            // Debug.Log(Array.Find(prefab.GetComponentsInChildren<MeshRenderer>(), FindBody));
            Material[] carmats = Array.Find(prefab.GetComponentsInChildren<MeshRenderer>(), FindBody).materials;
            carmats[0] = mats[UnityEngine.Random.Range(0, mats.Length)];
            Array.Find(prefab.GetComponentsInChildren<MeshRenderer>(), FindBody).materials = carmats;
            // Debug.Log("carmats set");
            // Debug.Log("carmat texture: " + Resources.Load<Texture>(palettes[UnityEngine.Random.Range(0, palettes.Length)]).name);
            // Debug.Log("failed");
            // carmats[0].SetTexture("_MainTex", Resources.Load<Texture>(palettes[UnityEngine.Random.Range(0, palettes.Length)]));
            // Debug.Log("carmats actually set");
            prefab.GetComponent<CarLights>().dayNightScript = dayNight;

            script.playerCar = playerCar;
            script.speed = UnityEngine.Random.Range(50, 60);
            script.path = pathCreator;
            script.distance = spawnDistance * UnityEngine.Random.Range(0, (int)script.path.path.length / spawnDistance);
            script.offset = 5 * UnityEngine.Random.Range(-2, (int)2);
            yield return new WaitForSeconds(1);
        }
    }

    void StartSpawn() //everything is broken
    {
        for(int i = 0; i < npcs.GetLength(0); i++)
        {
            for(int j = 0; j < npcs.GetLength(1); j++)
            {
                if(UnityEngine.Random.Range(0, 1) < spawnProbability)
                {
                    prefab = Instantiate(AssetDatabase.LoadAssetAtPath(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], typeof(GameObject)) as GameObject, pathCreator.path.GetPoint(i / length), pathCreator.path.GetRotation(i / length));
                    script = prefab.GetComponent<npcdriving>();
                    script.path = pathCreator;
                    script.distance = i / length;
                    script.offset = roadWidth / lanes * j - roadWidth;

                    Debug.Log(Array.Find(prefab.GetComponentsInChildren<MeshRenderer>(), FindBody));
                    Material[] carmats = Array.Find(prefab.GetComponentsInChildren<MeshRenderer>(), FindBody).materials;
                    carmats[0] = mats[UnityEngine.Random.Range(0, mats.Length)];
                    Array.Find(prefab.GetComponentsInChildren<MeshRenderer>(), FindBody).materials = carmats;
                    prefab.GetComponent<CarLights>().dayNightScript = dayNight;

                    script.playerCar = playerCar;
                    script.speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
                    script.path = pathCreator;
                    script.carSpawner = GetComponent<CarSpawner>();
                }
            }
        }
    }

    bool FindBody(MeshRenderer mR)
    {
        // Debug.Log(mR.transform.name + " " + mR.transform.name.Equals("body"));
        return mR.transform.name.Equals("body");
    }
}
