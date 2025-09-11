using System;
using System.Collections.Generic;
using PurrNet.Modules;
using PurrNet.Pooling;
using UnityEngine;

namespace PurrNet.Packing
{
    public static class PackCollections
    {
        [UsedByIL]
        public static void RegisterNullable<T>() where T : struct
        {
            Packer<T?>.RegisterWriter(PackNullables.WriteNullable);
            Packer<T?>.RegisterReader(PackNullables.ReadNullable);

            DeltaPacker<T?>.RegisterWriter(PackNullables.WriteDeltaNullable);
            DeltaPacker<T?>.RegisterReader(PackNullables.ReadDeltaNullable);
        }

        [UsedByIL]
        public static void RegisterDictionary<TKey, TValue>()
        {
            Packer<Dictionary<TKey, TValue>>.RegisterWriter(WriteDictionary);
            Packer<Dictionary<TKey, TValue>>.RegisterReader(ReadDictionary);
        }

        [UsedByIL]
        public static void RegisterDisposableDictionary<TKey, TValue>()
        {
            Packer<DisposableDictionary<TKey, TValue>>.RegisterWriter(PackDisposableDictionary.WriteDictionary);
            Packer<DisposableDictionary<TKey, TValue>>.RegisterReader(PackDisposableDictionary.ReadDictionary);
            DeltaPacker<DisposableDictionary<TKey, TValue>>.RegisterWriter(PackDisposableDictionary.WriteDeltaDictionary);
            DeltaPacker<DisposableDictionary<TKey, TValue>>.RegisterReader(PackDisposableDictionary.ReadDeltaDictionary);
        }

        [UsedByIL]
        public static void RegisterQueue<T>()
        {
            Packer<Queue<T>>.RegisterWriter(WriteQueue);
            Packer<Queue<T>>.RegisterReader(ReadQueue);
        }

        [UsedByIL]
        public static void RegisterStack<T>()
        {
            Packer<Stack<T>>.RegisterWriter(WriteStack);
            Packer<Stack<T>>.RegisterReader(ReadStack);
        }

        [UsedByIL]
        public static void RegisterDisposableList<T>()
        {
            Packer<DisposableList<T>>.RegisterWriter(PackDisposableLists.WriteDisposableList);
            Packer<DisposableList<T>>.RegisterReader(PackDisposableLists.ReadDisposableList);
            DeltaPacker<DisposableList<T>>.RegisterWriter(PackDisposableLists.WriteDisposableDeltaList);
            DeltaPacker<DisposableList<T>>.RegisterReader(PackDisposableLists.ReadDisposableDeltaList);
        }

        [UsedByIL]
        public static void RegisterDisposableHashSet<T>()
        {
            Packer<DisposableHashSet<T>>.RegisterWriter(WriteDisposableHashSet);
            Packer<DisposableHashSet<T>>.RegisterReader(ReadDisposableHashSet);
        }

        [UsedByIL]
        public static void WriteDisposableHashSet<T>(this BitPacker packer, DisposableHashSet<T> value)
        {
            if (value.isDisposed || value.set == null)
            {
                Packer<bool>.Write(packer, false);
                return;
            }

            Packer<bool>.Write(packer, true);

            int length = value.Count;
            packer.WriteInteger(length, 31);

            foreach (var v in value)
                Packer<T>.Write(packer, v);
        }

        [UsedByIL]
        public static void ReadDisposableHashSet<T>(this BitPacker packer, ref DisposableHashSet<T> value)
        {
            value.Dispose();

            bool hasValue = default;
            packer.Read(ref hasValue);

            if (!hasValue)
                return;

            long length = default;

            packer.ReadInteger(ref length, 31);
            value = new DisposableHashSet<T>((int)length);

            for (int i = 0; i < length; i++)
            {
                T item = default;
                Packer<T>.Read(packer, ref item);
                value.Add(item);
            }
        }

        public static void WriteQueue<T>(BitPacker packer, Queue<T> value)
        {
            if (value == null)
            {
                Packer<bool>.Write(packer, false);
                return;
            }

            Packer<bool>.Write(packer, true);

            int length = value.Count;
            packer.WriteInteger(length, 31);

            foreach (var v in value)
                Packer<T>.Write(packer, v);
        }

        public static void ReadQueue<T>(BitPacker packer, ref Queue<T> value)
        {
            bool hasValue = default;
            packer.Read(ref hasValue);

            if (!hasValue)
            {
                value = null;
                return;
            }

            long length = default;

            packer.ReadInteger(ref length, 31);

            if (value == null)
                value = new Queue<T>((int)length);
            else value.Clear();

            for (int i = 0; i < length; i++)
            {
                T item = default;
                Packer<T>.Read(packer, ref item);
                value.Enqueue(item);
            }
        }

        public static void WriteStack<T>(BitPacker packer, Stack<T> value)
        {
            if (value == null)
            {
                Packer<bool>.Write(packer, false);
                return;
            }

            Packer<bool>.Write(packer, true);

            int length = value.Count;
            packer.WriteInteger(length, 31);

            foreach (var v in value)
                Packer<T>.Write(packer, v);
        }

        public static void ReadStack<T>(BitPacker packer, ref Stack<T> value)
        {
            bool hasValue = default;
            packer.Read(ref hasValue);

            if (!hasValue)
            {
                value = null;
                return;
            }

            long length = default;

            packer.ReadInteger(ref length, 31);

            if (value == null)
                value = new Stack<T>((int)length);
            else value.Clear();

            for (int i = 0; i < length; i++)
            {
                T item = default;
                Packer<T>.Read(packer, ref item);
                value.Push(item);
            }
        }

