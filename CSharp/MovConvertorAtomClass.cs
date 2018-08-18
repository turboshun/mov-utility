using System;
using System.Collections.Generic;
using System.Text;

namespace MOVUtility
{
    /// <summary>
    /// movファイル変換クラス
    /// </summary>
    public partial class MovConvertor
    {
        /// <summary>
        /// ftyp atom class
        /// </summary>
        public class FTYP
        {
            /// <summary> major brand </summary>
            private const string MAJOR_BRAND = "qt  ";
            /// <summary> minor version </summary>
            private readonly byte[] MINOR_VERSION = new byte[4] { 0x20, 0x12, 0x08, 0x00 };
            /// <summary> compatible brand 1 </summary>
            private const string COMPATIBLE_BRAND1 = "qt  ";
            /// <summary> compatible brand 2 </summary>
            private const string COMPATIBLE_BRAND2 = "arai";

            /// <summary>
            /// 出力サイズを取得する。
            /// </summary>
            public uint OutputLength
            {
                get
                {
                    return 24;
                }
            }

            /// <summary>
            /// 出力する
            /// </summary>
            /// <returns>出力データ</returns>
            public byte[] Output()
            {
                byte[] output = new byte[OutputLength];

                Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                Array.Copy(Encoding.ASCII.GetBytes("ftyp"), 0, output, 4, 4);
                Array.Copy(Encoding.ASCII.GetBytes(MAJOR_BRAND), 0, output, 8, 4);
                Array.Copy(MINOR_VERSION, 0, output, 12, 4);
                Array.Copy(Encoding.ASCII.GetBytes(COMPATIBLE_BRAND1), 0, output, 16, 4);
                Array.Copy(Encoding.ASCII.GetBytes(COMPATIBLE_BRAND2), 0, output, 20, 4);

                return (byte[])output.Clone();
            }
        }

        /// <summary>
        /// moov atom class
        /// </summary>
        public class MOOV
        {
            /// <summary>
            /// トラックタイプ
            /// </summary>
            public enum TrackType
            {
                /// <summary> 未定義 </summary>
                None = -1,
                /// <summary> ビデオ </summary>
                Video = 0,
                /// <summary> オーディオ </summary>
                Audio,
                /// <summary> テキスト </summary>
                Text,
                /// <summary> トラックタイプ最大数 </summary>
                MaxSize = 3
            }

            /// <summary>
            /// mvhd atom class
            /// </summary>
            public class MVHD
            {
                /// <summary> version/flags </summary>
                public uint VersionFlags = 0;
                /// <summary> creation time </summary>
                public uint CreationTime = 0;
                /// <summary> modification time </summary>
                public uint ModificationTime = 0;
                /// <summary> time scale </summary>
                public uint TimeScale = 0;
                /// <summary> duration </summary>
                public uint Duration = 0;
                /// <summary> preferred rate </summary>
                private readonly byte[] PREFERRED_RATE = new byte[4] { 0x0, 0x1, 0x0, 0x0 };
                /// <summary> preferred volume </summary>
                private readonly byte[] PREFERRED_VOLUME = new byte[2] { 0x1, 0x0 };
                /// <summary> reserved </summary>
                private readonly byte[] RESERVED1 = new byte[10] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                /// <summary> matrix structure </summary>
                public byte[] MatrixStructure = new byte[36]
                {
                    0x00, 0x01, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x01, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x40, 0x00, 0x00, 0x00
                };
                /// <summary> preview time </summary>
                public uint PreviewTime = 0;
                /// <summary> preview duration </summary>
                public uint PreviewDuration = 0;
                /// <summary> poster time </summary>
                public uint PosterTime = 0;
                /// <summary> selection time </summary>
                public uint SelectionTime = 0;
                /// <summary> selection duration </summary>
                public uint SelectionDuration = 0;
                /// <summary> current time </summary>
                public uint CurrentTime = 0;
                /// <summary> next track ID </summary>
                public uint NextTrackID = 0;

                /// <summary>
                /// 出力サイズを取得する。
                /// </summary>
                public uint OutputLength
                {
                    get
                    {
                        return 108;
                    }
                }

                /// <summary>
                /// 出力する
                /// </summary>
                /// <returns>出力データ</returns>
                public byte[] Output()
                {
                    byte[] output = new byte[OutputLength];

                    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                    Array.Copy(Encoding.ASCII.GetBytes("mvhd"), 0, output, 4, 4);
                    Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                    Array.Copy(GetReverseBytes(CreationTime), 0, output, 12, 4);
                    Array.Copy(GetReverseBytes(ModificationTime), 0, output, 16, 4);
                    Array.Copy(GetReverseBytes(TimeScale), 0, output, 20, 4);
                    Array.Copy(GetReverseBytes(Duration), 0, output, 24, 4);
                    Array.Copy(PREFERRED_RATE, 0, output, 28, 4);
                    Array.Copy(PREFERRED_VOLUME, 0, output, 32, 2);
                    Array.Copy(RESERVED1, 0, output, 34, 10);
                    Array.Copy(MatrixStructure, 0, output, 44, 36);
                    Array.Copy(GetReverseBytes(PreviewTime), 0, output, 80, 4);
                    Array.Copy(GetReverseBytes(PreviewDuration), 0, output, 84, 4);
                    Array.Copy(GetReverseBytes(PosterTime), 0, output, 88, 4);
                    Array.Copy(GetReverseBytes(SelectionTime), 0, output, 92, 4);
                    Array.Copy(GetReverseBytes(SelectionDuration), 0, output, 96, 4);
                    Array.Copy(GetReverseBytes(CurrentTime), 0, output, 100, 4);
                    Array.Copy(GetReverseBytes(NextTrackID), 0, output, 104, 4);

                    return (byte[])output.Clone();
                }
            }

