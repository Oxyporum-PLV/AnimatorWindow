using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

public class FindAnimatorWindow : EditorWindow
{

    private GUIStyle titleStyle = null;
    private int tabIndex = 0;
    private int currentAnimIndex = 0;

    public static string[] tabs = new string[] { "Animator", "Animation" };

    private Animator[] animatorComponentsArr = null;
    private AnimationClip[] animClipsArr = null;
    private string[] animNamesArr = null;

    private Animator currenAnimator = null;

    private bool isPlaying = false;

    private float editorLastTime = 0f;

    private Animation currentAnimation = null;

    private float animationSpeed = 1f;

    private float samplerAnim = 1f;

    private float animTime;

    private float delayBetweenAnimation = 0f;

    private string nameAnimator;

    int markedObjects;

    [MenuItem("Window/Toolbox/AnimationSimulator")]
    static void InitWindow()
    {
        EditorWindow window = GetWindow<FindAnimatorWindow>();
        window.autoRepaintOnSceneChange = true;
        window.Show();
        window.titleContent = new GUIContent("AnimationSimulator");
    }

    private void OnGUI()
    {

        tabIndex = GUILayout.Toolbar(tabIndex, tabs);

        switch (tabIndex)
        {
            case 0: GUITabsAnimator(); break;
            case 1: GUITabsAnimation(currenAnimator); break;
        }
    }

    private void OnHierarchyChange()
    {
        animatorComponentsArr = FindAnimatorInScene();
    }


    private void GUITabsAnimator()
    {

        GUILayout.Space(10f);
        GUILayout.Label("List animators");

        if (null == animatorComponentsArr)
            animatorComponentsArr = FindAnimatorInScene();

        for (int i = 0; i < animatorComponentsArr.Length; i++)
        {
            if (animatorComponentsArr[i] == null) continue;
            if (GUILayout.Button(animatorComponentsArr[i].name))
            {
                Selection.activeGameObject = animatorComponentsArr[i].gameObject;
                SceneView.lastActiveSceneView.FrameSelected();
                EditorGUIUtility.PingObject(animatorComponentsArr[i].gameObject);
                currenAnimator = animatorComponentsArr[i];
                tabIndex = 1;
                currentAnimIndex = 0;

                markedObjects = animatorComponentsArr[i].gameObject.GetInstanceID();
                EditorApplication.hierarchyWindowItemOnGUI += HierarchyItem;
            }
        }
    }

    private void GUITabsAnimation(Animator animator = null)
    {
        GUILayout.Space(10f);
        GUILayout.Label("Animations");
        if (animator == null) return;

        animClipsArr = FindAnimationClipInAnimator(animator);
        animNamesArr = FindAnimNames(animClipsArr);
        EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;

        currentAnimIndex = EditorGUILayout.Popup("Current Anim", currentAnimIndex, animNamesArr);

        animationSpeed = EditorGUILayout.FloatField("Animation Speed", animationSpeed);

        delayBetweenAnimation = EditorGUILayout.FloatField("Delay Between Animation", delayBetweenAnimation);


        if (isPlaying)
        {
            if (GUILayout.Button("Stop") || PlayModeStateChanged.State == PlayModeStateChange.EnteredPlayMode)
                StopAnim();
        }
        else
        {
            if (GUILayout.Button("Play") && PlayModeStateChanged.State != PlayModeStateChange.EnteredPlayMode)
                PlayAnim();
        }

        GUILayout.Space(10f);

        GUILayout.Label("SamplerAnimation");
        samplerAnim = GUILayout.HorizontalSlider(samplerAnim, 0f, animClipsArr[currentAnimIndex].length);

        if (!isPlaying)
        {
            AnimationMode.StartAnimationMode();
            AnimationMode.SampleAnimationClip(currenAnimator.gameObject, animClipsArr[currentAnimIndex], samplerAnim);
            
        }

        GUILayout.Space(10f);
        GUILayout.Label("Info animation");
        if (isPlaying)
            GUILayout.Label("Current duration : " + animTime.ToString());
        else
            GUILayout.Label("Current duration : " + samplerAnim.ToString());

        GUILayout.Label("Total Duration : " + animClipsArr[currentAnimIndex].length.ToString());
        GUILayout.Label("Is looping : " + animClipsArr[currentAnimIndex].isLooping.ToString());

    }

    private void HierarchyItem(int instanceID, Rect selectionRect)
    {
        Rect r = new Rect(selectionRect);
        r.x = r.width - 20;
        r.width = 18;
        if(markedObjects == instanceID)
        GUI.Label(r, currenAnimator.ToString());
    }

    private void EditorSceneManager_sceneOpened(Scene scene, OpenSceneMode mode)
    {
        StopAnim();
        animatorComponentsArr = FindAnimatorInScene();
    }

    private void PlayAnim()
    {
        if (isPlaying) return;
        editorLastTime = Time.realtimeSinceStartup;
        EditorApplication.update += OnEditorUpdate;
        AnimationMode.StartAnimationMode();
        isPlaying = true;
    }

    public void StopAnim()
    {
        if (!isPlaying) return;
        EditorApplication.update -= OnEditorUpdate;
        AnimationMode.StopAnimationMode();
        isPlaying = false;
    }

    private void OnEditorUpdate()
    {
        animTime = (Time.realtimeSinceStartup - editorLastTime) * animationSpeed;
        AnimationClip animClip = animClipsArr[currentAnimIndex];
        animTime %= animClip.length + delayBetweenAnimation;
        AnimationMode.SampleAnimationClip(currenAnimator.gameObject, animClip, animTime);
    }


    private Animator[] FindAnimatorInScene()
    {
        List<Animator> liAnimatorComponents = new List<Animator>();

        foreach(GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            liAnimatorComponents.AddRange(rootGameObject.GetComponentsInChildren<Animator>());
        }
        return liAnimatorComponents.ToArray();
    }

    private AnimationClip[] FindAnimationClipInAnimator(Animator animator)
    {
        List<AnimationClip> liAnimClip = new List<AnimationClip>();

        AnimatorController animController = animator.runtimeAnimatorController as AnimatorController;

        AnimatorControllerLayer controllerLayer = animController.layers[0];
        foreach (ChildAnimatorState childState in controllerLayer.stateMachine.states)
        {
            AnimationClip animClip = childState.state.motion as AnimationClip;
            if (animClip != null)
                liAnimClip.Add(animClip);
        }

        return liAnimClip.ToArray();
    }

    private string[] FindAnimNames(AnimationClip[] animClipArr)
    {
        List<string> liAnimationClipName = new List<string>();

        foreach(AnimationClip animClip in animClipArr)
        {
            liAnimationClipName.Add(animClip.name);
        }

        return liAnimationClipName.ToArray();
    }

}

