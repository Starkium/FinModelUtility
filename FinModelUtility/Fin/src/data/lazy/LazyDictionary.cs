﻿using System;
using System.Collections.Concurrent;

namespace fin.data.lazy {
  public class LazyDictionary<TKey, TValue> : ILazyDictionary<TKey, TValue> {
    private readonly ConcurrentDictionary<TKey, TValue> impl_ = new();
    private readonly Func<TKey, TValue> handler_;

    public LazyDictionary(Func<TKey, TValue> handler) {
      this.handler_ = handler;
    }

    public LazyDictionary(
        Func<LazyDictionary<TKey, TValue>, TKey, TValue> handler) {
      this.handler_ = (TKey key) => handler(this, key);
    }

    public int Count => this.impl_.Count;
    public void Clear() => this.impl_.Clear();
    public bool ContainsKey(TKey key) => this.impl_.ContainsKey(key);

    public TValue this[TKey key] {
      get {
        if (this.impl_.TryGetValue(key, out var value)) {
          return value;
        }

        value = this.handler_(key);
        this.impl_[key] = value;
        return value;
      }
      set => this.impl_[key] = value;
    }
  }
}