            /// <summary>
            /// trak atom class
            /// </summary>
            public class TRAK
            {
                /// <summary>
                /// tkhd atom class
                /// </summary>
                public class TKHD
                {
                    /// <summary>
                    /// flag list
                    /// </summary>
                    [Flags]
                    public enum FlagList
                    {
                        /// <summary> enabled </summary>
                        Enabled = 0x0001,
                        /// <summary> in movie </summary>
                        InMovie = 0x0002,
                        /// <summary> in preview </summary>
                        InPreview = 0x0004,
                        /// <summary> in poster </summary>
                        InPoster = 0x0008,
                    }

                    /// <summary> version/flags </summary>
                    public FlagList VersionFlags = (FlagList.Enabled | FlagList.InMovie | FlagList.InPreview | FlagList.InPoster);
                    /// <summary> creation time </summary>
                    public uint CreationTime = 0;
                    /// <summary> modification time </summary>
                    public uint ModificationTime = 0;
                    /// <summary> track ID </summary>
                    public uint TrackId = 0;
                    /// <summary> reserved </summary>
                    private readonly byte[] RESERVED1 = new byte[4] { 0x0, 0x0, 0x0, 0x0 };
                    /// <summary> duration </summary>
                    public uint Duration = 0;
                    /// <summary> reserved </summary>
                    private readonly byte[] RESERVED2 = new byte[8] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                    /// <summary> layer </summary>
                    public ushort Layer = 0;
                    /// <summary> alternate group </summary>
                    public ushort AlternateGroup = 0;
                    /// <summary> volume </summary>
                    public byte[] Volume = new byte[2] { 0x1, 0x0 };
                    /// <summary> reserved </summary>
                    private readonly byte[] RESERVED3 = new byte[2] { 0x0, 0x0 };
                    /// <summary> matrix structure </summary>
                    public byte[] MatrixStructure = new byte[36]
                    {
                        0x00, 0x01, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x01, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00,
                        0x40, 0x00, 0x00, 0x00
                    };
                    /// <summary> width </summary>
                    public ushort Width = 0;
                    /// <summary>  height</summary>
                    public ushort Height = 0;

                    /// <summary>
                    /// 出力サイズを取得する。
                    /// </summary>
                    public uint OutputLength { get { return 92; } }

                    /// <summary>
                    /// 出力する
                    /// </summary>
                    /// <returns>出力データ</returns>
                    public byte[] Output()
                    {
                        byte[] output = new byte[OutputLength];

                        Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                        Array.Copy(Encoding.ASCII.GetBytes("tkhd"), 0, output, 4, 4);
                        Array.Copy(GetReverseBytes((uint)VersionFlags), 0, output, 8, 4);
                        Array.Copy(GetReverseBytes(CreationTime), 0, output, 12, 4);
                        Array.Copy(GetReverseBytes(ModificationTime), 0, output, 16, 4);
                        Array.Copy(GetReverseBytes(TrackId), 0, output, 20, 4);
                        Array.Copy(RESERVED1, 0, output, 24, 4);
                        Array.Copy(GetReverseBytes(Duration), 0, output, 28, 4);
                        Array.Copy(RESERVED2, 0, output, 32, 8);
                        Array.Copy(GetReverseBytes(Layer), 0, output, 40, 2);
                        Array.Copy(GetReverseBytes(AlternateGroup), 0, output, 42, 2);
                        Array.Copy(Volume, 0, output, 44, 2);
                        Array.Copy(RESERVED3, 0, output, 46, 2);
                        Array.Copy(MatrixStructure, 0, output, 48, 36);
                        Array.Copy(GetReverseBytes(Width), 0, output, 84, 2);
                        Array.Copy(GetReverseBytes(0), 0, output, 86, 2);
                        Array.Copy(GetReverseBytes(Height), 0, output, 88, 2);
                        Array.Copy(GetReverseBytes(0), 0, output, 90, 2);

                        return (byte[])output.Clone();
                    }
                }

                /// <summary>
                /// mdia atom class
                /// </summary>
                public class MDIA
                {
                    /// <summary>
                    /// mdhd atom class
                    /// </summary>
                    public class MDHD
                    {
                        /// <summary> version/flags </summary>
                        public uint VersionFlags = 0;
                        /// <summary> creation time </summary>
                        public uint CreationTime = 0;
                        /// <summary> modification time </summary>
                        public uint ModificationTime = 0;
                        /// <summary> time scale </summary>
                        public uint TimeScale = 0;
                        /// <summary> duration </summary>
                        public uint Duration = 0;
                        /// <summary> language </summary>
                        public ushort Language = 0;
                        /// <summary> quality </summary>
                        public ushort Quality = 0;

                        /// <summary>
                        /// 出力サイズを取得する。
                        /// </summary>
                        public uint OutputLength { get { return 32; } }

                        /// <summary>
                        /// 出力する
                        /// </summary>
                        /// <returns>出力データ</returns>
                        public byte[] Output()
                        {
                            byte[] output = new byte[OutputLength];

                            Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                            Array.Copy(Encoding.ASCII.GetBytes("mdhd"), 0, output, 4, 4);
                            Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                            Array.Copy(GetReverseBytes(CreationTime), 0, output, 12, 4);
                            Array.Copy(GetReverseBytes(ModificationTime), 0, output, 16, 4);
                            Array.Copy(GetReverseBytes(TimeScale), 0, output, 20, 4);
                            Array.Copy(GetReverseBytes(Duration), 0, output, 24, 4);
                            Array.Copy(GetReverseBytes(Language), 0, output, 28, 2);
                            Array.Copy(GetReverseBytes(Quality), 0, output, 30, 2);

                            return (byte[])output.Clone();
                        }
                    }

                    /// <summary>
                    /// minf atom class
                    /// </summary>
                    public class MINF
                    {
                        /// <summary>
                        /// graphics mode list
                        /// </summary>
                        [Flags]
                        public enum GraphicsModeList
                        {
                            /// <summary> copy </summary>
                            Copy = 0x0,
                            /// <summary> dither copy </summary>
                            DitherCopy = 0x40,
                            /// <summary> blend </summary>
                            Blend = 0x20,
                            /// <summary> transparent </summary>
                            Transparent = 0x24,
                            /// <summary> straight alpha </summary>
                            StraightAlpha = 0x100,
                            /// <summary> premul white alpha </summary>
                            PremulWhiteAlpha = 0x101,
                            /// <summary> premul black alpha </summary>
                            PremulBlackAlpha = 0x102,
                            /// <summary> straight alpha blend </summary>
                            StraightAlphaBlend = 0x104,
                            /// <summary> composition </summary>
                            Composition = 0x103,
                        }

