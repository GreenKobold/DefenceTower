using UnityEngine;
public enum StructureType
{
    Tower, Wall, Core
}
public abstract class Structure : MonoBehaviour
{
    StructureType type;
    float HP = 200f;
    float Defence = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region Getters & Setters
    public float GetHP()
    {
        return HP;
    }
    public void SetHP(float health)
    {
        HP = health;
    }
    public float GetDefence()
    {
        return Defence;
    }
    public void SetStructureType(StructureType structureType)
    {
        type = structureType;
    }
    public StructureType GetStructureType()
    {
        return type;
    }
    #endregion
}
