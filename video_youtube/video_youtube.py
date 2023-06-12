import sys
import yt_dlp

# Set the URL of the YouTube video you want to download
url = sys.argv[1]

# Set the options for downloading the video
ydl_opts = {
    'format': 'bestvideo[height<=720][ext=mp4]+bestaudio[ext=m4a]/best[height<=720][ext=mp4]',
    'outtmpl': 'file_path/video.mp4',
}

# Create a yt-dlp downloader object
with yt_dlp.YoutubeDL(ydl_opts) as ydl:
    # Download the video
    ydl.download([url])