                        /// <summary>
                        /// vmhd atom class
                        /// </summary>
                        public class VMHD
                        {
                            /// <summary> version/flags </summary>
                            public uint VersionFlags = 0;
                            /// <summary> graphics mode </summary>
                            public ushort GraphicsMode = (ushort)GraphicsModeList.Copy;
                            /// <summary> option color </summary>
                            public byte[] Opcolor = new byte[6] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

                            /// <summary>
                            /// 出力サイズを取得する。
                            /// </summary>
                            public uint OutputLength { get { return 20; } }

                            /// <summary>
                            /// 出力する
                            /// </summary>
                            /// <returns>出力データ</returns>
                            public byte[] Output()
                            {
                                byte[] output = new byte[OutputLength];

                                Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                Array.Copy(Encoding.ASCII.GetBytes("vmhd"), 0, output, 4, 4);
                                Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                                Array.Copy(GetReverseBytes(GraphicsMode), 0, output, 12, 2);
                                Array.Copy(Opcolor, 0, output, 14, 6);

                                return (byte[])output.Clone();
                            }
                        }

                        /// <summary>
                        /// smhd atom class
                        /// </summary>
                        public class SMHD
                        {
                            /// <summary> version/flags </summary>
                            public uint VersionFlags = 0;
                            /// <summary>  </summary>
                            public ushort Balance = 0;
                            /// <summary> reserved </summary>
                            private readonly byte[] RESERVED = new byte[2] { 0x0, 0x00 };

                            /// <summary>
                            /// 出力サイズを取得する。
                            /// </summary>
                            public uint OutputLength { get { return 16; } }

                            /// <summary>
                            /// 出力する
                            /// </summary>
                            /// <returns>出力データ</returns>
                            public byte[] Output()
                            {
                                byte[] output = new byte[OutputLength];

                                Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                Array.Copy(Encoding.ASCII.GetBytes("smhd"), 0, output, 4, 4);
                                Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                                Array.Copy(GetReverseBytes(Balance), 0, output, 12, 2);
                                Array.Copy(RESERVED, 0, output, 14, 2);

                                return (byte[])output.Clone();
                            }
                        }

                        /// <summary>
                        /// gmhd atom class
                        /// </summary>
                        public class GMHD
                        {
                            /// <summary>
                            /// gmin atom class
                            /// </summary>
                            public class GMIN
                            {
                                /// <summary> version/flags </summary>
                                public uint VersionFlags = 0;
                                /// <summary> graphics mode </summary>
                                public ushort GraphicsMode = (ushort)GraphicsModeList.Copy;
                                /// <summary> option color </summary>
                                public byte[] Opcolor = new byte[6] { 0x0, 0x80, 0x0, 0x80, 0x0, 0x80 };
                                /// <summary>  </summary>
                                public ushort Balance = 0;
                                /// <summary> reserved </summary>
                                private readonly byte[] RESERVED = new byte[2] { 0x0, 0x00 };

                                /// <summary>
                                /// 出力サイズを取得する。
                                /// </summary>
                                public uint OutputLength
                                {
                                    get
                                    {
                                        return 24;
                                    }
                                }

                                /// <summary>
                                /// 出力する
                                /// </summary>
                                /// <returns>出力データ</returns>
                                public byte[] Output()
                                {
                                    byte[] output = new byte[OutputLength];

                                    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                    Array.Copy(Encoding.ASCII.GetBytes("gmin"), 0, output, 4, 4);
                                    Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                                    Array.Copy(GetReverseBytes(GraphicsMode), 0, output, 12, 2);
                                    Array.Copy(Opcolor, 0, output, 14, 6);
                                    Array.Copy(GetReverseBytes(Balance), 0, output, 20, 2);
                                    Array.Copy(RESERVED, 0, output, 22, 2);

                                    return (byte[])output.Clone();
                                }
                            }

                            /// <summary> gmin atom </summary>
                            public GMIN Gmin = new GMIN();

                            /// <summary>
                            /// 出力サイズを取得する。
                            /// </summary>
                            public uint OutputLength { get { return (8 + Gmin.OutputLength); } }

                            /// <summary>
                            /// 出力する
                            /// </summary>
                            /// <returns>出力データ</returns>
                            public byte[] Output()
                            {
                                byte[] gminOutput = Gmin.Output();
                                byte[] output = new byte[OutputLength];

                                Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                Array.Copy(Encoding.ASCII.GetBytes("gmhd"), 0, output, 4, 4);
                                Array.Copy(gminOutput, 0, output, 8, gminOutput.LongLength);

                                return (byte[])output.Clone();
                            }
                        }

                        /// <summary>
                        /// dinf atom class
                        /// </summary>
                        public class DINF
                        {
                            /// <summary>
                            /// dref atom class
                            /// </summary>
                            public class DREF
                            {
                                /// <summary>
                                /// data reference information class
                                /// </summary>
                                public class DataReferenceInformation
                                {
                                    /// <summary>
                                    /// data reference information type list
                                    /// </summary>
                                    public enum DataReferenceInformationTypeList
                                    {
                                        /// <summary> alias </summary>
                                        alis = 0,
                                        /// <summary> resource type</summary>
                                        rsrc,
                                        /// <summary> url </summary>
                                        url
                                    }

                                    /// <summary> data reference information type </summary>
                                    public string DataReferenceInformationType = "    ";
                                    /// <summary> version/flags </summary>
                                    public uint VersionFlags = 1;
                                    /// <summary> data </summary>
                                    public byte[] Data = null;

