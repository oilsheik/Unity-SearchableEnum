using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer (typeof (SearchableEnumAttribute))]
public class SearchableEnumDrawer : PropertyDrawer
{
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Enum)
        {
            EditorGUI.PropertyField (position, property, label);
            return;
        }

        EditorGUI.BeginProperty (position, label, property);
        position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);
        string currentEnumName = "None";
        if (property.enumValueIndex >= 0 && property.enumValueIndex < property.enumDisplayNames.Length)
        {
            currentEnumName = property.enumDisplayNames[property.enumValueIndex];
        }

        // popup btn
        if (GUI.Button (position, currentEnumName, EditorStyles.popup))
        {
            string[] enumNames = property.enumDisplayNames;
            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;

            var popupContent = new SearchableEnumPopup (enumNames, property.enumValueIndex, (newIndex) =>
            {
                SerializedProperty freshProperty = serializedObject.FindProperty (propertyPath);
                if (freshProperty.enumValueIndex != newIndex)
                {
                    freshProperty.enumValueIndex = newIndex;
                    serializedObject.ApplyModifiedProperties ();
                }
            });

            PopupWindow.Show (position, popupContent);
        }

        EditorGUI.EndProperty ();
    }
}

public class SearchableEnumPopup : PopupWindowContent
{
    static Vector2 POP_UP_WINDOW_SIZE = new (250, 300);
    const int MAX_SEARCH_LEN = 16; //only allow 16 chars in search field
    const string CNTRL_NAME = "SearchEnumField";
    const string TXT_FIELD = "SearchTextField";
    const string CNCL_BTN = "SearchCancelButton";

    readonly string[] enumNames;
    readonly Action<int> onSelect;
    int currentIndex;
    string searchText = "";
    Vector2 scrollPosition;
    List<int> filteredIndices = new ();

    int keyboardSelectionIndex = 0;
    float itemHeight;
    Rect scrollviewRect;

    GUIStyle searchTextFieldStyle;
    GUIStyle searchCancelButtonStyle;

    public SearchableEnumPopup (string[] names, int i, Action<int> action)
    {
        enumNames = names;
        currentIndex = i;
        onSelect = action;
        itemHeight = EditorStyles.radioButton.CalcHeight (new GUIContent ("a"), POP_UP_WINDOW_SIZE.x);

        searchTextFieldStyle = GUI.skin.FindStyle (TXT_FIELD);
        searchCancelButtonStyle = GUI.skin.FindStyle (CNCL_BTN);

        UpdateFilteredList ();
    }

    public override Vector2 GetWindowSize ()
    {
        return POP_UP_WINDOW_SIZE;
    }

    public override void OnGUI (Rect rect)
    {
        Event e = Event.current;
        bool hasResult = filteredIndices.Count > 0;

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Escape)
            {
                editorWindow.Close ();
                e.Use ();
            }

            else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                onSelect (currentIndex);
                editorWindow.Close ();
                e.Use ();
            }

            else if (hasResult && e.keyCode == KeyCode.UpArrow)
            {
                keyboardSelectionIndex = Mathf.Max (0, keyboardSelectionIndex - 1);
                currentIndex = filteredIndices[keyboardSelectionIndex];
                ScrollToSelection ();
                e.Use ();
            }

            else if (hasResult && e.keyCode == KeyCode.DownArrow)
            {
                keyboardSelectionIndex = Mathf.Min (filteredIndices.Count - 1, keyboardSelectionIndex + 1);
                currentIndex = filteredIndices[keyboardSelectionIndex];
                ScrollToSelection ();
                e.Use ();
            }
        }

        DrawSearchBar ();

        //Scrollable list
        DrawEnumList ();
    }

    private void DrawSearchBar ()
    {
        GUILayout.BeginHorizontal (EditorStyles.toolbar);
        GUI.SetNextControlName (CNTRL_NAME);
        string newSearchText = GUILayout.TextField (searchText, MAX_SEARCH_LEN, searchTextFieldStyle);

        if (GUILayout.Button ("", searchCancelButtonStyle))
        {
            newSearchText = "";
            GUI.FocusControl (null);
        }

        GUILayout.EndHorizontal ();

        // If search text changed, update our filtered list
        if (newSearchText != searchText)
        {
            searchText = newSearchText;
            UpdateFilteredList ();
        }

        EditorGUI.FocusTextInControl (CNTRL_NAME);
    }

    private void DrawEnumList ()
    {
        scrollPosition = GUILayout.BeginScrollView (scrollPosition);

        for (int i = 0; i < filteredIndices.Count; i++)
        {
            int originalIndex = filteredIndices[i];
            bool isSelected = originalIndex == currentIndex;

            if (GUILayout.Toggle (isSelected, enumNames[originalIndex], EditorStyles.radioButton))
            {
                //close the window if the items was not already selected
                //or if it's the only item left
                if (!isSelected || filteredIndices.Count == 1)
                {
                    currentIndex = originalIndex;
                    onSelect (currentIndex);
                    editorWindow.Close ();
                }
            }
        }

        GUILayout.EndScrollView ();

        //auto-scrolling
        if (Event.current.type == EventType.Repaint)
        {
            scrollviewRect = GUILayoutUtility.GetLastRect ();
        }
    }

    private void UpdateFilteredList ()
    {
        filteredIndices.Clear ();
        for (int i = 0; i < enumNames.Length; i++)
        {
            if (string.IsNullOrEmpty (searchText) ||
                enumNames[i].IndexOf (searchText, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                filteredIndices.Add (i);
            }
        }

        int currentFilteredIndex = filteredIndices.IndexOf (currentIndex);
        keyboardSelectionIndex = (currentFilteredIndex != -1) ? currentFilteredIndex : 0;

        // Clamp to ensure it's always valid
        if (filteredIndices.Count > 0)
        {
            keyboardSelectionIndex = Mathf.Clamp (keyboardSelectionIndex, 0, filteredIndices.Count - 1);
        }
    }

    private void ScrollToSelection ()
    {
        if (filteredIndices.Count == 0)
        {
            return;
        }

        float selectionYPos = keyboardSelectionIndex * itemHeight;

        if (selectionYPos < scrollPosition.y)
        {
            scrollPosition.y = selectionYPos;
        }

        else if (selectionYPos + itemHeight > scrollPosition.y + scrollviewRect.height)
        {
            scrollPosition.y = selectionYPos + itemHeight - scrollviewRect.height;
        }
    }
}