using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Tensorflow.Serving;
using TensorFlowServingClient.Utils;

namespace ImageReader
{
    public static class TensorflowSharp
    {
        public static void SendRequest(string inputPath, string outputPath)
        {
            IPHostEntry host;
            string containerIP = "?";
            string hostName = "mw-tf-server.uksouth.azurecontainer.io";
            host = Dns.GetHostEntry(hostName); //; //Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    containerIP = ip.ToString();
                }
            }

            //Create gRPC Channel
            var channel = new Channel(containerIP + ":8500", ChannelCredentials.Insecure);
            var client = new PredictionService.PredictionServiceClient(channel);
            //Check available models
            //var responce = client.GetModelMetadata(new GetModelMetadataRequest()
            //{
            //    ModelSpec = new ModelSpec() { Name = "model" },
            //    MetadataField = { "signature_def" }
            //});

            //Console.WriteLine($"Model Available: {responce.ModelSpec.Name} Ver.{responce.ModelSpec.Version}");

            //string imagePath = "C:/WebcamSnapshots/picture.png";

            var request = new PredictRequest()
            {
                ModelSpec = new ModelSpec() { Name = "model", SignatureName = "serving_default" }
            };

            Stream stream = new FileStream(inputPath, FileMode.Open);
            byte[] b;
            using (BinaryReader br = new BinaryReader(stream))
            {
                b = br.ReadBytes((int)stream.Length);
            }
            string base64String = Convert.ToBase64String(b, 0, b.Length);
            //Console.WriteLine(base64String.Substring(0, 50));
            request.Inputs.Add("input_image", TensorBuilder.CreateTensorFromString(base64String));

            try
            {
                var predictResponse = client.Predict(request);
                var output = predictResponse.Outputs["output_image"];

                var image_output0 = output.StringVal[0];
                var stri = image_output0.ToString(Encoding.ASCII);
                //Console.WriteLine(stri);
                for (int i = 0; i < (stri.Length % 4); i++)
                    stri += "=";
                //byte[] image_output1 = ASCIIEncoding.ASCII.GetBytes(stri);

                stri = stri.Replace('_', '/').Replace('-', '+');
                byte[] test = Convert.FromBase64String(stri);

                //string savePath = "C:/WebcamSnapshots/csharp_prediction.png";
                ImageConverter converter = new ImageConverter();
                Image image = (Image)converter.ConvertFrom(test);

                image.Save(outputPath, ImageFormat.Png);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            channel.ShutdownAsync().Wait();

        }
    }
}
