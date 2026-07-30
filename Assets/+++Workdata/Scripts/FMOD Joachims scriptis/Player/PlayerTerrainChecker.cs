using UnityEngine;

public enum CURRENT_TERRAIN { WOOD, GRASS, STONE, Gravel, Carpet }

public class PlayerTerrainChecker : MonoBehaviour
{


    [SerializeField]
    private CURRENT_TERRAIN currentTerrain;
    public CURRENT_TERRAIN CurrentTerrain => currentTerrain;
    private FMOD.Studio.EventInstance footsteps;

    [Header("Raycast Settings")]
    [SerializeField] private float rayOriginOffset = 0.5f;
    [SerializeField] private float rayLength = 1.5f;
    
    private void Update()
    {
        DetermineTerrain();
        print(GetTerrainParameter());
    }

    private void DetermineTerrain()
    {
        Vector3 origin = transform.position + Vector3.up * rayOriginOffset;
        RaycastHit hit;

        // Nur den ersten (obersten) Treffer auswerten
        if (Physics.Raycast(origin, Vector3.down, out hit, rayLength))
        {
            string tag = hit.collider.gameObject.tag;

            switch (tag)
            {
                case "Gravel":
                    currentTerrain = CURRENT_TERRAIN.Gravel;
                    break;
                case "Grass":
                    currentTerrain = CURRENT_TERRAIN.GRASS;
                    break;
                case "Stone":
                    currentTerrain = CURRENT_TERRAIN.STONE;
                    break;
                case "Wood":
                    currentTerrain = CURRENT_TERRAIN.WOOD;
                    break;
                
                case "Carpet":
                    currentTerrain = CURRENT_TERRAIN.Carpet;
                    break;
                
                
               // default:
                    // Falls kein bekannter Tag gefunden wird, bleibe beim aktuellen Terrain
                    //#todo Default Sound abspielen z.B. 
                 //   currentTerrain = CURRENT_TERRAIN.DIRT;
                  //  break;
            }
        }
    }

    public int GetTerrainParameter()
    {
        return (int)currentTerrain;
    }
}
