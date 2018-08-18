using System.Windows;

namespace MOVUtility
{

    /// <summary>
    /// JPEG関連クラス
    /// </summary>
    public class JpegUtility
    {
#if false
        // アプリケーションデータ(APP0)。キヤノン製カメラならばここにイメージサイズが格納されている。
        // APP0の内容は次の通り
        //
        // APP0マーカー        2byte   0xFFE0(APP0)
        // サイズ              2byte
        // マジックナンバー    3byte   “VB”（0x5642）
        // カメラ              1byte   カメラ番号
        // パン                2byte   撮影時のパン角  （1/100度単位）
        // チルト              2byte   撮影時のチルト角 （1/100度単位）
        // ズーム              2byte   撮影時のズーム角 （1/100度単位）
        // 明るさ              2byte   撮影時の露出設定 （0：無効、1：通常、2：明）
        // 撮影時刻            4byte   撮影時刻 （1970/1/1 00:00:00 GMTからの秒数）
        // タイムゾーン        2byte   GMTからのオフセット （分）
        // 画質                1byte   JPEGのQ値 （0-4）
        // 機種                1byte
        // 接点入力状態        4byte   接点入力の状態
        // 接点出力状態        4byte   接点出力の状態
        // ファイル形式        1byte   APP0ヘッダー番号 (05h)
        // 認証子種別          1byte   認証子なし：0
        // 画像の幅            2byte   横方向の画素数
        // 画像の高さ          2byte   縦方向の画素数
        // 撮影時刻端数        2byte   撮影時刻（ミリ秒単位）
        // 設定値識別子        4byte   設定変更時に変更される整数値（0）
        // サーバー識別子      3byte   イーサネットのMACアドレス（下3バイト）
        // シェード補正        1byte    シェード補正値（0：オフ、1～7：補正レベル）
        // 電子ズーム倍率      2byte    撮影時の電子ズーム倍率
        // 露出補正値          2byte    撮影時の露出補正値 （-7 ～ +7）
        // MACアドレス         6byte   イーサネットのMACアドレス（6バイト）
        // 予約                2byte    未使用
        // 解析エンジン状態    4byte   画像解析エンジンの状態

        private static readonly DateTime dtStartPoint = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// 撮影時刻を取得する。
        /// </summary>
        /// <param name="jpegImageData">Jpegイメージ</param>
        /// <param name="date">記録日時</param>
        /// <param name="offset">記録日時のオフセット</param>
        /// <returns>撮影時刻が取得できたか？</returns>
        public static bool ReadHeader(byte[] jpegImageData, out DateTime date, out short offset)
        {
            date = DateTime.MinValue;
            offset = 0;

            for (int i = 0; i + 39 < jpegImageData.Length; i++)
            {
                if (jpegImageData[i] == 0xFF)
                {
                    // APP0ヘッダにキヤノン製カメラ用情報が格納されている
                    if (jpegImageData[i + 1] == 0xE0 && jpegImageData[i + 4] == 0x56 && jpegImageData[i + 5] == 0x42)
                    {
                        //撮影時刻(GMTからの秒数)
                        long tick = (jpegImageData[i + 16] << 24) + (jpegImageData[i + 17] << 16) + (jpegImageData[i + 18] << 8) + jpegImageData[i + 19];
                        DateTime gmt = new DateTime(dtStartPoint.Ticks + tick * 10000000);

                        //GMTオフセット
                        offset = (short)((jpegImageData[i + 20] << 8) + jpegImageData[i + 21]);

                        //撮影時刻端数
                        date = new DateTime(gmt.Ticks + (long)((jpegImageData[i + 38] << 8) + jpegImageData[i + 39]) * 10000);

                        return true;
                    }
                    // エンコードされたイメージデータ(SOS)。ここまで来るとイメージサイズは格納されていないと考えられる。
                    else if (jpegImageData[i + 1] == 0xDA)
                    {
                        return false;
                    }
                }
            }

            return false;
        }
#endif

        /// <summary>
        /// Jpegイメージサイズを取得する。
        /// </summary>
        /// <param name="jpegImageData">Jpegイメージ</param>
        /// <param name="size">Jpegイメージサイズ。</param>
        /// <returns>Jpegイメージサイズが取得できたか？</returns>
        public static bool ReadHeader(byte[] jpegImageData, out Size size)
        {
            size = Size.Empty;

            int jpegLen = jpegImageData.Length;
            for (int i = 0; i < jpegLen; i++)
            {
                if (jpegImageData[i] == 0xFF)
                {
                    if (i + 1 < jpegLen)
                    {
                        switch (jpegImageData[i + 1])
                        {
                            case 0xD8:  // SOS
                                i++;
                                continue;
                            case 0xDA:  // SOS
                            case 0xD9:  // EOI
                                // SOS、EOIまで来ると情報は取得できない
                                return false;
                            case 0xE0:  // APP0(キヤノン製カメラの場合にはAPP0に情報が格納されている)
                                if (i + 37 < jpegLen && jpegImageData[i + 4] == 0x56 && jpegImageData[i + 5] == 0x42)
                                {
                                    size = new Size(
                                        (jpegImageData[i + 34] << 8) + jpegImageData[i + 35],
                                        (jpegImageData[i + 36] << 8) + jpegImageData[i + 37]);
                                    return true;
                                }
                                else
                                {
                                    if (i + 3 < jpegLen)
                                    {
                                        i += (jpegImageData[i + 2] << 8) + jpegImageData[i + 3] + 1;
                                        break;
                                    }
                                    else
                                        return false;
                                }
                            case 0xC0:  // SOF
                            case 0xC2:
                                if (i + 8 < jpegLen)
                                {
                                    size = new Size(
                                        (jpegImageData[i + 7] << 8) + jpegImageData[i + 8],
                                        (jpegImageData[i + 5] << 8) + jpegImageData[i + 6]);
                                    return true;
                                }
                                else
                                    return false;
                            default:
                                if (i + 3 < jpegLen)
                                {
                                    i += (jpegImageData[i + 2] << 8) + jpegImageData[i + 3] + 1;
                                    break;
                                }
                                else
                                    return false;
                        }
                    }
                }
            }

            return false;
        }
    }
}