                                    /// <summary>
                                    /// 出力する
                                    /// </summary>
                                    /// <returns>出力データ</returns>
                                    public byte[] Output()
                                    {
                                        byte[] output = new byte[16 + (uint)(Data == null ? 0 : Data.LongLength)];

                                        Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                        Array.Copy(Encoding.ASCII.GetBytes(DataReferenceInformationType), 0, output, 4, 4);
                                        Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);

                                        if (Data != null)
                                        {
                                            Array.Copy(Data, 0, output, 16, Data.Length);
                                        }

                                        return (byte[])output.Clone();
                                    }
                                }

                                /// <summary> version/flags </summary>
                                public uint VersionFlags = 0;
                                /// <summary>  data reference informations </summary>
                                public List<DataReferenceInformation> DataReferenceInformations = new List<DataReferenceInformation>();

                                /// <summary>
                                /// 出力サイズを取得する。
                                /// </summary>
                                public uint OutputLength
                                {
                                    get
                                    {
                                        uint dataReferenceInformationsLength = 0;

                                        for (int i = 0; i < DataReferenceInformations.Count; i++)
                                        {
                                            if (DataReferenceInformations[i] != null)
                                            {
                                                dataReferenceInformationsLength += (uint)((DataReferenceInformation)DataReferenceInformations[i]).Output().LongLength;
                                            }
                                        }

                                        return (16 + dataReferenceInformationsLength);
                                    }
                                }

                                /// <summary>
                                /// 出力する
                                /// </summary>
                                /// <returns>出力データ</returns>
                                public byte[] Output()
                                {
                                    byte[] dataReferenceInformations = null;

                                    for (int i = 0; i < DataReferenceInformations.Count; i++)
                                    {
                                        if (DataReferenceInformations[i] != null)
                                        {
                                            byte[] tempBuffer = (DataReferenceInformations[i]).Output();
                                            uint copyOffset = 0;

                                            if (dataReferenceInformations == null)
                                            {
                                                dataReferenceInformations = new byte[tempBuffer.LongLength];
                                            }
                                            else
                                            {
                                                copyOffset = (uint)dataReferenceInformations.LongLength;
                                                Array.Resize(ref dataReferenceInformations, (int)(dataReferenceInformations.LongLength + tempBuffer.LongLength));
                                            }

                                            Array.Copy(tempBuffer, 0, dataReferenceInformations, copyOffset, tempBuffer.LongLength);
                                        }
                                    }

                                    byte[] output = new byte[OutputLength];

                                    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                    Array.Copy(Encoding.ASCII.GetBytes("dref"), 0, output, 4, 4);
                                    Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                                    Array.Copy(GetReverseBytes((uint)DataReferenceInformations.Count), 0, output, 12, 4);

                                    if (dataReferenceInformations != null)
                                    {
                                        Array.Copy(dataReferenceInformations, 0, output, 16, dataReferenceInformations.LongLength);
                                    }

                                    return (byte[])output.Clone();
                                }
                            }

                            /// <summary> dref atom </summary>
                            public DREF Dref = new DREF();

                            /// <summary>
                            /// 出力サイズを取得する。
                            /// </summary>
                            public uint OutputLength { get { return (8 + Dref.OutputLength); } }

                            /// <summary>
                            /// 出力する
                            /// </summary>
                            /// <returns>出力データ</returns>
                            public byte[] Output()
                            {
                                byte[] drefOutput = Dref.Output();
                                byte[] output = new byte[OutputLength];

                                Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                Array.Copy(Encoding.ASCII.GetBytes("dinf"), 0, output, 4, 4);
                                Array.Copy(drefOutput, 0, output, 8, drefOutput.LongLength);

                                return (byte[])output.Clone();
                            }
                        }

                        /// <summary>
                        /// stbl atom class
                        /// </summary>
                        public class STBL
                        {
                            /// <summary>
                            /// stsd atom class
                            /// </summary>
                            public class STSD
                            {
                                /// <summary> version/flags </summary>
                                public uint VersionFlags = 0;
                                /// <summary> sample descriptions </summary>
                                public List<byte[]> SampleDescriptions = new List<byte[]>();

                                /// <summary>
                                /// 出力サイズを取得する。
                                /// </summary>
                                public uint OutputLength
                                {
                                    get
                                    {
                                        uint discriptionLength = 0;

                                        for (int i = 0; i < SampleDescriptions.Count; i++)
                                        {
                                            discriptionLength += (uint)((byte[])SampleDescriptions[i]).LongLength;
                                        }

                                        return (16 + discriptionLength);
                                    }
                                }

                                /// <summary>
                                /// 出力する
                                /// </summary>
                                /// <returns>出力データ</returns>
                                public byte[] Output()
                                {
                                    byte[] output = new byte[OutputLength];

                                    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                    Array.Copy(Encoding.ASCII.GetBytes("stsd"), 0, output, 4, 4);
                                    Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                                    Array.Copy(GetReverseBytes((uint)SampleDescriptions.Count), 0, output, 12, 4);

                                    uint offset = 16;

                                    foreach (byte[] sampleDescription in SampleDescriptions)
                                    {
                                        Array.Copy(sampleDescription, 0, output, offset, sampleDescription.Length);
                                        offset += (uint)sampleDescription.Length;
                                    }

                                    return (byte[])output.Clone();
                                }
                            }

                            /// <summary>
                            /// stts atom class
                            /// </summary>
                            public class STTS
                            {
                                /// <summary>
                                /// time-to-sample table class
                                /// </summary>
                                public class TimeToSampleTable
                                {
                                    /// <summary> sample count </summary>
                                    public uint SampleCount = 0;
                                    /// <summary> sample duration </summary>
                                    public uint SampleDuration = 0;

                                    /// <summary>
                                    /// 出力する
                                    /// </summary>
                                    /// <returns>出力データ</returns>
                                    public byte[] Output()
                                    {
                                        byte[] output = new byte[8];

                                        Array.Copy(GetReverseBytes(SampleCount), 0, output, 0, 4);
                                        Array.Copy(GetReverseBytes(SampleDuration), 0, output, 4, 4);

                                        return (byte[])output.Clone();
                                    }
                                }

