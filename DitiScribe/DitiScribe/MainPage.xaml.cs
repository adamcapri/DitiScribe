using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.AudioRecorder;
using Amazon;
using Amazon.S3;
using Amazon.CognitoIdentity;
using Amazon.S3.Model;


namespace DitiScribe
{
    public partial class MainPage : ContentPage
    {
        AudioRecorderService recorder;
        AudioPlayer player;
        public MainPage()
        {
            InitializeComponent();

            recorder = new AudioRecorderService
            {
                StopRecordingAfterTimeout = true,
                TotalAudioTimeout = TimeSpan.FromSeconds(1000),
                AudioSilenceTimeout = TimeSpan.FromSeconds(2)
            };

            player = new AudioPlayer();
            player.FinishedPlaying += Player_FinishedPlaying;
        }

        async void Record_Clicked(object sender, EventArgs e)
        {
            await RecordAudio();
        }

        async Task RecordAudio()
        {
            try
            {
                if (!recorder.IsRecording) //Record button clicked
                {
                    recorder.StopRecordingOnSilence = TimeoutSwitch.IsToggled;

                    RecordButton.IsEnabled = false;
                    PlayButton.IsEnabled = false;
                    UploadButton.IsEnabled = false;

                    //start recording audio
                    var audioRecordTask = await recorder.StartRecording();

                    RecordButton.Text = "Stop Recording";
                    RecordButton.IsEnabled = true;

                    await audioRecordTask;

                    RecordButton.Text = "Record";
                    PlayButton.IsEnabled = true;
                    UploadButton.IsEnabled = true;
                }
                else //Stop button clicked
                {
                    RecordButton.IsEnabled = false;

                    //stop the recording...
                    await recorder.StopRecording();

                    RecordButton.IsEnabled = true;

                    var filePath = recorder.GetAudioFilePath();

                    
                }
            }
            catch (Exception ex)
            {
                //blow up the app!
                throw ex;
            }
        }

        void Play_Clicked(object sender, EventArgs e)
        {
            PlayAudio();
        }

        void Upload_Clicked(object sender, EventArgs e) 
        {
            UploadAudio();
        }

        async void UploadAudio()
        {
            
            var credentials = new CognitoAWSCredentials("eu-west-1:2884665c-cc1a-4d88-8896-7da8cbc06343", Amazon.RegionEndpoint.EUWest1);
            var s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.EUWest1);
            var filePath = recorder.GetAudioFilePath();
            PutObjectRequest request = new PutObjectRequest
            {
                BucketName = "adamserverlesstranscribe-mediabucket-e7elrnnvgles/audio",
                Key = "testfile2.wav",
                FilePath = filePath,
            };
            request.Metadata.Add("x-amz-meta-email", "adam@caprihealthcare.co.uk");
            request.Metadata.Add("x-amz-meta-maxspeakerlabels", "2");

            try
            {
               await s3Client.PutObjectAsync(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void PlayAudio()
        {
            try
            {
                var filePath = recorder.GetAudioFilePath();

                if (filePath != null)
                {
                    PlayButton.IsEnabled = false;
                    RecordButton.IsEnabled = false;

                    player.Play(filePath);
                }
            }
            catch (Exception ex)
            {
                //blow up the app!
                throw ex;
            }
        }

        void Player_FinishedPlaying(object sender, EventArgs e)
        {
            PlayButton.IsEnabled = true;
            RecordButton.IsEnabled = true;
        }
    }


}