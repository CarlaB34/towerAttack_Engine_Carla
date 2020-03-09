using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


[CustomEditor(typeof(MapManager))]
public class MapEditor : Editor
{
    #region VARIABLE BOOL GLOBAL
    // Ref instance editée
    private MapManager m_CurrentTarget = null;

    // Variable Edit Mode
    private bool m_IsInEditSquareMode = false;
    private SquareState m_CurrentEditState = SquareState.Normal;
    //+
    private ConditBrush m_CurrentEditBrush = ConditBrush.One;

    //editor la view
    private bool m_ShowMapView = false;

    //pour la variable de edit mode eddges //+
    private bool m_IsEditEdgeHoriMode = false;
    private bool m_IsEditEdgeVertMode = false;

    // un toglle pour savoir si on ajout des edges vert + hori //+
    private bool m_IsEditAddEdgesHori = false;
    private bool m_IsEditAddEdgesVert = false;

    // un toggle pour savoir si on suprime des edges vert + hori //+
    private bool m_IsEditRemoveEdgesHori = false;
    private bool m_IsEditRemoveEdgesVert = false;

    // Global edit constraint
    private bool m_CanEditMouseAndKeyConstraints = true;
    #endregion
    private void OnEnable()
    {
        // Recupération de l'instance editée et fait le lien avec le script mapmanager
        m_CurrentTarget = (MapManager)target;

        m_ShowMapView = m_CurrentTarget.navContainer.activeSelf;

        LoadLastEditorState();
    }

    private void OnDisable()
    {
        SaveCurrentEditorState();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("============ EDITOR MAP =============", EditorStyles.boldLabel);
        // Affichage Bouton Initialize Map And Generate.
        if (GUILayout.Button("Initialize Map Randomly"))
        {
            m_CurrentTarget.InitializeMapRandomly();
        }

        // Affichage Bouton de Reset de la map.
        if (GUILayout.Button("Initialize Empty Map"))
        {
            m_CurrentTarget.InitiliazeEmptyMap();
        }

        GUILayout.Space(10);
        m_ShowMapView = GUILayout.Toggle(m_ShowMapView, "Show View");
        m_CurrentTarget.navContainer.SetActive(m_ShowMapView);

        GUILayout.Label("============ EDIT SQUARE =============", EditorStyles.boldLabel); //+
        m_IsInEditSquareMode = GUILayout.Toggle(m_IsInEditSquareMode, "Edit Mode");
        if (m_IsInEditSquareMode)
        {
            // Affichage Bouton d'edition du state d'un square de la map, choisir le type
            m_CurrentEditState = (SquareState)EditorGUILayout.EnumPopup(m_CurrentEditState);
            //afffiche l'enumération quand edit mode est activé
            m_CurrentEditBrush = (ConditBrush)EditorGUILayout.EnumPopup(m_CurrentEditBrush);
        }

        GUILayout.Label("============ EDIT EDGES HORIZONTAL =============", EditorStyles.boldLabel); //+
        //affiche le bouton a cocher pour editer les edges hori //+
        m_IsEditEdgeHoriMode = GUILayout.Toggle(m_IsEditEdgeHoriMode, "EdgesHori Mode");
        if(m_IsEditEdgeHoriMode)
        {
            //pour ajouter
            m_IsEditAddEdgesHori = GUILayout.Toggle(m_IsEditAddEdgesHori, "AddEdgesHori Mode");
            // pour supr
            m_IsEditRemoveEdgesHori = GUILayout.Toggle(m_IsEditRemoveEdgesHori, "RemoveEdgesHori Mode");
        }

        GUILayout.Label("============ EDIT EDGES VERTICAL =============", EditorStyles.boldLabel); //+
        //affiche le bouton a cocher pour editer les edges vert //+
        m_IsEditEdgeVertMode = GUILayout.Toggle(m_IsEditEdgeVertMode, "EdgesVert Mode");
        if(m_IsEditEdgeVertMode)
        {
            //pour ajouter
            m_IsEditAddEdgesVert = GUILayout.Toggle(m_IsEditAddEdgesVert, "AddEdgesVert Mode");
            // pour supr
            m_IsEditRemoveEdgesVert = GUILayout.Toggle(m_IsEditRemoveEdgesVert, "RemoveEdgesVert Mode");

        }

        GUILayout.Label("============ MAP PROPERTIES =============", EditorStyles.boldLabel);
        base.OnInspectorGUI();

    }

