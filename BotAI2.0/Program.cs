using System;
using System.IO;
using VoiceRSS_SDK;
using DetectLanguage;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Globalization;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using IronOcr;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using System.Diagnostics;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Exceptions;
using System.Reflection;

namespace testAI
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TelegramBotClient("YOUR_API_KEY"); //tg bot api
            MyBot a = new MyBot();
            client.StartReceiving(a.Update, a.Error);
            Console.ReadLine();
        }
    }

    class MyBot
    {
        string an = "";
        string link = "";

        public async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message; //all messages

            //all messages dividing
            if (update.Type == UpdateType.Message)
            {
                Console.WriteLine($"{message.Chat.FirstName}|{message.Text}"); //console printing message

                //all text messages 
                if (message.Text != null)
                {
                    try
                    {
                        string strMessage = StringBuild(message.Text); //text message

                        if (message.Text == "/start") //start
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Hi, this is an AI chat room. You can ask me any question, and I can work with some graphic and audio information༼ つ ◕_◕ ༽つ\r\n For more details about my features call /help from the menu.");
                            Console.WriteLine("Hi, this is an AI chat room. You can ask me any question, and I can work with some graphic and audio information༼ つ ◕_◕ ༽つ\r\n For more details about my features call /help from the menu.");
                            return;
                        }
                        else if (message.Text == "/help")
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "•\t *Text*\r\nSend a text message\\. Receive a reply\\.\r\n\r\n•\t *Text to image*\r\nSend \\/tti with the description of the picture you want to get\\. \r\nReceive image based on your text\\.\r\n\r\n•\t *Text to speech*\r\nSend \\/tts with the text in one message\\. \r\nReceive voicing of your text\\.\r\n\r\n•\t *Picture*\r\nSend a picture\\.\r\nGet the transcription\\.\r\nChoose action on the button\\:_\r\n\\- summarizing \r\n\\- replying_\r\n\r\n•\t *Audio*\r\nSend an audio in mp3 format\\. Max size \\- 20mb\\. \r\nGet the transcription\\. \r\nChoose action on the button\\:\r\n_\\- summarizing_\r\n\r\n•\t *Video*\r\nSend a video in mp4 format\\. Max size \\- 20mb\\. \r\nGet the transcription\\.\r\nChoose action on the button\\:\r\n_\\- summarizing_\r\n\r\n•\t *Video to audio*\r\nSend \\/vta\\. Then send video you want to convert to audio\\. Get audio reply\\.\r\n\r\n•\t *YouTube video*\r\nSend a YouTube link to the video\\. \r\nChoose action on the button\\:\r\n_\\- getting audio\r\n\\- getting video\r\n\\- transcribing\r\n\\- summarizing_\r\n", ParseMode.MarkdownV2);
                            Console.WriteLine("/help");
                            return;
                        }
                        else if (message.Text.Contains("https://www.youtube.com/watch?v=") || message.Text.Contains("https://youtu.be/"))
                        {
                            link = strMessage;
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]{
                            // First row of buttons
                            new [] {
                                InlineKeyboardButton.WithCallbackData("download video", "download video"),
                                InlineKeyboardButton.WithCallbackData("download audio", "download audio"),
                            },
                            // Second row of buttons
                            new [] {
                                InlineKeyboardButton.WithCallbackData("transcribe", "transcribe"),
                                InlineKeyboardButton.WithCallbackData("summarize", "summarize")
                            }
                        });
                            //await botClient.SendTextMessageAsync(message.Chat.Id, OCRanswer);
                            await botClient.SendTextMessageAsync(message.Chat.Id, "You sent a YouTube link. Please choose how can I help you.", replyMarkup: inlineKeyboard, parseMode: ParseMode.Markdown);
                        }
                        //text to speech
                        else if (message.Text.Contains("/tts"))
                        {
                            //if the message is only /texttospeech
                            if (message.Text.Equals(message.Text.Equals("/tts", StringComparison.OrdinalIgnoreCase)))
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Send command /tts and text you want to convert in one message");
                            }
                            else
                            {
                                string text = strMessage.Replace("/tts", "");
                                Console.WriteLine("text to speech");
                                var DLapiKey = "YOUR_API_KEY";
                                var RSSapiKey = "YOUR_API_KEY";
                                var isSSL = false;
                                string lang = Languages.English_UnitedStates;

                                // creating a Detect Language API client instance
                                var client = new DetectLanguageClient(DLapiKey);

                                // language detection
                                var results = await client.DetectAsync(text);
                                lang = GetLanguageCode(results[0].language);
                                Console.WriteLine("Language: " + lang);

                                var voiceParams = new VoiceParameters(text, lang)
                                {
                                    AudioCodec = AudioCodec.MP3,
                                    AudioFormat = AudioFormat.Format_44KHZ.AF_44khz_16bit_stereo,
                                    IsBase64 = false,
                                    IsSsml = false,
                                    SpeedRate = 0
                                };
                                var voiceProvider = new VoiceProvider(RSSapiKey, isSSL);

                                voiceProvider.SpeechFailed += (Exception ex) =>
                                {
                                    Console.WriteLine(ex.Message);
                                };

                                voiceProvider.SpeechReady += (object data) =>
                                {
                                    var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file_path\\tts.mp3");
                                    System.IO.File.WriteAllBytes(fileName, (byte[])data);

                                };
                                voiceProvider.SpeechAsync<byte[]>(voiceParams);
                                FileInfo fileInfo = new FileInfo("file_path\\tts.mp3");
                                long fileSizeInBytes = fileInfo.Length;
                                long fileSizeInMB = fileSizeInBytes / (1024 * 1024);
                                if (fileSizeInMB <= 50)
                                {
                                    await using Stream stream = System.IO.File.OpenRead("file_path\\tts.mp3");
                                    await botClient.SendDocumentAsync(message.Chat.Id, new InputOnlineFile(stream, "tts.mp3"));

                                    stream.Close();
                                    DeleteCatalog();
                                    return;
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Telegram policy doesn't allow me to send a file larger than 50 mb.");
                                    Console.WriteLine("Telegram policy doesn't allow me to send a file larger than 50 mb.");
                                    DeleteCatalog();
                                }
                            }

                        }
                        else if (message.Text.Contains("/tti"))
                        {
                            if (message.Text.Equals("/tti", StringComparison.OrdinalIgnoreCase))
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Send command /tti with the description of the image in one message.");
                                return;
                            }
                            else 
                            {
                                string text = strMessage.Replace("/tti", "");
                                Console.WriteLine("text to image");
                                var result = await TextToImage(text);
                                string filePath = result.filePath;
                                string output = result.output;
                                if (!string.IsNullOrEmpty(filePath))
                                {
                                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                                    {
                                        InputOnlineFile photo = new InputOnlineFile(fileStream);
                                        await botClient.SendPhotoAsync(message.Chat.Id, photo, output);
                                    }
                                    Console.WriteLine("Image is sent.");
                                    DeleteCatalog();
                                    return;
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Failed to generate the image.");
                                    Console.WriteLine("Failed to generate the image.");
                                    DeleteCatalog();
                                    return;
                                }
                            }
                        }
                        else if (message.Text.Equals("/vta", StringComparison.OrdinalIgnoreCase))
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Send video you want to convert to audio.");
                            Console.WriteLine("Send video you want to convert to audio.");
                            BotState.SetWaitingForVideo(message.Chat.Id, true);

                        }
                        else //all text messages
                        {
                            var answer = callOpenAI(1000, strMessage, "text-davinci-003", 0.5, 0.8, 1.5, 0); //openAI settings etc
                            await botClient.SendTextMessageAsync(message.Chat.Id, answer);
                            Console.WriteLine(answer);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Sorry, an error occurred while processing your request.");
                        return;
                    }

                }
                else if (BotState.IsWaitingForVideo(message.Chat.Id) && message.Video != null)
                {
                    try
                    {
                        var file = await botClient.GetFileAsync(message.Video.FileId);
                        if (file.FileSize <= 20 * 1024 * 1024)
                        {
                            Console.WriteLine("Video to audio is processing");
                            var fileId = message.Video.FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId);
                            var filePath = fileInfo.FilePath;
                            string destinationFilePath = "file_path\\videotoaudio.mp4"; //file path
                            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                            await botClient.DownloadFileAsync(filePath, fileStream);
                            fileStream.Close();
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Please, wait. The video is converting to audio.");

                            // Convert video to audio using FFmpeg
                            string outputFilePath = "file_path\\videotoaudio.mp3"; // output audio file path
                            var ffmpegProcess = new Process
                            {
                                StartInfo =
                                {
                                    FileName = "C:\\ffmpeg\\bin\\ffmpeg.exe",
                                    Arguments = $"-i {destinationFilePath} {outputFilePath}",
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };
                            ffmpegProcess.Start();
                            await ffmpegProcess.WaitForExitAsync();
                            // Check the size of the converted audio file
                            FileInfo convertedAudioFile = new FileInfo(outputFilePath);
                            if (convertedAudioFile.Length <= 50 * 1024 * 1024)
                            {
                                using FileStream audioFileStream = System.IO.File.OpenRead(outputFilePath);

                                // Send the audio file to the user
                                await botClient.SendAudioAsync(
                                    chatId: message.Chat.Id,
                                    audio: new InputOnlineFile(audioFileStream),
                                    title: "Video to Audio"
                                );
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "The converted audio file is larger than 50MB and cannot be sent.");
                            }
                            DeleteCatalog();
                            BotState.SetWaitingForVideo(message.Chat.Id, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Sorry, an error occurred while processing your request. Maybe the video is more than 20mb.");
                        BotState.SetWaitingForVideo(message.Chat.Id, false);
                        return;
                    }
                }
                //the message is photo
                else if (message.Photo != null) //sending a photo
                {
                    try
                    {
                        Console.WriteLine("We got a photo");
                        string g = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).Remove(5); //name to the file
                        var fileId = update.Message.Photo.Last().FileId;
                        var fileInfo = await botClient.GetFileAsync(fileId);
                        var filePath = fileInfo.FilePath;
                        string destinationFilePath = $@"file_path\{g}.jpg"; //file path to the photo
                        await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                        await botClient.DownloadFileAsync(filePath, fileStream);
                        fileStream.Close();
                        var OCRanswer = StringBuild(OCR(destinationFilePath)); //ocr sending request
                        an = OCRanswer; //answer
                        DeleteCatalog(); //deleting all files from the catalog

                        //no text on the photo
                        if (OCRanswer == "")
                        {
                            Console.WriteLine("OCR function returned a ''.");
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Nothing on the image");
                            return;
                        }
                        else if (OCRanswer != null)
                        {
                            an = OCRanswer;
                            //buttons
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]{
                            InlineKeyboardButton.WithCallbackData("answer", "answer"),
                            InlineKeyboardButton.WithCallbackData("summary", "summary")
                        });
                            //await botClient.SendTextMessageAsync(message.Chat.Id, OCRanswer);
                            await botClient.SendTextMessageAsync(message.Chat.Id, OCRanswer, replyMarkup: inlineKeyboard, parseMode: ParseMode.Markdown);
                            Console.WriteLine(OCRanswer);
                        }
                        //error
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Something went wrong. Can you try again or try some other image?");
                            Console.WriteLine("OCR function returned a null value.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Sorry, an error occurred while processing your request.");
                        return;
                    }
                }

                else if (message.Video != null)
                {
                    try
                    {
                        var file = await botClient.GetFileAsync(message.Video.FileId);
                        if (file.FileSize <= 20 * 1024 * 1024)
                        {
                            Console.WriteLine("Video is less than 20MB, fine");
                            var fileId = message.Video.FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId);
                            var filePath = fileInfo.FilePath;
                            string destinationFilePath = "file_path\\video.mp4"; //file path
                            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                            await botClient.DownloadFileAsync(filePath, fileStream);
                            fileStream.Close();
                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Please, wait. The video is processing.");
                            string text_answer = ConvertToText();
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]{
                            InlineKeyboardButton.WithCallbackData("summary", "summary")});
                            await botClient.SendTextMessageAsync(message.Chat.Id, text_answer, replyMarkup: inlineKeyboard,
                                parseMode: ParseMode.Markdown);
                            Console.WriteLine(text_answer);
                            an = text_answer;
                        }
                    }
                    catch (ApiRequestException ex)
                    {
                        await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "There was an error getting file. Maybe the file is more than 20 mb.");
                        Console.WriteLine(ex.Message);
                    }
                }
                //the message is a audio
                else if (message.Audio != null)
                {
                    try
                    {
                        var file = await botClient.GetFileAsync(message.Audio.FileId);
                        if (file.FileSize <= 20 * 1024 * 1024)
                        {
                            Console.WriteLine("Audio is less than 20mb, ok.");
                            var fileId = update.Message.Audio.FileId;
                            var fileInfo = await botClient.GetFileAsync(fileId);
                            var filePath = fileInfo.FilePath;
                            string destinationFilePath = "file_path\\audio.mp3"; //file path
                            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                            await botClient.DownloadFileAsync(filePath, fileStream);
                            fileStream.Close();
                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Please, wait. The audio is processing.");
                            string text_answer = ConvertToText();
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]{
                            InlineKeyboardButton.WithCallbackData("summary", "summary")
                        });
                            await botClient.SendTextMessageAsync(message.Chat.Id, text_answer, replyMarkup: inlineKeyboard,
                                parseMode: ParseMode.Markdown);
                            Console.WriteLine(text_answer);
                            an = text_answer;
                        }
                    }
                    catch (ApiRequestException ex)
                    {
                        await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "There was an error getting file. Maybe the file is more than 20 mb.");
                        Console.WriteLine(ex.Message);
                    }
                }
                //unknown format
                else
                {
                    try
                    {
                        var sticker = new InputOnlineFile("CAACAgIAAxkBAAEH5EJj-cLUasbcNfufH1NDCuf10L63bQACqwADnKFJLZWJ7iPUOzICLgQ");
                        await botClient.SendStickerAsync(message.Chat.Id, sticker);
                        await botClient.SendTextMessageAsync(message.Chat.Id, "I dont't know this format");
                        Console.Write("I don't know this format");
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Sorry, an error occurred while processing your request.");
                        return;
                    }
                }
            }

            //buttons answers
            else if (update.Type == UpdateType.CallbackQuery)
            {
                try
                {
                    string codeOfButton = update.CallbackQuery.Data;
                    if (codeOfButton == "answer")
                    {
                        Console.WriteLine(codeOfButton);
                        if (Sticker() != "")
                        {
                            var sticker = new InputOnlineFile(Sticker());
                            await botClient.SendStickerAsync(update.CallbackQuery.Message.Chat.Id, sticker);
                        }
                        var answer = callOpenAI(1000, an, "text-davinci-003", 0.7, 1, 0, 0);
                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, answer);
                        Console.WriteLine(answer);
                        return;
                    }
                    else if (codeOfButton == "summary")
                    {
                        an = "summary this text please: " + an;
                        Console.WriteLine(codeOfButton);
                        var answer = callOpenAI(1000, an, "text-davinci-003", 0.7, 1, 0, 0);
                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, answer);
                        Console.WriteLine(answer);
                        return;
                    }
                    else if (codeOfButton == "download video")
                    {
                        if (link != "")
                        {
                            Console.WriteLine(link);
                            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Please wait a bit. The video is uploading.");
                            Console.WriteLine("Please wait a bit. The video is uploading.");
                            ytVideo(link);
                            FileInfo fileInfo = new FileInfo("file_path\\video.mp4");
                            long fileSizeInBytes = fileInfo.Length;
                            long fileSizeInMB = fileSizeInBytes / (1024 * 1024);
                            if (fileSizeInMB <= 50)
                            {
                                await using Stream stream = System.IO.File.OpenRead("file_path\\video.mp4");
                                await botClient.SendDocumentAsync(update.CallbackQuery.Message.Chat.Id, new InputOnlineFile(stream, "video.mp4"));
                                stream.Close();
                                DeleteCatalog();
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Telegram policy doesn't allow me to send a file larger than 50 mb. You can still download audio");
                                Console.WriteLine("Telegram policy doesn't allow me to send a file larger than 50 mb. You can still download audio.");
                                DeleteCatalog();
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Some processes are wrong. Send the link again, please.");
                            DeleteCatalog();
                        }
                        return;
                    }
                    else if (codeOfButton == "download audio")
                    {
                        if (link != "")
                        {
                            Console.WriteLine(link);
                            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Please wait a bit. The audio is uploading.");
                            Console.WriteLine("Please wait a bit. The audio is uploading.");
                            ytAudio(link);
                            FileInfo fileInfo = new FileInfo("file_path\\audio.mp3");
                            long fileSizeInBytes = fileInfo.Length;
                            long fileSizeInMB = fileSizeInBytes / (1024 * 1024);
                            if (fileSizeInMB <= 50)
                            {
                                await using Stream stream = System.IO.File.OpenRead("file_path\\audio.mp3");
                                await botClient.SendDocumentAsync(update.CallbackQuery.Message.Chat.Id, new InputOnlineFile(stream, "audio.mp3"));
                                stream.Close();
                                DeleteCatalog();
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Telegram policy doesn't allow me to send a file larger than 50 mb.");
                                Console.WriteLine("Telegram policy doesn't allow me to send a file larger than 50 mb.");
                                DeleteCatalog();
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Some processes are wrong. Send the link again, please.");
                            DeleteCatalog();
                        }
                        return;

                    }
                    else if (codeOfButton == "transcribe")
                    {
                        if (link != "")
                        {
                            Console.WriteLine("transcribe");
                            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Please wait a bit. The video is transcribing.");
                            ytAudio(link);
                            string text_answer = ConvertToText();
                            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, text_answer);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Some processes are wrong. Send the link again, please.");
                            DeleteCatalog();
                        }
                        return;
                    }

                    else if (codeOfButton == "summarize")
                    {
                        if (link != "")
                        {
                            Console.WriteLine("summarize");
                            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Please wait a bit. The video is summarizing.");
                            ytAudio(link);
                            string text_answer = ConvertToText();
                            text_answer = "summary this text please: " + text_answer;
                            var answer = callOpenAI(1000, text_answer, "text-davinci-003", 0.7, 1, 0, 0);
                            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, answer);
                            Console.WriteLine(answer);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Some processes are wrong. Send the link again, please.");
                            DeleteCatalog();
                        }
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Sorry, an error occurred while processing your request.");
                    return;
                }
            }

        }
        public Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        private static String StringBuild(String text)
        {
            text = text.Trim().Replace("\n", "").Replace("\r", "").Replace("  ", " ").Replace("•", "").Replace("-", "");
            return text;
        }
        private static string callOpenAI(int tokens, string input, string engine,
          double temperature, double topP, double frequencyPenalty, int presencePenalty)
        {
            var openAiKey = "YOUR_API_KEY"; //open ai key
            var apiCall = "https://api.openai.com/v1/engines/" + engine + "/completions";
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), apiCall))
                    {
                        if (openAiKey != null)
                        {
                            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + openAiKey);
                        }
                        else
                        {
                            Console.WriteLine("openAiKey is null. Cannot perform runtime binding.");
                            return null;
                        }
                        if (input != null)
                        {
                            request.Content = new StringContent("{\n  \"prompt\": \"" + input + "\",\n  \"temperature\": " +
                                                                temperature.ToString(CultureInfo.InvariantCulture) + ",\n  \"max_tokens\": " + tokens + ",\n  \"top_p\": " + topP +
                                                                ",\n  \"frequency_penalty\": " + frequencyPenalty + ",\n  \"presence_penalty\": " + presencePenalty + "\n}");
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        }
                        else
                        {
                            Console.WriteLine("input is null. Cannot perform runtime binding.");
                            return null;
                        }
                        var response = httpClient.SendAsync(request).Result;
                        var json = response.Content.ReadAsStringAsync().Result;
                        dynamic dynObj = JsonConvert.DeserializeObject(json);
                        if (dynObj != null)
                        {
                            if (dynObj.choices[0].text.ToString() != "")
                            {
                                return dynObj.choices[0].text.ToString();
                            }
                            else
                            {
                                return "Sorry, I can't answer this";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        private static string OCR(string input)
        {
            License.LicenseKey = "YOUR_API_KEY"; //Iron OCR key
            if ((License.IsValidLicense("")) == true)
            {
                var ocr = new IronTesseract();
                ocr.Language = OcrLanguage.EnglishFast;
                ocr.AddSecondaryLanguage(OcrLanguage.Russian);
                using (var Input = new OcrInput())
                {
                    Input.AddImage(input);
                    Input.DeNoise();
                    Input.Deskew();
                    var Result = ocr.Read(Input);
                    return Result.Text;
                }
            }
            else
            {
                return null;
            }
        }

        private static async Task<(string filePath, string output)> TextToImage(string text)
        {
            string apiKey = "YOUR_API_KEY"; //Stable Diffusion key
            string endpoint = "https://stablediffusionapi.com/api/v3/text2img";
            string downloadPath = @"file_path";
            string filePath = Path.Combine(downloadPath, "text2img.png");
            string output = string.Empty;

            try
            {
                var client = new HttpClient();
                var requestBody = new
                {
                    key = apiKey,
                    prompt = text,
                    negative_prompt = (string)null,
                    width = "512",
                    height = "512",
                    samples = "1",
                    num_inference_steps = "20",
                    safety_checker = "no",
                    enhance_prompt = "yes",
                    seed = (object)null,
                    guidance_scale = 7.5,
                    multi_lingual = "no",
                    panorama = "no",
                    self_attention = "no",
                    upscale = "no",
                    embeddings_model = "embeddings_model_id",
                    webhook = (string)null,
                    track_id = (string)null
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Распарсить JSON-ответ и извлечь поле "output"
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                output = result.output[0].ToString();

                // Скачать файл по ссылке
                using (var webClient = new HttpClient())
                {
                    var imageBytes = await webClient.GetByteArrayAsync(output);
                    System.IO.File.WriteAllBytes(filePath, imageBytes);
                }

                Console.WriteLine("Image generated: " + filePath);
                return (filePath, output);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in image generating: " + ex.Message);
                return (null, output);
            }
        }

        private static string ConvertToText()
        {
            string scriptPath = "..\\..\\..\\..\\audio\\audio.py";
            string argument = "";

            Process process = new Process();
            process.StartInfo.FileName = "python";
            process.StartInfo.Arguments = $"\"{scriptPath}\" \"{argument}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
            string text = ReturnTxtandDelete();
            return text;

        }
        private static void ytVideo(string ytlink)
        {
            string scriptPath = "..\\..\\..\\..\\video_youtube\\video_youtube.py";
            string argument = ytlink;
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python"; // The Python interpreter executable
            start.Arguments = $"{scriptPath} {argument}";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            using (Process process = Process.Start(start))
            {
                // Read the output of the Python script if needed
                string output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }
        }
        private static void ytAudio(string ytlink)
        {
            string scriptPath = "..\\..\\..\\..\\audio_youtube\\audio_youtube.py";
            string argument = ytlink;


            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python"; // The Python interpreter executable
            start.Arguments = $"{scriptPath} {argument}";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            using (Process process = Process.Start(start))
            {
                // Read the output of the Python script if needed
                string output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }
        }
        private static String ReturnTxtandDelete()
        {
            // Ищем первый .txt файл в списке
            string txtFileName = "";

            string[] files = Directory.GetFiles("file_path", "*.txt");

            // Если найдено хотя бы один текстовый файл, то читаем его содержимое и выводим в консоль
            if (files.Length > 0)
            {
                string filePath = files[0];
                txtFileName = System.IO.File.ReadAllText(filePath);
            }
            else
            {
                Console.WriteLine("Текстовые файлы не найдены.");
            }
            DeleteCatalog();
            return txtFileName; //text from the file actually

        }
        private static void DeleteCatalog()
        {
            string directoryPath = "file_path";

            // Получаем список всех файлов в каталоге
            string[] files = Directory.GetFiles(directoryPath);

            // Удаляем каждый файл из списка
            foreach (string file in files)
            {
                System.IO.File.Delete(file);
            }
        }

        private static string GetLanguageCode(string result)
        {
            switch (result)
            {
                case "en":
                    return Languages.English_UnitedStates;
                case "fr":
                    return Languages.French_France;
                case "ar":
                    return Languages.Arabic_SaudiArabia;
                case "bg":
                    return Languages.Bulgarian;
                case "ca":
                    return Languages.Catalan;
                case "cs":
                    return Languages.Czech;
                case "da":
                    return Languages.Danish;
                case "de":
                    return Languages.German_Germany;
                case "el":
                    return Languages.Greek;
                case "he":
                    return Languages.Hebrew;
                case "hi":
                    return Languages.Hindi;
                case "hu":
                    return Languages.Hungarian;
                case "id":
                    return Languages.Indonesian;
                case "it":
                    return Languages.Italian;
                case "ja":
                    return Languages.Japanese;
                case "ko":
                    return Languages.Korean;
                case "nl":
                    return Languages.Dutch_Netherlands;
                case "no":
                    return Languages.Norwegian;
                case "pl":
                    return Languages.Polish;
                case "pt":
                    return Languages.Portuguese_Brazil;
                case "ro":
                    return Languages.Romanian;
                case "ru":
                    return Languages.Russian;
                case "es":
                    return Languages.Spanish_Spain;
                case "sv":
                    return Languages.Swedish;
                case "ta":
                    return Languages.Tamil;
                case "th":
                    return Languages.Thai;
                case "tr":
                    return Languages.Turkish;
                case "vi":
                    return Languages.Vietnamese;
                case "zh":
                    return Languages.Chinese_China;
                case "zh-Hant":
                    return Languages.Chinese_Taiwan;
                // Добавьте другие соответствия для языков, поддерживаемых VoiceRSS_SDK
                default:
                    return Languages.English_UnitedStates;
            }
        }
        private static String Sticker()
        {
            List<string> stickerIds = new List<string> { "", "", "", "", "CAACAgEAAxkBAAEH5C1j-b79uVkvczcFq00_aSP23OaJzwACDQADoQUMDREdW5nit7BRLgQ", "", "", "CAACAgUAAxkBAAEH5DNj-b8Tx7XvcfDYyLp-ciLBY1WgowACjgMAAukKyANXArxl3WaqUi4E", "CAACAgEAAxkBAAEH5DVj-b8anyoOSYMsYlsPd3YkNTf6tAACDAADoQUMDe_DoKO3dIgxLgQ", "", "", "CAACAgIAAxkBAAEH64lj_Hvre0gYx5fM9D5ztaPf0zlSGwAC-QIAAlwCZQMs9V9TYyDSPC4E", "CAACAgIAAxkBAAEH641j_HzpOhAaeGKIzVZAjy1q5CtPMQACrQADZaIDLGDZ_6CCKHo7LgQ", "", "CAACAgIAAxkBAAEH65Fj_Hzz4F3vgOZJv3Eh1jrsVpRCAANKAAOQ7Twn3S-pTHJh2hYuBA", "" };

            // Generate a random index to select a sticker ID
            Random random = new Random();
            int randomIndex = random.Next(stickerIds.Count);

            // Get the randomly selected sticker ID
            string randomStickerId = stickerIds[randomIndex];

            return randomStickerId;
        }
    }

    public class BotState
    {
        // Use a dictionary to store the waiting state for each user
        private static Dictionary<long, bool> waitingForVideoStates = new Dictionary<long, bool>();

        // Check if the bot is waiting for a video for a specific user
        public static bool IsWaitingForVideo(long userId)
        {
            return waitingForVideoStates.ContainsKey(userId) && waitingForVideoStates[userId];
        }
        // Set the waiting state for a specific user
        public static void SetWaitingForVideo(long userId, bool isWaiting)
        {
            if (waitingForVideoStates.ContainsKey(userId))
            {
                waitingForVideoStates[userId] = isWaiting;
            }
            else
            {
                waitingForVideoStates.Add(userId, isWaiting);
            }
        }
    }
}