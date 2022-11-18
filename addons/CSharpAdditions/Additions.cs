using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Additions
{
    /// <summary>
    /// A class that contains some often used constants
    /// </summary>
    public static class Constants
    {
        public const string timeout = "timeout";

        public const float orthogonalAngleInRad = 1.5708f;
    }
    public static class GDAdditions
    {
        public static T GetOrAddProjectSetting<T>(string name, T defaultProperty, Variant.Type type)
        {
            if (ProjectSettings.HasSetting(name))
                return (T)ProjectSettings.GetSetting(name);

            ProjectSettings.SetSetting(name, defaultProperty);
            ProjectSettings.AddPropertyInfo(new()
            {
                {"name", name},
                {"type", type}
            });
            ProjectSettings.SetInitialValue(name, defaultProperty);
            return defaultProperty;
        }
        public static void AddProjectSetting(string name, object defaultProperty, Variant.Type type)
        {
            if (ProjectSettings.HasSetting(name)) return;

            ProjectSettings.SetSetting(name, defaultProperty);
            ProjectSettings.AddPropertyInfo(new()
            {
                {"name", name},
                {"type", type}
            });
            ProjectSettings.SetInitialValue(name, defaultProperty);
        }
    }
    public static class CollectionAdditions
    {
        public static T GetOrDefault<T>(this IDictionary dict, object key, T defaultValue, bool throwOnInvalidCast = true)
        {
            if (dict is null) return defaultValue;
            if (throwOnInvalidCast)
                return dict.Contains(key) ? (T)(object)dict[key] : defaultValue;
            return dict.Contains(key) ? (dict[key] is T value ? value : defaultValue) : defaultValue;
        }
        public static T Get<T>(this IDictionary dict, object key)
        {
            return (T)dict[key];
        }
    }
    public static class VectorAdditions
    {
        #region Diagonals

        public static Vector2 UpLeft => new Vector2(-0.7071068f, -0.7071068f);
        public static Vector2 UpRight => new Vector2(0.7071068f, -0.7071068f);
        public static Vector2 DownLeft => new Vector2(-0.7071068f, 0.7071068f);
        public static Vector2 DownRight => new Vector2(0.7071068f, 0.7071068f);

        #endregion Diagonals

        public static Transform2D SetXAxis(this Transform2D from, Vector2 newX) => new Transform2D(newX, from.y, from.origin);
        public static Transform2D SetYAxis(this Transform2D from, Vector2 newY) => new Transform2D(from.x, newY, from.origin);
    }

    public static class NumericAdditions
    {
        #region Range calculation

        /// <summary>
        /// Returns true if the value is an specific range (both min and max are inclusive use IsInRangeExlusive if you want them to be exclusive)
        /// </summary>
        public static bool IsInRange(this float value, float min, float max) => value >= min && value <= max;
        /// <summary>
        /// Returns true if the value is an specific range (both min and max are inclusive use IsInRangeExlusive if you want them to be exclusive)
        /// </summary>
        public static bool IsInRange(this int value, int min, int max) => value >= min && value <= max;
        /// <summary>
        /// Returns true if the value is an specific range (both min and max are exclusive use IsInRange if you want them to be inclusive)
        /// </summary>
        public static bool IsInRangeExlusive(this float value, float min, float max) => value > min && value < max;
        /// <summary>
        /// Returns true if the value is an specific range (both min and max are exclusive use IsInRange if you want them to be inclusive)
        /// </summary>
        public static bool IsInRangeExlusive(this int value, int min, int max) => value > min && value < max;

        #endregion Range calculation

        #region Mathf shortcuts 

        public static float Lerp(this float from, float to, float weight) => Mathf.Lerp(from, to, weight);
        public static float Clamp(this float from, float min, float max) => Mathf.Clamp(from, min, max);
        public static int Clamp(this int from, int min, int max) => Mathf.Clamp(from, min, max);
        public static float Clamp01(this float from) => Mathf.Clamp(from, 0f, 1f);
        public static int Clamp01(this int from) => Mathf.Clamp(from, 0, 1);
        public static float Abs(this float from) => Mathf.Abs(from);
        public static float Abs(this int from) => Mathf.Abs(from);
        public static int Round(this float from) => Mathf.RoundToInt(from);
        public static int Floor(this float from) => Mathf.FloorToInt(from);
        public static int Ceil(this float from) => Mathf.CeilToInt(from);

        #endregion Mathf shortcuts 

        public static string InvariantToString(this float value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public static class LazyReferenceMethods
    {
        /// <summary>
        /// Just gets a node if toStoreIn is null, and sets toStoreIn to the new value
        /// </summary>
        public static T LazyGetNode<T>(this Node from, ref T toStoreIn, NodePath path) where T : class
        {
            if (!Godot.Object.IsInstanceValid(toStoreIn as Node))
                toStoreIn = from.GetNodeOrNull<T>(path);

            return toStoreIn;
        }
        public static T LazyGetNode<T>(this Node from, ref T toStoreIn, NodePath path, System.Action onFirstGetted) where T : class
        {
            if (!Godot.Object.IsInstanceValid(toStoreIn as Node))
            {
                toStoreIn = from.GetNodeOrNull<T>(path);
                if (toStoreIn is not null)
                {
                    onFirstGetted();
                }
            }
            return toStoreIn;
        }
        /// <summary>
        /// Just gets the parent if toStoreIn is null, and sets toStoreIn to the new value
        /// </summary>
        public static T LazyGetParent<T>(this Node from, ref T toStoreIn) where T : class
        {
            if (!Godot.Object.IsInstanceValid(toStoreIn as Node))
                toStoreIn = from.GetParentOrNull<T>();

            return toStoreIn;
        }
        /// <summary>
        /// Just uses casting if toStoreIn is null, and sets toStoreIn to the new value
        /// </summary>
        public static T LazyObjectCast<T>(this object from, ref T toStoreIn) where T : class
        {
            if (!Godot.Object.IsInstanceValid(toStoreIn as Godot.Object))
                toStoreIn = from as T;

            return toStoreIn;
        }
        /// <summary>
        /// Just uses casting if toStoreIn is null, and sets toStoreIn to the new value,
        /// if checkForAnyInstanceBeingValid is true it will loop trough toStorIn and checks if any entry is null or an invalid reference
        /// if checkForAnyInstanceBeingValid is false it will only check if toStoreIn is null
        /// </summary>
        public static IEnumerable<T> LazyGetChildren<T>(this Node from, ref IEnumerable<T> toStoreIn, bool checkForAnyInstanceBeingValid) where T : class
        {
            toStoreIn ??= from.GetChildren<T>();

            if (checkForAnyInstanceBeingValid)
            {
                foreach (T node in toStoreIn)
                {
                    if (node == null || !Godot.Object.IsInstanceValid(node as Node))
                    {
                        toStoreIn = from.GetChildren<T>();
                        break;
                    }
                }
            }

            return toStoreIn;
        }
        /// <summary>
        /// Just uses casting if toStoreIn is null, and sets toStoreIn to the new value,
        /// if checkForAnyInstanceBeingValid is true it will loop trough toStorIn and checks if any entry is null or an invalid reference
        /// if checkForAnyInstanceBeingValid is false it will only check if toStoreIn is null
        /// </summary>
        public static List<T> LazyGetChildren<T>(this Node from, ref List<T> toStoreIn, bool checkForAnyInstanceBeingValid) where T : class
        {
            toStoreIn ??= from.GetChildren<T>().ToList();

            if (checkForAnyInstanceBeingValid)
            {
                foreach (T node in toStoreIn)
                {
                    if (node == null || !Godot.Object.IsInstanceValid(node as Node))
                    {
                        toStoreIn = from.GetChildren<T>().ToList();
                        break;
                    }
                }
            }

            return toStoreIn;
        }
    }
    public static class NodeExtensions
    {
        /// <summary>
        /// Gets the children from this object with an specific type
        /// </summary>
        public static IEnumerable<T> GetChildren<T>(this Node from) where T : class
        {
            return from.GetChildren().OfType<T>();
        }

        /// <summary>Does include itself</summary>
        public static void GetAllChildren<T>(this Node from, ref List<T> list) where T : class
        {
            if (from is T t)
                list.Add(t);

            foreach (Node node in from.GetChildren())
            {
                node.GetAllChildren<T>(ref list);
            }
        }

        /// <summary>Does not include itself</summary>
        public static List<T> GetAllChildren<T>(this Node from) where T : class
        {
            List<T> result = new();
            GetAllChildren<T>(from, ref result);

            if (from is T first && result.Contains(first))
                result.Remove(first);

            return result;
        }

        public static void GetAllNodeOfType<T>(SceneTree from, ref List<T> list) where T : class
        {
            GetAllChildren<T>(from.Root, ref list);
        }

        /// <summary>
        /// Gets the closests Area2D overlapping this Area2D
        /// </summary>
        public static Area2D GetClosestsArea(this Area2D from, Vector2 relativOffset)
        {
            Area2D result = null;
            float smallestDist = 0;

            Vector2 targetPos = from.GlobalTransform.Translated(relativOffset).origin;

            foreach (Area2D area in from.GetOverlappingAreas())
            {
                float dist = targetPos.DistanceSquaredTo(area.GlobalPosition);

                if (result == null || dist < smallestDist)
                {
                    smallestDist = dist;
                    result = area;
                }
            }

            return result;
        }
        /// <summary>
        /// Gets the closests Area overlapping this Area
        /// </summary>
        public static Area GetClosestsArea(this Area from)
        {
            Area result = null;
            float smallestDist = 0;

            foreach (Area area in from.GetOverlappingAreas())
            {
                float dist = from.GlobalTransform.origin.DistanceSquaredTo(area.GlobalTransform.origin);

                if (result == null || dist < smallestDist)
                {
                    smallestDist = dist;
                    result = area;
                }
            }

            return result;
        }

        #region Shortcuts for commonly used Get and Set functions
        public static void SetShaderParam(this CanvasItem from, string parameter, object value)
        {
            from.Material.Set($"shader_param/{parameter}", value);
        }
        public static object GetShaderParam(this CanvasItem from, string parameter)
        {
            return from.Material.Get($"shader_param/{parameter}");
        }
        public static void SetCondition(this AnimationTree from, string condition, bool value)
        {
            from.Set($"parameters/conditions/{condition}", value);
        }
        public static bool GetCondition(this AnimationTree from, string condition)
        {
            return (bool)from.Get($"parameters/conditions/{condition}");
        }
        public static void SetParam(this AnimationTree from, string parameter, object value)
        {
            from.Set($"parameters/{parameter}", value);
        }
        public static object GetParam(this AnimationTree from, string parameter)
        {
            return from.Get($"parameters/{parameter}");
        }
        #endregion
    }
    public static class RessourceExtensions
    {

    }

    public static class BitAdditions
    {
        public static int GetBitCount(this long value)
        {
            int count = 0;

            while (value != 0)
            {
                value = value & (value - 1);

                count++;
            }

            return count;
        }
    }
}

