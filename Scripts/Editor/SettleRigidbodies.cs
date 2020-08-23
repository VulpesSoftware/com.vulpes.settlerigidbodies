using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Vulpes.SettleRigidbodies
{
    [InitializeOnLoad]
    public sealed class SettleRigidbodies 
    {
        private const int PHYSICS_CYCLES = 10000;
       
        static SettleRigidbodies()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public sealed class RigidbodySelection
        {
            public Rigidbody rigidbody;
            public GameObject gameObject;
            public Transform transform;
            public Vector3 position;
            public Quaternion rotation;

            public RigidbodySelection(Rigidbody rbody)
            {
                rigidbody = rbody;
                gameObject = rigidbody.gameObject;
                transform = gameObject.transform;
                position = transform.position;
                rotation = transform.rotation;
            }

            public void Reset()
            {
                transform.position = position;
                transform.rotation = rotation;
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.P && e.modifiers == EventModifiers.Shift)
            {
                SettleSelection();
                e.Use();
            }
        }

        [MenuItem("Physics/Settle Rigidbodies/Selection")]
        private static void SettleSelection()
        {
            Settle(false);
        }

        [MenuItem("Physics/Settle Rigidbodies/All")]
        private static void SettleAll()
        {
            Settle(true);
        }

        private static void Settle(bool abSettleAll)
        {
            int i, count, cycles;
            Rigidbody[] rigidbodies = Object.FindObjectsOfType<Rigidbody>();
            Object[] selection = Selection.objects;
            List<RigidbodySelection> rigidbodySelection = new List<RigidbodySelection>();
            count = cycles = 0;
            if (abSettleAll || selection.Length > 0 && !abSettleAll)
            {
                for (i = 0; i < rigidbodies.Length; i++)
                {
                    if (rigidbodies[i].isKinematic)
                    {
                        continue;
                    }
                    rigidbodySelection.Add(new RigidbodySelection(rigidbodies[i]));
                }
                for (i = 0; i < rigidbodySelection.Count; i++)
                {
                    Undo.RecordObject(rigidbodySelection[i].transform, "Settle Rigidbody");
                }
                Physics.autoSimulation = false;
                for (i = 0; i < PHYSICS_CYCLES; i++)
                {
                    Physics.Simulate(Time.fixedDeltaTime);
                    if (rigidbodySelection.All(rbody => rbody.rigidbody.IsSleeping()))
                    {
                        break;
                    }
                }
                Physics.autoSimulation = true;
                count = rigidbodySelection.Count;
                cycles = i;
                if (!abSettleAll)
                {
                    for (i = 0; i < rigidbodySelection.Count; i++)
                    {
                        if (!selection.Contains(rigidbodySelection[i].gameObject))
                        {
                            rigidbodySelection[i].Reset();
                            count--;
                        }
                    }
                }
            }
            Debug.Log(string.Format(
                "Settled {0} Rigidbod{1} in {2} cycles.", 
                count.ToString(), 
                count != 1 ? "ies" : "y", 
                cycles.ToString()));
        }
    }
}