    // Ici on affiche dans la Scene les elements necessaire.
    // Ici on recupère les inputs qui ont été fait dans la Scene.
    private void OnSceneGUI()
    {
        ///test de l'activation et désactivation //+
        //di si square active = edges desactif
        if (m_IsInEditSquareMode == true)
        {
            m_IsEditEdgeHoriMode = false;
            m_IsEditEdgeVertMode = false;
        }

        //dit si edges actif = square desactif//+
         if (m_IsEditEdgeHoriMode == true)
         {
            m_IsEditEdgeVertMode = false;
            m_IsInEditSquareMode = false;
            // add
            if(m_IsEditAddEdgesHori == true)
            {
                m_IsEditRemoveEdgesHori = false;
            }
            //remove
            if (m_IsEditRemoveEdgesHori == true)
            {
                m_IsEditAddEdgesHori = false;
            }
         }

        //pour le vert
        if (m_IsEditEdgeVertMode == true)
        {
            m_IsEditEdgeHoriMode = false;
            m_IsInEditSquareMode = false;
            // add
            if (m_IsEditAddEdgesVert == true)
            {
                m_IsEditRemoveEdgesVert = false;
            }
            //remove
            if (m_IsEditRemoveEdgesVert == true)
            {
                m_IsEditAddEdgesVert = false;
            }
        }
        ///fin test


        // On valide si on peut editer
        UpdateGlobalEditState();

        // Si on peut editer
        if (m_CanEditMouseAndKeyConstraints)
        {
            // Si on est en edit square mode
            if (m_IsInEditSquareMode)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;

                // Calculate Interact Coordonnee
                //calcul de la position de l'intersection entre le rau*ycast et le plan XZ
                Vector3 intersectPos = CalculateInteractPosition();
                intersectPos.y = 0;

                Vector3 intersectPosInt = Vector3.zero;
                intersectPosInt.x = Mathf.FloorToInt(intersectPos.x);
                intersectPosInt.z = Mathf.FloorToInt(intersectPos.z);

                //Debug.Log("Scene GUI is painted");
                //on affiche le gizmos du square qu'on va editer en fonction dfe la postion
                DisplayGizmoEditSquareInScene(intersectPosInt);

                //mes data
                EditCurrentSquareState(intersectPosInt);
            }
           


            //test de l'afffichage du gizomo pour l'horizontal//+
            if(m_IsEditEdgeHoriMode)//+
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;

                //calcul de la position de l'intersection entre le raycast et le plan XZ
                Vector3 intersectPos = CalculateInteractPosition();
                intersectPos.y = 0;
                Vector3 intersectPosInt = Vector3.zero;
                intersectPosInt.x = Mathf.FloorToInt(intersectPos.x);
                intersectPosInt.z = Mathf.FloorToInt(intersectPos.z);

                //Debug.Log("Scene GUI is painted");
                //on affiche le gizmos du square qu'on va editer en fonction dfe la postion
                DisplayGizmoEditSquareInScene(intersectPosInt);

                //mes data
                EditCurrentEdgesHoriState(intersectPosInt);

            }

            //test de l'afffichage du gizomo pour vertical//+
            if (m_IsEditEdgeVertMode)//+
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;

                //calcul de la position de l'intersection entre le rau*ycast et le plan XZ
                Vector3 intersectPos = CalculateInteractPosition();
                intersectPos.y = 0;
                Vector3 intersectPosInt = Vector3.zero;
                intersectPosInt.x = Mathf.FloorToInt(intersectPos.x);
                intersectPosInt.z = Mathf.FloorToInt(intersectPos.z);

                //Debug.Log("Scene GUI is painted");
                //on affiche le gizmos du square qu'on va editer en fonction dfe la postion
                DisplayGizmoEditSquareInScene(intersectPosInt);