                                /// <summary> version/flags </summary>
                                public uint VersionFlags = 0;
                                /// <summary> time-to-sample tables </summary>
                                public List<TimeToSampleTable> TimeToSampleTables = new List<TimeToSampleTable>();

                                /// <summary>
                                /// TimeToSampleTableを追加する。
                                /// </summary>
                                /// <param name="timeToSampleTable">time-to-sample table</param>
                                public void AddTimeToSampleTable(TimeToSampleTable timeToSampleTable)
                                {
                                    if (TimeToSampleTables.Count > 0 &&
                                        ((TimeToSampleTable)TimeToSampleTables[TimeToSampleTables.Count - 1]).SampleDuration == timeToSampleTable.SampleDuration)
                                    {
                                        ((TimeToSampleTable)TimeToSampleTables[TimeToSampleTables.Count - 1]).SampleCount++;
                                    }
                                    else
                                    {
                                        TimeToSampleTables.Add(timeToSampleTable);
                                    }
                                }

                                /// <summary>
                                /// 出力サイズを取得する。
                                /// </summary>
                                public uint OutputLength { get { return (16 + ((uint)TimeToSampleTables.Count * 8)); } }

                                /// <summary>
                                /// 出力する
                                /// </summary>
                                /// <returns>出力データ</returns>
                                public byte[] Output()
                                {
                                    byte[] output = new byte[OutputLength];

                                    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                    Array.Copy(Encoding.ASCII.GetBytes("stts"), 0, output, 4, 4);
                                    Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                                    Array.Copy(GetReverseBytes((uint)TimeToSampleTables.Count), 0, output, 12, 4);

                                    uint offset = 16;

                                    foreach (TimeToSampleTable timeToSampleTable in TimeToSampleTables)
                                    {
                                        byte[] tempBuffer = timeToSampleTable.Output();

                                        Array.Copy(tempBuffer, 0, output, offset, tempBuffer.LongLength);
                                        offset += (uint)tempBuffer.Length;
                                    }

                                    return (byte[])output.Clone();
                                }
                            }

                            /// <summary>
                            /// stss atom class
                            /// </summary>
                            public class STSS
                            {
                                /// <summary> version/flags </summary>
                                public uint VersionFlags = 0;
                                /// <summary> sample numbers </summary>
                                public List<uint> SampleNumbers = new List<uint>();

                                /// <summary>
                                /// 出力サイズを取得する。
                                /// </summary>
                                public uint OutputLength { get { return (16 + ((uint)SampleNumbers.Count * 4)); } }

                                /// <summary>
                                /// 出力する
                                /// </summary>
                                /// <returns>出力データ</returns>
                                public byte[] Output()
                                {
                                    byte[] output = new byte[OutputLength];

                                    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                    Array.Copy(Encoding.ASCII.GetBytes("stss"), 0, output, 4, 4);
                                    Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                                    Array.Copy(GetReverseBytes((uint)SampleNumbers.Count), 0, output, 12, 4);

                                    uint offset = 16;

                                    foreach (uint sampleNumber in SampleNumbers)
                                    {
                                        Array.Copy(GetReverseBytes(sampleNumber), 0, output, offset, 4);
                                        offset += 4;
                                    }

                                    return (byte[])output.Clone();
                                }
                            }

                            /// <summary>
                            /// stsc atom class
                            /// </summary>
                            public class STSC
                            {
                                /// <summary>
                                /// sample-to-chunk table class
                                /// </summary>
                                public class SampleToChunkTable
                                {
                                    /// <summary> first chunk </summary>
                                    public uint FirstChunk = 0;
                                    /// <summary> samples per chunk </summary>
                                    public uint SamplesPerChunk = 0;
                                    /// <summary> sample discription ID </summary>
                                    public uint SampleDescriptionID = 0;

                                    /// <summary>
                                    /// 出力する
                                    /// </summary>
                                    /// <returns>出力データ</returns>
                                    public byte[] Output()
                                    {
                                        byte[] output = new byte[12];

                                        Array.Copy(GetReverseBytes(FirstChunk), 0, output, 0, 4);
                                        Array.Copy(GetReverseBytes(SamplesPerChunk), 0, output, 4, 4);
                                        Array.Copy(GetReverseBytes(SampleDescriptionID), 0, output, 8, 4);

                                        return (byte[])output.Clone();
                                    }
                                }

                                /// <summary> version/flags </summary>
                                public uint VersionFlags = 0;
                                /// <summary> sample-to-chunk tables </summary>
                                public List<SampleToChunkTable> SampleToChunkTables = new List<SampleToChunkTable>();

                                /// <summary>
                                /// SampleToChunkTableを追加する。
                                /// </summary>
                                /// <param name="sampleToChunkTable">sample-to-chunk table</param>
                                public void AddSampleToChunkTable(SampleToChunkTable sampleToChunkTable)
                                {
                                    if (!(SampleToChunkTables.Count > 0 &&
                                        ((SampleToChunkTable)SampleToChunkTables[SampleToChunkTables.Count - 1]).SampleDescriptionID == sampleToChunkTable.SampleDescriptionID))
                                    {
                                        SampleToChunkTables.Add(sampleToChunkTable);
                                    }
                                }

                                /// <summary>
                                /// 出力サイズを取得する。
                                /// </summary>
                                public uint OutputLength { get { return (uint)(16 + (SampleToChunkTables.Count * 12)); } }

                                /// <summary>
                                /// 出力する
                                /// </summary>
                                /// <returns>出力データ</returns>
                                public byte[] Output()
                                {
                                    byte[] output = new byte[OutputLength];

                                    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                    Array.Copy(Encoding.ASCII.GetBytes("stsc"), 0, output, 4, 4);
                                    Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                                    Array.Copy(GetReverseBytes((uint)SampleToChunkTables.Count), 0, output, 12, 4);

                                    uint offset = 16;

                                    foreach (SampleToChunkTable sampleToChunkTable in SampleToChunkTables)
                                    {
                                        byte[] tempBuffer = sampleToChunkTable.Output();

                                        Array.Copy(tempBuffer, 0, output, offset, tempBuffer.Length);
                                        offset += (uint)tempBuffer.LongLength;
                                    }

                                    return (byte[])output.Clone();
                                }
                            }

