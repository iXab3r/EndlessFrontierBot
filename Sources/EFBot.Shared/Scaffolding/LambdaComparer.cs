﻿using System;
using System.Collections.Generic;

namespace EFBot.Shared.Scaffolding
{
    public sealed class LambdaComparer<T> : IEqualityComparer<T>
    {
        readonly Func<T, T, bool> comparer;
        readonly Func<T, int> hash;

        public LambdaComparer(Func<T, T, bool> comparer)
            : this(comparer, t => 0)
        {
        }

        public LambdaComparer(Func<T, T, bool> comparer, Func<T, int> hash)
        {
            this.comparer = comparer;
            this.hash = hash;
        }

        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (ReferenceEquals(x, null))
            {
                return false;
            }
            if (ReferenceEquals(y, null))
            {
                return false;
            }

            return comparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return hash(obj);
        }
    }
}