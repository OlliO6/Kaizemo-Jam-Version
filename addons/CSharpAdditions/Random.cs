using Godot;

namespace Additions
{
    public static class Random
    {
        private class AutoRandomizeRng : RandomNumberGenerator
        {
            public AutoRandomizeRng() => Randomize();
        }

        private static AutoRandomizeRng rng = new();
        public static RandomNumberGenerator NumberGenerator => rng as RandomNumberGenerator;

        public static ulong Seed { get => rng.Seed; set => rng.Seed = value; }
        public static ulong State { get => rng.State; set => rng.State = value; }

        public static void Randomize() => rng.Randomize();

        public static int IntRange(int min, int max) => rng.RandiRange(min, max);
        public static int IntRange(Vector2 range) => rng.RandiRange((int)range.x, (int)range.y);

        public static float FloatRange(float min, float max) => rng.RandfRange(min, max);
        public static float FloatRange(Vector2 range) => rng.RandfRange(range.x, range.y);

        public static uint RandI() => rng.Randi();
        public static float Float01() => rng.Randf();

        public static float NormallyDistributedFloat(float mean = 0, float deviation = 1) => rng.Randfn(mean, deviation);
    }
}