        private static void WriteDictionary<K, V>(BitPacker packer, Dictionary<K, V> value)
        {
            if (value == null)
            {
                Packer<bool>.Write(packer, false);
                return;
            }

            Packer<bool>.Write(packer, true);

            int length = value.Count;
            packer.WriteInteger(length, 31);

            foreach (var pair in value)
            {
                Packer<K>.Write(packer, pair.Key);
                Packer<V>.Write(packer, pair.Value);
            }
        }

        private static void ReadDictionary<K, V>(BitPacker packer, ref Dictionary<K, V> value)
        {
            bool hasValue = default;
            packer.Read(ref hasValue);

            if (!hasValue)
            {
                value = null;
                return;
            }

            long length = default;

            packer.ReadInteger(ref length, 31);

            if (value == null)
                value = new Dictionary<K, V>((int)length);
            else value.Clear();

            for (int i = 0; i < length; i++)
            {
                K key = default;
                V val = default;
                Packer<K>.Read(packer, ref key);
                Packer<V>.Read(packer, ref val);

                try
                {
                    value.Add(key, val);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        [UsedByIL]
        public static void RegisterHashSet<T>()
        {
            Packer<HashSet<T>>.RegisterWriter(WriteCollection);
            Packer<HashSet<T>>.RegisterReader(ReadHashSet);
        }

        [UsedByIL]
        public static void RegisterList<T>()
        {
            Packer<List<T>>.RegisterWriter(WriteList);
            Packer<List<T>>.RegisterReader(ReadList);

            DeltaPacker<List<T>>.RegisterWriter(WriteDeltaList);
            DeltaPacker<List<T>>.RegisterReader(ReadDeltaList);
        }

        [UsedByIL]
        public static void RegisterArray<T>()
        {
            Packer<T[]>.RegisterWriter(WriteList);
            Packer<T[]>.RegisterReader(ReadArray);

            DeltaPacker<T[]>.RegisterWriter(WriteDeltaList);
            DeltaPacker<T[]>.RegisterReader(ReadDeltaArray);
        }

        private static bool WriteDeltaList<T>(BitPacker packer, IList<T> oldvalue, IList<T> newvalue)
        {
            bool areEqual = Packer.AreEqual(oldvalue, newvalue);

            Packer<bool>.Write(packer, areEqual);

            if (!areEqual)
                WriteList(packer, newvalue);

            return areEqual;
        }

        private static void ReadDeltaArray<T>(BitPacker packer, T[] oldvalue, ref T[] value)
        {
            bool areEqual = default;
            packer.Read(ref areEqual);

            if (!areEqual)
                ReadArray(packer, ref value);
        }

        private static void ReadDeltaList<T>(BitPacker packer, List<T> oldvalue, ref List<T> value)
        {
            bool areEqual = default;
            packer.Read(ref areEqual);

            if (!areEqual)
                ReadList(packer, ref value);
        }

        [UsedByIL]
        public static void WriteCollection<T>(this BitPacker packer, ICollection<T> value)
        {
            if (value == null)
            {
                Packer<bool>.Write(packer, false);
                return;
            }

            Packer<bool>.Write(packer, true);

            int length = value.Count;
            packer.WriteInteger(length, 31);

            foreach (var v in value)
                Packer<T>.Write(packer, v);
        }

        [UsedByIL]
        public static void ReadHashSet<T>(this BitPacker packer, ref HashSet<T> value)
        {
            bool hasValue = default;
            packer.Read(ref hasValue);

            if (!hasValue)
            {
                value = null;
                return;
            }

            long length = default;

            packer.ReadInteger(ref length, 31);

            if (value == null)
                value = new HashSet<T>((int)length);
            else value.Clear();

            for (int i = 0; i < length; i++)
            {
                T item = default;
                Packer<T>.Read(packer, ref item);
                try
                {
                    value.Add(item);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        [UsedByIL]
        public static void WriteList<T>(this BitPacker packer, IList<T> value)
        {
            if (value == null)
            {
                Packer<bool>.Write(packer, false);
                return;
            }

            Packer<bool>.Write(packer, true);

            int length = value.Count;
            packer.WriteInteger(length, 31);

            for (int i = 0; i < length; i++)
                Packer<T>.Write(packer, value[i]);
        }

        [UsedByIL]
        public static void ReadList<T>(this BitPacker packer, ref List<T> value)
        {
            bool hasValue = default;
            packer.Read(ref hasValue);

            if (!hasValue)
            {
                value = null;
                return;
            }

            long length = default;

            packer.ReadInteger(ref length, 31);

            if (value == null)
                value = new List<T>((int)length);
            else value.Clear();

            for (int i = 0; i < length; i++)
            {
                T item = default;
                Packer<T>.Read(packer, ref item);
                value.Add(item);
            }
        }

        [UsedByIL]
        public static void ReadArray<T>(this BitPacker packer, ref T[] value)
        {
            bool hasValue = default;
            packer.Read(ref hasValue);

            if (!hasValue)
            {
                value = null;
                return;
            }

            long length = default;

            packer.ReadInteger(ref length, 31);

            if (length == -1)
            {
                value = null;
                return;
            }

            if (value == null)
                value = new T[length];
            else if (value.Length != length)
                Array.Resize(ref value, (int)length);

            for (int i = 0; i < length; i++)
                Packer<T>.Read(packer, ref value[i]);
        }
    }
}
