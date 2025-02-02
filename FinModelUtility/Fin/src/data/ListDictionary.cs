﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace fin.data {
  public class ListDictionary<TKey, TValue>
      : IEnumerable<KeyValuePair<TKey, IList<TValue>>> {
    private readonly NullFriendlyDictionary<TKey, IList<TValue>> impl_ = new();

    public void Clear() => this.impl_.Clear();

    public int Count
      => this.impl_.Values
             .Select(list => list.Count)
             .Sum();

    public void Add(TKey key, TValue value) {
      IList<TValue> list;
      if (!this.impl_.TryGetValue(key, out list)) {
        this.impl_[key] = list = new List<TValue>();
      }

      list.Add(value);
    }

    public IList<TValue> this[TKey key] => this.impl_[key];

    public bool TryGetList(TKey key, out IList<TValue>? list)
      => this.impl_.TryGetValue(key, out list);

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<KeyValuePair<TKey, IList<TValue>>> GetEnumerator()
      => this.impl_.GetEnumerator();
  }
}