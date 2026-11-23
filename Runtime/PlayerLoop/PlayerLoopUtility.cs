using System;
using UnityEngine.LowLevel;

namespace SwiftTimer.PlayerLoop
{

    /// <summary>
    /// Provides utility methods for creating, inserting, and removing custom systems within Unity's PlayerLoop hierarchy.
    /// </summary>
    public static class PlayerLoopUtility
    {
        /// <summary>
        /// Creates a custom PlayerLoop system using the specified identifier and callback.
        /// </summary>
        /// <returns>
        /// A PlayerLoop system configured with the specified identifier and callback.
        /// </returns>
        public static PlayerLoopSystem CreateRunner<T>(Action callback) => new()
        {
            type = typeof(T),
            updateDelegate = new(callback),
        };

        /// <summary>
        /// Inserts a PlayerLoop system at the beginning of the specified group's subsystem list.
        /// </summary>
        /// <returns>
        /// True if the specified group was found and the system was inserted.
        /// </returns>
        public static bool InsertAtBegin(ref PlayerLoopSystem parent, Type group, PlayerLoopSystem runner) => InsertAtEdge(ref parent, group, runner, false);

        /// <summary>
        /// Inserts a PlayerLoop system at the end of the specified group's subsystem list.
        /// </summary>
        /// <returns>
        /// True if the specified group was found and the system was inserted.
        /// </returns>
        public static bool InsertAtEnd(ref PlayerLoopSystem parent, Type group, PlayerLoopSystem runner) => InsertAtEdge(ref parent, group, runner, true);

        /// <summary>
        /// Inserts a PlayerLoop system immediately before the specified system.
        /// </summary>
        /// <returns>
        /// True if the specified group was found and the system was inserted.
        /// </returns>
        public static bool InsertBefore(ref PlayerLoopSystem parent, Type target, PlayerLoopSystem runner) => InsertRelative(ref parent, target, runner, false);

        /// <summary>
        /// Inserts a PlayerLoop system immediately after the specified system.
        /// </summary>
        /// <returns>
        /// True if the specified group was found and the system was inserted.
        /// </returns>
        public static bool InsertAfter(ref PlayerLoopSystem parent, Type target, PlayerLoopSystem runner) => InsertRelative(ref parent, target, runner, true);

        /// <summary>
        /// Inserts a PlayerLoop system immediately before or after the specified system. Searches the PlayerLoop hierarchy recursively.
        /// </summary>
        /// <returns>
        /// True if the specified group was found and the system was inserted.
        /// </returns>
        public static bool InsertRelative(ref PlayerLoopSystem parent, Type target, PlayerLoopSystem runner, bool insertAfter)
        {
            var systems = parent.subSystemList;
            if (systems == null) return false;

            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i].type == target)
                {
                    int index = insertAfter ? i + 1 : i;
                    var newSystems = new PlayerLoopSystem[systems.Length + 1];

                    Array.Copy(systems, 0, newSystems, 0, index);
                    newSystems[index] = runner;

                    Array.Copy(systems, index, newSystems, index + 1, systems.Length - index);
                    parent.subSystemList = newSystems;

                    return true;
                }

                if (InsertRelative(ref systems[i], target, runner, insertAfter))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Inserts a PlayerLoop system at the beginning or end of the specified group's subsystem list. Searches the PlayerLoop hierarchy recursively.
        /// </summary>
        /// <returns>
        /// True if the specified group was found and the system was inserted.
        /// </returns>
        public static bool InsertAtEdge(ref PlayerLoopSystem parent, Type group, PlayerLoopSystem runner, bool insertAtEnd)
        {
            if (parent.type == group)
            {
                var systems = parent.subSystemList ?? Array.Empty<PlayerLoopSystem>();
                var newSystems = new PlayerLoopSystem[systems.Length + 1];

                if (insertAtEnd)
                {
                    Array.Copy(systems, 0, newSystems, 0, systems.Length);
                    newSystems[systems.Length] = runner;
                }
                else
                {
                    newSystems[0] = runner;
                    Array.Copy(systems, 0, newSystems, 1, systems.Length);
                }

                parent.subSystemList = newSystems;
                return true;
            }

            var children = parent.subSystemList;
            if (children == null) return false;

            for (int i = 0; i < children.Length; i++)
                if (InsertAtEdge(ref children[i], group, runner, insertAtEnd))
                {
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Removes all PlayerLoop systems matching the specified type from the hierarchy. Searches recursively through the subsystem lists.
        /// </summary>
        /// <returns>
        /// True if at least one matching system was removed.
        /// </returns>
        public static bool Remove(ref PlayerLoopSystem parent, Type runner)
        {
            var systems = parent.subSystemList;
            if (systems == null) return false;

            bool changed = false;
            int writeIndex = 0;

            for (int i = 0; i < systems.Length; i++)
            {
                var system = systems[i];

                if (system.type == runner)
                {
                    changed = true;
                    continue;
                }

                if (Remove(ref system, runner))
                    changed = true;

                systems[writeIndex++] = system;
            }

            if (writeIndex != systems.Length)
            {
                var newSystems = new PlayerLoopSystem[writeIndex];
                Array.Copy(systems, newSystems, writeIndex);
                parent.subSystemList = writeIndex == 0 ? null : newSystems;
            }

            return changed;
        }
    }

}