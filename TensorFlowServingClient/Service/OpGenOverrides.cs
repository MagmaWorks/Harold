// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: tensorflow/core/framework/op_gen_overrides.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Tensorflow {

  /// <summary>Holder for reflection information generated from tensorflow/core/framework/op_gen_overrides.proto</summary>
  public static partial class OpGenOverridesReflection {

    #region Descriptor
    /// <summary>File descriptor for tensorflow/core/framework/op_gen_overrides.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static OpGenOverridesReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjB0ZW5zb3JmbG93L2NvcmUvZnJhbWV3b3JrL29wX2dlbl9vdmVycmlkZXMu",
            "cHJvdG8SCnRlbnNvcmZsb3caKnRlbnNvcmZsb3cvY29yZS9mcmFtZXdvcmsv",
            "YXR0cl92YWx1ZS5wcm90byKnAwoNT3BHZW5PdmVycmlkZRIMCgRuYW1lGAEg",
            "ASgJEgwKBHNraXAYAiABKAgSDAoEaGlkZRgDIAEoCBIRCglyZW5hbWVfdG8Y",
            "BCABKAkSDQoFYWxpYXMYBSADKAkSOwoMYXR0cl9kZWZhdWx0GAYgAygLMiUu",
            "dGVuc29yZmxvdy5PcEdlbk92ZXJyaWRlLkF0dHJEZWZhdWx0EjUKC2F0dHJf",
            "cmVuYW1lGAcgAygLMiAudGVuc29yZmxvdy5PcEdlbk92ZXJyaWRlLlJlbmFt",
            "ZRI2CgxpbnB1dF9yZW5hbWUYCCADKAsyIC50ZW5zb3JmbG93Lk9wR2VuT3Zl",
            "cnJpZGUuUmVuYW1lEjcKDW91dHB1dF9yZW5hbWUYCSADKAsyIC50ZW5zb3Jm",
            "bG93Lk9wR2VuT3ZlcnJpZGUuUmVuYW1lGkEKC0F0dHJEZWZhdWx0EgwKBG5h",
            "bWUYASABKAkSJAoFdmFsdWUYAiABKAsyFS50ZW5zb3JmbG93LkF0dHJWYWx1",
            "ZRoiCgZSZW5hbWUSDAoEZnJvbRgBIAEoCRIKCgJ0bxgCIAEoCSI3Cg5PcEdl",
            "bk92ZXJyaWRlcxIlCgJvcBgBIAMoCzIZLnRlbnNvcmZsb3cuT3BHZW5PdmVy",
            "cmlkZWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Tensorflow.AttrValueReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.OpGenOverride), global::Tensorflow.OpGenOverride.Parser, new[]{ "Name", "Skip", "Hide", "RenameTo", "Alias", "AttrDefault", "AttrRename", "InputRename", "OutputRename" }, null, null, new pbr::GeneratedClrTypeInfo[] { new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.OpGenOverride.Types.AttrDefault), global::Tensorflow.OpGenOverride.Types.AttrDefault.Parser, new[]{ "Name", "Value" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.OpGenOverride.Types.Rename), global::Tensorflow.OpGenOverride.Types.Rename.Parser, new[]{ "From", "To" }, null, null, null)}),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.OpGenOverrides), global::Tensorflow.OpGenOverrides.Parser, new[]{ "Op" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// Used to override the default API &amp; behavior in the generated code
  /// for client languages, from what you would get from the OpDef alone.
  /// This is so we can evolve the API while remaining backwards
  /// compatible when interpretting old graphs.  Overrides go in an
  /// "op_gen_overrides.pbtxt" file with a text-format OpGenOverrides
  /// message.  Right now these only apply to the C++ API.
  /// TODO(josh11b): In the future there will be a common set of overrides
  /// and per-client-language overrides.
  ///
  /// WARNING: Be *very* careful using these features -- these overrides
  /// can change the semantics of existing code.  These changes may need
  /// to wait until a major release of TensorFlow to avoid breaking our
  /// compatibility promises.
  /// </summary>
  public sealed partial class OpGenOverride : pb::IMessage<OpGenOverride> {
    private static readonly pb::MessageParser<OpGenOverride> _parser = new pb::MessageParser<OpGenOverride>(() => new OpGenOverride());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<OpGenOverride> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.OpGenOverridesReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpGenOverride() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpGenOverride(OpGenOverride other) : this() {
      name_ = other.name_;
      skip_ = other.skip_;
      hide_ = other.hide_;
      renameTo_ = other.renameTo_;
      alias_ = other.alias_.Clone();
      attrDefault_ = other.attrDefault_.Clone();
      attrRename_ = other.attrRename_.Clone();
      inputRename_ = other.inputRename_.Clone();
      outputRename_ = other.outputRename_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpGenOverride Clone() {
      return new OpGenOverride(this);
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 1;
    private string name_ = "";
    /// <summary>
    /// Name of the op to apply overrides to.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "skip" field.</summary>
    public const int SkipFieldNumber = 2;
    private bool skip_;
    /// <summary>
    /// Do not include this op in the generated API.
    /// If `skip` is true, all other overrides are ignored for this op.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Skip {
      get { return skip_; }
      set {
        skip_ = value;
      }
    }

    /// <summary>Field number for the "hide" field.</summary>
    public const int HideFieldNumber = 3;
    private bool hide_;
    /// <summary>
    /// Hide this op by putting it into an internal namespace (or whatever
    /// is appropriate in the target language).
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Hide {
      get { return hide_; }
      set {
        hide_ = value;
      }
    }

    /// <summary>Field number for the "rename_to" field.</summary>
    public const int RenameToFieldNumber = 4;
    private string renameTo_ = "";
    /// <summary>
    /// Use a different name in the API than the op's name. Note that
    /// the op's name in `backticks` will also be replaced in the docs.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string RenameTo {
      get { return renameTo_; }
      set {
        renameTo_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "alias" field.</summary>
    public const int AliasFieldNumber = 5;
    private static readonly pb::FieldCodec<string> _repeated_alias_codec
        = pb::FieldCodec.ForString(42);
    private readonly pbc::RepeatedField<string> alias_ = new pbc::RepeatedField<string>();
    /// <summary>
    /// Create *additional* API endpoints with different names (contrast
    /// with rename_to, which affects the original name).
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<string> Alias {
      get { return alias_; }
    }

    /// <summary>Field number for the "attr_default" field.</summary>
    public const int AttrDefaultFieldNumber = 6;
    private static readonly pb::FieldCodec<global::Tensorflow.OpGenOverride.Types.AttrDefault> _repeated_attrDefault_codec
        = pb::FieldCodec.ForMessage(50, global::Tensorflow.OpGenOverride.Types.AttrDefault.Parser);
    private readonly pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.AttrDefault> attrDefault_ = new pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.AttrDefault>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.AttrDefault> AttrDefault {
      get { return attrDefault_; }
    }

    /// <summary>Field number for the "attr_rename" field.</summary>
    public const int AttrRenameFieldNumber = 7;
    private static readonly pb::FieldCodec<global::Tensorflow.OpGenOverride.Types.Rename> _repeated_attrRename_codec
        = pb::FieldCodec.ForMessage(58, global::Tensorflow.OpGenOverride.Types.Rename.Parser);
    private readonly pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.Rename> attrRename_ = new pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.Rename>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.Rename> AttrRename {
      get { return attrRename_; }
    }

    /// <summary>Field number for the "input_rename" field.</summary>
    public const int InputRenameFieldNumber = 8;
    private static readonly pb::FieldCodec<global::Tensorflow.OpGenOverride.Types.Rename> _repeated_inputRename_codec
        = pb::FieldCodec.ForMessage(66, global::Tensorflow.OpGenOverride.Types.Rename.Parser);
    private readonly pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.Rename> inputRename_ = new pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.Rename>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.Rename> InputRename {
      get { return inputRename_; }
    }

    /// <summary>Field number for the "output_rename" field.</summary>
    public const int OutputRenameFieldNumber = 9;
    private static readonly pb::FieldCodec<global::Tensorflow.OpGenOverride.Types.Rename> _repeated_outputRename_codec
        = pb::FieldCodec.ForMessage(74, global::Tensorflow.OpGenOverride.Types.Rename.Parser);
    private readonly pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.Rename> outputRename_ = new pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.Rename>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tensorflow.OpGenOverride.Types.Rename> OutputRename {
      get { return outputRename_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as OpGenOverride);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(OpGenOverride other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Name != other.Name) return false;
      if (Skip != other.Skip) return false;
      if (Hide != other.Hide) return false;
      if (RenameTo != other.RenameTo) return false;
      if(!alias_.Equals(other.alias_)) return false;
      if(!attrDefault_.Equals(other.attrDefault_)) return false;
      if(!attrRename_.Equals(other.attrRename_)) return false;
      if(!inputRename_.Equals(other.inputRename_)) return false;
      if(!outputRename_.Equals(other.outputRename_)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (Skip != false) hash ^= Skip.GetHashCode();
      if (Hide != false) hash ^= Hide.GetHashCode();
      if (RenameTo.Length != 0) hash ^= RenameTo.GetHashCode();
      hash ^= alias_.GetHashCode();
      hash ^= attrDefault_.GetHashCode();
      hash ^= attrRename_.GetHashCode();
      hash ^= inputRename_.GetHashCode();
      hash ^= outputRename_.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (Skip != false) {
        output.WriteRawTag(16);
        output.WriteBool(Skip);
      }
      if (Hide != false) {
        output.WriteRawTag(24);
        output.WriteBool(Hide);
      }
      if (RenameTo.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(RenameTo);
      }
      alias_.WriteTo(output, _repeated_alias_codec);
      attrDefault_.WriteTo(output, _repeated_attrDefault_codec);
      attrRename_.WriteTo(output, _repeated_attrRename_codec);
      inputRename_.WriteTo(output, _repeated_inputRename_codec);
      outputRename_.WriteTo(output, _repeated_outputRename_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (Skip != false) {
        size += 1 + 1;
      }
      if (Hide != false) {
        size += 1 + 1;
      }
      if (RenameTo.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(RenameTo);
      }
      size += alias_.CalculateSize(_repeated_alias_codec);
      size += attrDefault_.CalculateSize(_repeated_attrDefault_codec);
      size += attrRename_.CalculateSize(_repeated_attrRename_codec);
      size += inputRename_.CalculateSize(_repeated_inputRename_codec);
      size += outputRename_.CalculateSize(_repeated_outputRename_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(OpGenOverride other) {
      if (other == null) {
        return;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.Skip != false) {
        Skip = other.Skip;
      }
      if (other.Hide != false) {
        Hide = other.Hide;
      }
      if (other.RenameTo.Length != 0) {
        RenameTo = other.RenameTo;
      }
      alias_.Add(other.alias_);
      attrDefault_.Add(other.attrDefault_);
      attrRename_.Add(other.attrRename_);
      inputRename_.Add(other.inputRename_);
      outputRename_.Add(other.outputRename_);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 16: {
            Skip = input.ReadBool();
            break;
          }
          case 24: {
            Hide = input.ReadBool();
            break;
          }
          case 34: {
            RenameTo = input.ReadString();
            break;
          }
          case 42: {
            alias_.AddEntriesFrom(input, _repeated_alias_codec);
            break;
          }
          case 50: {
            attrDefault_.AddEntriesFrom(input, _repeated_attrDefault_codec);
            break;
          }
          case 58: {
            attrRename_.AddEntriesFrom(input, _repeated_attrRename_codec);
            break;
          }
          case 66: {
            inputRename_.AddEntriesFrom(input, _repeated_inputRename_codec);
            break;
          }
          case 74: {
            outputRename_.AddEntriesFrom(input, _repeated_outputRename_codec);
            break;
          }
        }
      }
    }

    #region Nested types
    /// <summary>Container for nested types declared in the OpGenOverride message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static partial class Types {
      /// <summary>
      /// Map the name of an attr to a new default value to use.  This
      /// default will be used when creating new graphs, as opposed to the
      /// default in the OpDef, which will be used when interpreting old
      /// GraphDefs.  If this attr is also renamed (using attr_rename
      /// below), use the original name of the attr.
      /// </summary>
      public sealed partial class AttrDefault : pb::IMessage<AttrDefault> {
        private static readonly pb::MessageParser<AttrDefault> _parser = new pb::MessageParser<AttrDefault>(() => new AttrDefault());
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<AttrDefault> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::Tensorflow.OpGenOverride.Descriptor.NestedTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public AttrDefault() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public AttrDefault(AttrDefault other) : this() {
          name_ = other.name_;
          Value = other.value_ != null ? other.Value.Clone() : null;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public AttrDefault Clone() {
          return new AttrDefault(this);
        }

        /// <summary>Field number for the "name" field.</summary>
        public const int NameFieldNumber = 1;
        private string name_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string Name {
          get { return name_; }
          set {
            name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        /// <summary>Field number for the "value" field.</summary>
        public const int ValueFieldNumber = 2;
        private global::Tensorflow.AttrValue value_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public global::Tensorflow.AttrValue Value {
          get { return value_; }
          set {
            value_ = value;
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as AttrDefault);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(AttrDefault other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (Name != other.Name) return false;
          if (!object.Equals(Value, other.Value)) return false;
          return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (Name.Length != 0) hash ^= Name.GetHashCode();
          if (value_ != null) hash ^= Value.GetHashCode();
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (Name.Length != 0) {
            output.WriteRawTag(10);
            output.WriteString(Name);
          }
          if (value_ != null) {
            output.WriteRawTag(18);
            output.WriteMessage(Value);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (Name.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
          }
          if (value_ != null) {
            size += 1 + pb::CodedOutputStream.ComputeMessageSize(Value);
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(AttrDefault other) {
          if (other == null) {
            return;
          }
          if (other.Name.Length != 0) {
            Name = other.Name;
          }
          if (other.value_ != null) {
            if (value_ == null) {
              value_ = new global::Tensorflow.AttrValue();
            }
            Value.MergeFrom(other.Value);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(pb::CodedInputStream input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                input.SkipLastField();
                break;
              case 10: {
                Name = input.ReadString();
                break;
              }
              case 18: {
                if (value_ == null) {
                  value_ = new global::Tensorflow.AttrValue();
                }
                input.ReadMessage(value_);
                break;
              }
            }
          }
        }

      }

      /// <summary>
      /// Change the name used to access attrs/inputs/outputs in the API
      /// from what is used in the GraphDef.  Note that these names in
      /// `backticks` will also be replaced in the docs.
      /// </summary>
      public sealed partial class Rename : pb::IMessage<Rename> {
        private static readonly pb::MessageParser<Rename> _parser = new pb::MessageParser<Rename>(() => new Rename());
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<Rename> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::Tensorflow.OpGenOverride.Descriptor.NestedTypes[1]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Rename() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Rename(Rename other) : this() {
          from_ = other.from_;
          to_ = other.to_;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Rename Clone() {
          return new Rename(this);
        }

        /// <summary>Field number for the "from" field.</summary>
        public const int FromFieldNumber = 1;
        private string from_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string From {
          get { return from_; }
          set {
            from_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        /// <summary>Field number for the "to" field.</summary>
        public const int ToFieldNumber = 2;
        private string to_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string To {
          get { return to_; }
          set {
            to_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as Rename);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(Rename other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (From != other.From) return false;
          if (To != other.To) return false;
          return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (From.Length != 0) hash ^= From.GetHashCode();
          if (To.Length != 0) hash ^= To.GetHashCode();
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (From.Length != 0) {
            output.WriteRawTag(10);
            output.WriteString(From);
          }
          if (To.Length != 0) {
            output.WriteRawTag(18);
            output.WriteString(To);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (From.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(From);
          }
          if (To.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(To);
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(Rename other) {
          if (other == null) {
            return;
          }
          if (other.From.Length != 0) {
            From = other.From;
          }
          if (other.To.Length != 0) {
            To = other.To;
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(pb::CodedInputStream input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                input.SkipLastField();
                break;
              case 10: {
                From = input.ReadString();
                break;
              }
              case 18: {
                To = input.ReadString();
                break;
              }
            }
          }
        }

      }

    }
    #endregion

  }

  public sealed partial class OpGenOverrides : pb::IMessage<OpGenOverrides> {
    private static readonly pb::MessageParser<OpGenOverrides> _parser = new pb::MessageParser<OpGenOverrides>(() => new OpGenOverrides());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<OpGenOverrides> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.OpGenOverridesReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpGenOverrides() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpGenOverrides(OpGenOverrides other) : this() {
      op_ = other.op_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpGenOverrides Clone() {
      return new OpGenOverrides(this);
    }

    /// <summary>Field number for the "op" field.</summary>
    public const int OpFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Tensorflow.OpGenOverride> _repeated_op_codec
        = pb::FieldCodec.ForMessage(10, global::Tensorflow.OpGenOverride.Parser);
    private readonly pbc::RepeatedField<global::Tensorflow.OpGenOverride> op_ = new pbc::RepeatedField<global::Tensorflow.OpGenOverride>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tensorflow.OpGenOverride> Op {
      get { return op_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as OpGenOverrides);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(OpGenOverrides other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!op_.Equals(other.op_)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= op_.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      op_.WriteTo(output, _repeated_op_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += op_.CalculateSize(_repeated_op_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(OpGenOverrides other) {
      if (other == null) {
        return;
      }
      op_.Add(other.op_);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            op_.AddEntriesFrom(input, _repeated_op_codec);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
