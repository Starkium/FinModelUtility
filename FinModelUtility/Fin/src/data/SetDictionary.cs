﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace fin.data {
  public class SetDictionary<TKey, TValue>
      : IEnumerable<KeyValuePair<TKey, ISet<TValue>>> {
    private readonly NullFriendlyDictionary<TKey, ISet<TValue>> impl_ = new();

    public void Clear() => this.impl_.Clear();

    public int Count
      => this.impl_.Values
             .Select(list => list.Count)
             .Sum();

    public void Add(TKey key, TValue value) {
      ISet<TValue> set;
      if (!this.impl_.TryGetValue(key, out set)) {
        this.impl_[key] = set = new HashSet<TValue>();
      }

      set.Add(value);
    }

    public ISet<TValue> this[TKey key] => this.impl_[key];

    public bool TryGetSet(TKey key, out ISet<TValue>? set) 
      => this.impl_.TryGetValue(key, out set);

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<KeyValuePair<TKey, ISet<TValue>>> GetEnumerator()
      => this.impl_.GetEnumerator();
  }
}