                            /// <summary>
                            /// stsz atom class
                            /// </summary>
                            public class STSZ
                            {
                                /// <summary> version/flags </summary>
                                public uint VersionFlags = 0;
                                /// <summary> sample sizes </summary>
                                public List<uint> SampleSizes = new List<uint>();

                                /// <summary>
                                /// 出力サイズを取得する。
                                /// </summary>
                                public uint OutputLength
                                {
                                    get
                                    {
                                        if (SampleSizes.Count > 0)
                                        {
                                            uint firstSampleSize = (uint)SampleSizes[0];
                                            int i = 0;

                                            for (i = 0; i < SampleSizes.Count; i++)
                                            {
                                                if (firstSampleSize != (uint)SampleSizes[i])
                                                {
                                                    break;
                                                }
                                            }

                                            if (i == SampleSizes.Count)
                                            {
                                                return 20;
                                            }
                                            else
                                            {
                                                return (20 + ((uint)SampleSizes.Count * 4));
                                            }
                                        }
                                        else
                                        {
                                            return 20;
                                        }
                                    }
                                }

                                /// <summary>
                                /// 出力する
                                /// </summary>
                                /// <returns>出力データ</returns>
                                public byte[] Output()
                                {
                                    byte[] output = new byte[OutputLength];

                                    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                    Array.Copy(Encoding.ASCII.GetBytes("stsz"), 0, output, 4, 4);
                                    Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);

                                    if (SampleSizes.Count > 0)
                                    {
                                        uint firstSampleSize = (uint)SampleSizes[0];
                                        int i = 0;

                                        for (i = 0; i < SampleSizes.Count; i++)
                                        {
                                            if (firstSampleSize != (uint)SampleSizes[i])
                                            {
                                                break;
                                            }
                                        }

                                        if (i == SampleSizes.Count)
                                        {
                                            Array.Copy(GetReverseBytes(firstSampleSize), 0, output, 12, 4);
                                            Array.Copy(GetReverseBytes((uint)SampleSizes.Count), 0, output, 16, 4);
                                        }
                                        else
                                        {
                                            Array.Copy(GetReverseBytes((uint)0), 0, output, 12, 4);
                                            Array.Copy(GetReverseBytes((uint)SampleSizes.Count), 0, output, 16, 4);

                                            uint offset = 20;
                                            foreach (uint sampleSize in SampleSizes)
                                            {
                                                Array.Copy(GetReverseBytes(sampleSize), 0, output, offset, 4);
                                                offset += 4;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Array.Copy(GetReverseBytes((uint)0), 0, output, 12, 4);
                                        Array.Copy(GetReverseBytes((uint)0), 0, output, 16, 4);
                                    }

                                    return (byte[])output.Clone();
                                }
                            }

                            /// <summary>
                            /// stco atom class
                            /// </summary>
                            public class STCO
                            {
                                /// <summary> version/flags </summary>
                                public uint VersionFlags = 0;
                                /// <summary> chunk offsets </summary>
                                public List<uint> ChunkOffsets = new List<uint>();

                                /// <summary>
                                /// 出力サイズを取得する。
                                /// </summary>
                                public uint OutputLength
                                {
                                    get
                                    {
                                        return (16 + ((uint)ChunkOffsets.Count * 4));
                                    }
                                }

                                /// <summary>
                                /// 出力する
                                /// </summary>
                                /// <returns>出力データ</returns>
                                public byte[] Output()
                                {
                                    byte[] output = new byte[OutputLength];

                                    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                    Array.Copy(Encoding.ASCII.GetBytes("stco"), 0, output, 4, 4);
                                    Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                                    Array.Copy(GetReverseBytes((uint)ChunkOffsets.Count), 0, output, 12, 4);

                                    uint offset = 16;
                                    foreach (uint chunkOffset in ChunkOffsets)
                                    {
                                        Array.Copy(GetReverseBytes(chunkOffset), 0, output, offset, 4);
                                        offset += 4;
                                    }

                                    return (byte[])output.Clone();
                                }
                            }

                            /// <summary> stsd atom </summary>
                            public STSD Stsd = new STSD();
                            /// <summary> stts atom </summary>
                            public STTS Stts = new STTS();
                            /// <summary> stss atom。ビデオトラックにMPEG4、H.264形式のデータを設定する場合に必要。 </summary>
                            public STSS Stss = null;
                            /// <summary> stsc atom </summary>
                            public STSC Stsc = new STSC();
                            /// <summary> stsz atom </summary>
                            public STSZ Stsz = new STSZ();
                            /// <summary> stco atom </summary>
                            public STCO Stco = new STCO();

                            /// <summary> 登録したサンプル総数 </summary>
                            public int SampleCount = 0;

                            /// <summary>
                            /// 出力サイズを取得する。
                            /// </summary>
                            public uint OutputLength
                            {
                                get
                                {
                                    return (8 + Stsd.OutputLength + Stts.OutputLength + (Stss != null ? Stss.OutputLength : 0) + Stsc.OutputLength + Stsz.OutputLength + Stco.OutputLength);
                                }
                            }

                            /// <summary>
                            /// 出力する
                            /// </summary>
                            /// <returns>出力データ</returns>
                            public byte[] Output()
                            {
                                byte[] stsdOutput = Stsd.Output();
                                byte[] sttsOutput = Stts.Output();
                                byte[] stscOutput = Stsc.Output();
                                byte[] stszOutput = Stsz.Output();
                                byte[] stcoOutput = Stco.Output();
                                byte[] output = new byte[OutputLength];
                                uint offset = 8;

                                Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                                Array.Copy(Encoding.ASCII.GetBytes("stbl"), 0, output, 4, 4);
                                Array.Copy(stsdOutput, 0, output, offset, stsdOutput.LongLength);
                                offset += (uint)stsdOutput.LongLength;
                                Array.Copy(sttsOutput, 0, output, offset, sttsOutput.LongLength);
                                offset += (uint)sttsOutput.LongLength;
                                if (Stss != null)
                                {
                                    byte[] stssOutput = Stss.Output();
                                    Array.Copy(stssOutput, 0, output, offset, stssOutput.LongLength);
                                    offset += (uint)stssOutput.LongLength;
                                }
                                Array.Copy(stscOutput, 0, output, offset, stscOutput.LongLength);
                                offset += (uint)stscOutput.LongLength;
                                Array.Copy(stszOutput, 0, output, offset, stszOutput.LongLength);
                                offset += (uint)stszOutput.LongLength;
                                Array.Copy(stcoOutput, 0, output, offset, stcoOutput.LongLength);
                                offset += (uint)stcoOutput.LongLength;

                                return (byte[])output.Clone();
                            }
                        }