                //mes data //+
                EditCurrentEdgesVertState(intersectPosInt);

            }

        }

       
    }

    private Vector3 CalculateInteractPosition()
    {
        Vector3 mousePosition = Event.current.mousePosition;

        // recupération d'un Ray (rayon) à partir de la position de la mouse sur l'ecran
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        // Creation d'un plan dans l'espace
        // Il n'y a pas de plan créer dans la scene
        Plane hPlane = new Plane(Vector3.up, Vector3.zero);

        // On envoi le rayon par rapport à la scene
        if (hPlane.Raycast(ray, out float distance))
        {
            // get the hit point:
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    #region DISPLAY EDIT SQUARE GIZMO
    private void DisplayGizmoEditSquareInScene(Vector3 intersectPosInt)
    {
        // Affichage du gizmo uniquement si on est dans la grille
        if (TestIfPositionIsInLimit(intersectPosInt))
        {
            DrawSquareSelectedGizmo(intersectPosInt);

        }
        SceneView.RepaintAll();
    }

    //était en private
    public bool TestIfPositionIsInLimit(Vector3 position)
    {
        return position.x >= 0 && position.z >= 0 && position.x < m_CurrentTarget.mapData.width && position.z < m_CurrentTarget.mapData.height;
    }

    //le petit carré de sélection
    private void DrawSquareSelectedGizmo(Vector3 intersectPos) 
    {
        Handles.color = Color.cyan;

        Vector3 pos1 = intersectPos;
        Vector3 pos2 = intersectPos;    
        pos2.x = intersectPos.x + 1;
        float screenSpace = 20;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);
        pos1.x += 1;
        pos1.z += 1;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);

        pos1 = intersectPos;
        pos2.x = intersectPos.x;
        pos2.z = intersectPos.z + 1;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);
        pos1.x += 1;
        pos1.z += 1;
        Handles.DrawDottedLine(pos1, pos2, screenSpace);
    }
    #endregion DISPLAY EDIT SQUARE GIZMO

    #region EDIT SQUARE AND EDGES
    //click de la sourit
    //data
    private void EditCurrentSquareState(Vector3 intersectPos) 
    {
        if (Event.current.button == 0)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    m_CurrentTarget.SetSquareState(intersectPos, m_CurrentEditState);
                    m_CurrentTarget.CreateMapViewFromData();
                    SetObjectDirty(m_CurrentTarget);
                    break;
            }
        }

    }

    //data edges pour les créer hori //+
    private void EditCurrentEdgesHoriState(Vector3 intersectPos)
    {
        if (Event.current.button == 0)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    m_CurrentTarget.SetEdgesHoriState(intersectPos, m_IsEditAddEdgesHori);
                    m_CurrentTarget.CreateMapViewFromData();
                    SetObjectDirty(m_CurrentTarget);
                    break;
            }
        }
    }

    //data edges pour les créer vert //+
    private void EditCurrentEdgesVertState(Vector3 intersectPos) 
    {
        if (Event.current.button == 0)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    m_CurrentTarget.SetEdgesVertState(intersectPos, m_IsEditAddEdgesVert);
                    m_CurrentTarget.CreateMapViewFromData();
                    //  Faire en sorte que si on ajoute une edge et qu'il y a des murs sur ses côtés adjacents? //+
                    // tentative2, de l'exo pour Faire en sorte que si on ajoute une edge et qu'il y a des murs sur ses côtés adjacents 
                    m_CurrentTarget.ProcessRuleSquareVSEdge(m_IsEditEdgeHoriMode,/*width*/ /*m_CurrentTarget.mapData.edgesHori.width*/, m_IsEditAddEdgesHori);
                    SetObjectDirty(m_CurrentTarget);
                    break;
            }
        }
    }

    // Skip Update si en train d'appuyer sur Alt
    private void UpdateGlobalEditState()
    {
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.LeftAlt)
                {
                    m_CanEditMouseAndKeyConstraints = false;
                }
                break;
            case EventType.KeyUp:
                if (e.keyCode == KeyCode.LeftAlt)
                {
                    m_CanEditMouseAndKeyConstraints = true;
                }
                break;
        }
    }
    #endregion

    #region LOAD / SAVE EDITOR STATE
    private void SetObjectDirty(Object objectDirty)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(objectDirty);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    private void LoadLastEditorState()
    {
        m_IsInEditSquareMode = EditorPrefs.GetBool(nameof(m_IsInEditSquareMode));
        m_CurrentEditState = (SquareState)EditorPrefs.GetInt(nameof(m_CurrentEditState));
    }
    private void SaveCurrentEditorState()
    {
        EditorPrefs.SetBool(nameof(m_IsInEditSquareMode), m_IsInEditSquareMode);
        EditorPrefs.SetInt(nameof(m_CurrentEditState), (int)m_CurrentEditState);
    }
    #endregion
}
