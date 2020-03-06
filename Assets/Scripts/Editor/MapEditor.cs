using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(MapManager))]
public class MapEditor : Editor
{
    private MapManager m_CurrentMapManager;

    private bool m_IsInSquareEditMode = false; // savoir si on est dans le mode edition des squares
    private SquareState m_StateSquareEditMode = SquareState.Normal;
    public void OnEnable()
    {
        m_CurrentMapManager = (MapManager)target;
        LoadEditorState();
    }

    private void LoadEditorState()
    {
        m_IsInSquareEditMode = EditorPrefs.GetBool(nameof(m_IsInSquareEditMode));
        m_StateSquareEditMode = (SquareState)EditorPrefs.GetInt(nameof(m_StateSquareEditMode));

    }

    public void OnDisable()
    {
        SaveEditorState();
    }

    public void SaveEditorState()
    {
        EditorPrefs.SetBool(nameof(m_IsInSquareEditMode), m_IsInSquareEditMode);
        EditorPrefs.SetInt(nameof(m_StateSquareEditMode),(int)m_StateSquareEditMode);
    }
    public override void OnInspectorGUI()
    {
        GUILayout.Label("========== MAP EDITOR ==========", EditorStyles.boldLabel);

        if (GUILayout.Button("Initialize Map Randomly"))
        {
            m_CurrentMapManager.InitializeMapRandomly();
        }
            
        if (GUILayout.Button("Initialize Empty Map"))
        {
            m_CurrentMapManager.InitializeEmptyMap();
        }

        m_IsInSquareEditMode = GUILayout.Toggle(m_IsInSquareEditMode, "Edit Mode");

        if(m_IsInSquareEditMode)
        {
           m_StateSquareEditMode = (SquareState)EditorGUILayout.EnumPopup(m_StateSquareEditMode);
        }


        GUILayout.Label("========== MAP PROPERTIES ==========");
        base.OnInspectorGUI();
    }

    public void OnSceneGUI()
    {
        if (m_IsInSquareEditMode)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Tools.current = Tool.None;
            
            //calcul de la position de l'intersection entre le rau*ycast et le plan XZ
            Vector3 posIntersection = CalculatePlanInteractPositionXZ();

            //on arrondi la pos pour la snsapper sur la grille
            Vector3 posInt = new Vector3((int)posIntersection.x, 0, (int)posIntersection.z);

            if (posIntersection.x >= 0 && posIntersection.x < m_CurrentMapManager.mapData.width
                && posIntersection.z >= 0 && posIntersection.x < m_CurrentMapManager.mapData.height)
            {
                //on affiche le gizmos du square qu'on va editer en fonction dfe la postion
                DisplayGizmosSquareEdit(posInt);

                EditSquareState(posInt);

                m_CurrentMapManager.CreateMapView();
            }

        }

        //Repaint();
       SceneView.RepaintAll();
    }

    private void EditSquareState(Vector3 posInt)//data
    {

        if(Event.current.button == 0)
        {
            switch(Event.current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    //On recup l'index du bon square en fonction de posInt
                    int index = m_CurrentMapManager.GetIndexSquareFromPos(posInt);

                    //On set le state du square en fontion m_StateSquareEditMode
                    m_CurrentMapManager.mapData.grid[index].state = m_StateSquareEditMode;
                    break;

                case EventType.MouseUp:
                    //  m_CurrentMapManager.CreateSquaresView();
                    break;

            }
        }
        //On update la vue
    }
    private void DisplayGizmosSquareEdit(Vector3 postInt)
    {
        //Handles.DrawWireDisc(posIntersection, Vector3.up, 3);
        Handles.color = m_CurrentMapManager.GetColorFromState(m_StateSquareEditMode);

        postInt.x += 0.5f;
        postInt.z += 0.5f;
        Vector3 sizeWireSquare = Vector3.one; //applati le carrer de sélection des square
        sizeWireSquare.y = 0.2f;
        Handles.DrawWireCube(postInt, Vector3.one);
    }


    private Vector3 CalculatePlanInteractPositionXZ()
    {
        //Recup de la pos de la mouse dans le viewport
        Vector2 pos = Event.current.mousePosition;

        //créa d'un rayon dans l'espace 3d par rapport à la camera
       Ray ray = HandleUtility.GUIPointToWorldRay(pos);

        //crea d'un plan temporaire
        Plane plan = new Plane(Vector3.up, Vector3.zero);

        //affiche ne debug le rayon
        Handles.DrawLine(ray.origin, ray.origin + (ray.direction * 1000));

        float distance;
        if(plan.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

}

//possibilité de soit créer un dossier Editor + unityEditdor pour eviter des erreur quand le champ : est Editor
// soit de faire en + du unityEditor, au dessus tu fais un #if UNITY_EDITOR et à la fin du code un #endif.