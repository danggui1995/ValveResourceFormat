using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ValveResourceFormat.ThirdParty;

namespace ValveResourceFormat.Utils
{
    public static class StringToken
    {
        public const uint MURMUR2SEED = 0x31415926; // It's pi!

        internal static readonly ConcurrentDictionary<string, uint> Lookup = new();

        public static Dictionary<uint, string> InvertedTable
        {
            get
            {
                var inverted = new Dictionary<uint, string>(Lookup.Count);

                foreach (var (key, hash) in Lookup)
                {
                    inverted[hash] = key;
                }

                return inverted;
            }
        }

        static StringToken()
        {
            EntityLumpKnownKeys.FillKeys();
        }

        public static uint Get(string key)
        {
            return Lookup.GetOrAdd(key, s =>
            {
#if DEBUG
                Console.WriteLine($"New string: {s}");
#endif

                return MurmurHash2.Hash(s, MURMUR2SEED);
            });
        }

        internal static void Add(string key)
        {
            if (!Lookup.TryAdd(key, MurmurHash2.Hash(key, MURMUR2SEED)))
            {
                throw new InvalidOperationException($"Key {key} already exists");
            }
        }
    }
}
