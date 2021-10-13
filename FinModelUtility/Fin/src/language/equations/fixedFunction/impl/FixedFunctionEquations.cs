﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace fin.language.equations.fixedFunction {
  public partial class FixedFunctionEquations<TIdentifier> :
      IFixedFunctionEquations<TIdentifier> {
    public FixedFunctionEquations() {
      this.ScalarInputs =
          new ReadOnlyDictionary<TIdentifier, IScalarInput<TIdentifier>>(
              this.scalarInputs_);
      this.ScalarOutputs =
          new ReadOnlyDictionary<TIdentifier, IScalarOutput<TIdentifier>>(
              this.scalarOutputs_);

      this.ColorInputs =
          new ReadOnlyDictionary<TIdentifier, IColorInput<TIdentifier>>(
              this.colorInputs_);
      this.ColorOutputs =
          new ReadOnlyDictionary<TIdentifier, IColorOutput<TIdentifier>>(
              this.colorOutputs_);
    }
  }
}