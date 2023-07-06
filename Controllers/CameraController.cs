using System.Net;
using System.Net.Http.Headers;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;

namespace cs_streaming.Controllers;

[ApiController]
[Route("[controller]")]
public class CameraController : ControllerBase
{
    private readonly ILogger<CameraController> _logger;
    private VideoCapture _capture;
    private Mat frame;

    public CameraController(ILogger<CameraController> logger)
    {
        _logger = logger;
        _capture = new VideoCapture(0); // 0 is the ID of the webcam
        frame = new Mat();
    }

    [HttpGet("stream3")]
    public async Task<IActionResult> Stream()
    {
        _capture.Open(0);

        if (!_capture.IsOpened())
        {
            return NotFound("Camera is not available.");
        }

        while (true)
        {
            using var frame = new Mat();
            _capture.Read(frame);

            if (frame.Empty())
            {
                continue;
            }

            Cv2.Resize(frame, frame, new OpenCvSharp.Size(500, 500));
            Cv2.PutText(frame, $"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", new OpenCvSharp.Point(5, 20),
                HersheyFonts.HersheySimplex, 0.5, Scalar.Green, 2, LineTypes.AntiAlias);

            using var stream = frame.ToMemoryStream(".jpg");
            var bytes = stream.ToArray();
            await Response.Body.WriteAsync(bytes);
        }
    }

    [HttpGet("stream")]
    public HttpResponseMessage Get()
    {
        _capture.Read(frame);

        Cv2.ImShow("Live", frame);

        // Convert frame to JPEG
        var imageBytes = frame.ToBytes(".jpeg");

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(imageBytes)
        };

        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        return response;
    }

    [HttpGet("stream2")]
    public async Task GetVideoStream()
    {
        var videoCapture = new VideoCapture(0);
        var channel = Channel.CreateUnbounded<byte[]>();

        // バックグラウンドでカメラの映像をキャプチャーし続けます。
        _ = Task.Run(async () =>
        {
            using Mat image = new Mat();
            while (true)
            {
                videoCapture.Read(image);
                if (image.Empty()) break;

                using var memoryStream = new MemoryStream();
                image.WriteToStream(memoryStream, ".jpg");

                await channel.Writer.WriteAsync(memoryStream.ToArray());
            }
        });

        var boundary = "frame";
        Response.ContentType = "multipart/x-mixed-replace; boundary=" + boundary;
        await Response.Body.WriteAsync(System.Text.Encoding.ASCII.GetBytes($"\r\n--{boundary}\r\n"));

        await foreach (var imageData in channel.Reader.ReadAllAsync())
        {
            var data = System.Text.Encoding.ASCII.GetBytes($"Content-Type: image/jpeg\r\nContent-Length: {imageData.Length}\r\n\r\n");
            await Response.Body.WriteAsync(data, 0, data.Length);
            await Response.Body.WriteAsync(imageData, 0, imageData.Length);
            await Response.Body.WriteAsync(System.Text.Encoding.ASCII.GetBytes($"\r\n--{boundary}\r\n"));
        }
    }
}
