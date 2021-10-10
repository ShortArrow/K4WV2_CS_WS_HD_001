using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WindowsPreview.Kinect;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace K4WV2_CS_WS_HD_001
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Kinect SDK
        KinectSensor kinect;
        BodyIndexFrameReader bodyIndexFrameReader;
        FrameDescription bodyIndexFrameDesc;

        //データ取得用
        byte[] bodyIndexBuffer;

        //表示用
        int bodyIndexColorBytePerPixels = 4;
        byte[] bodyIndexColorBuffer;
        WriteableBitmap bodyIndexColorBitmap;
        Color[] bodyIndexColors;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                //Kinectを開く
                kinect = KinectSensor.GetDefault();
                kinect.Open();

                //ボディーインデックスリーダーを開く
                bodyIndexFrameReader = kinect.BodyIndexFrameSource.OpenReader();
                bodyIndexFrameReader.FrameArrived += bodyIndexFrameReader_FrameArrived;

                //表示のためのデータを作成
                bodyIndexFrameDesc = kinect.BodyIndexFrameSource.FrameDescription;

                //ビットマップ
                bodyIndexColorBitmap = new WriteableBitmap(bodyIndexFrameDesc.Width, bodyIndexFrameDesc.Height);
                ImageBodyIndex.Source = bodyIndexColorBitmap;

                //ボディーインデックスデータ用のバッファ
                bodyIndexBuffer = new byte[bodyIndexFrameDesc.LengthInPixels];

                //ボディーインデックスデータをBGRA（カラー）データにするためのバッファ
                bodyIndexColorBuffer = new byte[bodyIndexFrameDesc.LengthInPixels * bodyIndexColorBytePerPixels];

                //色付けするために色の配列を作る
                bodyIndexColors = new Color[]
                {
                    Colors.Red,Colors.Blue,Colors.Green,Colors.Yellow,Colors.Pink,Colors.Purple,
                };
            }
            catch (Exception ex)
            {
                MessageDialog dlg = new MessageDialog(ex.Message);
                dlg.ShowAsync();
            }
        }

        private void bodyIndexFrameReader_FrameArrived(BodyIndexFrameReader sender, BodyIndexFrameArrivedEventArgs args)
        {
            UpdateBodyIndexFrame(args);
            DrawBodyIndexFrame();
        }

        private void DrawBodyIndexFrame()
        {
            //ボディーインデックスデータをBGRAデータに変換する
            for (int i = 0; i < bodyIndexBuffer.Length; i++)
            {
                var index = bodyIndexBuffer[i];
                int colorindex = i * 4;

                if (index!=255)
                {
                    var color = bodyIndexColors[index];
                    bodyIndexColorBuffer[colorindex + 0] = color.B;
                    bodyIndexColorBuffer[colorindex + 1] = color.G;
                    bodyIndexColorBuffer[colorindex + 2] = color.R;
                    bodyIndexColorBuffer[colorindex + 3] = 255;
                }
                else
                {
                    bodyIndexColorBuffer[colorindex + 0] = 0;
                    bodyIndexColorBuffer[colorindex + 1] = 0;
                    bodyIndexColorBuffer[colorindex + 2] = 0;
                    bodyIndexColorBuffer[colorindex + 3] = 255;
                }
            }
            //ビットマップにする
            var stream = bodyIndexColorBitmap.PixelBuffer.AsStream();
            stream.Write(bodyIndexColorBuffer, 0, bodyIndexColorBuffer.Length);
            bodyIndexColorBitmap.Invalidate();
        }

        private void UpdateBodyIndexFrame(BodyIndexFrameArrivedEventArgs args)
        {
            //ボディーインデックスフレームを取得する
            using (var bodyIndexFrame=args.FrameReference.AcquireFrame())
            {
                if (bodyIndexFrame==null)
                {
                    return;
                }

                //ボディーインデックスデータを取得する
                bodyIndexFrame.CopyFrameDataToArray(bodyIndexBuffer);
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (bodyIndexFrameReader!=null)
            {
                bodyIndexFrameReader.Dispose();
                bodyIndexFrameReader = null;
            }
            if (kinect!=null)
            {
                kinect.Close();
                kinect = null;
            }
        }
    }
}
