using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LittleDriver.BusKid
{
    public static class BusKidSetup
    {
        const string MenuPath = "Little Driver/Bus Kid/Make Selected Character a Bus Kid";

        [MenuItem(MenuPath, true)]
        static bool CanSetup() =>
            !EditorApplication.isPlaying && Selection.activeGameObject != null &&
            Selection.activeGameObject.GetComponent<Animator>() != null;

        // Select a character (with an Animator) standing behind the bus, facing the road, then run this.
        [MenuItem(MenuPath)]
        public static void Setup()
        {
            var kid = Selection.activeGameObject;
            Undo.SetCurrentGroupName("Make Bus Kid");
            int undoGroup = Undo.GetCurrentGroup();

            var animator = kid.GetComponent<Animator>();
            Undo.RecordObject(animator, "Turn off root motion");
            animator.applyRootMotion = false;

            var body = kid.GetComponent<Rigidbody>();
            if (body == null) body = Undo.AddComponent<Rigidbody>(kid);
            Undo.RecordObject(body, "Configure kid body");
            body.isKinematic = true;
            body.useGravity = false;

            var capsule = kid.GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                capsule = Undo.AddComponent<CapsuleCollider>(kid);
                FitCapsule(kid.transform, capsule);
            }

            var npc = kid.GetComponent<BusKidNPC>();
            if (npc == null) npc = Undo.AddComponent<BusKidNPC>(kid);
            Undo.RecordObject(npc, "Configure bus kid");
            if (npc.car == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) npc.car = player.transform;
            }
            if (!npc.HasPath) npc.crossingPath = CreatePath(kid.transform);

            var tree = kid.GetComponent<BehaviorTree>();
            if (tree == null) tree = Undo.AddComponent<BehaviorTree>(kid);
            Undo.RecordObject(tree, "Build bus kid tree");
            BuildTree(tree);
            npc.behaviorTree = tree;

            EditorUtility.SetDirty(npc);
            EditorUtility.SetDirty(tree);
            EditorSceneManager.MarkSceneDirty(kid.scene);
            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log(npc.car == null
                ? $"Bus Kid ready on '{kid.name}'. No object tagged Player yet: assign Car on Bus Kid NPC. Move the path markers across the road, then save the scene."
                : $"Bus Kid ready on '{kid.name}', watching '{npc.car.name}'. Move the path markers across the road, then save the scene.", kid);
        }

        // Entry > Repeat Forever > Utility Selector > the four actions. Same build steps as SprintWorldEventSetup.
        static void BuildTree(BehaviorTree tree)
        {
            tree.StartWhenEnabled = true;
            var entry = new EntryTask { ID = 0, FriendlyName = "Entry", NodeData = new NodeData { Offset = new Vector2(0, 0) } };
            var repeat = new Repeater { ID = 1, FriendlyName = "Repeat Forever", repeatForever = true, endOnFailure = false, NodeData = new NodeData { Offset = new Vector2(0, 100) } };
            var selector = new UtilitySelector { ID = 2, FriendlyName = "What Should The Kid Do?", NodeData = new NodeData { Offset = new Vector2(0, 120) } };
            var actions = new List<Task>
            {
                new DartOutFromBus { ID = 3, FriendlyName = "Dart Out From Bus", NodeData = new NodeData { Offset = new Vector2(-330, 150) } },
                new GreetSlowCar { ID = 4, FriendlyName = "Greet Slow Car", NodeData = new NodeData { Offset = new Vector2(-110, 150) } },
                new WalkBackBehindBus { ID = 5, FriendlyName = "Walk Back Behind Bus", NodeData = new NodeData { Offset = new Vector2(110, 150) } },
                new StandIdle { ID = 6, FriendlyName = "Stand Idle", NodeData = new NodeData { Offset = new Vector2(330, 150) } }
            };
            for (int i = 0; i < actions.Count; i++) selector.AddChild(actions[i], i);
            repeat.AddChild(selector, 0);
            var source = tree.GetBehaviorSource();
            source.behaviorName = "Bus Kid";
            source.Save(entry, repeat, null);
            source.HasSerialized = true;
            BehaviorDesigner.Editor.JSONSerialization.Save(source);
        }

        // Starting guesses straight ahead of the kid; drag them across the road in front of the bus.
        static Transform[] CreatePath(Transform kid)
        {
            var root = new GameObject(kid.name + " Path").transform;
            root.SetPositionAndRotation(kid.position, kid.rotation);
            Undo.RegisterCreatedObjectUndo(root.gameObject, "Create bus kid path");
            return new[]
            {
                Marker(root, "Hide Spot", kid.position, "sv_label_3"),
                Marker(root, "Road Edge", kid.position + kid.forward * 2, "sv_label_4"),
                Marker(root, "Far Side", kid.position + kid.forward * 9, "sv_label_6")
            };
        }

        static Transform Marker(Transform parent, string name, Vector3 position, string icon)
        {
            var marker = new GameObject(name).transform;
            marker.SetParent(parent, false);
            marker.position = position;
            if (EditorGUIUtility.IconContent(icon).image is Texture2D label)
                EditorGUIUtility.SetIconForObject(marker.gameObject, label);
            Undo.RegisterCreatedObjectUndo(marker.gameObject, "Create " + name);
            return marker;
        }

        static void FitCapsule(Transform kid, CapsuleCollider capsule)
        {
            var renderers = kid.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            float scale = Mathf.Max(0.0001f, kid.lossyScale.y);
            capsule.direction = 1;
            capsule.height = bounds.size.y / scale;
            capsule.radius = Mathf.Min(capsule.height / 2, Mathf.Min(bounds.size.x, bounds.size.z) / 2 / scale);
            capsule.center = kid.InverseTransformPoint(bounds.center);
        }
    }
}
