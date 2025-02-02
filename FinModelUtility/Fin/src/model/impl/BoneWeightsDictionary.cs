﻿using fin.util.asserts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fin.model.impl {
  public class BoneWeightsDictionary {
    private readonly List<IBoneWeights> boneWeights_ = new();
    private readonly Dictionary<int, BoneWeightsSet> boneWeightsByCount_ =
      new();

    public IReadOnlyList<IBoneWeights> List => boneWeights_;

    public IBoneWeights GetOrCreate(
      PreprojectMode preprojectMode,
      params IBoneWeight[] weights
    ) {
      var error = .0001;
      if (weights.Length > 1) {
        weights = weights.Where(boneWeight => boneWeight.Weight > error)
          .ToArray();
      }

      var totalWeight = weights.Select(weight => weight.Weight).Sum();
      Asserts.True(Math.Abs(totalWeight - 1) < error);

      if (!this.boneWeightsByCount_.TryGetValue(
            weights.Length,
            out var allBoneWeightsWithCount)) {
        allBoneWeightsWithCount = this.boneWeightsByCount_[weights.Length] =
          new BoneWeightsSet();
      }

      if (!allBoneWeightsWithCount.TryGetExisting(preprojectMode, weights,
            out var boneWeights)) {
        allBoneWeightsWithCount.Add(boneWeights = CreateInstance_(preprojectMode, weights));
      }

      return boneWeights;
    }

    public IBoneWeights Create(
      PreprojectMode preprojectMode,
      params IBoneWeight[] weights
    ) {
      var error = .0001;
      if (weights.Length > 1) {
        weights = weights.Where(boneWeight => boneWeight.Weight > error)
          .ToArray();
      }

      var totalWeight = weights.Select(weight => weight.Weight).Sum();
      Asserts.True(Math.Abs(totalWeight - 1) < error);

      if (!this.boneWeightsByCount_.TryGetValue(
            weights.Length,
            out var allBoneWeightsWithCount)) {
        allBoneWeightsWithCount = this.boneWeightsByCount_[weights.Length] =
          new BoneWeightsSet();
      }

      var boneWeights = CreateInstance_(preprojectMode, weights);
      allBoneWeightsWithCount.Add(boneWeights);

      return boneWeights;
    }

    private IBoneWeights CreateInstance_(
      PreprojectMode preprojectMode,
      params IBoneWeight[] weights) {
      var error = .0001;
      if (weights.Length > 1) {
        weights = weights.Where(boneWeight => boneWeight.Weight > error)
          .ToArray();
      }

      var totalWeight = weights.Select(weight => weight.Weight).Sum();
      Asserts.True(Math.Abs(totalWeight - 1) < error);

      var boneWeights = new BoneWeightsImpl {
        Index = boneWeights_.Count,
        PreprojectMode = preprojectMode,
        Weights = weights,
      };

      this.boneWeights_.Add(boneWeights);

      return boneWeights;
    }

    public static int GetHashCode(PreprojectMode preprojectMode, IReadOnlyList<IBoneWeight> weights) {
      int hash = 216613626;
      var sub = 16780669;
      hash = hash * sub ^ preprojectMode.GetHashCode();
      foreach (var weight in weights) {
        hash = hash * sub ^ weight.GetHashCode();
      }
      return hash;
    }

    private class BoneWeightsImpl : IBoneWeights {
      public int Index { get; init; }
      public PreprojectMode PreprojectMode { get; init; }
      public IReadOnlyList<IBoneWeight> Weights { get; init; }

      public override int GetHashCode()
        => BoneWeightsSet.GetHashCode(PreprojectMode, Weights);

      public override bool Equals(object? obj) {
        if (obj is not BoneWeightsImpl other) {
          return false;
        }

        return Equals(other);
      }

      public bool Equals(IBoneWeights? weights)
        => weights != null && this.Equals(weights.PreprojectMode, weights.Weights);

      public bool Equals(PreprojectMode preprojectMode, IReadOnlyList<IBoneWeight> weights) {
        if (preprojectMode != PreprojectMode) {
          return false;
        }

        var otherWeights = weights;
        if (Weights.Count != otherWeights.Count) {
          return false;
        }

        for (var w = 0; w < Weights.Count; ++w) {
          var weight = Weights[w];
          var existingWeight = otherWeights[w];

          if (weight.Bone != existingWeight.Bone) {
            return false;
          }

          if (Math.Abs(weight.Weight - existingWeight.Weight) > .0001) {
            return false;
          }

          if (!(weight.SkinToBone == null &&
                existingWeight.SkinToBone == null) ||
              !(weight.SkinToBone?.Equals(existingWeight.SkinToBone) ??
                false)) {
            return false;
          }
        }

        return true;
      }
    }
  }
}
