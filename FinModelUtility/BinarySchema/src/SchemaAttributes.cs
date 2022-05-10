﻿using System;

namespace schema {
  /// <summary>
  ///   Attribute for automatically generating Read/Write methods on
  ///   classes/structs. These are generated at compile-time, so the field
  ///   order will be 1:1 to the original class/struct and there should be no
  ///   performance cost compared to manually defined logic.
  ///
  ///   For any types that have this attribute, DO NOT modify or move around
  ///   the fields unless you know what you're doing!
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public class SchemaAttribute : Attribute {}


  public enum IntType {
    BYTE,
    SBYTE,
    INT16,
    UINT16,
    INT32,
    UINT32,
    INT64,
    UINT64
  }


  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class NullTerminatedSchemaStringAttribute : Attribute {
    /// <summary>
    ///   Parses a length with the given format immediately before the string/array.
    /// </summary>
    public NullTerminatedSchemaStringAttribute(int maxLength) {
      this.MaxLength = maxLength;
    }

    public int MaxLength { get; }
  }


  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class ArrayLengthSourceAttribute : Attribute {
    /// <summary>
    ///   Parses a length with the given format immediately before the array.
    /// </summary>
    public ArrayLengthSourceAttribute(IntType lengthType) {
      this.Method = SequenceLengthSourceType.IMMEDIATE_VALUE;
      this.LengthType = lengthType;
    }

    /// <summary>
    ///   Uses another field for the length. This separate field will only be used when
    ///   reading/writing.
    /// </summary>
    public ArrayLengthSourceAttribute(string otherMemberName) {
      this.Method = SequenceLengthSourceType.OTHER_MEMBER;
      this.OtherMemberName = otherMemberName;
    }

    public SequenceLengthSourceType Method { get; }

    public IntType LengthType { get; }
    public string OtherMemberName { get; }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class StringLengthSourceAttribute : Attribute {
    /// <summary>
    ///   Parses a length with the given format immediately before the string.
    /// </summary>
    public StringLengthSourceAttribute(IntType lengthType) {
      this.Method = StringLengthSourceType.IMMEDIATE_VALUE;
      this.LengthType = lengthType;
    }

    /// <summary>
    ///   Uses another field for the length. This separate field will only be used when
    ///   reading/writing.
    /// </summary>
    public StringLengthSourceAttribute(string otherMemberName) {
      this.Method = StringLengthSourceType.OTHER_MEMBER;
      this.OtherMemberName = otherMemberName;
    }

    public StringLengthSourceAttribute(int constLength) {
      this.Method = StringLengthSourceType.CONST;
      this.ConstLength = constLength;
    }

    public StringLengthSourceType Method { get; }

    public IntType LengthType { get; }
    public string? OtherMemberName { get; }
    public int ConstLength { get; }
  }


  public enum SchemaNumberType {
    UNDEFINED,

    SBYTE,
    BYTE,
    INT16,
    UINT16,
    INT32,
    UINT32,
    INT64,
    UINT64,

    SINGLE,
    DOUBLE,

    UN8,

    SN16,
    UN16,
  }


  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class FormatAttribute : Attribute {
    public FormatAttribute(SchemaNumberType numberType) {
      this.NumberType = numberType;
    }

    public SchemaNumberType NumberType { get; }
  }


  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class IfBooleanAttribute : Attribute {
    public IfBooleanAttribute(IntType lengthType) {
      this.Method = IfBooleanSourceType.IMMEDIATE_VALUE;
      this.BooleanType = lengthType;
    }

    public IfBooleanAttribute(string otherMemberName) {
      this.Method = IfBooleanSourceType.OTHER_MEMBER;
      this.OtherMemberName = otherMemberName;
    }

    public IfBooleanSourceType Method { get; }

    public IntType BooleanType { get; }
    public string? OtherMemberName { get; }
  }


  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class AlignAttribute : Attribute {
    public AlignAttribute(int align) {
      this.Align = align;
    }

    public int Align { get; }
  }
}