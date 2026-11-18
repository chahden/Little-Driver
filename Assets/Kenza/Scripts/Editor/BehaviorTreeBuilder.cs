using System.Collections.Generic;
using System.Reflection;
using BehaviorDesigner.Editor;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using LittleDriver.AI.Pedestrians;
using LittleDriver.AI.Tasks;
using LittleDriver.AI.Traffic;
using LittleDriver.Traffic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using BDBehaviorTree = BehaviorDesigner.Runtime.BehaviorTree;
using BDTask = BehaviorDesigner.Runtime.Tasks.Task;

namespace LittleDriver.EditorTools
{
    /// <summary>
    /// Builds the Behavior Designer trees for traffic lights, AI cars and pedestrians as shared assets
    /// (Assets/Kenza/AI Trees), and gives every such object in the open scene a Behavior Tree component
    /// that runs its asset. Open the assets in Tools > Behavior Designer > Editor to see or edit the graphs.
    /// Re-running it rebuilds the trees (any hand edits to them are replaced).
    /// </summary>
    public static class BehaviorTreeBuilder
    {
        private const string Folder = "Assets/Kenza/AI Trees";
        private static readonly FieldInfo AbortField =
            typeof(Composite).GetField("abortType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        [MenuItem("Tools/Little Driver/Build Behavior Trees")]
        public static void BuildAll()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Kenza", "AI Trees");

            ExternalBehaviorTree light = SaveTree("Traffic Light FSM",
                "Finite state machine of a traffic light: each node is a state, the sequence order is the transitions (Green -> Yellow -> Red -> Green).",
                BuildTrafficLight());
            ExternalBehaviorTree car = SaveTree("AI Car",
                "Traffic car: stop for the light, let pedestrians cross, keep distance, give way at a busy junction, otherwise follow the lane.",
                BuildCar());
            ExternalBehaviorTree pedestrian = SaveTree("Pedestrian",
                "Pedestrian: finish crossing once on the road, wait at the kerb while a car is too close, step out, otherwise walk the sidewalk.",
                BuildPedestrian());
            AssetDatabase.SaveAssets();

            int lights = Attach<TrafficLight>(light, "Traffic Light FSM");
            int cars = Attach<AITrafficCar>(car, "AI Car");
            int pedestrians = Attach<Pedestrian>(pedestrian, "Pedestrian");

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[BehaviorTreeBuilder] Built 3 trees in {Folder}; attached to {lights} traffic lights, {cars} cars, {pedestrians} pedestrians.");
        }

        // Entry -> Repeat Forever -> Sequence [Green, Yellow, Red]
        private static (EntryTask, BDTask) BuildTrafficLight()
        {
            var entry = Node<EntryTask>("Entry", 0, 0);
            var repeat = RepeatForever(0, 100);
            var cycle = Node<Sequence>("Cycle", 0, 200);
            repeat.AddChild(cycle, 0);
            cycle.AddChild(Phase("Green", TrafficLightState.Green, -240, 320), 0);
            cycle.AddChild(Phase("Yellow", TrafficLightState.Yellow, 0, 320), 1);
            cycle.AddChild(Phase("Red", TrafficLightState.Red, 240, 320), 2);
            return (entry, repeat);
        }

        // Entry -> Repeat Forever -> Selector "Drive" [4 guarded branches, Follow lane]
        private static (EntryTask, BDTask) BuildCar()
        {
            var entry = Node<EntryTask>("Entry", 0, 0);
            var repeat = RepeatForever(0, 100);
            var drive = Node<Selector>("Drive", 0, 200);
            repeat.AddChild(drive, 0);
            drive.AddChild(Branch("Obey traffic light", -600, Node<LightSaysStop>("Light says stop?", 0, 0), Node<StopAtLine>("Stop at red light", 0, 0)), 0);
            drive.AddChild(Branch("Let pedestrians cross", -300, Node<CrossingBusy>("Zebra crossing busy?", 0, 0), Node<StopAtLine>("Wait at zebra crossing", 0, 0)), 1);
            drive.AddChild(Branch("Keep distance", 0, Node<ObstacleAhead>("Obstacle ahead?", 0, 0), Node<BrakeForObstacle>("Brake for obstacle", 0, 0)), 2);
            drive.AddChild(Branch("Give way", 300, Node<JunctionBusy>("Junction busy?", 0, 0), Node<StopAtLine>("Give way at junction", 0, 0)), 3);
            drive.AddChild(Node<FollowLane>("Follow lane", 560, 330), 4);
            return (entry, repeat);
        }

