using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace MOVUtility
{
    /// <summary>
    /// movファイル変換クラス
    /// </summary>
    public partial class MovConvertor
    {
        /// <summary>
        /// 映像種別
        /// </summary>
        public enum ImageType
        {
            /// <summary> JPEG </summary>
            JPEG = 0,
            /// <summary> H.264 </summary>
            H264
        }

        /// <summary>
        /// エクスポートするmovファイルのファイルストリーム
        /// </summary>
        private FileStream _fileStream = null;

        /// <summary>
        /// エクスポートするmovファイルが開いているか?
        /// </summary>
        public bool IsFileOpen { get { return (_fileStream != null); } }

        /// <summary>
        /// 映像種別
        /// </summary>
        private ImageType _imageType = ImageType.JPEG;

        /// <summary>
        /// タイムスケール
        /// </summary>
        private uint _timeScale = 30000;

        /// <summary>
        /// moov atom
        /// </summary>
        private MOOV _moov = new MOOV();

        /// <summary>
        /// mdat atom size
        /// </summary>
        private uint _mdatSize = 0;

        /// <summary>
        /// 映像開始時刻
        /// </summary>
        private DateTime _startTime = DateTime.MinValue;

        /// <summary>
        /// ビデオフレームの時刻。開始時刻からの経過時間をms単位で表現。
        /// </summary>
        public double _lastVideoMilliseconds = 0;

        /// <summary>
        /// 保存パス
        /// </summary>
        private string _movFilePath = string.Empty;

        /// <summary>
        /// 対象ドライブにデータを保存するための空きスペースがあるか？
        /// </summary>
        private bool IsFreeSpace
        {
            get
            {
                if (string.IsNullOrEmpty(_movFilePath) == false)
                {
                    // 保存先が1GBを切ったところで保存処理を中止する。
                    foreach (DriveInfo drive in DriveInfo.GetDrives())
                    {
                        if (drive.Name == Path.GetPathRoot(_movFilePath) && drive.TotalFreeSpace < 1024 * 1024 * 1024)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 生成処理
        /// </summary>
        /// <returns>生成に成功したか？</returns>
        public bool CreateVideoFile(string movSavePath, ImageType videoType)
        {
            if (string.IsNullOrEmpty(movSavePath) == true || IsFreeSpace == false)
            {
                return false;
            }

            _imageType = videoType;

            try
            {
                _movFilePath = movSavePath;

                Directory.CreateDirectory(Path.GetDirectoryName(movSavePath));
                _fileStream = File.Create(_movFilePath);

                // ftyp atomをファイルに書き込む
                WriteFile(new FTYP().Output());

                // mdat atomのヘッダ部をファイルに書き込む
                _fileStream.Write(GetReverseBytes((uint)0), 0, 4);
                _fileStream.Write(Encoding.ASCII.GetBytes("mdat"), 0, 4);

                // トラックを作成
                return CreateTrack();
            }
            catch
            {
                _fileStream = null;
            }

            return false;
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Close()
        {
            if (_fileStream != null)
            {
                _fileStream.Position = new FTYP().OutputLength;
                _fileStream.Write(GetReverseBytes(_mdatSize + 8), 0, 4);

                CloseTrack();

                byte[] moovOutput = _moov.Output();

                _fileStream.Position = _fileStream.Length;
                _fileStream.Write(moovOutput, 0, moovOutput.Length);

                _fileStream.Close();
                _fileStream = null;
            }
        }

        /// <summary>
        /// イメージ追加処理
        /// </summary>
        /// <param name="imageBuffer">イメージデータ</param>
        /// <param name="codecInformation">コーデック情報</param>
        /// <param name="timestamp">画像の記録日時</param>
        /// <param name="gmtOffset">GMTオフセット(分)</param>
        /// <param name="spanMilliseconds">前フレームからの時間(単位：ミリ秒)</param>
        /// <param name="isKeyFrame">追加するフレームはキーフレームか？(H.264で使用)</param>
        /// <param name="cameraName">記録したカメラのホスト名</param>
        /// <returns>追加に成功したか？</returns>
        public bool AddImage(byte[] imageBuffer, byte[] codecInformation, DateTime timestamp, double gmtOffset, double spanMilliseconds, bool isKeyFrame, string cameraName)
        {
            if (_fileStream == null)
            {
                return false;
            }

            int videoTrackID = GetTrackID(MOOV.TrackType.Video);
            //int textTrackID = GetTrackID(MOOV.TrackType.Text);

            if (videoTrackID < 0)
            {
                return false;
            }

            //byte[] textBuffer = Encoding.UTF8.GetBytes(string.Format("{0}\n{1:d}\n{2}", cameraName, timestamp.AddMinutes(gmtOffset), timestamp.AddMinutes(gmtOffset).ToString("HH:mm:ss.fff")));

            //// 容量チェック。mdatの領域が4GB弱、movファイルが4GBを超過しないようにする。
            //if (IsFreeSpace == false ||
            //    _mdatSize + (ulong)imageBuffer.LongLength + (ulong)textBuffer.LongLength > uint.MaxValue - (1024 * 1024 * 200) ||    // サンプル情報は最大200MB程度を想定
            //    (ulong)_fileStream.Length + _moov.OutputLength + (ulong)imageBuffer.LongLength + (ulong)textBuffer.LongLength + 1024 > uint.MaxValue)   // 1024(1KB)はサンプル情報増加分
            //{
            //    return false;
            //}

            // コーデック情報設定
            if (codecInformation != null)
            {
                SetCodecInformation(codecInformation, MOOV.TrackType.Video);
            }
            else
            {
                if (_imageType == ImageType.JPEG)
                {
                    JpegUtility.ReadHeader(imageBuffer, out Size size);
                    SetCodecInformation(CreateJPEGCodecInformation(size), MOOV.TrackType.Video);
                }
            }

            if (_startTime == DateTime.MinValue)
            {
                _startTime = timestamp;
            }

            // CreationTime・ModificationTime設定
            uint currentTime = (uint)((timestamp.ToFileTimeUtc() - new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToFileTimeUtc()) / 10000000);
            if (_moov.Mvhd.CreationTime == 0)
            {
                _moov.Mvhd.CreationTime = _moov.Mvhd.ModificationTime = currentTime;
            }

            if (_moov.Traks[videoTrackID].Tkhd.CreationTime == 0)
            {
                _moov.Traks[videoTrackID].Tkhd.CreationTime = _moov.Traks[videoTrackID].Tkhd.ModificationTime = currentTime;
            }

            if (_moov.Traks[videoTrackID].Mdia.Mdhd.CreationTime == 0)
            {
                _moov.Traks[videoTrackID].Mdia.Mdhd.CreationTime = _moov.Traks[videoTrackID].Mdia.Mdhd.ModificationTime = currentTime;
            }

            //if (_moov.Traks[textTrackID].Tkhd.CreationTime == 0)
            //{
            //    _moov.Traks[textTrackID].Tkhd.CreationTime = _moov.Traks[textTrackID].Tkhd.ModificationTime = currentTime;
            //}

            //if (_moov.Traks[textTrackID].Mdia.Mdhd.CreationTime == 0)
            //{
            //    _moov.Traks[textTrackID].Mdia.Mdhd.CreationTime = _moov.Traks[textTrackID].Mdia.Mdhd.ModificationTime = currentTime;
            //}

            uint sampleCount = (uint)(++_moov.Traks[videoTrackID].Mdia.Minf.Stbl.SampleCount);

            #region Video
            // stts atom
            _moov.Traks[videoTrackID].Mdia.Minf.Stbl.Stts.AddTimeToSampleTable(
                new MOOV.TRAK.MDIA.MINF.STBL.STTS.TimeToSampleTable()
                {
                    SampleCount = 1,
                    SampleDuration = (uint)((_timeScale * spanMilliseconds) / 1000.0)
                });

            // stss atom
            if (_imageType == ImageType.H264 && isKeyFrame == true)
            {
                if (_moov.Traks[videoTrackID].Mdia.Minf.Stbl.Stss == null)
                {
                    _moov.Traks[videoTrackID].Mdia.Minf.Stbl.Stss = new MOOV.TRAK.MDIA.MINF.STBL.STSS();
                }

                _moov.Traks[videoTrackID].Mdia.Minf.Stbl.Stss.SampleNumbers.Add(sampleCount);
            }

            // stsc atom
            _moov.Traks[videoTrackID].Mdia.Minf.Stbl.Stsc.AddSampleToChunkTable(
                new MOOV.TRAK.MDIA.MINF.STBL.STSC.SampleToChunkTable()
                {
                    FirstChunk = sampleCount,
                    SamplesPerChunk = 1,
                    SampleDescriptionID = (uint)_moov.Traks[videoTrackID].Mdia.Minf.Stbl.Stsd.SampleDescriptions.Count
                });

            // stsz atom
            _moov.Traks[videoTrackID].Mdia.Minf.Stbl.Stsz.SampleSizes.Add((uint)imageBuffer.LongLength);

            // stco atom
            _moov.Traks[videoTrackID].Mdia.Minf.Stbl.Stco.ChunkOffsets.Add((uint)(_fileStream.Position = _fileStream.Length));

            WriteFile(imageBuffer);
            _mdatSize += (uint)imageBuffer.LongLength;
            #endregion Video


            //#region Text
            //// stts atom
            //_moov.Traks[textTrackID].Mdia.Minf.Stbl.Stts.AddTimeToSampleTable(
            //new MOOV.TRAK.MDIA.MINF.STBL.STTS.TimeToSampleTable()
            //{
            //    SampleCount = 1,
            //    SampleDuration = (uint)((_timeScale * spanMilliseconds) / 1000.0)
            //});

            //// stsc atom
            //_moov.Traks[textTrackID].Mdia.Minf.Stbl.Stsc.AddSampleToChunkTable(
            //    new MOOV.TRAK.MDIA.MINF.STBL.STSC.SampleToChunkTable()
            //    {
            //        FirstChunk = sampleCount,
            //        SamplesPerChunk = 1,
            //        SampleDescriptionID = (uint)_moov.Traks[textTrackID].Mdia.Minf.Stbl.Stsd.SampleDescriptions.Count
            //    });

            //// stsz atom
            //_moov.Traks[textTrackID].Mdia.Minf.Stbl.Stsz.SampleSizes.Add(2 + (uint)textBuffer.LongLength);

            //// stco atom
            //_moov.Traks[textTrackID].Mdia.Minf.Stbl.Stco.ChunkOffsets.Add((uint)(_fileStream.Position = _fileStream.Length));

            //WriteFile(GetReverseBytes((ushort)textBuffer.Length));
            //WriteFile(textBuffer);
            //_mdatSize += (2 + (uint)textBuffer.LongLength);
            //#endregion Text

            _lastVideoMilliseconds += spanMilliseconds;

            return true;
        }

        /// <summary>
        /// コーデック情報を設定する。
        /// </summary>
        /// <param name="codecInformation">コーデック情報(stsd atom内に設定するデータそのものを指定する)</param>
        /// <param name="trackType">トラックタイプ</param>
        /// <returns>設定に成功したか？</returns>
        public bool SetCodecInformation(byte[] codecInformation, MOOV.TrackType trackType)
        {
            if (codecInformation == null)
            {
                return false;
            }

            int trackID = GetTrackID(trackType);

            if (trackID < 0)
            {
                return false;
            }

            if (_moov.Traks[trackID].Mdia.Minf.Stbl.Stsd.SampleDescriptions.Count > 0)
            {
                if (_moov.Traks[trackID].Mdia.Minf.Stbl.Stsd.SampleDescriptions[_moov.Traks[trackID].Mdia.Minf.Stbl.Stsd.SampleDescriptions.Count - 1].SequenceEqual(codecInformation) == true)
                {
                    return true;
                }
            }

            bool isSuccess = false;

            if (trackType == MOOV.TrackType.Video)
            {
                // ビデオトラックの場合はコーデック情報からイメージサイズを取得し、トラックに反映する。
                if (codecInformation.LongLength >= 36)
                {
                    byte[] typeBytes = new byte[4];
                    string type = string.Empty;

                    Array.Copy(codecInformation, 4, typeBytes, 0, typeBytes.Length);
                    type = Encoding.ASCII.GetString(typeBytes);

                    if (type == "jpeg" || type == "avc1")
                    {
                        byte[] sizeBytes = new byte[2];

                        Array.Copy(codecInformation, 32, sizeBytes, 0, sizeBytes.Length);
                        Array.Reverse(sizeBytes);

                        ushort width = BitConverter.ToUInt16(sizeBytes, 0);

                        Array.Copy(codecInformation, 34, sizeBytes, 0, sizeBytes.Length);
                        Array.Reverse(sizeBytes);

                        ushort height = BitConverter.ToUInt16(sizeBytes, 0);

                        // イメージサイズ設定
                        _moov.Traks[trackID].Tkhd.Width = width;
                        _moov.Traks[trackID].Tkhd.Height = height;

                        //// テキストトラックの配置も設定
                        //int textTrackID = GetTrackID(MOOV.TrackType.Text);
                        //if (textTrackID >= 0)
                        //{
                        //    _moov.Traks[textTrackID].Tkhd.Width = width;
                        //    Array.Copy(GetReverseBytes(height), 0, _moov.Traks[textTrackID].Tkhd.MatrixStructure, 28, 2);
                        //}

                        isSuccess = true;
                    }
                }
            }
            else
            {
                isSuccess = true;
            }

            if (isSuccess == true)
            {
                _moov.Traks[trackID].Mdia.Minf.Stbl.Stsd.SampleDescriptions.Add(codecInformation);
            }

            return isSuccess;
        }

        /// <summary>
        /// 指定値のエンディアンを切り替えた上でbyte配列型に変更した値を取得する。
        /// </summary>
        /// <param name="value">指定値</param>
        /// <returns>byte配列型に変更した値</returns>
        private static byte[] GetReverseBytes(uint value)
        {
            byte[] reverseBytes = new byte[4];

            reverseBytes = BitConverter.GetBytes(value);
            Array.Reverse(reverseBytes);

            return reverseBytes;
        }

        /// <summary>
        /// 指定値のエンディアンを切り替えた上でbyte配列型に変更した値を取得する。
        /// </summary>
        /// <param name="value">指定値</param>
        /// <returns>byte配列型に変更した値</returns>
        private static byte[] GetReverseBytes(ushort value)
        {
            byte[] reverseBytes = new byte[2];

            reverseBytes = BitConverter.GetBytes(value);
            Array.Reverse(reverseBytes);

            return reverseBytes;
        }

        /// <summary>
        /// トラックを作成する。
        /// </summary>
        /// <returns>作成に成功したか？</returns>
        private bool CreateTrack()
        {
            return (CreateTrack(MOOV.TrackType.Video) == true);
        }

        /// <summary>
        /// トラックを作成する。
        /// </summary>
        /// <param name="trackType">トラックタイプ</param>
        /// <returns>作成に成功したか？</returns>
        private bool CreateTrack(MOOV.TrackType trackType)
        {
            if (trackType == MOOV.TrackType.None || trackType == MOOV.TrackType.MaxSize)
            {
                return false;
            }

            uint trackID = 1;

            if (_moov.Traks == null)
            {
                _moov.Traks = new MOOV.TRAK[1];
            }
            else
            {
                Array.Resize(ref _moov.Traks, _moov.Traks.Length + 1);
                trackID = (uint)_moov.Traks.Length;
            }

            _moov.Mvhd.TimeScale = _timeScale;
            _moov.Mvhd.NextTrackID = trackID + 1;

            MOOV.TRAK track = new MOOV.TRAK()
            {
                TrackType = trackType,
            };

            track.Tkhd.TrackId = trackID;

            //if (trackType == MOOV.TrackType.Text)
            //{
            //    track.Tkhd.Height = 64;
            //}

            track.Mdia.Mdhd.TimeScale = _timeScale;

            track.Mdia.Hdlr.ComponentType = MOOV.TRAK.MDIA.HDLR.ComponentTypeNameList.mhlr.ToString();
            track.Mdia.Hdlr.ComponentSubtype = ((MOOV.TRAK.MDIA.HDLR.ComponentSubtypeNameList)trackType).ToString();

            switch (trackType)
            {
                case MOOV.TrackType.Video:
                    track.Mdia.Minf.Vmhd = new MOOV.TRAK.MDIA.MINF.VMHD();
                    break;
                case MOOV.TrackType.Audio:
                    track.Mdia.Minf.Smhd = new MOOV.TRAK.MDIA.MINF.SMHD();
                    break;
                //case MOOV.TrackType.Text:
                //    track.Mdia.Minf.Gmhd = new MOOV.TRAK.MDIA.MINF.GMHD();
                //    break;
                default:
                    break;
            }

            track.Mdia.Minf.Hdlr.ComponentType = MOOV.TRAK.MDIA.HDLR.ComponentTypeNameList.dhlr.ToString();
            track.Mdia.Minf.Hdlr.ComponentSubtype = MOOV.TRAK.MDIA.HDLR.ComponentSubtypeNameList.alis.ToString();

            track.Mdia.Minf.Dinf.Dref.DataReferenceInformations.Add(
                new MOOV.TRAK.MDIA.MINF.DINF.DREF.DataReferenceInformation()
                {
                    DataReferenceInformationType = MOOV.TRAK.MDIA.MINF.DINF.DREF.DataReferenceInformation.DataReferenceInformationTypeList.alis.ToString()
                });

            _moov.Traks[_moov.Traks.Length - 1] = track;

            //// コーデック情報設定
            //SetCodecInformation(CreateTX3GCodecInformation(), MOOV.TrackType.Text);

            return true;
        }

        /// <summary>
        /// トラックを閉じる
        /// </summary>
        private void CloseTrack()
        {
            int videoTrackID = GetTrackID(MOOV.TrackType.Video);
            //int textTrackID = GetTrackID(MOOV.TrackType.Text);

            if (videoTrackID < 0)
            {
                return;
            }

            _moov.Mvhd.Duration
                = _moov.Traks[videoTrackID].Tkhd.Duration
                = _moov.Traks[videoTrackID].Mdia.Mdhd.Duration
                //= _moov.Traks[textTrackID].Tkhd.Duration
                //= _moov.Traks[textTrackID].Mdia.Mdhd.Duration
                = (uint)((_timeScale / 1000.0) * _lastVideoMilliseconds);
        }

        /// <summary>
        /// 指定したトラックタイプのトラックIDを取得する。
        /// </summary>
        /// <param name="trackType">トラックタイプ</param>
        /// <returns>該当するトラックが見つかった場合にそのトラックIDを返す。見つからなかった場合は-1</returns>
        private int GetTrackID(MOOV.TrackType trackType)
        {
            if (_moov.Traks != null)
            {
                for (int i = 0; i < _moov.Traks.Length; i++)
                {
                    if (_moov.Traks[i] != null && _moov.Traks[i].TrackType == trackType)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// コーデック情報を作成する(JPEG)
        /// </summary>
        /// <param name="imageSize">画像サイズ</param>
        /// <returns>作成されたコーデック情報。失敗した場合はnull</returns>
        private byte[] CreateJPEGCodecInformation(Size imageSize)
        {
            if (imageSize == Size.Empty)
            {
                return null;
            }

            byte[] output = new byte[86];

            Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes("jpeg"), 0, output, 4, 4);
            Array.Copy(new byte[6] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 }, 0, output, 8, 6);
            Array.Copy(GetReverseBytes(1), 0, output, 14, 2);
            Array.Copy(GetReverseBytes(1), 0, output, 16, 2);
            Array.Copy(GetReverseBytes(0), 0, output, 18, 2);
            Array.Copy(Encoding.ASCII.GetBytes("appl"), 0, output, 20, 4);
            Array.Copy(GetReverseBytes((uint)0), 0, output, 24, 4);
            Array.Copy(GetReverseBytes((uint)512), 0, output, 28, 4);
            Array.Copy(GetReverseBytes((ushort)imageSize.Width), 0, output, 32, 2);
            Array.Copy(GetReverseBytes((ushort)imageSize.Height), 0, output, 34, 2);
            Array.Copy(GetReverseBytes(96), 0, output, 36, 2); // Horizontal resolution。とりあえず96dpiで。
            Array.Copy(GetReverseBytes(0), 0, output, 38, 2);
            Array.Copy(GetReverseBytes(96), 0, output, 40, 2); // Vertical resolution。とりあえず96dpiで。
            Array.Copy(GetReverseBytes(0), 0, output, 42, 2);
            Array.Copy(GetReverseBytes((uint)0), 0, output, 44, 4);
            Array.Copy(GetReverseBytes(1), 0, output, 48, 2);
            Array.Copy(Encoding.ASCII.GetBytes(" jpeg"), 0, output, 50, 5);
            Array.Copy(
                new byte[31]
                {
                    0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x18, 0xFF,
                    0xFF
                },
                0, output, 55, 31);

            return output;
        }

        ///// <summary>
        ///// コーデック情報を作成する(TEXT)
        ///// </summary>
        ///// <returns>作成されたコーデック情報。失敗した場合はnull</returns>
        //private byte[] CreateTX3GCodecInformation()
        //{
        //    byte[] output = new byte[71];

        //    Array.Copy(GetReverseBytes((uint)output.LongLength), 0, output, 0, 4);
        //    Array.Copy(Encoding.ASCII.GetBytes("tx3g"), 0, output, 4, 4);
        //    Array.Copy(new byte[6] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 }, 0, output, 8, 6);
        //    Array.Copy(GetReverseBytes(1), 0, output, 14, 2);
        //    Array.Copy(GetReverseBytes((uint)0x04), 0, output, 16, 4);         // flags
        //    output[20] = 0x1;                                                               // centre justify
        //    output[21] = 0;                                                                 // skip
        //    Array.Copy(GetReverseBytes(0xFFFFFFFF), 0, output, 22, 4);   // bkground colour
        //    Array.Copy(GetReverseBytes((uint)0), 0, output, 26, 4);
        //    Array.Copy(GetReverseBytes((uint)0), 0, output, 30, 4);
        //    Array.Copy(GetReverseBytes((uint)0x26), 0, output, 34, 4);
        //    Array.Copy(GetReverseBytes(1), 0, output, 38, 2);
        //    Array.Copy(GetReverseBytes((uint)0), 0, output, 40, 4);            // reserved
        //    Array.Copy(GetReverseBytes(0xff), 0, output, 44, 2);

        //    byte[] font = Encoding.ASCII.GetBytes("MS UI Gothic");

        //    Array.Copy(GetReverseBytes(13 + (uint)font.LongLength), 0, output, 46, 4);
        //    Array.Copy(Encoding.ASCII.GetBytes("ftab"), 0, output, 50, 4);
        //    Array.Copy(GetReverseBytes(1), 0, output, 54, 2);
        //    Array.Copy(GetReverseBytes(1), 0, output, 56, 2);
        //    output[58] = (byte)font.LongLength;
        //    Array.Copy(font, 0, output, 59, font.LongLength);

        //    return output;
        //}

        /// <summary>
        /// ファイルにデータを書き込む
        /// </summary>
        /// <param name="writeData">書き込むデータ</param>
        /// <returns>書き込みに成功したか</returns>
        private bool WriteFile(byte[] writeData)
        {
            if (_fileStream == null || writeData == null)
            {
                return false;
            }

            for (long quantity = writeData.LongLength; quantity > 0;)
            {
                int writeCount = (quantity > int.MaxValue ? int.MaxValue : (int)quantity);

                _fileStream.Write(writeData, 0, writeCount);
                quantity -= writeCount;
            }

            return true;
        }
    }
}