                        /// <summary> vmhd atom。ビデオトラックとして設定する場合に必要。 </summary>
                        public VMHD Vmhd = null;
                        /// <summary> smhd atom。オーディオトラックとして設定する場合に必要。 </summary>
                        public SMHD Smhd = null;
                        /// <summary> gmhd atom。テキストトラックとして設定する場合に必要。 </summary>
                        public GMHD Gmhd = null;
                        /// <summary> hdlr atom </summary>
                        public HDLR Hdlr = new HDLR();
                        /// <summary> dinf atom </summary>
                        public DINF Dinf = new DINF();
                        /// <summary> stbl atom </summary>
                        public STBL Stbl = new STBL();

                        /// <summary>
                        /// 出力サイズを取得する。
                        /// </summary>
                        public uint OutputLength
                        {
                            get
                            {
                                return (8 + (Vmhd != null ? Vmhd.OutputLength : 0) + (Smhd != null ? Smhd.OutputLength : 0) + (Gmhd != null ? Gmhd.OutputLength : 0) + Hdlr.OutputLength + Dinf.OutputLength + Stbl.OutputLength);
                            }
                        }

                        /// <summary>
                        /// 出力する
                        /// </summary>
                        /// <returns>出力データ</returns>
                        public byte[] Output()
                        {
                            byte[] hdlrOutput = Hdlr.Output();
                            byte[] dinfOutput = Dinf.Output();
                            byte[] stblOutput = Stbl.Output();
                            byte[] output = new byte[OutputLength];
                            uint offset = 8;

                            Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                            Array.Copy(Encoding.ASCII.GetBytes("minf"), 0, output, 4, 4);
                            if (Vmhd != null)
                            {
                                byte[] vmhdOutput = Vmhd.Output();

                                Array.Copy(vmhdOutput, 0, output, offset, vmhdOutput.LongLength);
                                offset += (uint)vmhdOutput.LongLength;
                            }
                            if (Smhd != null)
                            {
                                byte[] smhdOutput = Smhd.Output();

                                Array.Copy(smhdOutput, 0, output, offset, smhdOutput.LongLength);
                                offset += (uint)smhdOutput.LongLength;
                            }
                            if (Gmhd != null)
                            {
                                byte[] gmhdOutput = Gmhd.Output();

                                Array.Copy(gmhdOutput, 0, output, offset, gmhdOutput.LongLength);
                                offset += (uint)gmhdOutput.LongLength;
                            }
                            Array.Copy(hdlrOutput, 0, output, offset, hdlrOutput.LongLength);
                            offset += (uint)hdlrOutput.LongLength;
                            Array.Copy(dinfOutput, 0, output, offset, dinfOutput.LongLength);
                            offset += (uint)dinfOutput.LongLength;
                            Array.Copy(stblOutput, 0, output, offset, stblOutput.LongLength);
                            offset += (uint)stblOutput.LongLength;

                            return (byte[])output.Clone();
                        }
                    }

                    /// <summary>
                    /// hdlr atom class
                    /// </summary>
                    public class HDLR
                    {
                        /// <summary>
                        /// component type name list
                        /// </summary>
                        public enum ComponentTypeNameList
                        {
                            /// <summary> media handlers </summary>
                            mhlr = 0,
                            /// <summary> data handlers </summary>
                            dhlr
                        }

                        /// <summary>
                        /// component subtype name list
                        /// </summary>
                        public enum ComponentSubtypeNameList
                        {
                            /// <summary> video data </summary>
                            vide = 0,
                            /// <summary> sound data </summary>
                            soun,
                            /// <summary> text </summary>
                            text,
                            /// <summary> alias </summary>
                            alis
                        }

                        /// <summary> version/flags </summary>
                        public uint VersionFlags = 0;
                        /// <summary> component type </summary>
                        public string ComponentType = "    ";
                        /// <summary> component subtype </summary>
                        public string ComponentSubtype = "    ";
                        /// <summary> component manufacturer </summary>
                        public uint ComponentManufacturer = 0;
                        /// <summary> component flags </summary>
                        public uint ComponentFlags = 0;
                        /// <summary> component flags mask </summary>
                        public uint ComponentFlagsMask = 0;
                        /// <summary> component name </summary>
                        public string ComponentName = string.Empty;

                        /// <summary>
                        /// 出力サイズを取得する。
                        /// </summary>
                        public uint OutputLength
                        {
                            get
                            {
                                byte[] componentNameBytes = null;

                                if (string.IsNullOrEmpty(ComponentName) == false)
                                {
                                    componentNameBytes = Encoding.ASCII.GetBytes(ComponentName);
                                }

                                return (32 + (uint)(componentNameBytes == null ? 1 : componentNameBytes.Length));
                            }
                        }