        // Entry -> Repeat Forever -> Selector "Pedestrian" [4 guarded branches, idle until given a route]
        private static (EntryTask, BDTask) BuildPedestrian()
        {
            var entry = Node<EntryTask>("Entry", 0, 0);
            var repeat = RepeatForever(0, 100);
            var root = Node<Selector>("Pedestrian", 0, 200);
            repeat.AddChild(root, 0);
            root.AddChild(Branch("Finish crossing", -600, Node<IsOnRoad>("On the road?", 0, 0), Node<WalkRoute>("Cross the street", 0, 0)), 0);
            root.AddChild(Branch("Wait at the kerb", -300, Node<WantsToCross>("Wants to cross?", 0, 0), Node<CarTooClose>("Car too close?", 0, 0), Node<WaitForCars>("Wait for cars", 0, 0)), 1);
            root.AddChild(Branch("Step onto the crossing", 0, Node<WantsToCross>("Wants to cross?", 0, 0), Node<StepOut>("Step out", 0, 0)), 2);
            root.AddChild(Branch("Walk", 300, Node<HasRoute>("Has a route?", 0, 0), Node<WalkRoute>("Walk on the sidewalk", 0, 0)), 3);
            root.AddChild(Node<Idle>("Wait for a route", 560, 330), 4);
            return (entry, repeat);
        }

        // A Sequence of conditions then an action. "Both" conditional aborts: the conditions are re-checked
        // every frame, so the branch stops when they turn false and interrupts lower branches when they turn true.
        private static Sequence Branch(string name, float x, params BDTask[] children)
        {
            var sequence = Node<Sequence>(name, x, 330);
            AbortField.SetValue(sequence, AbortType.Both);
            float childX = -110f * (children.Length - 1) / 2f;
            for (int i = 0; i < children.Length; i++)
            {
                children[i].NodeData.Offset = new Vector2(childX + 110f * i, 130);
                sequence.AddChild(children[i], i);
            }
            return sequence;
        }

        private static TrafficLightPhase Phase(string name, TrafficLightState state, float x, float y)
        {
            var phase = Node<TrafficLightPhase>(name, x, y);
            phase.state = state;
            return phase;
        }

        private static Repeater RepeatForever(float x, float y)
        {
            var repeat = Node<Repeater>("Repeat Forever", x, y);
            repeat.count = new SharedInt { Value = 1 };
            repeat.repeatForever = new SharedBool { Value = true };
            repeat.endOnFailure = new SharedBool { Value = false };
            return repeat;
        }

        private static T Node<T>(string name, float x, float y) where T : BDTask, new()
        {
            var task = new T { FriendlyName = name };
            task.NodeData = new NodeData { Offset = new Vector2(x, y) };
            return task;
        }

        private static ExternalBehaviorTree SaveTree(string name, string description, (EntryTask entry, BDTask root) tree)
        {
            string path = $"{Folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>(path);
            bool isNew = asset == null;
            if (isNew) asset = ScriptableObject.CreateInstance<ExternalBehaviorTree>();

            // Behavior Designer links nodes by ID, so every node needs a unique one.
            int nextId = 0;
            AssignIds(tree.entry, ref nextId);
            AssignIds(tree.root, ref nextId);

            var source = new BehaviorSource(asset) { behaviorName = name, behaviorDescription = description };
            asset.BehaviorSource = source;
            source.Save(tree.entry, tree.root, new List<BDTask>());
            JSONSerialization.Save(source);

            if (isNew) AssetDatabase.CreateAsset(asset, path);
            else EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void AssignIds(BDTask task, ref int nextId)
        {
            task.ID = nextId++;
            if (task is ParentTask parent && parent.Children != null && !(task is EntryTask))
                foreach (BDTask child in parent.Children) AssignIds(child, ref nextId);
        }

        // Every object with the body component (inactive templates included) runs the shared tree.
        private static int Attach<TBody>(ExternalBehaviorTree tree, string name) where TBody : Component
        {
            int count = 0;
            foreach (TBody body in Object.FindObjectsByType<TBody>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var behavior = body.GetComponent<BDBehaviorTree>();
                if (behavior == null) behavior = Undo.AddComponent<BDBehaviorTree>(body.gameObject);
                else Undo.RecordObject(behavior, "Assign behaviour tree");
                behavior.ExternalBehavior = tree;
                behavior.BehaviorName = name;
                behavior.StartWhenEnabled = true;
                behavior.RestartWhenComplete = false;
                behavior.PauseWhenDisabled = false;
                EditorUtility.SetDirty(behavior);
                count++;
            }
            return count;
        }
    }
}
