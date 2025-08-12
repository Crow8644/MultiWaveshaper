module Files
open Functionality
open System
open System.IO
open System.Windows
open Microsoft.Win32
open NAudio.Wave

// default_path holds the last used directory, which we restore to if another file is selected
// We start the proccess trying to use this user profile's directory

let userFolder: string = Environment.SpecialFolder.UserProfile |> Environment.GetFolderPath
let mutable default_path: string = if Directory.Exists userFolder then userFolder else "c:\\"

let seperateParentPath = Utils.regexSeperate "^(.*\\\)([^\\\]*)(\.[^\\\]*)$" default_path

// wav, mp3, aiff, and wma are the four currently supported extentions
let isValidAudioFile = Utils.matchesExtention [|".wav"; ".mp3"; ".aiff"; "wma"|]

let setupAudioFile(file: string) = 
    if File.Exists file
    then
        let fileList = seperateParentPath file
        // Sets the default path to be used on next open to the parent folder of this selection
        default_path <- if fileList.Length > 1 then fileList.Item 1 else fileList.Item 0
        true
    else
        false

// This function runs the file selection dialog and proccesses the result, ultimately initiating playback
// Must run in the UI thread
// Returns true if audio is playing after excecution, false if not
let getAudioFile(): option<string> =
    let dialog = new OpenFileDialog()

    // Extra protection, resets the user's home directory if the previously used directory was deleted
    if not (Directory.Exists default_path)
    then if Directory.Exists userFolder then default_path <- userFolder else default_path <- "c:\\"

    // Set dialog properties
    dialog.InitialDirectory <- default_path                                                     // Starts at the saved default
    dialog.Filter <- "supported audio files (*.wav;*.mp3;.aiff;.wma)|*.wav;*.mp3;*.aiff;*.wma"  // Only allows the selection of supported files

    // Triggers the user selection and saves whether a file was selected or cancelled

    if dialog.ShowDialog().Value && setupAudioFile(dialog.FileName)
        then
        //If selection was a success
        Some dialog.FileName
    else 
        None
       
let saveToUserSelectedStream (inStream: ISampleProvider) =
    let dialog: SaveFileDialog = new SaveFileDialog()

    if not (Directory.Exists default_path)
    then if Directory.Exists userFolder then default_path <- userFolder else default_path <- "c:\\"

    // Set dialog properties
    dialog.InitialDirectory <- default_path
    dialog.Filter <- "WAV Files|*.wav"
    dialog.Title <- "Chose where to save the processed file"

    if dialog.ShowDialog().Value && File.Exists dialog.FileName then
        let fs: Stream = dialog.OpenFile()
        // This static member writes out a wave file. !!THIS ONLY WORKS IF STREAM RETURNS 0 AT THE END!!
        WaveFileWriter.WriteWavFileToStream(fs, inStream.ToWaveProvider())

        Some(dialog.FileName)
    else
        None