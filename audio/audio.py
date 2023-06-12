import os
import openai

def process_content(file_name):
    openai.api_key = "YOUR_API_KEY"
    audio_file = open(file_name, "rb")
    transcript = openai.Audio.transcribe("whisper-1", audio_file)
    transcript_str = transcript.text
    print("converted")
    txtf = open("file_path\\speech.txt", "w", encoding="utf-8")
    txtf.write(transcript_str)

if __name__=="__main__":
    folder_name = "file_path"  # replace this with the actual folder name
    f_name = os.listdir(folder_name)[0]  # get the first file in the folder
    file_path = os.path.join(folder_name, f_name)  # construct the file path
    process_content(file_path)
