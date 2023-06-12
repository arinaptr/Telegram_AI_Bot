import sys
import yt_dlp

# Get the YouTube link from the command line arguments
url = sys.argv[1]

# Set the options for downloading the audio
ydl_opts = {
    'format': 'bestaudio/best',
    'outtmpl': 'file_path/audio.mp3',
}

# Create a yt-dlp downloader object
with yt_dlp.YoutubeDL(ydl_opts) as ydl:
    # Download the audio
    ydl.download([url])