                        /// <summary>
                        /// 出力する
                        /// </summary>
                        /// <returns>出力データ</returns>
                        public byte[] Output()
                        {
                            byte[] componentNameBytes = null;

                            if (string.IsNullOrEmpty(ComponentName) == false)
                            {
                                componentNameBytes = Encoding.ASCII.GetBytes(ComponentName);
                            }
                            else
                            {
                                componentNameBytes = new byte[1] { 0x0 };
                            }

                            byte[] output = new byte[OutputLength];

                            Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                            Array.Copy(Encoding.ASCII.GetBytes("hdlr"), 0, output, 4, 4);
                            Array.Copy(GetReverseBytes(VersionFlags), 0, output, 8, 4);
                            Array.Copy(Encoding.ASCII.GetBytes(ComponentType), 0, output, 12, 4);
                            Array.Copy(Encoding.ASCII.GetBytes(ComponentSubtype), 0, output, 16, 4);
                            Array.Copy(GetReverseBytes(ComponentManufacturer), 0, output, 20, 4);
                            Array.Copy(GetReverseBytes(ComponentFlags), 0, output, 24, 4);
                            Array.Copy(GetReverseBytes(ComponentFlagsMask), 0, output, 28, 4);

                            if (componentNameBytes != null)
                            {
                                Array.Copy(componentNameBytes, 0, output, 32, output.Length - 32);
                            }

                            return (byte[])output.Clone();
                        }
                    }

                    /// <summary> mdhd atom </summary>
                    public MDHD Mdhd = new MDHD();
                    /// <summary> hdlr atom </summary>
                    public HDLR Hdlr = new HDLR();
                    /// <summary> minf atom </summary>
                    public MINF Minf = new MINF();

                    /// <summary>
                    /// 出力サイズを取得する。
                    /// </summary>
                    public uint OutputLength
                    {
                        get
                        {
                            return (8 + Mdhd.OutputLength + Hdlr.OutputLength + Minf.OutputLength);
                        }
                    }

                    /// <summary>
                    /// 出力する
                    /// </summary>
                    /// <returns>出力データ</returns>
                    public byte[] Output()
                    {
                        byte[] mdhdOutput = Mdhd.Output();
                        byte[] hdlrOutput = Hdlr.Output();
                        byte[] minfOutput = Minf.Output();
                        byte[] output = new byte[OutputLength];
                        uint offset = 8;

                        Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                        Array.Copy(Encoding.ASCII.GetBytes("mdia"), 0, output, 4, 4);
                        Array.Copy(mdhdOutput, 0, output, offset, mdhdOutput.LongLength);
                        offset += (uint)mdhdOutput.LongLength;
                        Array.Copy(hdlrOutput, 0, output, offset, hdlrOutput.LongLength);
                        offset += (uint)hdlrOutput.LongLength;
                        Array.Copy(minfOutput, 0, output, offset, minfOutput.LongLength);
                        offset += (uint)minfOutput.LongLength;

                        return (byte[])output.Clone();
                    }
                }

                /// <summary> tkhd atom </summary>
                public TKHD Tkhd = new TKHD();
                /// <summary> mdia atom </summary>
                public MDIA Mdia = new MDIA();

                /// <summary> トラックタイプ </summary>
                public TrackType TrackType = MOOV.TrackType.None;

                /// <summary>
                /// 出力サイズを取得する。
                /// </summary>
                public uint OutputLength
                {
                    get
                    {
                        return (8 + Tkhd.OutputLength + Mdia.OutputLength);
                    }
                }

                /// <summary>
                /// 出力する
                /// </summary>
                /// <returns>出力データ</returns>
                public byte[] Output()
                {
                    byte[] tkhdOutput = Tkhd.Output();
                    byte[] mdiaOutput = Mdia.Output();
                    byte[] output = new byte[OutputLength];
                    uint offset = 8;

                    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                    Array.Copy(Encoding.ASCII.GetBytes("trak"), 0, output, 4, 4);
                    Array.Copy(tkhdOutput, 0, output, offset, tkhdOutput.LongLength);
                    offset += (uint)tkhdOutput.LongLength;
                    Array.Copy(mdiaOutput, 0, output, offset, mdiaOutput.LongLength);
                    offset += (uint)mdiaOutput.LongLength;

                    return (byte[])output.Clone();
                }
            }

            /// <summary> mvhd atom </summary>
            public MVHD Mvhd = new MVHD();
            /// <summary> trak atom。設定したトラックの数だけ追加する。 </summary>
            public TRAK[] Traks = null;

            /// <summary>
            /// 出力サイズを取得する。
            /// </summary>
            public uint OutputLength
            {
                get
                {
                    uint tkhdOutputsLength = 0;

                    if (Traks != null)
                    {
                        for (int i = 0; i < Traks.Length; i++)
                        {
                            if (Traks[i] != null)
                            {
                                tkhdOutputsLength += (uint)Traks[i].Output().LongLength;
                            }
                        }
                    }

                    return (8 + Mvhd.OutputLength + tkhdOutputsLength);
                }
            }

            /// <summary>
            /// 出力する
            /// </summary>
            /// <returns>出力データ</returns>
            public byte[] Output()
            {
                byte[] mvhdOutput = Mvhd.Output();
                byte[] tkhdOutputs = null;

                if (Traks != null)
                {
                    for (int i = 0; i < Traks.Length; i++)
                    {
                        if (Traks[i] != null)
                        {
                            byte[] tempBuffer = Traks[i].Output();
                            uint copyOffset = 0;

                            if (tkhdOutputs == null)
                            {
                                tkhdOutputs = new byte[tempBuffer.LongLength];
                            }
                            else
                            {
                                copyOffset = (uint)tkhdOutputs.LongLength;
                                Array.Resize(ref tkhdOutputs, (int)(tkhdOutputs.LongLength + tempBuffer.LongLength));
                            }

                            Array.Copy(tempBuffer, 0, tkhdOutputs, copyOffset, tempBuffer.LongLength);
                        }
                    }
                }

                byte[] output = new byte[OutputLength];
                uint offset = 8;

                Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
                Array.Copy(Encoding.ASCII.GetBytes("moov"), 0, output, 4, 4);
                Array.Copy(mvhdOutput, 0, output, offset, mvhdOutput.LongLength);
                offset += (uint)mvhdOutput.LongLength;

                if (tkhdOutputs != null)
                {
                    Array.Copy(tkhdOutputs, 0, output, offset, tkhdOutputs.LongLength);
                    offset += (uint)tkhdOutputs.LongLength;
                }

                return (byte[])output.Clone();
            }
        }
    }
}
