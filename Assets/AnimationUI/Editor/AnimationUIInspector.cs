using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AnimationUI))]
public class AnimationUIInspector : Editor
{
    
    public override void OnInspectorGUI()
    {
        AnimationUI animationUI = (AnimationUI)target;
        if(animationUI.AnimationSequence == null) //Prevent error when adding component
        {
            DrawDefaultInspector();
            return;
        }

#region buttons
        if(!animationUI.IsPlaying)
        {
            if(GUILayout.Button("Preview Animation"))
            {
                animationUI.PreviewAnimation();
            }
        }
        else 
        {
            Color defaultGUIColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if(GUILayout.Button("Stop Animation"))
            {
                animationUI.IsPlaying = false;
            }
            GUI.backgroundColor = defaultGUIColor;
        }
        GUILayout.BeginHorizontal();
            if(GUILayout.Button("Preview Start"))
            {
                animationUI.PreviewStart();
                
            }
            else if(GUILayout.Button("Preview End"))
            {
                animationUI.PreviewEnd();
            }
        GUILayout.EndHorizontal();
#endregion buttons

#region timing
        animationUI.InitTime();
        animationUI.CurrentTime = GUILayout.HorizontalSlider(animationUI.CurrentTime, 
            0, animationUI.TotalDuration, GUILayout.ExpandWidth(true), GUILayout.Height(20));
        if(!animationUI.IsPlaying)animationUI.UpdateBySlider();

        Color defaultColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.2f, 1, 0.2f);
        Rect position = GUILayoutUtility.GetLastRect();
        EditorGUI.ProgressBar(position, animationUI.CurrentTime/animationUI.TotalDuration, 
            Mathf.Clamp((Mathf.Round(animationUI.CurrentTime*100)/100), 0, 100)
                .ToString()+"/"+animationUI.TotalDuration.ToString()+" Seconds, ["+(Mathf.Round(animationUI.CurrentTime/animationUI.TotalDuration*10000)/100)
                .ToString()+"%]"
        );
        GUI.backgroundColor = defaultColor;
        
        DrawDefaultInspector();

#endregion timing

#region List
        float _currentTime = 0;
        
        Rect rect = GUILayoutUtility.GetLastRect();

        if(animationUI.AnimationSequence.Length > 0)
            if(animationUI.AnimationSequence[0].PropertyRectY < 115)return; // prevent drawing the white element progress when list is not expanded
        
        foreach(Sequence sequence in animationUI.AnimationSequence)
        {
            Rect currentRect = new Rect(rect.x, sequence.PropertyRectY-3, 5, sequence.PropertyRectHeight+2);
            if(animationUI.CurrentTime >= sequence.StartTime)
            {
                EditorGUI.DrawRect(currentRect, new Color(0.2f, 1, 0.2f, 0.4f));
            }


            sequence.AtTime = "At "+_currentTime.ToString() + "s";
            sequence.StartTime = _currentTime;

            if(sequence.Duration < 0)sequence.Duration = 0;// Clamp
            if(sequence.SequenceType == Sequence.Type.Animation)
            {
                if(sequence.TargetComp != null)
                {
                    sequence.AtTime += " ["+sequence.TargetComp.name+"]";
                    if(sequence.TargetType == Sequence.ObjectType.Automatic)
                    {
                        if(sequence.TargetComp.GetComponent<RectTransform>() != null)
                        {
                            sequence.TargetType = Sequence.ObjectType.RectTransform;
                            sequence.AtTime += " [RectTransform]";
                        }
                        else if(sequence.TargetComp.transform != null)
                        {
                            sequence.TargetType = Sequence.ObjectType.Transform;
                            sequence.AtTime += " [Transform]";
                        }
                    }
                    else if(sequence.TargetType == Sequence.ObjectType.RectTransform)
                    {
                        if(sequence.TargetComp.GetComponent<RectTransform>() != null)sequence.AtTime += " [RectTransform]";
                        else
                        {
                            sequence.TargetComp = null;
                            // sequence.AtTime += " [Unassigned] [RectTransform]";
                        }
                    }
                    else if(sequence.TargetType == Sequence.ObjectType.Image)
                    {
                        if(sequence.TargetComp.GetComponent<Image>() != null)sequence.AtTime += " [Image]";
                        else
                        {
                            sequence.TargetComp = null;
                            // sequence.AtTime += " [Unassigned] [Image]";
                        }
                    }
                    else if(sequence.TargetType == Sequence.ObjectType.Transform)
                    {
                        if(sequence.TargetComp.transform != null)sequence.AtTime += " [Transform]";
                        else
                        {
                            sequence.TargetComp = null;
                            // sequence.AtTime += " [Unassigned] [Transform]";
                        }
                    }
                    else if(sequence.TargetType == Sequence.ObjectType.UnityEventDynamic)
                    {
                        sequence.AtTime += " [UnityEvent]";
                    }
                }
                else // if TargetComp isn't assigned in inspector
                {
                    if(sequence.TargetType == Sequence.ObjectType.Automatic)
                        sequence.AtTime += " [Unassigned] [Animation]";
                    else if(sequence.TargetType == Sequence.ObjectType.RectTransform)
                        sequence.AtTime += " [Unassigned] [RectTransform]";
                    else if(sequence.TargetType == Sequence.ObjectType.Transform)
                        sequence.AtTime += " [Unassigned] [Transform]";
                    else if(sequence.TargetType == Sequence.ObjectType.Image)
                        sequence.AtTime += " [Unassigned] [Image]";
                    else if(sequence.TargetType == Sequence.ObjectType.UnityEventDynamic)
                        sequence.AtTime += " [UnityEvent]";
                }

#region preview element
                if(sequence.TriggerStart)
                {
                    sequence.TriggerStart = false;
                }
                else if(sequence.TriggerEnd)
                {
                    sequence.TriggerEnd = false;
                }
#endregion preview element
            }

            else if(sequence.SequenceType == Sequence.Type.Wait)
            {
                _currentTime += sequence.Duration;
                sequence.AtTime += " [Wait "+sequence.Duration+"s]";
            }
            else if(sequence.SequenceType == Sequence.Type.SetActiveAllInput)
            {
                sequence.AtTime += " [SetActiveAllInput to "+sequence.IsActivating+"]";
            }
            else if(sequence.SequenceType == Sequence.Type.SetActive)
            {
                if(sequence.Target != null)
                {
                    sequence.AtTime += " ["+sequence.Target.name+"] [SetActive to "+sequence.IsActivating+"]";
                }
                else // if Target isn't assigned in inspector
                {
                    sequence.AtTime += " [Unassigned] [SetActive to "+sequence.IsActivating+"]";
                }
            }
            else if(sequence.SequenceType == Sequence.Type.SFX)
            {
                if(sequence.SFX != null)
                    sequence.AtTime += " ["+sequence.SFX.name+"] [SFX]";
                else // if SFX isn't assigned in inspector
                    sequence.AtTime += " [Unassigned] [SFX]";
            }
            else if(sequence.SequenceType == Sequence.Type.UnityEvent)
            {
                sequence.AtTime += " [UnityEvent]";
            }
        }

    }
#endregion List


}
