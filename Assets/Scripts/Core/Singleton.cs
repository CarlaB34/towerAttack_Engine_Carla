using UnityEngine;

//singleton est pour une instance global et unique
//singleton classique
public class Singleton
{
    private static Singleton m_Instance;
    private static Singleton Instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = new Singleton();
            }
            return m_Instance;
        }
    }
    private Singleton()
    {
       
    }
}
