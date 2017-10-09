using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wallpaper
{
    public partial class MainForm : Form
    {
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        public MainForm()
        {
            InitializeComponent();
            SettingManager.Initialize();
            UI_Init();
        }

        public void UI_Init()
        {
            txtID.Text = Setting.YoutubeID;
            lblVideoPath.Text = Setting.VideoPath;
            lblGIFPath.Text = Setting.GIFPath;
            pictureBox1.ImageLocation = Setting.MorphID;
            txtFirst.Text = Setting.First_Ment;
            txtSecond.Text = Setting.Second_Ment;
            if (Setting.IsStartUp)
                checkBox1.Checked = true;
            trkVolume.Value = Setting.Volume;
        }

        #region Variables
        private GIFForm m_GifForm = null;
        private VideoForm m_VideoForm = null;
        private YoutubeForm m_YoutubeForm = null;
        private MorphForm m_MorphForm = null;
        private int m_Volume = 0;
        private string VideoPath = "";
        private string GIFPath = "";
        private string ImagePath = "";
        public int Volume { get { return m_Volume; } set { waveOutSetVolume(IntPtr.Zero, (uint)value); } }
        private const string VideoExt = "Video|*.mp4;*.avi;*.wmv;*.mpeg";
        private const string ImageExt = "Image|*.jpg;*.jpeg;*.bmp;*.png";
        #endregion

        private void StopWallpaper()
        {
            if (m_GifForm != null)
                m_GifForm.Close();
            if (m_VideoForm != null)
                m_VideoForm.Close();
            if (m_YoutubeForm != null)
                m_YoutubeForm.Close();
            if (m_MorphForm != null)
                m_MorphForm.Close();
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            StopWallpaper();
        }

        private void trkVolume_Scroll(object sender, EventArgs e)
        {
            int NewVolume = ((ushort.MaxValue / 10) * trkVolume.Value);
            // Set the same volume for both the left and the right channels
            uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
            // Set the volume
            waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
            //Save The Setting
            Setting.Volume = trkVolume.Value;
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            var youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyBgwr13tNn2BL5TbKLvT6e_kCR8egeQ5og", // 키 지정
                ApplicationName = "wallpaperengine"
            });
            listView1.Items.Clear();
            // Search용 Request 생성
            var request = youtube.Search.List("snippet");
            request.Q = txtSearch.Text;  //ex: "양희은"
            request.MaxResults = 25;

            // Search용 Request 실행
            var result = await request.ExecuteAsync();

            // Search 결과를 리스트뷰에 담기
            foreach (var item in result.Items)
            {
                if (item.Id.Kind == "youtube#video")
                {
                    listView1.Items.Add(item.Id.VideoId.ToString(), item.Snippet.Title, 0);
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(listView1.SelectedItems.Count > 0)
            {
                string videoId = listView1.SelectedItems[0].Name;
                txtID.Text = videoId;
            }
        }

        private void btnYoutube_Click(object sender, EventArgs e)
        {
            StopWallpaper();
            StringBuilder URi = new StringBuilder("https://youtube.com/embed/");
            URi.Append(txtID.Text);
            URi.Append("?autoplay=1&loop=1&controls=0&showinfo=0&vq=hd1080&playlist=");
            URi.Append(txtID.Text);
            Setting.YoutubeID = txtID.Text;
            m_YoutubeForm = new YoutubeForm()
            {
                Videopath = URi.ToString()
            };
            m_YoutubeForm.Show();
        }

        private void btnOpenVideo_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = VideoExt;
            openFileDialog1.Title = "Video";
            if(openFileDialog1.ShowDialog()==DialogResult.OK)
            {
                lblVideoPath.Text = openFileDialog1.FileName;
                VideoPath = openFileDialog1.FileName;

            }
        }

        private void btnVideo_Click(object sender, EventArgs e)
        {
            StopWallpaper();
            if(VideoPath != "")
            {
                Setting.VideoPath = VideoPath;
                m_VideoForm = new VideoForm()
                {
                    Videopath = VideoPath
                };
                m_VideoForm.Show();
            }
        }

        private void btnOpenGIF_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "*.gif|*.gif";
            openFileDialog1.Title = "GIF Open";
            if(openFileDialog1.ShowDialog()  == DialogResult.OK)
            {
                GIFPath = openFileDialog1.FileName;
                lblGIFPath.Text = GIFPath;
            }
        }

        private void btnGIF_Click(object sender, EventArgs e)
        {
            StopWallpaper();
            if(GIFPath != "")
            {
                Setting.GIFPath = GIFPath;
                m_GifForm = new GIFForm()
                {
                    Gifpath = GIFPath
                };
                m_GifForm.Show();
            }
        }

        private void btnText_Click(object sender, EventArgs e)
        {
            StopWallpaper();
            if(Setting.MorphID != "" && txtFirst.Text != "" && txtSecond.Text != "")
            {
                Setting.First_Ment = txtFirst.Text;
                Setting.Second_Ment = txtSecond.Text;
                m_MorphForm = new MorphForm()
                {
                    Text_First = txtFirst.Text,
                    Text_Second = txtSecond.Text,
                    ImagePath = Setting.MorphID
                };
                m_MorphForm.Show();
            }
        }

        private void btnOpenText_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open Image";
            openFileDialog1.Filter = ImageExt;
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Setting.MorphID = openFileDialog1.FileName;
                pictureBox1.ImageLocation = Setting.MorphID;
            }

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingManager.Dispose();
        }
    }
}
